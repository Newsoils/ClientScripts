using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Path = System.IO.Path;

namespace CLIP.Framework_Unity.Asset
{
    public static class Project_Mouse_Resource_Management
    {
        public static string mode = "R.L.";
        public static void load_game_object_async(string path, Action<GameObject> _callback = null)
        {
            if (mode != "R.L.")
                return;

            // ✅ Step 1. 预检查资源是否存在
            var test = Resources.Load<GameObject>(path);
            if (test == null)
            {
                Debug.LogWarning($"⚠️ Resource not found at path: {path}\n{Environment.StackTrace}");

                // 可选：生成占位物
                GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                placeholder.name = $"MissingPrefab_{Path.GetFileName(path)}";
                placeholder.transform.localScale = Vector3.one * 0.3f;
                placeholder.GetComponent<Renderer>().material.color = Color.magenta;

                _callback?.Invoke(placeholder);
                return;
            }

            // ✅ Step 2. 异步加载（确认存在后再异步加载）
            var rq = Resources.LoadAsync<GameObject>(path);
            rq.completed += (_a_o) =>
            {
                if (rq.asset != null)
                {
                    Debug.Log($"✅ load_game_object_OK_from_path: {path}");
                    _callback?.Invoke(rq.asset as GameObject);
                }
                else
                {
                    Debug.LogWarning($"⚠️ Unexpected null after async load: {path}");
                    _callback?.Invoke(null);
                }
            };
        }

        public static async Task<GameObject> LoadGameObjectAsync(string path)
        {
            if (mode != "R.L.")
                return null;
            Debug.LogError("LoadGameObjectAsync: " + path);
            // 预检查
            var test = Resources.Load<GameObject>(path);
            if (test == null)
            {
                Debug.LogWarning($"Resource not found: {path}");

                GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                placeholder.name = $"MissingPrefab_{Path.GetFileName(path)}";
                placeholder.transform.localScale = Vector3.one * 0.3f;
                placeholder.GetComponent<Renderer>().material.color = Color.magenta;

                return placeholder;
            }

            // 异步加载
            var rq = Resources.LoadAsync<GameObject>(path);

            while (!rq.isDone)
                await Task.Yield();

            return rq.asset as GameObject;
        }

        public static void load_game_object(string path, Action<GameObject> _callback = null)
        {
            if (mode == "R.L.")
            {
                Debug.LogError("LoadGameObjectAsync: " + path);
                var go = Resources.Load<GameObject>(path);

                if (go != null)
                {
                    _callback(go);
                }
            }

        }
        public static void load_sprite_async(string path, Action<Sprite> _callback = null)
        {
            if (mode == "R.L.")
            {
                var rq = Resources.LoadAsync<Sprite>(path);
                rq.completed += (_a_o) =>
                {
                    if (rq.asset != null)
                    {
                        //Debug.Log("load_sprite()_OK_from_path:_" + path);
                        _callback(rq.asset as Sprite);
                    }
                    else
                    {
                        Debug.LogError("Failed to load Sprite from path: " + path);
                    }
                };
            }
        }

        //private static int _requestCounter = 0;
        //public static int load_sprite_async(string path,   Action<Sprite, int> callback)
        //{
        //    int requestId = ++_requestCounter;

        //    if (mode == "R.L.")
        //    {
        //        var rq = Resources.LoadAsyncByKey<Sprite>(path);
        //        rq.completed += _ =>
        //        {
        //            if (rq.asset != null)
        //            {
        //                callback(rq.asset as Sprite, requestId);
        //            }
        //        };
        //    }

        //    return requestId;
        //}


        public static void load_material_async(string path, Action<Material> _callback = null)
        {
            if (mode == "R.L.")
            {
                var rq = Resources.LoadAsync<Material>(path);
                rq.completed += (_a_o) =>
                {
                    if (rq.asset != null)
                    {
                        Debug.Log("load_material()_OK_from_path:_" + path);
                        _callback(rq.asset as Material);
                    }
                    else
                    {
                        Debug.LogError("Failed to load Material from path: " + path);
                    }
                };
            }
        }

        public static void load_sub_sprite(string image_path, string sprite_name, Action<Sprite> _callback =null)
        {
            if (mode == "R.L.")
            {

                var _sp = Resources.LoadAll<Sprite>(image_path);
                Sprite _ans = null;
                foreach (var _s in _sp)
                {
                    if (_s.name == sprite_name)
                    {
                        _ans = _s;
                        break;
                    }
                }
                if (_ans != null) _callback(_ans);
            }
        }

        public static void Load_Sprite(string path, Action<Sprite> _callback)
        {
            if (mode == "R.L.")
            {
                var rq = Resources.Load<Sprite>(path);
                _callback.Invoke(rq);
            }
        }

        public static async Task<Texture2D> load_png_as_texture(string path, Action<Texture2D> _callback = null)
        {
            if (mode == "R.L.")
            {
                if (File.Exists(path))
                {
                    byte[] fileData = await File.ReadAllBytesAsync(path);
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(fileData); // Load the image data into the texture
                    _callback?.Invoke(texture);
                    return texture;
                }
                else
                {
                    Debug.LogError("File not found at path: " + path);
                }
            }
            return null;

        }
        public static void load_scene_async(string _path, Action _callback = null)
        {
            load_scene_async(_path, (_s) =>
            {
                Debug.Log(("Scene loaded and callback executed for scene: " + _s.name));
                if(_callback!=null) _callback.Invoke();
            });
        }
        public static void load_scene_async(string path, Action<Scene> _callback_finishing)
        {
            if (mode == "R.L.")
            {
                var async_loader = SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
                async_loader.completed += (async_operation) =>
                {
                    if (async_loader.isDone)
                    {

                        Debug.Log("Scene loaded successfully: " + path);
                        Scene loadedScene = SceneManager.GetSceneByName(path);

                        _callback_finishing(loadedScene);
                    }
                    else
                    {
                        Debug.LogError("Failed to load scene: " + path);
                    }
                };
            }
        }


        public static void load_main_character(string main_character_info_json, Action<GameObject> _callback)
        {
            if (mode == "R.L.")
            {
                var path = "Prefabs/Character/Main_Character";
                var rq = Resources.LoadAsync<GameObject>(path);
                rq.completed += (_a_o) =>
                {
                    if (rq.asset != null)
                    {
                        Debug.Log("load_main_character()_OK_from_path:_" + path);
                        _callback(rq.asset as GameObject);
                    }
                    else
                    {
                        Debug.LogError("Failed to load_main_character from path: " + path);
                    }
                };
            }
        }
    }
}