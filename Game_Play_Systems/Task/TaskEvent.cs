using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// 任务触发入口。按 missionId 触发，或按 Item_Type 自动触发所有相关任务。
    /// 用法：TaskEvent.Trigger(2);  或  TaskEvent.TriggerByCategory(Item_Type.Food);
    /// </summary>
    public static class TaskEvent
    {
        public const string Task_Progress = "Task_Progress";

        // ===== missionId 常量（便于代码阅读）=====
        // 名称规则：Task_[类别]_[目标数量]
        // 其中 "首次" 固定对应 targetCount=1

        // 通用
        public const int Task_PlaceFurniture = 1;     // 首次摆放1个家具
        public const int Task_XiaoTaiReturnHome = 3; // 小苔第1次回家
        public const int Task_OpenPhotoAlbum = 4;     // 第1次打开相册

        // Food（食物）
        public const int Task_Buy_Food_1 = 2;     // 首次购买1个食物
        public const int Task_Buy_Food_2 = 9;     // 累计购买2个食物
        public const int Task_Buy_Food_3 = 16;    // 累计购买3个食物
        public const int Task_Buy_Food_4 = 22;    // 累计购买4个食物
        public const int Task_Buy_Food_5 = 28;    // 累计购买5个食物
        public const int Task_Buy_Food_10 = 34;   // 累计购买10个食物
        public const int Task_Buy_Food_15 = 40;   // 累计购买15个食物
        public const int Task_Buy_Food_20 = 52;   // 累计购买20个食物
        public const int Task_Buy_Food_25 = 60;   // 累计购买25个食物
        public const int Task_Buy_Food_30 = 65;   // 累计购买30个食物
        public const int Task_Buy_Food_35 = 71;   // 累计购买35个食物
        public const int Task_Buy_Food_50 = 82;   // 累计购买50个食物
        public const int Task_Buy_Food_60 = 88;   // 累计购买60个食物

        // Snack（幸运小物）
        public const int Task_Buy_Snack_1 = 5;     // 首次购买1个幸运小物
        public const int Task_Buy_Snack_2 = 10;    // 累计购买2个幸运小物
        public const int Task_Buy_Snack_3 = 17;    // 累计购买2个幸运小物 (同一 targetCount)
        public const int Task_Buy_Snack_4 = 23;    // 累计购买3个幸运小物
        public const int Task_Buy_Snack_5 = 29;    // 累计购买4个幸运小物
        public const int Task_Buy_Snack_10 = 35;   // 累计购买5个幸运小物
        public const int Task_Buy_Snack_15 = 41;   // 累计购买10个幸运小物
        public const int Task_Buy_Snack_20 = 53;   // 累计购买20个幸运小物
        public const int Task_Buy_Snack_30 = 66;   // 累计购买30个幸运小物
        public const int Task_Buy_Snack_35 = 78;   // 累计购买35个幸运小物
        public const int Task_Buy_Snack_50 = 84;   // 累计购买50个幸运小物
        public const int Task_Buy_Snack_60 = 90;   // 累计购买60个幸运小物

        // Tape（唱片）
        public const int Task_Buy_Tape_1 = 6;      // 首次购买1个唱片
        public const int Task_Buy_Tape_3 = 61;      // 累计购买3个唱片

        // Seed（种子）
        public const int Task_Buy_Seed_2 = 7;       // 累计购买2个种子
        public const int Task_Buy_Seed_3 = 14;     // 累计购买3个种子
        public const int Task_Buy_Seed_4 = 20;     // 累计购买4个种子
        public const int Task_Buy_Seed_5 = 26;     // 累计购买5个种子
        public const int Task_Buy_Seed_10 = 32;    // 累计购买10个种子
        public const int Task_Buy_Seed_15 = 38;    // 累计购买15个种子
        public const int Task_Buy_Seed_20 = 50;    // 累计购买20个种子
        public const int Task_Buy_Seed_25 = 58;    // 累计购买25个种子
        public const int Task_Buy_Seed_30 = 63;    // 累计购买30个种子
        public const int Task_Buy_Seed_35 = 69;    // 累计购买35个种子
        public const int Task_Buy_Seed_45 = 77;    // 累计购买45个种子
        public const int Task_Buy_Seed_70 = 83;    // 累计购买70个种子
        public const int Task_Buy_Seed_90 = 89;    // 累计购买90个种子

        // Pot（花盆）
        public const int Task_Buy_Pot_2 = 8;       // 累计购买2个花盆
        public const int Task_Buy_Pot_3 = 15;      // 累计购买3个花盆
        public const int Task_Buy_Pot_4 = 21;      // 累计购买4个花盆
        public const int Task_Buy_Pot_5 = 27;      // 累计购买5个花盆
        public const int Task_Buy_Pot_10 = 33;    // 累计购买10个花盆
        public const int Task_Buy_Pot_15 = 39;    // 累计购买15个花盆
        public const int Task_Buy_Pot_20 = 51;    // 累计购买20个花盆
        public const int Task_Buy_Pot_25 = 59;    // 累计购买25个花盆

        // Cloth（服装）
        public const int Task_Buy_Cloth_1 = 13;     // 累计购买1件服装
        public const int Task_Buy_Cloth_2 = 25;     // 累计购买2件服装
        public const int Task_Buy_Cloth_3 = 37;     // 累计购买3件服装
        public const int Task_Buy_Cloth_4 = 49;     // 累计购买4件服装
        public const int Task_Buy_Cloth_5 = 62;     // 累计购买5件服装
        public const int Task_Buy_Cloth_10 = 68;   // 累计购买10件服装
        public const int Task_Buy_Cloth_15 = 76;   // 累计购买15件服装

        // Furniture（家具购买，非摆放）
        public const int Task_Buy_Furniture_1 = 12;  // 累计购买1个家具
        public const int Task_Buy_Furniture_2 = 19;  // 累计购买2个家具
        public const int Task_Buy_Furniture_3 = 31;  // 累计购买3个家具
        public const int Task_Buy_Furniture_4 = 43;  // 累计购买4个家具
        public const int Task_Buy_Furniture_5 = 57;  // 累计购买5个家具
        public const int Task_Buy_Furniture_10_1 = 64; // 累计购买10个家具
        public const int Task_Buy_Furniture_10_2 = 70; // 累计购买10个家具
        public const int Task_Buy_Furniture_20 = 73;  // 累计购买20个家具
        public const int Task_Buy_Furniture_40 = 79;  // 累计购买40个家具
        public const int Task_Buy_Furniture_60 = 85;  // 累计购买60个家具

        // 家具摆放（独立于购买）
        public const int Task_PlaceFurniture_10 = 72; // 累计摆放10个新家具
        public const int Task_PlaceFurniture_20 = 75; // 累计摆放20个新家具
        public const int Task_PlaceFurniture_40 = 81; // 累计摆放40个新家具
        public const int Task_PlaceFurniture_50 = 87; // 累计摆放50个新家具

        // ===== 内部映射 =====

        private static readonly int[] _foodIds = {
            Task_Buy_Food_1, Task_Buy_Food_2, Task_Buy_Food_3, Task_Buy_Food_4,
            Task_Buy_Food_5, Task_Buy_Food_10, Task_Buy_Food_15, Task_Buy_Food_20,
            Task_Buy_Food_25, Task_Buy_Food_30, Task_Buy_Food_35, Task_Buy_Food_50, Task_Buy_Food_60
        };

        private static readonly int[] _snackIds = {
            Task_Buy_Snack_1, Task_Buy_Snack_2, Task_Buy_Snack_3, Task_Buy_Snack_4,
            Task_Buy_Snack_5, Task_Buy_Snack_10, Task_Buy_Snack_15, Task_Buy_Snack_20,
            Task_Buy_Snack_30, Task_Buy_Snack_35, Task_Buy_Snack_50, Task_Buy_Snack_60
        };

        private static readonly int[] _tapeIds = { Task_Buy_Tape_1, Task_Buy_Tape_3 };

        private static readonly int[] _seedIds = {
            Task_Buy_Seed_2, Task_Buy_Seed_3, Task_Buy_Seed_4, Task_Buy_Seed_5,
            Task_Buy_Seed_10, Task_Buy_Seed_15, Task_Buy_Seed_20, Task_Buy_Seed_25,
            Task_Buy_Seed_30, Task_Buy_Seed_35, Task_Buy_Seed_45, Task_Buy_Seed_70, Task_Buy_Seed_90
        };

        private static readonly int[] _potIds = {
            Task_Buy_Pot_2, Task_Buy_Pot_3, Task_Buy_Pot_4, Task_Buy_Pot_5,
            Task_Buy_Pot_10, Task_Buy_Pot_15, Task_Buy_Pot_20, Task_Buy_Pot_25
        };

        private static readonly int[] _clothIds = {
            Task_Buy_Cloth_1, Task_Buy_Cloth_2, Task_Buy_Cloth_3, Task_Buy_Cloth_4,
            Task_Buy_Cloth_5, Task_Buy_Cloth_10, Task_Buy_Cloth_15
        };

        private static readonly int[] _furnitureIds = {
            Task_Buy_Furniture_1, Task_Buy_Furniture_2, Task_Buy_Furniture_3, Task_Buy_Furniture_4,
            Task_Buy_Furniture_5, Task_Buy_Furniture_10_1, Task_Buy_Furniture_10_2,
            Task_Buy_Furniture_20, Task_Buy_Furniture_40, Task_Buy_Furniture_60
        };

        private static readonly int[] _placeFurnitureIds = {
            Task_PlaceFurniture_10, Task_PlaceFurniture_20, Task_PlaceFurniture_40, Task_PlaceFurniture_50
        };

        private static readonly int[] _xiaoTaiReturnHomeIds = { Task_XiaoTaiReturnHome, 55 }; // 累计回家

        // ===== 公开 API =====

        /// <summary>直接按 missionId 触发单个任务。</summary>
        public static void Trigger(int taskId, int triggerTimes = 1)
        {
            EvtDsp.TriggerEvt(Task_Progress, taskId, triggerTimes);
        }

        /// <summary>
        /// 根据物品类型触发所有相关的"累计购买"任务。
        /// 传入 Game_Item_Info 即可自动判断类型。
        /// </summary>
        public static void TriggerByCategory(Item_Type type, int triggerTimes = 1)
        {
            int[] ids = type switch
            {
                Item_Type.Food => _foodIds,
                Item_Type.Snack => _snackIds,
                Item_Type.Tape => _tapeIds,
                Item_Type.Seed => _seedIds,
                Item_Type.Pot => _potIds,
                Item_Type.Cloth => _clothIds,
                Item_Type.Room_Placement => _furnitureIds,
                _ => null
            };

            if (ids == null) return;
            foreach (var id in ids)
                EvtDsp.TriggerEvt(Task_Progress, id, triggerTimes);
        }

        /// <summary>触发家具摆放类任务。</summary>
        public static void TriggerPlaceFurniture(int triggerTimes = 1)
        {
            foreach (var id in _placeFurnitureIds)
                EvtDsp.TriggerEvt(Task_Progress, id, triggerTimes);
        }

        /// <summary>触发小苔回家类任务。</summary>
        public static void TriggerXiaoTaiReturnHome(int triggerTimes = 1)
        {
            foreach (var id in _xiaoTaiReturnHomeIds)
                EvtDsp.TriggerEvt(Task_Progress, id, triggerTimes);
        }
    }
}
