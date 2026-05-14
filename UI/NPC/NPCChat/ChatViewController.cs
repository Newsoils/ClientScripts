using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.LYC.Tools;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace CLIP.Project_Mouse.UI
{
    /// <summary>
    /// NPC 聊天面板视图层 + 业务逻辑层。
    ///
    /// 视图职责（原生）：
    /// - EnhancedScroller 驱动的消息列表渲染
    /// - CellData 管理与尺寸计算
    /// - 头像解析、滚动动画
    ///
    /// 业务职责：
    /// - 查询 NPCChatManager 获取对话数据
    /// - 批量加载历史消息（无动画）
    /// - 逐条播放待触发对话（有动画）
    /// - 选项分支逻辑
    ///
    /// 不再持有任何视图引用，所有子视图通过事件总线驱动。
    /// </summary>
    public class ChatViewController : MonoBehaviour, IEnhancedScrollerDelegate
    {
        #region Scroller & Prefabs

        [Header("Scroller")]
        public EnhancedScroller scroller;
        public EnhancedScrollerCellView characterTextCellViewPrefab;
        public EnhancedScrollerCellView npcTextCellViewPrefab;

        #endregion

        #region Layout Parameters

        [Header("文字资源")]
        public TMP_FontAsset characterFontAsset;
        public int fontSize = 25;

        [Header("布局参数（从 Prefab 读取运行时值）")]
        [Tooltip("预设 CellView 总高度（含头像+气泡+间距），由首次添加消息时从 Prefab 同步")]
        public int defaultCellViewHeight = 145;
        [Tooltip("预设气泡背景高度，由首次添加消息时从 Prefab 同步")]
        public int defaultTextWinHeight = 60;
        [Tooltip("预设文字安全区高度，由首次添加消息时从 Prefab 同步")]
        public int defaultSafeAreaHeight = 30;

        [Header("滚动行为")]
        public bool autoScrollToBottom = true;
        [Tooltip("滚动动画时长（秒）")]
        public float scrollAnimationTime = 0.8f;

        #endregion

        #region Head Icons

        [Header("头像资源")]
        public Sprite xiaoTaiHeadIcon;
        private Sprite _npcIconSprite;

        /// <summary>设置 NPC 头像（由 Panel 在头像加载完成后调用）。</summary>
        public void SetNpcIcon(Sprite icon) => _npcIconSprite = icon;

        /// <summary>根据发言者身份解析头像。</summary>
        public Sprite ResolveHeadIcon(bool isCharacter, string speakerName)
        {
            if (isCharacter || speakerName == "小苔")
                return xiaoTaiHeadIcon;
            return _npcIconSprite;
        }

        #endregion

        #region Scroller Data

        private List<CellData> _data = new List<CellData>();
        private float _totalCellSize = 0f;
        private bool _cellSizeSyncedFromPrefab = false;

        private UnityAction _onNoPendingDialogue;

        #endregion

        #region Playback State（从 NPCChatPanelController 迁移）

        private Coroutine _playbackCoroutine;
        private int _currentNpcId;
        private int _currentFavorLevel;
        private bool _messageFinished = false;
        private bool _optionHandled = false;

        #endregion

        #region Lifecycle

        void Awake()
        {
            _data = new List<CellData>();
        }

        void Start()
        {
            scroller.Delegate = this;
        }

        void OnDestroy()
        {
            if (_playbackCoroutine != null)
                StopCoroutine(_playbackCoroutine);
        }

        #endregion

        #region Public API — Panel Entry Point

        /// <summary>
        /// 由 NPCChatPanel 调用，传入已加载完成的 NPC 头像。
        /// 职责：清空视图 → 批量加载历史 → 播放待触发对话。
        /// onNoPendingDialogue 会在没有任何待播放对话时触发（用于弹出提示）。
        /// </summary>
        public void OpenChat(Sprite npcIcon, NPC_Info info, UnityAction onNoPendingDialogue = null)
        {
            if (info == null) return;

            SetNpcIcon(npcIcon);
            ClearAllMessages();

            _currentNpcId = info._npc_Base.npc_id;
            _currentFavorLevel = info._npc_RuntimeData.favor_level;
            _onNoPendingDialogue = onNoPendingDialogue;

            LoadFinishedParagraphs(_currentNpcId, _currentFavorLevel);
            PlayPendingDialogues(_currentNpcId, info._npc_RuntimeData);
        }

        /// <summary>
        /// 由 NPCChatPanel 关闭时调用，停止所有协程。
        /// </summary>
        public void StopPlayback()
        {
            if (_playbackCoroutine != null)
            {
                StopCoroutine(_playbackCoroutine);
                _playbackCoroutine = null;
            }
        }

        #endregion

        #region Public Send / Load API（原生视图方法，保留给其他调用方）

        /// <summary>玩家（小苔）发送消息，自动滚动到底部。</summary>
        public void CharacterSendMsg(Sprite headIcon, string speakerName, string text, string favorHint, UnityAction onFinishScroll)
        {
            AddNewMessage(CellData.CellType.MyText, headIcon, speakerName, text, favorHint);
            ScrollToBottom(onFinishScroll);
        }

        /// <summary>NPC 发送消息，自动滚动到底部。</summary>
        public void NPCSendMsg(Sprite headIcon, string speakerName, string text, string favorHint, UnityAction onFinishScroll)
        {
            AddNewMessage(CellData.CellType.OtherText, headIcon, speakerName, text, favorHint);
            ScrollToBottom(onFinishScroll);
        }

        /// <summary>
        /// 批量加载历史消息（看过/已完成段落），一次性生成全部 CellData 并刷新视图。
        /// 跳过滚动动画，直接静默添加，节省 O(n) 次 ReloadData 开销。
        /// </summary>
        public void LoadHistoryBatch(CellData[] cells)
        {
            if (cells == null || cells.Length == 0) return;

            if (!_cellSizeSyncedFromPrefab && _data.Count == 0)
                SyncCellSizeFromAny();

            _data.AddRange(cells);
            UpdateTotalCellSize();
            scroller.ReloadData();

            if (autoScrollToBottom)
                JumpToBottom();
        }

        /// <summary>清空所有消息。</summary>
        public void ClearAllMessages()
        {
            _data.Clear();
            _totalCellSize = 0f;
            scroller.ReloadData();
        }

        #endregion

        #region History Loading（批量，无动画）

        private void LoadFinishedParagraphs(int npcId, int atFavorLevel)
        {
            var mgr = NPCChatManager.Instance;
            var finishedProgress = mgr.DialogueHistory.TryGetValue(npcId, out var history)
                ? history.DialogueProgressList.FindAll(p => p.FavorLevel <= atFavorLevel && p.IsFinished)
                : new List<DialogueProgress>();

            var allCells = new List<CellData>();

            foreach (var progress in finishedProgress)
            {
                if (!mgr.paragraphsDic.TryGetValue(progress.ParagraphId, out var para))
                {
                    Debug.LogWarning($"[ChatViewController] 段落 {progress.ParagraphId} 未加载。");
                    continue;
                }
                BuildParagraphHistoryCells(para, npcId, allCells);
            }

            if (allCells.Count > 0)
                LoadHistoryBatch(allCells.ToArray());
        }

        private void BuildParagraphHistoryCells(ParagraphModel para, int npcId, List<CellData> cells)
        {
            var dialogue = para.GetDialogueById(para.FirstDialogueId);
            while (dialogue != null)
            {
                if (dialogue.IsOptionDialogue)
                {
                    if (!NPCChatManager.Instance.TryGetChoiceRecord(npcId, dialogue.DialogueId, out var choice))
                    {
                        dialogue = dialogue.NextDialogueId != -1
                            ? para.GetDialogueById(dialogue.NextDialogueId)
                            : null;
                        continue;
                    }
                    dialogue.SetNextDialogueId(dialogue.Options[choice]);

                    cells.Add(BuildCell(
                        isCharacter: true,
                        speakerName: "小苔",
                        text: dialogue.Options.Keys.First(),
                        favorHint: dialogue.ChatHints));
                }
                else
                {
                    bool isChar = dialogue.Speaker == EnumDialogueSpeaker.Character;
                    string name = isChar ? "小苔" : NPCManager.Instance.NPC_Info_Dict[npcId]._npc_Base.npc_name;
                    cells.Add(BuildCell(isChar, name, dialogue.Contents, dialogue.ChatHints));
                }

                dialogue = dialogue.NextDialogueId != -1
                    ? para.GetDialogueById(dialogue.NextDialogueId)
                    : null;
            }
        }

        #endregion

        #region Dialogue Playback（逐条，有动画）

        private void PlayPendingDialogues(int npcId, NPC_RuntimeData runtimeData)
        {
            var mgr = NPCChatManager.Instance;

            if (mgr.TryGetPendingParagraphId(npcId, runtimeData, out int paraId, out int favorLevel))
            {
                _currentFavorLevel = favorLevel;
                Debug.Log($"[ChatViewController] 开始播放待触发段落 {paraId}（FavorLevel={favorLevel}）。");

                _playbackCoroutine = StartCoroutine(PlayParagraphRoutine(paraId, npcId, favorLevel, () =>
                {
                    mgr.MarkParagraphFinished(npcId, favorLevel);
                    PlayPendingDialogues(npcId, runtimeData);
                }));
            }
            else
            {
                Debug.Log($"[ChatViewController] NPC {npcId} 当前无待触发对话。");
                _onNoPendingDialogue?.Invoke();
            }
        }

        private IEnumerator PlayParagraphRoutine(int paraId, int npcId, int favorLevel, UnityAction onComplete)
        {
            var mgr = NPCChatManager.Instance;

            if (!mgr.paragraphsDic.TryGetValue(paraId, out var para))
            {
                Debug.LogError($"[ChatViewController] 段落 {paraId} 不存在。");
                onComplete?.Invoke();
                yield break;
            }

            int resumeFromId = mgr.GetProgressLastDialogueId(npcId, favorLevel);
            int startDialogueId = resumeFromId != -1 ? resumeFromId : para.FirstDialogueId;

            var dialogue = para.GetDialogueById(startDialogueId);

            while (dialogue != null)
            {
                if (dialogue.IsOptionDialogue)
                    yield return PlayOptionRoutine(dialogue, npcId, favorLevel);
                else
                    yield return PlayNormalRoutine(dialogue, npcId, favorLevel);

                mgr.RecordProgress(npcId, favorLevel, dialogue.DialogueId);

                dialogue = dialogue.NextDialogueId != -1
                    ? para.GetDialogueById(dialogue.NextDialogueId)
                    : null;
            }

            onComplete?.Invoke();
        }

        private IEnumerator PlayNormalRoutine(DialogueModel dialogue, int npcId, int favorLevel)
        {
            bool isChar = dialogue.Speaker == EnumDialogueSpeaker.Character;
            string name = isChar ? "小苔" : NPCManager.Instance.NPC_Info_Dict[npcId]._npc_Base.npc_name;
            Sprite icon = ResolveHeadIcon(isChar, name);

            _messageFinished = false;
            if (isChar)
                CharacterSendMsg(icon, name, dialogue.Contents, dialogue.ChatHints, OnMessageFinished);
            else
                NPCSendMsg(icon, name, dialogue.Contents, dialogue.ChatHints, OnMessageFinished);

            yield return new WaitUntil(() => _messageFinished);
        }

        private IEnumerator PlayOptionRoutine(DialogueModel option, int npcId, int favorLevel)
        {
            yield return WaitForOptionChoice(option, npcId, favorLevel);
        }

        private IEnumerator WaitForOptionChoice(DialogueModel option, int npcId, int favorLevel)
        {
            var handlers = new List<UnityAction>();

            foreach (var kv in option.Options)
            {
                handlers.Add(() =>
                {
                    StartCoroutine(HandleOptionSelected(kv.Key, kv.Value, option, npcId, favorLevel));
                });
            }

            EvtDsp.TriggerEvt<EvtData_OpenOption>(
                EvtNames.Evt_NPCChat_OpenOption,
                new EvtData_OpenOption(option, handlers));

            yield return new WaitUntil(() => _optionHandled);
            _optionHandled = false;
        }

        private IEnumerator HandleOptionSelected(string selectedText, int nextDialogueId, DialogueModel option, int npcId, int favorLevel)
        {
            var mgr = NPCChatManager.Instance;

            mgr.RecordChoice(npcId, option.DialogueId, selectedText);
            option.SetNextDialogueId(nextDialogueId);

            EvtDsp.TriggerEvt(EvtNames.Evt_NPCChat_CloseOption);

            _messageFinished = false;
            CharacterSendMsg(
                xiaoTaiHeadIcon,
                "小苔",
                selectedText,
                option.ChatHints,
                OnMessageFinished);

            yield return new WaitUntil(() => _messageFinished);
            _optionHandled = true;
        }

        #endregion

        #region Helpers

        private CellData BuildCell(bool isCharacter, string speakerName, string text, string favorHint)
        {
            Sprite icon = ResolveHeadIcon(isCharacter, speakerName);
            return new CellData
            {
                cellType = isCharacter ? CellData.CellType.MyText : CellData.CellType.OtherText,
                headIconSprite = icon,
                speakerName = speakerName,
                contentText = text,
                favorHint = favorHint,
                defaultCellViewHeight = defaultCellViewHeight,
                textWinHeight = defaultTextWinHeight,
                cellViewHeight = defaultCellViewHeight,
                safeTextAreaHeight = defaultSafeAreaHeight,
            };
        }

        #endregion

        #region View Callbacks

        private void OnMessageFinished()
        {
            _messageFinished = true;
        }

        #endregion

        #region Internal Messaging

        private void AddNewMessage(CellData.CellType type, Sprite headIcon, string speakerName, string text, string favorHint)
        {
            SyncCellSizeFromAny();

            Vector2 predictedSize = TMPTextSizePredictor.GetExactTextSize(text, characterFontAsset, fontSize, new Vector2(400f, 30f));

            float textWinHeight = defaultTextWinHeight - fontSize + predictedSize.y;
            float cellViewHeight = defaultCellViewHeight - fontSize + predictedSize.y;
            float safeAreaHeight = defaultSafeAreaHeight - fontSize + predictedSize.y;

            float oldTotal = _totalCellSize;

            _data.Add(new CellData
            {
                cellType = type,
                headIconSprite = headIcon,
                speakerName = speakerName,
                contentText = text,
                favorHint = favorHint,
                defaultCellViewHeight = defaultCellViewHeight,
                textWinHeight = textWinHeight,
                cellViewHeight = cellViewHeight,
                safeTextAreaHeight = safeAreaHeight,
            });

            UpdateTotalCellSize();

            float scrollRatio = oldTotal > 0f ? oldTotal / _totalCellSize : 0f;
            scroller.ReloadData(scrollRatio);

            if (autoScrollToBottom)
                ScrollToBottom();
        }

        private void SyncCellSizeFromAny()
        {
            if (_cellSizeSyncedFromPrefab) return;

            var prefab = characterTextCellViewPrefab != null
                ? characterTextCellViewPrefab
                : npcTextCellViewPrefab;

            if (prefab == null) return;

            defaultCellViewHeight = (int)prefab.transform.Find("BG_Image").GetComponent<RectTransform>().sizeDelta.y;
            defaultTextWinHeight = (int)prefab.transform.Find("textBG_Image").GetComponent<RectTransform>().sizeDelta.y;

            _cellSizeSyncedFromPrefab = true;
        }

        private void UpdateTotalCellSize()
        {
            _totalCellSize = scroller.padding.top + scroller.padding.bottom;
            for (int i = 0; i < _data.Count; i++)
            {
                _totalCellSize += _data[i].cellViewHeight;
                if (i < _data.Count - 1)
                    _totalCellSize += scroller.spacing;
            }
        }

        private void ScrollToBottom(UnityAction onFinishScroll = null)
        {
            if (_data.Count == 0) return;

            scroller.JumpToDataIndex(_data.Count - 1, 1f, 1f,
                tweenType: EnhancedScroller.TweenType.easeInOutSine,
                tweenTime: scrollAnimationTime);

            if (onFinishScroll != null)
                StartCoroutine(WaitAndInvoke(onFinishScroll));
        }

        private void JumpToBottom()
        {
            if (_data.Count == 0) return;
            scroller.JumpToDataIndex(_data.Count - 1);
        }

        private IEnumerator WaitAndInvoke(UnityAction callback)
        {
            yield return new WaitForSeconds(scrollAnimationTime + 0.5f);
            callback?.Invoke();
        }

        #endregion

        #region IEnhancedScrollerDelegate

        public int GetNumberOfCells(EnhancedScroller scroller) => _data.Count;

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) => _data[dataIndex].cellViewHeight;

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var prefab = _data[dataIndex].cellType == CellData.CellType.MyText
                ? characterTextCellViewPrefab
                : npcTextCellViewPrefab;

            var cell = scroller.GetCellView(prefab) as CellView;
            cell.name = $"Cell_{dataIndex}";
            cell.SetData(_data[dataIndex]);
            return cell;
        }

        #endregion
    }
}
