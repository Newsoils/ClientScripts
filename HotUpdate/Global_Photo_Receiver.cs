//using System;
//using CLIP.Framework_Core.Serialization;
//using CLIP.Framework_Unity;
//using CLIP.Project_Mouse.Game_Play_System;
//using UnityEngine;
//using Newtonsoft.Json.Linq;
//using Newtonsoft.Json;

//public class Global_Photo_Receiver : SingletonMono<Global_Photo_Receiver>
//{
//    private Global_Photo_Manager _photoManager;

//    private const string ReceiverName = "Global_Photo_Receiver";

//    #region Unity Life Cycle

//    private void Start()
//    {
//        _photoManager = GetComponent<Global_Photo_Manager>();
//        if (_photoManager == null)
//        {
//            Debug.LogError("Global_Photo_Receiver: Photo_Manager not found");
//            return;
//        }

//        BindUnityEvents();

//        Debug.Log($"Global_Photo_Receiver initialized on {_photoManager.gameObject.name}");
//    }

//    protected override void OnDestroy()
//    {
//        base.OnDestroy();
//        UnbindUnityEvents();
//        _photoManager = null;
//    }

//    #endregion

//    #region UnityEvent Binding

//    private void BindUnityEvents()
//    {
//        if (_photoManager != null)
//        {
//            _photoManager._get_photo_from_server.AddListener(GetPhotoFromServer);
//            _photoManager._upload_photo_to_server.AddListener(UploadPhotoToServer);
//        }
//    }

//    private void UnbindUnityEvents()
//    {
//        if (_photoManager != null)
//        {
//            _photoManager._get_photo_from_server.RemoveListener(GetPhotoFromServer);
//            _photoManager._upload_photo_to_server.RemoveListener(UploadPhotoToServer);
//        }
//    }

//    #endregion

//    #region Network Send Helpers

//    // private void SendMsg(string action, string detail)
//    // {
//    //     if (_networkCenter == null || !_networkCenter._connect_to_player_server)
//    //         return;

//    //     Network_Msg msg = new Network_Msg
//    //     {
//    //         sender = ReceiverName,
//    //         action_target = "Player_Server",
//    //         action = action,
//    //         detail_info = detail,
//    //         _sending_mode = Msg_Sending_Mode.Client_to_Server
//    //     };

//    //     _networkCenter.send_via_wss(msg);
//    // }

//    #endregion

//    #region Client -> Server Methods

//    private void GetPhotoFromServer()
//    {
//        if (_photoManager == null || string.IsNullOrEmpty(_photoManager._current_try_get_photo_name))
//        {
//            Debug.Log("Global_Photo_Receiver: No photo name specified to get");
//            return;
//        }

//        // string photoName = _photoManager._current_try_get_photo_name;
//        // SendMsg("Get_Photo", photoName);
//        // TODO zhaorui
//        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
//            NetWork_Center_WSS.SendMsg(new Cmd.EmptyReq());

//        Debug.Log("Get_Photo request sent");
//    }

//    private void UploadPhotoToServer()
//    {
//        if (_photoManager == null ||
//            string.IsNullOrEmpty(_photoManager._current_upload_photo_name) ||
//            string.IsNullOrEmpty(_photoManager._current_photo_data_str))
//        {
//            Debug.Log("Global_Photo_Receiver: Photo data incomplete for upload");
//            return;
//        }

//        // 创建JSON数组 [photoName, photoData]
//        // string jsonArray = $"[\"{_photoManager._current_upload_photo_name}\",\"{_photoManager._current_photo_data_str}\"]";
//        // SendMsg("Upload_Photo", jsonArray);
//        // TODO zhaorui
//        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
//            NetWork_Center_WSS.SendMsg(new Cmd.EmptyReq());

//        Debug.Log("Upload_Photo request sent");
//    }

//    #endregion

//    private void HandleReceivePhoto(string detailInfo)
//    {
//        try
//        {
//            var parts = Serialization_Provider.DeserializeObject<string[]>(detailInfo);

//            if (parts.Length == 2)
//            {
//                // 移除引号
//                string photoName = parts[0].Trim().Trim('"');
//                string photoData = parts[1].Trim().Trim('"');

//                _photoManager._current_photo_data_str = photoData;
//                _photoManager.add_photo_to_list(photoName);

//                Debug.Log($"Photo received: {photoName}");
//            }
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError($"Global_Photo_Receiver: Failed to parse photo data - {ex.Message}");
//        }
//    }
//}