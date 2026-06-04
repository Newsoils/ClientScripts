using System;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Kernel;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class NPCManager : SingletonMono<NPCManager>
    {
        //NPC数据
        [SerializeField]
        public Dictionary<int, NPC_Info> NPC_Info_Dic = new Dictionary<int, NPC_Info>();
        public Dictionary<int, NPC_Base> NPC_Base_Dic = new Dictionary<int, NPC_Base>();

        /// <summary>
        /// 好感度升级所需经验（NPC通用），
        /// Key = 好感度等级，Value = 升级所需经验值
        /// </summary>
        public Dictionary<int, int> NPCFavor_LevelUp_neededExp = new Dictionary<int, int>();

        /// <summary>
        /// 请求NPC相关数据，接给TS层调用
        /// </summary>
        public UnityEvent<string> ask_Single_NPC_Data;

        public UnityEvent<string> receive_NPC_Gift_TO_Player;

        void Start()
        {
            DontDestroyOnLoad(this.gameObject);

            JsonDataManager.LoadNPCData(out NPC_Info_Dic, out NPC_Base_Dic, out NPCFavor_LevelUp_neededExp);
            NPCChatManager.Instance.Initialize();

            EvtDsp.AddEvt<int, long>(EvtNames.Give_Gift_TO_NPC, Give_Gift_To_NPC);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EvtDsp.RemoveEvt<int, long>(EvtNames.Give_Gift_TO_NPC, Give_Gift_To_NPC);
        }

        void Update()
        {
            //测试代码

            //var keyboard = Keyboard.current;
            //if (keyboard == null) return;

            //if (keyboard.gKey.wasPressedThisFrame)
            //{
            //    var item = Global_Inventory_Manager.Items.FirstOrDefault();
            //    Give_Gift_To_NPC(1, item.item_id);
            //}
            //if(keyboard.f10Key.wasPressedThisFrame)
            //{

            //}
        }

        public bool TryGetNpcInfo(int npcId,out NPC_Info info)
        {
            return NPC_Info_Dic.TryGetValue(npcId, out  info);
        }


        public void ReceiveNpcDataChange(Cmd.NpcChangeS2C npcChangeS2C)
        {
            // 新增 NPC（初始化对话历史）
            if (npcChangeS2C.NpcAdd != null)
            {
                foreach (var p in npcChangeS2C.NpcAdd)
                {
                    var runtimeData = BuildRuntimeData(p);
                    UpdateNpcRuntimeData(p.NpcId, runtimeData);

                    if (p.NPCDialogueHistoryInfo != null)
                        NPCChatManager.Instance.OnReceiveNPCDataUpdate(p.NPCDialogueHistoryInfo);

                    Debug.Log($"[NPCManager] NpcAdd：NPC {p.NpcId} 好感度 {p.FavorLevel}，已同步对话历史。");
                }
            }

            // 更新现有 NPC（好感度变化或对话进度变化）
            if (npcChangeS2C.NpcUpd != null)
            {
                foreach (var p in npcChangeS2C.NpcUpd)
                {
                    var runtimeData = BuildRuntimeData(p);
                    UpdateNpcRuntimeData(p.NpcId, runtimeData);

                    if (p.NPCDialogueHistoryInfo != null)
                        NPCChatManager.Instance.OnReceiveNPCDataUpdate(p.NPCDialogueHistoryInfo);

                    Debug.Log($"[NPCManager] NpcUpd：NPC {p.NpcId} 好感度 {p.FavorLevel}，已同步对话历史。");
                }
            }

            if(npcChangeS2C.NpcDel != null)
            {
                foreach (var data in npcChangeS2C.NpcDel)
                {
                    var npcId = data.NpcId;

                    if (NPC_Info_Dic.ContainsKey(npcId))
                    {
                        NPC_Info_Dic.Remove(npcId);
                        Debug.Log($"[NPCManager] NpcDel：NPC {npcId} 已从 NPC_Info_Dic 中移除。");
                    }
                    else
                    {
                        Debug.LogWarning($"[NPCManager] NpcDel：尝试删除 NPC {npcId}，但它不在 NPC_Info_Dic 中。");
                    }
                }
            }
        }

        /// <summary>
        /// 从 NpcFavovInfo 构建 NPC_RuntimeData。
        /// </summary>
        private NPC_RuntimeData BuildRuntimeData(Cmd.NpcFavovInfo p)
        {
            return new NPC_RuntimeData
            {
                npc_id = p.NpcId,
                favor_level = p.FavorLevel,
                favor_Value = p.FavorValue,
                encounter_count = p.EncounterCount,
                is_met = p.IsMet == 1,
                is_acquainted = p.IsAcquainted == 1,
                last_interaction_time = DateTimeOffset.FromUnixTimeSeconds(p.LastInteractionTime).DateTime
            };
        }

        /// <summary>
        /// 将 runtimeData 写入 NPC_Info_Dic，并触发单个 NPC 好感度变更事件。
        /// </summary>
        private void UpdateNpcRuntimeData(int npcId, NPC_RuntimeData runtimeData)
        {
            if (!NPC_Info_Dic.TryGetValue(npcId, out var npcInfo))
            {
                Debug.LogWarning($"[NPCManager] UpdateNpcRuntimeData：NPC {npcId} 不存在于 NPC_Info_Dic 中。");
                return;
            }

            npcInfo._npc_RuntimeData = runtimeData;
            npcInfo.npc_Next_Favor_Level = Next_Level_Favor_Exp_Needed(runtimeData.favor_level);

            EvtDsp.TriggerEvt(EvtNames.On_NPC_Data_Update);
        }

        public void Receive_All_NPC_Data(Cmd.GetAllNpcInfoRes res)
        {
            if (res == null)
            {
                Debug.LogWarning("[NPCManager] GetAllNpcInfoRes 为 null，跳过 NPC 数据应用。");
                return;
            }

            var list = new List<NPC_RuntimeData>();
            if (res.NpcFavovInfos != null)
            {
                foreach (var p in res.NpcFavovInfos)
                {
                    var data = new NPC_RuntimeData();
                    data.npc_id = p.NpcId;
                    data.favor_level = p.FavorLevel;
                    data.favor_Value = p.FavorValue;
                    data.encounter_count = p.EncounterCount;
                    data.is_met = p.IsMet == 1;
                    data.is_acquainted = p.IsAcquainted == 1;
                    data.last_interaction_time = DateTimeOffset.FromUnixTimeSeconds(p.LastInteractionTime).DateTime;
                    list.Add(data);
                }
            }

            foreach (var p in list)
            {
                if (!NPC_Info_Dic.TryGetValue(p.npc_id, out NPC_Info npc_Info) || npc_Info == null)
                    continue;

                if (npc_Info._npc_RuntimeData == null)
                {
                    NPC_RuntimeData data = new NPC_RuntimeData();
                    npc_Info._npc_RuntimeData = p;
                }
                else
                {
                    npc_Info._npc_RuntimeData = p;
                }

                npc_Info.npc_Next_Favor_Level = Next_Level_Favor_Exp_Needed(npc_Info._npc_RuntimeData.favor_level);
            }

            Log.Info("收到所有NPC数据，数量：" + list.Count);
            NPCChatManager.Instance?.InitAllChat(res);

            EvtDsp.TriggerEvt(EvtNames.On_All_NPC_Data_Received, list);
            EvtDsp.TriggerEvt(EvtNames.On_NPC_Data_Update);
            //foreach (var newData in datas)
            //{
            //    Debug.Log("收到NPC数据：" + newData.npc_id + "\t" + NPC_Base_Dic[newData.npc_id].npc_name
            //        + "\t" + "好感度等级：" + newData.favor_level + "\t" + "好感度：" + newData.favor_Value);
            //}
        }


        /// <summary>
        /// 计算到下级好感度需要的经验值
        /// </summary>
        /// <param name="current_Level"></param>
        /// <returns></returns>
        public int Next_Level_Favor_Exp_Needed(int current_Level)
        {
            if (NPCFavor_LevelUp_neededExp.TryGetValue(current_Level, out int neededExp))
            {
                return neededExp;
            }
            else
            {
                Debug.LogWarning("没有找到好感度等级 " + (current_Level) + " 所需经验数据，可能已达最高等级");
                return -1;
            }
        }

        /// <summary>
        /// 给 NPC 送礼请求，序列化后发往 TS 层。
        /// 服务器回包不需要单独处理（好感度变化由 NpcChangeS2C 推送）。
        /// </summary>
        public void Give_Gift_To_NPC(int npc_id, long itemUId)
        {
            var item = Global_Inventory_Manager.GetItem(itemUId);
            if (item == null)
            {
                Debug.LogWarning($"[NPCManager] Give_Gift_To_NPC: 背包中未找到 uid={itemUId}");
                return;
            }

            if (!NPC_Base_Dic.TryGetValue(npc_id, out var npcBase))
            {
                Debug.LogWarning($"[NPCManager] Give_Gift_To_NPC: 未知 NPC id={npc_id}");
                return;
            }

            Debug.Log($"给NPC送礼物请求：{npc_id}\t{npcBase.npc_name}\t礼物：{item}");

            var req = new Cmd.GiveGiftToNpcReq
            {
                NpcId = npc_id,
                ItemID = item.item_id
            };
            EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, req);
        }

        /// <summary>
        /// 收到 NPC 送给玩家的礼物推送，触发事件。
        /// </summary>
        /// <param name="s2c">服务器推送的 NpcGiftToPlayerS2C</param>
        public void Receive_NPC_Gift_TO_Player(Cmd.NpcGiftToPlayerS2C s2c)
        {
            if (s2c == null) return;

            Debug.Log("收到NPC送礼推送：NPC=" + s2c.NpcId + ", giftId=" + s2c.GiftId);

            var info = new NPC_Gift_To_PLayer_Info
            {
                NPC_ID = s2c.NpcId,
                gift_ID = (int)s2c.GiftId
            };
            EvtDsp.TriggerEvt< NPC_Gift_To_PLayer_Info>(EvtNames.Receive_NPC_Gift_TO_Player, info);
        }

    }
}
