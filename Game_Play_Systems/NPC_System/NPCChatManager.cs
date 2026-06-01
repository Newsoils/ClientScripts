using System;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using Google.Protobuf;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// NPC 聊天系统数据总控。
    /// 职责：
    /// - 静态配置数据（对话/段落/好感度→段落映射）由 <see cref="JsonDataManager"/> 加载
    /// - 对话历史数据（段落进度/选项记录）由服务器管理
    /// - UI 逻辑全部由 <see cref="ChatViewController"/> 承担，通过事件总线与本类解耦
    /// </summary>
    public class NPCChatManager : SingletonMono<NPCChatManager>
    {
        /// <summary>对话字典（dialogueId → Model）。</summary>
        public Dictionary<int, DialogueModel> dialoguesDic = new Dictionary<int, DialogueModel>();

        /// <summary>段落字典（ParaId → Model）。</summary>
        public Dictionary<int, ParagraphModel> paragraphsDic = new Dictionary<int, ParagraphModel>();

        /// <summary>各 NPC 的对话进度与选项记录。</summary>
        public Dictionary<int, NPCDialogueHistory> npcDialogueHistory = new Dictionary<int, NPCDialogueHistory>();

        /// <summary>
        /// NPC 解锁段落所需的好感度等级映射（npcId → (favorLevel → paraId)）。
        /// </summary>
        private Dictionary<int, Dictionary<int, int>> npc_FavorLevel_ParaId_Dic = new Dictionary<int, Dictionary<int, int>>();

        #region 生命周期

        private void Start()
        {
            //Initialize();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion

        #region 初始化入口

        /// <summary>
        /// 幂等初始化。一次性加载全部静态数据。
        /// 对话历史由 <see cref="InitAllChat"/> 从服务器数据填充。
        /// </summary>
        public void Initialize()
        {
            LoadStaticData();
        }

        #endregion

        #region 数据加载

        /// <summary>
        /// 从 JsonDataManager 一次性加载全部对话系统静态数据。
        /// </summary>
        private void LoadStaticData()
        {
            JsonDataManager.LoadAllDialogueData(out dialoguesDic, out paragraphsDic, out npc_FavorLevel_ParaId_Dic);
        }

        /// <summary>
        /// 填充服务器返回的对话历史数据（由 <see cref="Global_Game_Data_Sync_Receiver"/> 调用）。
        /// 从 <c>GetAllNpcInfoRes</c> 中读取每个 <c>NpcFavovInfo.NPCDialogueHistoryInfo</c>。
        /// </summary>
        public void InitAllChat(Cmd.GetAllNpcInfoRes res)
        {
            npcDialogueHistory.Clear();

            var npcDic = NPCManager.Instance?.NPC_Info_Dic;
            if (npcDic == null)
                return;

            foreach (var (npcId, npcInfo) in npcDic)
            {
                var favInfo = res?.NpcFavovInfos?.FirstOrDefault(n => n.NpcId == npcId);
                var hisProto = favInfo?.NPCDialogueHistoryInfo;
                var favorLevel = npcInfo?._npc_RuntimeData?.favor_level ?? favInfo?.FavorLevel ?? 0;

                if (!TryBuildHistoryFromServerData(npcId, favorLevel, hisProto, out var history, out var shouldSync))
                    continue;

                npcDialogueHistory[npcId] = history;

                Debug.Log($"[NPCChatManager] NPC {npcId} lastDialogueId={history.lastDialogueId}");

                if (shouldSync)
                    SyncNpcHisToServer(npcId);
            }

            Debug.Log($"[NPCChatManager] 已用服务器数据更新对话历史，共 {npcDialogueHistory.Count} 个 NPC。");
        }

        private bool TryBuildHistoryFromServerData(
            int npcId,
            int favorLevel,
            Cmd.NPCDialogueHistoryInfo hisProto,
            out NPCDialogueHistory history,
            out bool shouldSync)
        {
            history = null;
            shouldSync = false;

            if (hisProto == null && !npc_FavorLevel_ParaId_Dic.ContainsKey(npcId))
                return false;

            var dialogueProgressList = new List<DialogueProgress>();
            if (hisProto != null)
            {
                foreach (var p in hisProto.DialogueProgressList)
                {
                    dialogueProgressList.Add(new DialogueProgress(p.ParagraphId, p.IsUnlocked, p.IsFinished));
                    Debug.Log($"[NPCChatManager] 服务器对话历史 - NPC {npcId} 段落 {p.ParagraphId}，" +
                              $"已解锁={p.IsUnlocked}，已完成={p.IsFinished}");
                }
            }

            ApplyFavorUnlockProgress(npcId, favorLevel, dialogueProgressList, ref shouldSync);

            // 如果服务器没有返回历史数据，且当前列表为空，则基于好感度映射补齐全部段落
            if ((hisProto == null || hisProto.DialogueProgressList.Count == 0)
                && dialogueProgressList.Count == 0)
            {
                ApplyFavorUnlockProgress(npcId, favorLevel, dialogueProgressList, ref shouldSync);
                Debug.Log($"[NPCChatManager] NPC {npcId} 无服务器历史，已按好感等级 {favorLevel} 补齐段落，共 {dialogueProgressList.Count} 个。");
            }

            var optionChoices = new List<OptionChoiceRecord>();
            if (hisProto != null)
            {
                foreach (var o in hisProto.OptionChoices)
                {
                    optionChoices.Add(new OptionChoiceRecord(o.DialogueId, o.ChoiceIndex));
                    Debug.Log($"[NPCChatManager] 服务器对话历史 - NPC {npcId} 选项对话 {o.DialogueId}，" +
                              $"玩家选择了选项 {o.ChoiceIndex}");
                }
            }

            history = new NPCDialogueHistory(npcId, dialogueProgressList, optionChoices, DateTime.Now)
            {
                lastDialogueId = hisProto?.LastDialogueId ?? -1
            };
            return true;
        }

        private void ApplyFavorUnlockProgress(
            int npcId,
            int favorLevel,
            List<DialogueProgress> dialogueProgressList,
            ref bool shouldSync)
        {
            if (!npc_FavorLevel_ParaId_Dic.TryGetValue(npcId, out var paraMap))
                return;

            foreach (var (unlockFavorLevel, paraId) in paraMap.OrderBy(pair => pair.Key))
            {
                var shouldUnlock = unlockFavorLevel <= favorLevel;
                var progress = dialogueProgressList.FirstOrDefault(p => p.paragraphId == paraId);

                if (progress == null)
                {
                    dialogueProgressList.Add(new DialogueProgress(paraId, shouldUnlock, isFinished: false));
                    shouldSync = true;
                    Debug.Log($"[NPCChatManager] 补齐缺失段落 {paraId}，要求好感等级={unlockFavorLevel}，" +
                              $"当前好感等级={favorLevel}，已解锁={shouldUnlock}");
                    continue;
                }

                var correctedUnlocked = progress.isUnlocked || progress.isFinished || shouldUnlock;
                if (progress.isUnlocked != correctedUnlocked)
                {
                    progress.isUnlocked = correctedUnlocked;
                    shouldSync = true;
                    Debug.Log($"[NPCChatManager] 按好感等级修正段落 {paraId} 解锁状态，要求好感等级={unlockFavorLevel}，" +
                              $"当前好感等级={favorLevel}，已解锁={correctedUnlocked}");
                }
            }
        }

        #endregion

        #region 对话查询

        /// <summary>
        /// 根据 npcId，获取下一个"已解锁但未完成"的段落 id。
        /// </summary>
        /// <returns>是否存在待触发的段落。</returns>
        public bool TryGetNextPendingParagraphId(int npcId, out int paraId)
        {
            paraId = -1;

            if (!npcDialogueHistory.TryGetValue(npcId, out var history))
                return false;

            foreach (var progress in history.dialogueProgressList)
            {
                if (progress.isUnlocked && !progress.isFinished)
                {
                    paraId = progress.paragraphId;
                    Debug.Log($"[NPCChatManager] NPC {npcId} 检测到待触发段落 {paraId}");
                    return true;
                }
            }

            Debug.Log($"[NPCChatManager] NPC {npcId} 当前无待触发段落。");
            return false;
        }

        /// <summary>
        /// 获取指定 NPC 已完成的段落 id 列表（用于历史回放）。
        /// </summary>
        public List<int> GetFinishedParagraphs(int npcId)
        {
            var res = new List<int>();

            if (npcDialogueHistory.TryGetValue(npcId, out var history))
            {
                foreach (var progress in history.dialogueProgressList)
                {
                    if (progress.isFinished)
                        res.Add(progress.paragraphId);
                }
                Debug.Log($"[NPCChatManager] NPC {npcId} 历史上已完成 {res.Count} 个段落。");
            }
            return res;
        }

        /// <summary>
        /// 查询历史选项记录。
        /// </summary>
        public bool TryGetChoiceRecord(int npcId, int dialogueId, out int choice)
        {
            choice = 0;
            if (npcDialogueHistory.TryGetValue(npcId, out var history))
            {
                return history.TryGetChoiceRecord(dialogueId, out choice);
            }
            return false;
        }

        /// <summary>
        /// 获取指定 NPC 最后播放到的对话 id。
        /// </summary>
        public int GetLastDialogueId(int npcId)
        {
            if (!npcDialogueHistory.TryGetValue(npcId, out var history))
                return -1;
            return history.lastDialogueId;
        }

        #endregion

        #region 状态更新（由事件触发）

        /// <summary>
        /// 直接调用：收到服务器推送的单个 NPC 对话历史增量更新（来自 NpcChangeS2C）。
        /// 会覆盖该 NPC 的进度列表和选项记录，并同步解锁状态。
        /// </summary>
        public void OnReceiveNPCDataUpdate(Cmd.NPCDialogueHistoryInfo hisProto)
        {
            if (hisProto == null) return;

            int npcId = hisProto.NpcId;
            var favorLevel = 0;
            if (NPCManager.Instance != null &&
                NPCManager.Instance.NPC_Info_Dic.TryGetValue(npcId, out var npcInfo) &&
                npcInfo?._npc_RuntimeData != null)
                favorLevel = npcInfo._npc_RuntimeData.favor_level;

            if (!TryBuildHistoryFromServerData(npcId, favorLevel, hisProto, out var history, out var shouldSync))
                return;

            npcDialogueHistory[npcId] = history;

            Debug.Log($"[NPCChatManager] OnReceiveNPCDataUpdate：已用服务器数据更新 NPC {npcId}，" +
                      $"lastDialogueId={history.lastDialogueId}，段落数={history.dialogueProgressList.Count}。");

            if (shouldSync)
                SyncNpcHisToServer(npcId);
        }

        #endregion

        #region 记录操作

        /// <summary>
        /// 记录玩家在某个选项对话中的选择。
        /// </summary>
        public void RecordChoice(int npcId, int dialogueId, int choice)
        {
            if (!npcDialogueHistory.TryGetValue(npcId, out var history))
            {
                Debug.LogWarning($"[NPCChatManager] RecordChoice：未找到 NPC {npcId} 的对话历史。");
                return;
            }
            history.AddChoiceRecord(dialogueId, choice);
        }

        /// <summary>
        /// 标记指定 NPC 指定段落为已完成。
        /// </summary>
        public void MarkParagraphFinished(int npcId, int paragraphId)
        {
            if (!npcDialogueHistory.TryGetValue(npcId, out var history))
            {
                Debug.LogWarning($"[NPCChatManager] MarkParagraphFinished：未找到 NPC {npcId} 的对话历史。");
                return;
            }
            history.FinishParagraph(paragraphId);
            SyncNpcHisToServer(npcId);
            Debug.Log($"[NPCChatManager] NPC {npcId} 段落 {paragraphId} 已标记为完成。");
        }

        /// <summary>
        /// 记录当前播放到的对话 id。
        /// </summary>
        public void RecordProgress(int npcId, int dialogueId)
        {
            if (!npcDialogueHistory.TryGetValue(npcId, out var history))
                return;
            history.RecordProgress(dialogueId);
        }

        #endregion


        /// <summary>
        /// 将指定 NPC 的对话历史同步到服务器（用于进度变更时增量上报）。
        /// </summary>
        public void SyncNpcHisToServer(int npcId)
        {
            if (!npcDialogueHistory.TryGetValue(npcId, out var history))
            {
                Debug.LogWarning($"[NPCChatManager] SyncNpcHisToServer：未找到 NPC {npcId} 的对话历史。");
                return;
            }

            var protoHistory = new Cmd.NPCDialogueHistoryInfo
            {
                NpcId = history.npcId,
                LastDialogueId = history.lastDialogueId
            };

            foreach (var progress in history.dialogueProgressList)
            {
                protoHistory.DialogueProgressList.Add(new Cmd.NPCDialogueProgress
                {
                    ParagraphId = progress.paragraphId,
                    IsUnlocked = progress.isUnlocked,
                    IsFinished = progress.isFinished
                });
            }

            foreach (var choice in history.optionChoices)
            {
                protoHistory.OptionChoices.Add(new Cmd.NPCDialogueChoiceRecord
                {
                    DialogueId = choice.DialogueId,
                    ChoiceIndex = choice.choiceIndex
                });
            }

            var req = new Cmd.NPCChatHistroyUpdateReq { History = protoHistory };
            EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, req);
            Debug.Log($"[NPCChatManager] 已向服务器同步 NPC {npcId} 的对话历史。");
        }
    }
}
