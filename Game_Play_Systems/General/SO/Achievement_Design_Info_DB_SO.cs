using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using CLIP.Project_Mouse.Kernel.Achievement;

namespace CLIP.Project_Mouse.Game_Play_System
{
    [CreateAssetMenu(fileName = "Achievement_Design_Info_DB_SO", menuName = "Project_Mouse/Achievement/Achievement_Design_Info_DB_SO")]
    public class Achievement_Design_Info_DB_SO : ScriptableObject
    {
        [Header("Achievement DB")]
        public List<achievement_design_info> achievement_db = new List<achievement_design_info>();
        [Header("Source JSON (TextAsset)")]
        public TextAsset achievement_json_file;



        /// <summary>
        /// Load achievement DB from assigned TextAsset (JSON array).
        /// </summary>
        public void LoadFromJson()
        {
            if (achievement_json_file == null)
            {
                Debug.LogWarning("Achievement_Design_Info_DB_SO.LoadFromJson: achievement_json_file is not assigned.");
                return;
            }

            try
            {
                var data = JsonConvert.DeserializeObject<List<achievement_design_info>>(achievement_json_file.text);
                if (data != null)
                {
                    achievement_db = data;
                    Debug.Log($"Achievement_Design_Info_DB_SO: Loaded {achievement_db.Count} achievement entries.");
                }
                else
                {
                    Debug.LogWarning("Achievement_Design_Info_DB_SO.LoadFromJson: Deserialized data is null.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Achievement_Design_Info_DB_SO.LoadFromJson: Failed to parse JSON. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Optional helper to serialize current DB to JSON string.
        /// </summary>
        public string SerializeToJson(bool formatted = true)
        {
            return JsonConvert.SerializeObject(achievement_db, formatted ? Formatting.Indented : Formatting.None);
        }
    }
}