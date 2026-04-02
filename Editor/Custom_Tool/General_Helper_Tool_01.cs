using UnityEngine;
using TMPro;
using CLIP.Framework_Unity;
#if WX
using WeChatWASM;
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using CLIP.Project_Mouse.Game_Play_System;
#endif

namespace CLIP
{
    namespace Project_Mouse
    {
#if UNITY_EDITOR
        namespace Custom_Tool
        {
            public class General_Helper_Tool_01 : EditorWindow
            {
                public string _local_ws;
                public string _romote_ws;
                public string _local_CDN;
                public string _romote_CDN;


                public NetWork_Center _network_center;
                // public WXEditorScriptObject _mini_game_config;
                public WeChat_Env _wechat_env;
                public TMP_FontAsset _tmp_asset;

                public SerializedProperty _sp_network_center;
                public SerializedProperty _sp_mini_game_config;
                public SerializedProperty _sp_wechat_env;
                public SerializedProperty _sp_tmp_asset;

                public SerializedObject _local_editor_so;
                [MenuItem("Tools/Project_Mouse/General_Helper_Tool_01")]
                private static void ShowWindow()
                {
                    GetWindow<General_Helper_Tool_01>().Show();

                }
                private void OnEnable()
                {
                    _local_editor_so = new SerializedObject(this);
                    _sp_network_center = _local_editor_so.FindProperty("_network_center");
                    _sp_mini_game_config = _local_editor_so.FindProperty("_mini_game_config");
                    _sp_wechat_env = _local_editor_so.FindProperty("_wechat_env");
                    _sp_tmp_asset = _local_editor_so.FindProperty("_tmp_asset");
                }

                private void OnGUI()
                {
                    _local_ws = EditorGUILayout.TextField("_local_ws", _local_ws);
                    _romote_ws = EditorGUILayout.TextField("_romote_ws", _romote_ws);
                    _local_CDN = EditorGUILayout.TextField("_local_CDN", _local_CDN);
                    _romote_CDN = EditorGUILayout.TextField("_romote_CDN", _romote_CDN);

                    EditorGUILayout.PropertyField(_sp_mini_game_config);
                    EditorGUILayout.PropertyField(_sp_network_center);
                    EditorGUILayout.PropertyField(_sp_wechat_env);
                    _local_editor_so.ApplyModifiedProperties();
                    if (GUILayout.Button("Set_To_Local"))
                    {

                        //_mini_game_config.ProjectConf.CDN = _local_CDN;
                        // _wechat_env.CDN_URL = _local_CDN;
                        _network_center.ws_url_local = _local_ws;
                        _network_center.ws_url_remote = _local_ws;


                        //  EditorUtility.SetDirty(_mini_game_config);
                        EditorUtility.SetDirty(_wechat_env);
                        EditorUtility.SetDirty(_network_center);
                    }

                    if (GUILayout.Button("Set_To_Remote"))
                    {
                        // _mini_game_config.ProjectConf.CDN = _romote_CDN;
                        //  _wechat_env.CDN_URL = _romote_CDN;
                        _network_center.ws_url_local = _romote_ws;
                        _network_center.ws_url_remote = _romote_ws;

                        //   EditorUtility.SetDirty(_mini_game_config);
                        EditorUtility.SetDirty(_wechat_env);
                        EditorUtility.SetDirty(_network_center);
                    }

                    GUILayout.Space(64);
                    EditorGUILayout.PropertyField(_sp_tmp_asset);
                    if (GUILayout.Button("Change_Font_At_Open_Scene"))
                    {
                        if (_sp_tmp_asset == null) return;

                        var editorPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                        if (editorPrefabStage == null)
                        {
                            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                            foreach (var _root in roots)
                            {
                                var _texts = _root.GetComponentsInChildren<TextMeshProUGUI>(true);
                                foreach (var _text in _texts)
                                {
                                    _text.font = _tmp_asset;
                                    EditorUtility.SetDirty(_text);
                                }
                                EditorUtility.SetDirty(_root);
                            }

                        }
                        else
                        {
                            var _path = editorPrefabStage.assetPath;
                            var _root = editorPrefabStage.prefabContentsRoot;

                            _root = PrefabUtility.LoadPrefabContents(_path);
                            var _texts = _root.GetComponentsInChildren<TextMeshProUGUI>(true);
                            foreach (var _text in _texts)
                            {
                                _text.font = _tmp_asset;
                            }
                            PrefabUtility.SaveAsPrefabAsset(_root, _path);
                        }

                    }

                }

            }

        }
#endif
    }
}


