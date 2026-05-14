using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// NPC 聊天系统数据总控。
    /// 职责：
    /// - 静态配置数据（对话/段落/好感度映射）由 <see cref="JsonData_Manager"/> 加载
    /// - 本地玩家存档数据（好感度/选项记录）由 <see cref="Save_Load_Tools"/> 管理持久化
    /// - UI 逻辑全部由 <see cref="ChatViewController"/> 承担，通过事件总线与本类解耦
    ///
    /// 【本地存档数据说明 / 待上线的服务器同步准备】
    /// 以下数据目前以 JSON 文件形式存储在本地（Save_Load_Tools），后续应迁移到服务器：
    /// - DialogueHistory（NPCDialogueHistory）: 每个 NPC 的好感度等级、经验值、已触发对话记录
    ///   → 建议：服务端按 NPCId 存储玩家的好感度快照，对话触发状态由段落完成事件驱动同步
    /// - OptionChoices（选项记录）: 玩家在选项对话中的历史选择
    ///   → 建议：服务端存储选项对话的答案映射表，供回放和分支逻辑使用
    /// 以下数据为纯配置，建议继续使用 Resources 加载：
    /// - 对话内容（DialogueModel）、段落配置（ParagraphModel）、
    ///   好感度等级映射（NPCFavorCorrelativeData）
    /// </summary>
    public class NPCChatManager : SingletonMono<NPCChatManager>
    {

        #region 本地存档文件名

        private const string FileName_NPCDialogueHistory = "NPCDialogueHistory.json";

        #endregion

        /// <summary>对话字典（DialogueId → Model）。</summary>
        public Dictionary<int, DialogueModel> dialoguesDic = new Dictionary<int, DialogueModel>();

        /// <summary>段落字典（ParaId → Model）。</summary>
        public Dictionary<int, ParagraphModel> paragraphsDic = new Dictionary<int, ParagraphModel>();

        /// <summary>各 NPC 的对话进度与选项记录。</summary>
        public Dictionary<int, NPCDialogueHistory> DialogueHistory = new Dictionary<int, NPCDialogueHistory>();

        private Dictionary<int, Dictionary<int, int>> _favorLevelToParaIdDic;

        #region 生命周期


        private void Start()
        {
            //Initialize();

            EvtDsp.AddEvt<NPC_RuntimeData>(EvtNames.On_Single_NPC_Data_Updated, OnReceiveSingleNPCFavor);
            EvtDsp.AddEvt<List<NPC_RuntimeData>>(EvtNames.On_All_NPC_Data_Received, OnReceiveAllNPCFavor);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EvtDsp.RemoveEvt<NPC_RuntimeData>(EvtNames.On_Single_NPC_Data_Updated, OnReceiveSingleNPCFavor);
            EvtDsp.RemoveEvt<List<NPC_RuntimeData>>(EvtNames.On_All_NPC_Data_Received, OnReceiveAllNPCFavor);
        }

        #endregion

        #region 初始化入口

        /// <summary>
        /// 幂等初始化。一次性加载全部静态数据和本地存档。
        /// </summary>
        public void Initialize()
        {
            LoadStaticData();
            LoadDialogueHistory();
        }

        #endregion

        #region 数据加载

        /// <summary>
        /// 从 JsonData_Manager 一次性加载全部对话系统静态数据。
        /// </summary>
        private void LoadStaticData()
        {
            JsonData_Manager.LoadAllDialogueData(
                out var loadedDialogues,
                out var loadedParagraphs,
                out var loadedFavorMapping);

            dialoguesDic = loadedDialogues;
            paragraphsDic = loadedParagraphs;
            _favorLevelToParaIdDic = loadedFavorMapping;

        }

        /// <summary>
        /// 初始化对话进度数据：优先从本地存档加载，否则为每个 NPC 创建新数据。
        /// </summary>
        private void LoadDialogueHistory()
        {
            DialogueHistory = new Dictionary<int, NPCDialogueHistory>();
            DialogueHistory = LoadHistoryFromDisk();

            if (DialogueHistory.Count == 0)
            {
                Debug.Log("[NPCChatManager] 本地对话历史存档不存在，创建初始数据。");
                CreateHistoryForAllNPCs();
            }
            else
            {
                Debug.Log($"[NPCChatManager] 从本地加载对话历史，共 {DialogueHistory.Count} 个 NPC。");
                foreach (var info in NPCManager.Instance.NPC_Info_Dict.Values)
                    EnsureHistoryInitialized(info, allowOverwrite: false);
            }

            SaveHistoryToDisk();
        }

        private void EnsureHistoryInitialized(NPC_Info info, bool allowOverwrite)
        {
            int npcId = info._npc_Base.npc_id;

            if (allowOverwrite || !DialogueHistory.ContainsKey(npcId))
            {
                DialogueHistory[npcId] = new NPCDialogueHistory(npcId);
            }

            if (_favorLevelToParaIdDic != null && _favorLevelToParaIdDic.TryGetValue(npcId, out var paraMap))
                DialogueHistory[npcId].InitDialogueProgress(paraMap);
        }

        private void CreateHistoryForAllNPCs()
        {
            foreach (var info in NPCManager.Instance.NPC_Info_Dict.Values)
                EnsureHistoryInitialized(info, allowOverwrite: true);
        }

        #endregion

        #region 对话查询

        /// <summary>
        /// 根据 npcId 和 runtime 数据，获取下一个"未完成"的段落 id 及对应的好感度等级。
        /// </summary>
        public bool TryGetPendingParagraphId(int npcId, NPC_RuntimeData runtimeData, out int paraId, out int favorLevel)
        {
            paraId = -1;
            favorLevel = -1;

            if (runtimeData == null) return false;

            if (!DialogueHistory.TryGetValue(npcId, out var history))
            {
                history = new NPCDialogueHistory(npcId);
                DialogueHistory[npcId] = history;
            }

            history.UpdateUnlockStatus(runtimeData.favor_level);

            var pending = history.GetNextPendingProgress();
            if (pending == null)
            {
                Debug.Log($"[NPCChatManager] NPC {npcId} 当前无待触发对话（FavorLevel={runtimeData.favor_level}）。");
                return false;
            }

            paraId = pending.ParagraphId;
            favorLevel = pending.FavorLevel;
            Debug.Log($"[NPCChatManager] NPC {npcId} 检测到待触发对话：ParaId={paraId}, FavorLevel={favorLevel}");
            return true;
        }

        /// <summary>
        /// 获取指定 NPC 在指定好感度等级及以下已完成的段落 id 列表（用于历史回放）。
        /// </summary>
        public List<int> GetFinishedParagraphIds(int npcId, int atFavorLevel)
        {
            if (!DialogueHistory.TryGetValue(npcId, out var history))
                return new List<int>();

            var result = history.GetFinishedParagraphIds(atFavorLevel);
            Debug.Log($"[NPCChatManager] NPC {npcId} 历史上已完成 {result.Count} 个段落。");
            return result;
        }

        /// <summary>
        /// 查询历史选项记录。
        /// </summary>
        public bool TryGetChoiceRecord(int npcId, int dialogueId, out string choice)
        {
            if (DialogueHistory.TryGetValue(npcId, out var history))
                return history.TryGetChoiceRecord(dialogueId, out choice);
            choice = null;
            return false;
        }

        /// <summary>
        /// 获取指定 NPC 指定好感度等级的当前进度（最后播放到的 dialogueId）。
        /// </summary>
        public int GetProgressLastDialogueId(int npcId, int favorLevel)
        {
            if (!DialogueHistory.TryGetValue(npcId, out var history))
                return -1;

            foreach (var p in history.DialogueProgressList)
            {
                if (p.FavorLevel == favorLevel)
                    return p.LastDialogueId;
            }
            return -1;
        }

        #endregion

        #region 状态更新（由事件触发）

        private void OnReceiveSingleNPCFavor(NPC_RuntimeData data)
        {
            if (data == null) return;

            if (!DialogueHistory.TryGetValue(data.npc_id, out var history))
            {
                history = new NPCDialogueHistory(data.npc_id);
                if (_favorLevelToParaIdDic != null && _favorLevelToParaIdDic.TryGetValue(data.npc_id, out var paraMap))
                    history.InitDialogueProgress(paraMap);
                DialogueHistory[data.npc_id] = history;
            }

            history.UpdateUnlockStatus(data.favor_level);

            SaveHistoryToDisk();
        }

        private void OnReceiveAllNPCFavor(List<NPC_RuntimeData> allDatas)
        {
            if (allDatas == null || allDatas.Count == 0) return;

            foreach (var data in allDatas)
            {
                if (data == null) continue;

                if (!DialogueHistory.TryGetValue(data.npc_id, out var history))
                {
                    history = new NPCDialogueHistory(data.npc_id);
                    if (_favorLevelToParaIdDic != null && _favorLevelToParaIdDic.TryGetValue(data.npc_id, out var paraMap))
                        history.InitDialogueProgress(paraMap);
                    DialogueHistory[data.npc_id] = history;
                }

                history.UpdateUnlockStatus(data.favor_level);
            }

            SaveHistoryToDisk();
        }

        #endregion

        #region 本地存档持久化（Save_Load_Tools）

        private Dictionary<int, NPCDialogueHistory> LoadHistoryFromDisk()
        {
            var data = Save_Load_Tools.Load <Dictionary<int, NPCDialogueHistory>>(FileName_NPCDialogueHistory);

            var res = new Dictionary<int, NPCDialogueHistory>();

            //没有数据，首次加载
            if (data != null)
            {
                res = data;
                //foreach (var his in data)
                //{
                //    if (data != null) res[his.Value. NPCId] = his;
                //}
            }
            return res;
        }

        private void SaveHistoryToDisk()
        {
            Save_Load_Tools.Save(FileName_NPCDialogueHistory, DialogueHistory);
        }

        #endregion

        #region 记录操作

        /// <summary>
        /// 记录玩家在某个选项对话中的选择。
        /// </summary>
        public void RecordChoice(int npcId, int dialogueId, string optionText)
        {
            if (!DialogueHistory.TryGetValue(npcId, out var history))
            {
                Debug.LogWarning($"[NPCChatManager] RecordChoice：未找到 NPC {npcId} 的对话历史。");
                return;
            }
            history.AddChoiceRecord(dialogueId, optionText);
            SaveHistoryToDisk();
        }

        /// <summary>
        /// 标记指定 NPC 指定好感度等级的段落为已完成。
        /// </summary>
        public void MarkParagraphFinished(int npcId, int favorLevel)
        {
            if (!DialogueHistory.TryGetValue(npcId, out var history))
            {
                Debug.LogWarning($"[NPCChatManager] MarkParagraphFinished：未找到 NPC {npcId} 的对话历史。");
                return;
            }
            history.MarkParagraphFinished(favorLevel);
            SaveHistoryToDisk();
            Debug.Log($"[NPCChatManager] NPC {npcId} 好感度等级 {favorLevel} 的段落已标记为完成。");
        }

        /// <summary>
        /// 记录当前播放到的对话 id。
        /// </summary>
        public void RecordProgress(int npcId, int favorLevel, int dialogueId)
        {
            if (!DialogueHistory.TryGetValue(npcId, out var history))
                return;
            history.RecordProgress(favorLevel, dialogueId);
        }

        #endregion
    }
}
