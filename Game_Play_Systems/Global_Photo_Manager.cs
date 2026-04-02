using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Kernel.Dispatch;
using Sych.ShareAssets.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;
using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;


namespace CLIP.Project_Mouse.Game_Play_System
{
    public enum PhotoMode
    {
        Default,
        Room,
        Dispatch,
        Firend
    }


    public class Global_Photo_Manager : SingletonMono<Global_Photo_Manager>
    {
        public List<photo_info_saved> _local_image_list;

        public RenderTexture roomPhotoRT;
        public RenderTexture dispatchPhotoRT;
        public RenderTexture defaultPhotoRT;

        private RenderTexture _current_RT;

        [Header("For_Network")]
        public Texture2D _photo_to_upload;
        public RawImage _photo_display;

        public List<Texture2D> _photo_from_network = new List<Texture2D>();

        public string _current_try_get_photo_name = string.Empty;
        public string _current_upload_photo_name = string.Empty;

        [HideInInspector]
        public string _current_photo_data_str = string.Empty;
        [Header("Event")]
        public UnityEvent _get_photo_from_server = new UnityEvent();
        public UnityEvent _upload_photo_to_server = new UnityEvent();

        [HideInInspector]
        public string Last_Photo_Path => _local_image_list[^1]?._local_path;
        private string Last_Photo_Name => _local_image_list[^1] ?._photo_name;

        private string lastDispatchPhotoPath = string.Empty;

        [Header("开发：按 photo_name 快速拍派遣照")]
        [Tooltip("填写 Photo_Info.photo_name，运行游戏后在组件右键菜单选 Capture")]
        [SerializeField] private string _debugDispatchPhotoName;

        /// <summary>项目根目录旁的文件夹，便于在资源管理器中查看（与 Assets 同级）。</summary>
        public static string GetDebugDispatchPhotoOutputDirectory()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PhotoDebugCaptures"));
        }

        void Start()
        {
            roomPhotoRT = new RenderTexture(Screen.width, Screen.height, 32);
            roomPhotoRT.name = "RoomPhotoRT";

            if (dispatchPhotoRT == null)
            {
                Log.Error("PhotoManager: DispatchRT Misssing!");
            }

            DontDestroyOnLoad(this.gameObject);
            Load_PhotoList_Info();

            EvtDsp.AddEvt(EvtNames.Dispatch_On_End, StartCaptureDispatchPhoto);
        }

        protected override void OnDestroy()
        {
            EvtDsp.RemoveEvt(EvtNames.Dispatch_On_End, StartCaptureDispatchPhoto);
        }


        public void CapturePhoto(PhotoMode mode, string name = "", bool isScreen = false, string saveDirectoryOverride = null)
        {
            string fileName = name;

            if (string.IsNullOrEmpty(name))
            {
                fileName = mode.ToString() + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
            }

            StartCoroutine(CaptureRT(mode, (tex) =>
            {
                string path = SaveTextureToDisk(tex, fileName, saveDirectoryOverride);
                AddPhotoInfo(path, fileName, mode);
                Destroy(tex);
            }));

            if (isScreen == true)
            {
                StartCoroutine(CaptureScreen((tex) =>
                {
                    string path = SaveTextureToDisk(tex, fileName, saveDirectoryOverride);
                    AddPhotoInfo(path, fileName, mode);
                    Destroy(tex);
                }));
            }
        }

        private IEnumerator CaptureRT(PhotoMode mode, Action<Texture2D> onFinish= null)
        {
            yield return new WaitForEndOfFrame();

            RenderTexture rt = GetRenderTexture(mode);

            RenderTexture.active = rt;

            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            RenderTexture.active = null;

            onFinish?.Invoke(tex);
        }

