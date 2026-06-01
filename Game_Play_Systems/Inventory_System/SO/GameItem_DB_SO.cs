using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CLIP.Project_Mouse.Game_Play_System
{
    [CreateAssetMenu(fileName = "GameItem_DB_SO", menuName = "Project_Mouse/GameItem_DB_SO")]
    public class GameItem_DB_SO : SerializedScriptableObject 
    {
        [Title("物品配置表", Bold = true)]
        [GUIColor(0.2f, 1f, 0.8f)]
        [Header("GameItem_DB数据Json文件")]
        public TextAsset _game_item_json_file;

        [Title("物品列表", Bold = true)]
        [ListDrawerSettings(
            ShowFoldout = true,          // 每个物品可折叠
            ListElementLabelName = "name", // 折叠栏显示物品名
            DraggableItems = true        // 可拖拽排序
        )]
        [SerializeField]
        public List<Game_Item_Info> _gameItem_db = new List<Game_Item_Info>();

        [ReadOnly] // 字典只显示，不能编辑
        [ShowInInspector] // Odin 显示字典
        [Title("字典缓存（自动生成）", Bold = true)]
        public readonly Dictionary<int, Game_Item_Info> _idDic = new Dictionary<int, Game_Item_Info>();

        [ReadOnly]
        [ShowInInspector]
        public readonly Dictionary<string, Game_Item_Info> _nameDic = new Dictionary<string, Game_Item_Info>();

        // —— Odin 按钮，替代你原来的原生 Editor ——
        [Button("从 JSON 加载数据", ButtonSizes.Large), GUIColor(0.2f, 0.8f, 1f)]
        [ContextMenu("加载数据")]
        public void RefreshData()
        {
            if (_game_item_json_file == null)
            {
                Debug.LogError("请先指定 JSON 文件！");
                return;
            }

            _gameItem_db = JsonConvert.DeserializeObject<List<Game_Item_Info>>(_game_item_json_file.text);
            _idDic.Clear();
            _nameDic.Clear();

            foreach (var item_info in _gameItem_db)
            {
                if (!_idDic.ContainsKey(item_info.item_id))
                    _idDic.Add(item_info.item_id, item_info);

                if (!_nameDic.ContainsKey(item_info.name))
                    _nameDic.Add(item_info.name, item_info);
            }
        }

        public Game_Item_Info GetItemInfo(string itemName)
        {
            _nameDic.TryGetValue(itemName, out var info);
            return info;
        }

        public Game_Item_Info GetItemInfo(int itemId)
        {
            _idDic.TryGetValue(itemId, out var info);
            return info;
        }

        public List<Game_Item_Info> GetItemInfos(IEnumerable<int> ids)
        {
            List<Game_Item_Info> result = new List<Game_Item_Info>();
            foreach (var id in ids)
            {
                _idDic.TryGetValue(id, out var info);
                if (info != null) result.Add(info);
            }
            return result;
        }

        public List<Game_Item_Info> GetItemInfos(IEnumerable<string> names)
        {
            List<Game_Item_Info> result = new List<Game_Item_Info>();
            foreach (var name in names)
            {
                _nameDic.TryGetValue(name, out var info);
                if (info != null) result.Add(info);
            }
            return result;
        }

        private void OnEnable()
        {
            RefreshData();
        }

        private void OnValidate()
        {
            RefreshData();
        }
    }
}