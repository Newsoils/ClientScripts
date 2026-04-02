using System.Collections.Generic;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    [CreateAssetMenu(fileName = "Placement_SO", menuName = "Project_Mouse/Placement_SO")]

    public class Placement_SO : ScriptableObject
    {
        public TextAsset _room_placement_info_json_file;

        public List<Room_Placement_Info> _placement_db = new List<Room_Placement_Info>();

        public readonly Dictionary<int, Room_Placement_Info> placementDic = new Dictionary<int, Room_Placement_Info>();
        public readonly Dictionary<string, Room_Placement_Info> placementNameDic = new Dictionary<string, Room_Placement_Info>();

        private void OnEnable()
        {
            RefreshData();
        }

        // 情况 B：在编辑器里手动修改了 TextAsset，或者 JSON 文件更新被 Unity 检测到时
        private void OnValidate()
        {
            RefreshData();
        }

        [ContextMenu("加载数据")] 
        public void RefreshData()
        {
            if (_room_placement_info_json_file != null && !string.IsNullOrEmpty(_room_placement_info_json_file.text))
            {
                try
                {
                    _placement_db = JsonConvert.DeserializeObject<List<Room_Placement_Info>>(_room_placement_info_json_file.text);
                    // Debug.Log($"[Placement_SO] {name} 自动同步成功，加载了 {_placement_db.Count} 条数据");
                    foreach (var placementInfo in _placement_db)
                    {
                        placementDic[placementInfo.room_placement_id] = placementInfo;
                        placementNameDic[placementInfo.room_placement_name] = placementInfo;
                    }
                }
                catch (System.Exception e)
                {
                    Log.Error($"[Placement_SO] 解析 JSON 失败: {e.Message}");
                }
            }
        }
    }
}