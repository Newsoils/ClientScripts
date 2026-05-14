using System;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CLIP.Project_Mouse.Game_Play_System
{

    public class LevelUpInfo
    {
        public int level;
        public int nextLevelExp;
        public int thresholdNum;
        public List<ItemList> rewardItem;
    }
    public class ItemList
    {
        public List<(string, int)> items;
    }
    public class ExpManager : SingletonMono<ExpManager> 
    {
        public Dictionary<int, LevelUpInfo> levels;
        public Action onLevelUp;

        public LevelUpInfo curLevelInfo => levels[curLevel];
        public int curExp;
        public int curLevel;
        public int curThreshold;

        protected override void Awake()
        {
            base.Awake();
        }


        private void Start()
        {
            string text = JsonData_Manager.Load_Single_JsonData("project_mouse_tb_friendship_info");
            levels = JsonConvert.DeserializeObject<List<LevelUpInfo>>(text).ToDictionary(property => property.level, property => property);
            curExp = 0;
            curLevel = 1;
            curThreshold = 0;
            LoadFromServer();
        }

        public void AddExp(int exp)
        {
            curExp += exp;
            CheckThreshold();
            TryLevelUp();
            EvtDsp.TriggerEvt(EvtNames.On_Get_Exp);
            SaveToServer();
        }
        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.f1Key.wasPressedThisFrame)
            {
                curExp += levels[curLevel].nextLevelExp;
                TryLevelUp();
            }
            //if (keyboard.digit2Key.wasPressedThisFrame || keyboard.digit5Key.wasPressedThisFrame)
            //{
            //    curLevel = 40;
            //    SaveToServer();
            //}
            //if (keyboard.digit3Key.wasPressedThisFrame || keyboard.digit6Key.wasPressedThisFrame)
            //{
            //    curLevel = 50;
            //    SaveToServer();
            //}
        }
        private void TryLevelUp()
        {
            //List<(string, int)> rewardList = new List<(string, int)> ();
            while (curExp > curLevelInfo.nextLevelExp)
            {
                if (curLevelInfo.nextLevelExp < 0) break;
                CheckThreshold();
                curExp -= levels[curLevel].nextLevelExp;
                GetReward(curLevelInfo.rewardItem.Count - 1);
                curLevel += 1;
                curThreshold = 0;
                Log.Info("升级了，当前等级：" + curLevel);
                onLevelUp?.Invoke();
                EvtDsp.TriggerEvt(EvtNames.Task_Unlocked);
            }
        }
        private void CheckThreshold()
        {
            if (curLevelInfo.nextLevelExp <= 0)
            {
                return;
            }
            for (int i = 0; i < levels[curLevel].thresholdNum; i++)
            {
                if (i >= curThreshold)
                {
                    if (curExp > curLevelInfo.nextLevelExp / curLevelInfo.thresholdNum * curThreshold)
                    {
                        GetReward(curThreshold);
                        curThreshold += 1;
                    }
                }
            }
        }
        private void GetReward(int index)
        {
            if (index < 0 || index >= curLevelInfo.rewardItem.Count)
            {
                Debug.LogWarning("升级奖励物品数据错误");
                return;
            }
            Global_Inventory_Manager.Change_Items_Count(curLevelInfo.rewardItem[index].items, "升级");
        }
        private void SaveToServer()
        {
            string data = JsonConvert.SerializeObject(new List<int> { curLevel, curExp });
            EvtDsp.TriggerEvt<string, string>(EvtNames.Save_Data_To_Server, "Save_Player_Level", data);
        }
        private void LoadFromServer()
        {
            EvtDsp.TriggerEvt<string, string, Action<string>>(EvtNames.Get_Data_From_Server, "Load_Player_Level", "", OnGetData);
        }
        private void OnGetData(string data)
        {
            if (data == null)
            {
                Debug.Log("连接失败");
                curLevel = 1;
                EvtDsp.TriggerEvt(EvtNames.PlayerLevelDataLoaded);
                return;
            }
            if (data == "NoData")
            {
                Debug.Log("没有等级数据,已经将数据重置为默认值");
                curLevel = 1;
                curExp = 0;
                SaveToServer();
                EvtDsp.TriggerEvt(EvtNames.PlayerLevelDataLoaded);
                return;
            }
            Log.Info("获取等级数据成功");
            Log.Info(data);
            List<int> res = JsonConvert.DeserializeObject<List<int>>(data);
            curLevel = res[0];
            curExp = res[1];
            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
            EvtDsp.TriggerEvt(EvtNames.PlayerLevelDataLoaded);
        }

        /// <summary>
        /// [临时] 商店购买或杂志抽卡获得「房间放置物」时，按件数每件 +5 亲密度经验。
        /// </summary>
        public static void TempAddExpForRoomPlacementGains(IEnumerable<(string itemName, int delta)> changes)
        {
            if (changes == null || Instance == null)
                return;
            int total = 0;
            foreach (var (itemName, delta) in changes)
            {
                if (delta <= 0)
                    continue;
                var info = Global_Inventory_Manager.GetItemInfo(itemName);
                if (info != null && info.type == Item_Type.Room_Placement)
                    total += 5 * delta;
            }
            if (total > 0)
                Instance.AddExp(total);
        }

#if UNITY_EDITOR
        [ContextMenu("调试/立刻获得 100 经验")]
        public void Editor_DebugAddExp100()
        {
            AddExp(100);
        }

        [ContextMenu("调试/清空等级与经验")]
        public void Editor_DebugResetLevelAndExp()
        {
            curLevel = 1;
            curExp = 0;
            curThreshold = 0;
            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
            SaveToServer();
        }
#endif
    }

}

