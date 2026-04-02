using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Kernel;
using DataStructures.RandomSelector;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class GachaManager : SingletonMono<GachaManager>
    {
        private const int RARITY_4_PITY_THRESHOLD = 10;
        public int count;
        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if(keyboard.gKey.wasPressedThisFrame)
                {
                    Gacha_Multi_Pull(0, 5);
                }
            }
        }
        public List<int> Gacha_Multi_Pull(int poolID, int pullCount)
        {
            string json = JsonData_Manager.Load_Single_JsonData("project_mouse_tb_gacha_pool");
            List<GachaPool> gacha_Pools = JsonConvert.DeserializeObject<List<GachaPool>>(json) ?? new List<GachaPool>();
            var pool = gacha_Pools[poolID];
            if (pool == null) return new List<int>();

            var allPoolItems = Global_Inventory_Manager.GetItemInfos(pool.items);
            var probDic = pool.probs.ToDictionary(p => p.Item1, p => p.Item2);

            List<int> resultItemIds = new List<int>();
            // 正常随机
            DynamicRandomSelector<Enum_RarityType> selector = new DynamicRandomSelector<Enum_RarityType>();
            foreach (var item in probDic)
                selector.Add((Enum_RarityType)item.Key, item.Value);
            selector.Build();
            // --- 内存循环开始 ---
            for (int i = 0; i < pullCount; i++)
            {
                Game_Item_Info selected;
                Enum_RarityType rarity = selector.SelectRandomItem();
                // 判定保底 (第11抽逻辑)
                if (count >= RARITY_4_PITY_THRESHOLD)
                {
                    var four_star_items = allPoolItems.Where(x => x.rarity == Enum_RarityType.Elegant).ToList();
                    selected = four_star_items[Random.Range(0, four_star_items.Count)];
                }
                else
                {
                    var items = allPoolItems.Where(x => x.rarity == rarity).ToList();
                    selected = items[Random.Range(0, items.Count)];
                }

                // 更新当前循环内的临时水位
                resultItemIds.Add(selected.item_id);

                if (selected.rarity == Enum_RarityType.Elegant)
                    count = 0; // 中了4星，立刻重置计数
                else
                    count++;
            }
            // --- 内存循环结束 ---

            // 一次性保存到数据库
            Debug.Log(JsonConvert.SerializeObject( resultItemIds));
            return resultItemIds;
        }
    }
    public class GachaPool
    {
        /// <summary>
        /// 奖池序号
        /// </summary>
        public int id;
        /// <summary>
        /// 需要加入奖池的物品id(对应gameItem）
        /// </summary>
        public List<int> items;
        /// <summary>
        /// 每种稀有度对应的概率
        /// </summary>
        public List<(int, float)> probs;
    }
}

