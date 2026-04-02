using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Custom_Tool
        {
#if UNITY_EDITOR

            [CustomEditor(typeof(CLIP.Project_Mouse.Game_Play_System.Email_And_Announcement_Manager))]
            public class Email_And_Announcement_Manager_Editor : Editor
            {
                public CLIP.Project_Mouse.Game_Play_System.Email_And_Announcement_Manager _instance;

                private string deletedMailIdsInput = "";
                private string readMailIdsInput = "";
                private string getRewardMailIdsInput = "";
                private void OnEnable()
                {
                    _instance = (CLIP.Project_Mouse.Game_Play_System.Email_And_Announcement_Manager)target;

                }


                public override void OnInspectorGUI()
                {
                    DrawDefaultInspector();
                    // Save temp mail record
                    if (GUILayout.Button("Save Temp Mail Record to JSON"))
                    {
                        if (_instance == null)
                        {
                            Debug.LogWarning("Email_And_Announcement_Manager instance is null.");
                        }
                        else if (_instance._temp_mail_record == null)
                        {
                            Debug.LogWarning("_temp_mail_record is null, cannot save.");
                        }
                        else
                        {
                            string json = JsonConvert.SerializeObject(_instance._temp_mail_record);
                            string defaultName = "temp_mail_record.json";
                            string path = EditorUtility.SaveFilePanel("Save Temp Mail Record", Application.dataPath, defaultName, "json");
                            if (!string.IsNullOrEmpty(path))
                            {
                                File.WriteAllText(path, json);
                                Debug.Log($"Temp mail record saved to: {path}");
                                AssetDatabase.Refresh();
                            }
                        }
                    }

                    GUILayout.Space(6);

                    // Save temp announcement record
                    if (GUILayout.Button("Save Temp Announcement Record to JSON"))
                    {
                        if (_instance == null)
                        {
                            Debug.LogWarning("Email_And_Announcement_Manager instance is null.");
                        }
                        else if (_instance._temp_announcement_record == null)
                        {
                            Debug.LogWarning("_temp_announcement_record is null, cannot save.");
                        }
                        else
                        {
                            string json = JsonConvert.SerializeObject(_instance._temp_announcement_record);
                            string defaultName = "temp_announcement_record.json";
                            string path = EditorUtility.SaveFilePanel("Save Temp Announcement Record", Application.dataPath, defaultName, "json");
                            if (!string.IsNullOrEmpty(path))
                            {
                                File.WriteAllText(path, json);
                                Debug.Log($"Temp announcement record saved to: {path}");
                                AssetDatabase.Refresh();
                            }
                        }
                    }

                    GUILayout.Space(12);
                    GUILayout.Label("Mail Bulk Operations", EditorStyles.boldLabel);

                    // Deleted mail input
                    EditorGUILayout.LabelField("Deleted Mail IDs (comma or space separated)", EditorStyles.miniLabel);
                    deletedMailIdsInput = EditorGUILayout.TextField(deletedMailIdsInput);
                    if (GUILayout.Button("Trigger on_deleted_mail"))
                    {
                        if (_instance == null)
                        {
                            Debug.LogWarning("Email_And_Announcement_Manager instance is null.");
                        }
                        else
                        {
                            var ids = ParseIdList(deletedMailIdsInput);
                            if (ids.Count == 0)
                            {
                                Debug.LogWarning("No valid mail ids parsed for deletion.");
                            }
                            else
                            {
                                _instance.on_delete_mail(ids);
                                Debug.Log($"Triggered on_deleted_mail with ids: {string.Join(",", ids)}");
                            }
                        }
                    }

                    GUILayout.Space(8);

                    // Read mail input
                    EditorGUILayout.LabelField("Read Mail IDs (comma or space separated)", EditorStyles.miniLabel);
                    readMailIdsInput = EditorGUILayout.TextField(readMailIdsInput);
                    if (GUILayout.Button("Trigger on_read_mail"))
                    {
                        if (_instance == null)
                        {
                            Debug.LogWarning("Email_And_Announcement_Manager instance is null.");
                        }
                        else
                        {
                            var ids = ParseIdList(readMailIdsInput);
                            if (ids.Count == 0)
                            {
                                Debug.LogWarning("No valid mail ids parsed for read.");
                            }
                            else
                            {
                                _instance.on_read_mail(ids);
                                Debug.Log($"Triggered on_read_mail with ids: {string.Join(",", ids)}");
                            }
                        }
                    }

                    GUILayout.Space(6);

                    // Helper: populate input with selected mail ids from panel._mail_record
                    if (GUILayout.Button("Load All Mail IDs into Deleted Input"))
                    {
                        if (_instance != null && _instance._mail_record != null)
                        {
                            deletedMailIdsInput = string.Join(",", _instance._mail_record.Select(m => m.mail_id));
                            Debug.Log("Loaded all mail ids into deleted input.");
                        }
                        else Debug.LogWarning("No mail records available to load.");
                    }

                    if (GUILayout.Button("Load All Mail IDs into Read Input"))
                    {
                        if (_instance != null && _instance._mail_record != null)
                        {
                            readMailIdsInput = string.Join(",", _instance._mail_record.Select(m => m.mail_id));
                            Debug.Log("Loaded all mail ids into read input.");
                        }
                        else Debug.LogWarning("No mail records available to load.");
                    }


                    GUILayout.Space(8);

                    // Get mail reward input
                    EditorGUILayout.LabelField("Get Mail Reward IDs (comma or space separated)", EditorStyles.miniLabel);
                    getRewardMailIdsInput = EditorGUILayout.TextField(getRewardMailIdsInput);
                    if (GUILayout.Button("Trigger on_get_mail_reward"))
                    {
                        if (_instance == null)
                        {
                            Debug.LogWarning("Email_And_Announcement_Manager instance is null.");
                        }
                        else
                        {
                            var ids = ParseIdList(getRewardMailIdsInput);
                            if (ids.Count == 0)
                            {
                                Debug.LogWarning("No valid mail ids parsed for get reward.");
                            }
                            else
                            {
                                _instance.on_get_mail_reward(ids);
                                Debug.Log($"Triggered on_get_mail_reward with ids: {string.Join(",", ids)}");
                            }
                        }
                    }
                }



                // Parse comma/space separated integers
                private List<int> ParseIdList(string input)
                {
                    var result = new List<int>();
                    if (string.IsNullOrWhiteSpace(input)) return result;

                    var parts = input.Split(new char[] { ',', ' ', ';' }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in parts)
                    {
                        if (int.TryParse(p.Trim(), out int v))
                        {
                            result.Add(v);
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to parse id '{p}' as int.");
                        }
                    }
                    return result;
                }
            }
 
#endif

    }
    }
}


 