        private IEnumerator CaptureGivenRT(PhotoMode mode, float x, float y, float width, float height, Action<Texture2D> onFinish = null)
        {
            yield return new WaitForEndOfFrame();
            RenderTexture rt = GetRenderTexture(mode);
            RenderTexture.active = rt;
            var tex = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(x, y, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            onFinish?.Invoke(tex);
        }
        private IEnumerator CaptureScreen(Action<Texture2D> onFinish = null)
        {
            yield return new WaitForEndOfFrame();

            Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

            tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            tex.Apply();

            onFinish?.Invoke(tex);
        }

        private RenderTexture GetRenderTexture(PhotoMode mode)
        {
            switch(mode)
            {
                case PhotoMode.Dispatch:
                    return dispatchPhotoRT;
                case PhotoMode.Room:
                    return roomPhotoRT;
                default:
                    return defaultPhotoRT;
            }
        }

        public string SaveTextureToDisk(Texture2D tex, string fileName, string directoryOverride = null)
        {
            byte[] bytes = tex.EncodeToPNG();

            string folder = string.IsNullOrEmpty(directoryOverride) ? Application.persistentDataPath : directoryOverride;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string path = Path.Combine(folder, fileName);

            File.WriteAllBytes(path, bytes);

            Debug.Log("Screenshot saved to: " + path);

            return path;
        }

        public void AddPhotoInfo(string path, string fileName, PhotoMode mode)
        {
            photo_info_saved info = new photo_info_saved();

            info._photo_name = fileName;
            info._local_path = path;
            info._photo_upload_time = DateTime.Now;
            info._photo_type = "normal";
            info._photo_source = mode.ToString();

            _local_image_list.Add(info);

            Save_PhotoList_Info();
        }

        public void SaveLastPhotoToGallery()
        {
            var last = _local_image_list[^1];
            SaveToGallery(last._local_path, last._photo_name);
        }

        public void SaveToGallery(string path, string fileName)
        {
#if UNITY_ANDROID || UNITY_IOS
            NativeGallery.SaveImageToGallery(path, "Project_Mouse", fileName);
#endif
        }

        public void Try_Load_Last_Image(RawImage rawImage)
        {
            if (File.Exists(Last_Photo_Path))
            {
                PM_RM.load_png_as_texture(Last_Photo_Path, (tex) =>
                {
                    if (rawImage != null)
                    {
                        rawImage.texture = tex;
                    }
                    else
                    {
                        Debug.LogError("RawImage component is not assigned.");
                    }
                });
            }
        }

        public void try_load_image(string imagePath, RawImage rawImage)
        {
            if (File.Exists(imagePath) == false)
            {
                Debug.LogWarning($"File does not exist at path: {imagePath}");
                return;
            }

            if (File.Exists(imagePath) == true)
            {
                PM_RM.load_png_as_texture(imagePath, (tex) =>
                {
                    if (rawImage != null)
                    {
                        rawImage.texture = tex;
                    }
                    else
                    {
                        Debug.LogWarning("RawImage component is not assigned.");
                    }
                });
            }
        }

        public async Task<Texture2D> Load_Last_Image(Action<Texture2D> callback )
        {
            if (File.Exists(Last_Photo_Path))
            {
                return await PM_RM.load_png_as_texture(Last_Photo_Path, callback);
            }
            return null;
        }

        public async Task<Texture2D> Load_Image(string imageName, Action<Texture2D> callback= null)
        {
            var imageInfo = _local_image_list.Find(info => info._photo_name == imageName);
            if(imageInfo != null && File.Exists(imageInfo._local_path))
            {
                return await PM_RM.load_png_as_texture(imageInfo._local_path, callback);
            }
            else
            {
                Debug.LogWarning($"Image with name {imageName} not found in local image list or file does not exist.");
                return null;
            }
        }

        public void share_previous_saved_image_via_native()
        {
            
            string image_path = Path.Combine(Application.persistentDataPath, Last_Photo_Name);
            Debug.Log("share_previous_saved_image_via_native()_image_path_=_" + image_path);
#if UNITY_ANDROID || UNITY_IOS
            var _paths = new List<string>();
            _paths.Add(image_path);
            Share.Item(image_path, share_callback);
#endif
        }

        public void share_callback(bool _flag)
        {
            if (_flag == true)
            {
                Debug.Log("Share operation completed successfully.");
            }
            else
            {
                Debug.LogError("Share operation failed or was cancelled.");
            }
        }


        #region 服务器通讯
        public void get_photo_from_server()
        {
            _get_photo_from_server.Invoke();
        }
        public void upload_photo_to_server()
        {
            _upload_photo_to_server.Invoke();
        }
        public void get_photo_from_server(string _photo_name)
        {
            _current_try_get_photo_name = _photo_name;
            get_photo_from_server();
        }


        public void add_photo_to_list(string _photo_name)
        {
            byte[] _png_data = GF_SP.DeserializeObject<byte[]>(_current_photo_data_str);

            // 1. 创建一个临时的 Texture2D 对象。尺寸可以任意，LoadImage 会自动调整。
            Texture2D _tex = new Texture2D(2, 2);

            // 2. 调用 LoadImage 方法，将字节数组加载到纹理中。
            //    如果加载成功，该方法返回 true。
            if (_tex.LoadImage(_png_data))
            {
                _tex.name = _photo_name; // 给纹理命名是一个好习惯
                _photo_from_network.Add(_tex);
                if (_photo_display != null) _photo_display.texture = _tex;
            }
            else
            {
                Debug.LogWarning("从 PNG 数据创建 Texture2D 失败，照片名称: " + _photo_name);
            }
        }

        //public string get_next_photo_name()
        //{
        //    var player_name = "";
        //    if (Global_Game_Manager._instance != null)
        //    {
        //        player_name = Global_Game_Manager._instance._current_player_name;
        //    }
        //    string _name = "#Photo#_" + player_name + "_" + Guid.NewGuid() + ".png";
        //    return _name;
        //}

        //public string upload_photo_to_server_and_return_photo_name(Texture2D _photo_texture)
        //{
        //    var _photo_name = get_next_photo_name();
        //    upload_photo_to_server(_photo_name, _photo_texture);
        //    return _photo_name;
        //}
        //public async void upload_photo_to_server(string _photo_name, Texture2D _photo_texture)
        //{
        //    byte[] _png_data = _photo_texture.EncodeToPNG();
        //    _current_photo_data_str = GF_SP.SerializeObject(_png_data);
        //    _current_upload_photo_name = _photo_name;
        //    upload_photo_to_server();
        //}

        #endregion

        #region 存取PhotoList数据信息
        public void Save_PhotoList_Info()
        {
            Save_Load_Tools.Save<List<photo_info_saved>>("local_image_list.json", _local_image_list);
        }
        public void Load_PhotoList_Info()
        {
            _local_image_list = Save_Load_Tools.Load<List<photo_info_saved>>("local_image_list.json") ?? new List<photo_info_saved>();
        }


        // 序列化照片路径列表
        public void Save_Dispatch_PhotoList()
        {
            Save_Load_Tools.Save<List<string>>("dispatch_photo_path_list.json", Dispatch_Manager._instance._dispatch_photo_path_list);
        }

        // 反序列化照片路径列表
        public void Load_Dispatch_PhotoList()
        {
            Dispatch_Manager._instance._dispatch_photo_path_list = Save_Load_Tools.Load<List<string>>("dispatch_photo_path_list.json") ?? new List<string>();
        }
        #endregion

        public void StartCaptureDispatchPhoto()
        {
            if (Dispatch_Manager._instance == null || Dispatch_Manager._instance._player_dispatch_state == null)
            {
                Debug.LogWarning("StartCaptureDispatchPhoto: Dispatch_Manager 未就绪。");
                return;
            }

            string photoName = Dispatch_Manager._instance._player_dispatch_state.last_reward_photo_name;
            BeginCaptureDispatchPhotoInternal(photoName, null);
        }

        /// <summary>
        /// 按 Photo_Info.photo_name 拍一张派遣照片，保存到 <see cref="GetDebugDispatchPhotoOutputDirectory"/>（或 <paramref name="outputDirectoryOverride"/>）。
        /// 需在运行态且派遣配置已加载；会切换场景并在结束后回到主场景。
        /// </summary>
        public void DebugCaptureDispatchPhotoByName(string photoName, string outputDirectoryOverride = null)
        {
            if (string.IsNullOrWhiteSpace(photoName))
            {
                Debug.LogWarning("DebugCaptureDispatchPhotoByName: photo_name 为空。");
                return;
            }

            var dir = string.IsNullOrEmpty(outputDirectoryOverride) ? GetDebugDispatchPhotoOutputDirectory() : outputDirectoryOverride;
            BeginCaptureDispatchPhotoInternal(photoName.Trim(), dir);
        }

        [ContextMenu("Debug/按 Debug Dispatch Photo Name 拍摄（需 Play + 填 Inspector 字段）")]
        private void ContextMenu_DebugCaptureByInspectorName()
        {
            DebugCaptureDispatchPhotoByName(_debugDispatchPhotoName);
        }

        private void BeginCaptureDispatchPhotoInternal(string photoName, string saveDirectoryOverride)
        {
            if (Dispatch_Manager._instance == null)
            {
                Debug.LogWarning("BeginCaptureDispatchPhotoInternal: Dispatch_Manager._instance 为空。");
                return;
            }

            var _dispatch_config = Dispatch_Manager._instance._dispatch_configuration_so?._dispatch_config;
            if (_dispatch_config == null || _dispatch_config.photo_info_list == null)
            {
                Debug.LogWarning("BeginCaptureDispatchPhotoInternal: 派遣配置或 photo_info_list 为空。");
                return;
            }

            var photoInfo = _dispatch_config.photo_info_list.Find(p => p.photo_name == photoName);
            if (photoInfo == null)
            {
                Debug.LogWarning($"BeginCaptureDispatchPhotoInternal: 未找到 photo_name = \"{photoName}\" 的 Photo_Info。");
                return;
            }

            var mapEntry = _dispatch_config.map_info_list.Find(p => p.photo_Ids != null && p.photo_Ids.Contains(photoInfo.photo_id));
            if (mapEntry == null)
            {
                Debug.LogWarning($"BeginCaptureDispatchPhotoInternal: photo_id {photoInfo.photo_id} 未出现在任何 map 的 photo_Ids 中。");
                return;
            }

            var mapIndex = mapEntry.map_id;

            if (_dispatch_config.map_info_list.Count < mapIndex - 1)
            {
                Log.Error("Dispatch Take Photo Error:" + photoName + "超出数组下标:" + (mapIndex - 1) + "总共只有" + _dispatch_config.map_info_list.Count + "个地图");
                return;
            }

            var sceneName = _dispatch_config.map_info_list[mapIndex - 1].map_Scene_Name;

            PM_RM.load_scene_async(sceneName, (loadedScene) =>
            {
                var loadedCharacter = GameAssets.Instance.mainCharacter_Dispatch;
                StartCoroutine(CaptureDispatchPhotoCo(photoInfo, loadedCharacter, saveDirectoryOverride));
            });
        }

        private IEnumerator CaptureDispatchPhotoCo(Photo_Info photoInfo, GameObject loadedCharacter, string saveDirectoryOverride = null)
        {

            GameObject rain_Image = GameObject.Find("Rain_Image");
            if (rain_Image != null)
            {
                var weather = Global_Game_Manager._instance?._weather_state;
                if (weather != null && weather._current_weather == "Rain")
                    rain_Image.SetActive(true);
                else
                    rain_Image.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Rain_Image not found in the scene.");
            }

            if (loadedCharacter == null)
            {
                Log.Error("CaptureDispatchPhotoCo: mainCharacter_Dispatch 为空，无法实例化角色。请确认 GameAssets / 派遣主角已配置。");
                BackToMainScene();
                yield break;
            }

            var photoRoot = GameObject.Find("PhotoRoot")?.transform;

            if (photoRoot == null)
            {
                Log.Error("Photo_Root not found in the scene.");
                BackToMainScene();
                yield break;
            }

            var photo_Camera_ID = photoInfo.camera_id;

            var groupCount = photoRoot.childCount;

            if (groupCount == 0)
            {
                Log.Error("CaptureDispatchPhotoCo: PhotoRoot 下没有相机分组子物体。");
                BackToMainScene();
                yield break;
            }

            if (photo_Camera_ID < 0 || photo_Camera_ID >= groupCount + 1)
            {
                Log.Error($"CaptureDispatchPhotoCo: photo_Camera_ID={photo_Camera_ID} 越界，PhotoRoot 子物体数量为 {groupCount}（有效下标 0～{groupCount - 1}）。请核对 Photo_Info.camera_id 与场景 PhotoRoot 子节点顺序。");
                BackToMainScene();
                yield break;
            }

            Transform currentCameraT = null;

            for (int i = 1; i < groupCount + 1; i++)
            {
                var cameraGroup = photoRoot.GetChild(i - 1);
                if (i != photo_Camera_ID)
                    cameraGroup.gameObject.SetActive(false);
                else
                {
                    currentCameraT = cameraGroup;
                    cameraGroup.gameObject.SetActive(true);
                }
            }

            if (currentCameraT == null)
            {
                Log.Error("CaptureDispatchPhotoCo: 未选中相机分组（currentCameraT 为空）。");
                BackToMainScene();
                yield break;
            }

            var camera = currentCameraT.GetComponentInChildren<Camera>(true);
            if (camera == null)
            {
                Log.Error($"CaptureDispatchPhotoCo: 在相机分组「{currentCameraT.name}」下未找到 Camera。");
                BackToMainScene();
                yield break;
            }

            camera.gameObject.SetActive(true);
            var characterPoint = currentCameraT.GetComponentInChildren<Model_Placeholder>();
            if (characterPoint == null)
            {
                Log.Error($"CaptureDispatchPhotoCo: 在相机分组「{currentCameraT.name}」下未找到 Model_Placeholder。");
                BackToMainScene();
                yield break;
            }

            Unity_Tools.ClearAllChildren(characterPoint.transform);
            var characterOB = Instantiate(loadedCharacter, characterPoint.transform.position, characterPoint.transform.rotation);
            characterOB.transform.parent = characterPoint.transform;
            Unity_Tools.IdentityGameObject(characterOB);
            var animator = characterOB.GetComponentInChildren<Animator>();
            var aniName = photoInfo.main_character_pose_name;
            if (animator != null && !string.IsNullOrEmpty(aniName))
            {
                bool hasAnim = false;
                var rac = animator.runtimeAnimatorController;
                if (rac != null && rac.animationClips != null)
                {
                    foreach (var clip in rac.animationClips)
                    {
                        if (clip != null && clip.name == aniName)
                        {
                            hasAnim = true;
                            break;
                        }
                    }
                }
                if (hasAnim)
                    animator.Play(aniName);
            }

            yield return new WaitForSeconds(0.2f);

            if (dispatchPhotoRT == null)
            {
                Log.Error("CaptureDispatchPhotoCo: dispatchPhotoRT 未赋值，无法渲染到 RT。");
                BackToMainScene();
                yield break;
            }

            camera.targetTexture = Global_Photo_Manager.Instance.dispatchPhotoRT;
            camera.gameObject.SetActive(true);


            Canvas weatherCanvas = GameObject.Find("WeatherCanvas")?.GetComponent<Canvas>();
            if (weatherCanvas != null)
            {
                weatherCanvas.worldCamera = camera;
            }
            else
            {
                Debug.LogWarning("Canvas not found in the scene.");
            }


            yield return null;

            string fileName = $"{photoInfo.photo_name}_{photoInfo.camera_id}_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";

            //Global_Photo_Manager.Instance.Save_RT_to_PNG("dispatch", fileName);
            CapturePhoto(PhotoMode.Dispatch, fileName, false, saveDirectoryOverride);

            yield return new WaitForEndOfFrame();

            string saveRoot = string.IsNullOrEmpty(saveDirectoryOverride) ? Application.persistentDataPath : saveDirectoryOverride;
            string photoPath = Path.Combine(saveRoot, fileName);
            if (Dispatch_Manager._instance != null && Dispatch_Manager._instance._dispatch_photo_path_list != null)
                Dispatch_Manager._instance._dispatch_photo_path_list.Add(photoPath);

            yield return null;

            BackToMainScene();

            Save_Dispatch_PhotoList();
            Debug.Log($"Photo saved to: {photoPath}");
            lastDispatchPhotoPath = photoPath;

        }

        public void BackToMainScene()
        {
            SceneLoadHelper.Load_MainScene((s) => EvtDsp.TriggerEvt(EvtNames.Set_MainPanel_All_Active));
        }

        //public void Save_RT_to_PNG(RenderTexture RT, string _photo_source, string file_name = "screenshot")
        //{
        //    _current_RT = _photo_source == "Dispatch" ? dispatchPhotoRT : roomPhotoRT;
        //    StartCoroutine(CaptureAndSaveScreen(file_name, RT, _photo_source, true));
        //}

        //public void Save_RT_to_PNG(float x, float y, float width, float height, string _photo_source, string file_name = "screenshot")
        //{
        //    _current_RT = _photo_source == "Dispatch" ? dispatchPhotoRT : roomPhotoRT;
        //    StartCoroutine(CaptureAndSaveScreen(file_name, x, y, width, height, _photo_source, true));
        //}


        //private IEnumerator CaptureAndSaveScreen(string fileName, float x, float y, float width, float height, string _photo_source, bool is_RT = false)
        //{
        //    yield return new WaitForEndOfFrame();

        //    if (is_RT == true)
        //    {
        //        RenderTexture.active = _current_RT;
        //        var tex = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
        //        tex.ReadPixels(new Rect(x, y, width, height), 0, 0);
        //        tex.Apply();
        //        save_image_to_disk(fileName, tex, _photo_source);
        //    }
        //    else
        //    {
        //        // 截取屏幕 
        //        var tex = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
        //        tex.ReadPixels(new Rect(x, y, width, height), 0, 0);
        //        tex.Apply();
        //        save_image_to_disk(fileName, tex, _photo_source);
        //        Destroy(tex);
        //    }
        //}

        //public IEnumerator CaptureAndSaveScreen(string fileName, RenderTexture RT, string _photo_source, bool is_RT = true)
        //{
        //    yield return new WaitForEndOfFrame();

        //    if (is_RT == true)
        //    {
        //        RenderTexture.active = RT;
        //        var tex = new Texture2D(RT.width, RT.height, TextureFormat.RGB24, false);
        //        tex.ReadPixels(new Rect(0, 0, RT.width, RT.height), 0, 0);
        //        tex.Apply();
        //        save_image_to_disk(fileName, tex, _photo_source);
        //    }
        //    else
        //    {
        //        // 截取屏幕 
        //        var tex = new Texture2D(RT.width, RT.height, TextureFormat.RGB24, false);
        //        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        //        tex.Apply();
        //        save_image_to_disk(fileName, tex, _photo_source);
        //    }

        //}

        //        private void save_image_to_disk(string fileName, Texture2D _tex, string _photo_source, bool to_photo = true)
        //        {
        //            // 编码为 PNG
        //            byte[] bytes = _tex.EncodeToPNG();

        //            // 保存到硬盘
        //            //fileName = fileName + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        //            //Debug.Log("Image_Byte_as_str_=_" + General_Function.Serialization_Provider.SerializeObject(bytes));
        //            string path = Path.Combine(Application.persistentDataPath, fileName);
        //            File.WriteAllBytes(path, bytes);

        //            Debug.Log("Screenshot saved to: " + path);
        //            Last_Photo_Path = path;
        //            Last_Photo_Name = fileName;

        //            photo_info_saved _new_photo_info = new photo_info_saved();
        //            _new_photo_info._photo_name = fileName;
        //            _new_photo_info._local_path = path;
        //            _new_photo_info._photo_type = "normal";
        //            _new_photo_info._photo_upload_time = System.DateTime.Now;
        //            if (_photo_source == "dispatch")
        //            {
        //                _new_photo_info._photo_source = "dispatch";
        //            }
        //            else if (_photo_source == "indoor")
        //            {
        //                _new_photo_info._photo_source = "indoor";
        //            }
        //            else if (_photo_source == "indoor_with_friend")
        //            {
        //                _new_photo_info._photo_source = "indoor_with_friend";
        //            }
        //            if (!string.IsNullOrEmpty(_photo_source))
        //            {
        //                _local_image_list.Add(_new_photo_info);
        //            }
        //            Save_PhotoList_Info();

        //#if UNITY_ANDROID || UNITY_IOS
        //            if (to_photo == true)
        //            {
        //                SaveToGallery(path, fileName);
        //            }
        //#endif
        //        }
    }

}
