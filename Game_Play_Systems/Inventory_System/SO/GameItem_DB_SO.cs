using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// 物品配置表 ScriptableObject（仅用于编辑器展示）。
    /// </summary>
    [CreateAssetMenu(fileName = "GameItem_DB_SO", menuName = "Project_Mouse/GameItem_DB_SO")]
    public class GameItem_DB_SO : SerializedScriptableObject 
    {
        [Title("物品配置表", Bold = true)]
        [GUIColor(0.2f, 1f, 0.8f)]
        [Header("GameItem_DB数据Json文件")]
        public TextAsset _game_item_json_file;

        [Title("物品列表（仅作展示，数据由 JsonDataManager 加载）", Bold = true)]
        [ListDrawerSettings(
            ShowFoldout = true,
            ListElementLabelName = "name",
            DraggableItems = true
        )]
        [SerializeField]
        public List<Game_Item_Info> _gameItem_db = new List<Game_Item_Info>();

        [Button("从 JSON 刷新预览", ButtonSizes.Large), GUIColor(0.2f, 0.8f, 1f)]
        [ContextMenu("从 JSON 刷新预览")]
        private void RefreshPreview()
        {
            if (_game_item_json_file == null)
            {
                Debug.LogWarning("[GameItem_DB_SO] 请先指定 JSON 文件！");
                return;
            }

            var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Game_Item_Info>>(_game_item_json_file.text);
            if (list != null)
            {
                _gameItem_db = list;
                Debug.Log($"[GameItem_DB_SO] 预览刷新完毕，共 {list.Count} 条。");
            }
        }
    }
}
