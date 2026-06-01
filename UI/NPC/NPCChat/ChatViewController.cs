using System.Collections;
using System.Collections.Generic;
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
        private bool _messageFinished = false;
        private bool _optionHandled = false;
        private int _pendingNextDialogueId = -1;

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
           
            _onNoPendingDialogue = onNoPendingDialogue;

            LoadFinishedParagraphs(_currentNpcId);
            PlayPendingDialogues(_currentNpcId);
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

        private void LoadFinishedParagraphs(int npcId)
        {
            var mgr = NPCChatManager.Instance;
            var finishedProgress = mgr.npcDialogueHistory.TryGetValue(npcId, out var history)
                ? history.dialogueProgressList.FindAll(p => p.isFinished)
                : new List<DialogueProgress>();

            var allCells = new List<CellData>();

            foreach (var progress in finishedProgress)
            {
                if (!mgr.paragraphsDic.TryGetValue(progress.paragraphId, out var para))
                {
                    Debug.LogWarning($"[ChatViewController] 段落 {progress.paragraphId} 未加载。");
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
                if (dialogue.isOptionDialogue)
                {
                    if (!NPCChatManager.Instance.TryGetChoiceRecord(npcId, dialogue.dialogueId, out int choiceIndex))
                    {
                        dialogue = dialogue.nextDialogueId != -1
                            ? para.GetDialogueById(dialogue.nextDialogueId)
                            : null;
                        continue;
                    }

                    cells.Add(BuildCell(
                        isCharacter: true,
                        speakerName: "小苔",
                        text: dialogue.optionsLookup[choiceIndex].content,
                        favorHint: dialogue.chatHints));
                }
                else
                {
                    bool isChar = dialogue.speaker == EnumDialogueSpeaker.Character;
                    string name = isChar ? "小苔" : NPCManager.Instance.NPC_Info_Dic[npcId]._npc_Base.npc_name;
                    cells.Add(BuildCell(isChar, name, dialogue.contents, dialogue.chatHints));
                }

                dialogue = dialogue.nextDialogueId != -1
                    ? para.GetDialogueById(dialogue.nextDialogueId)
                    : null;
            }
        }

        #endregion

        #region Dialogue Playback（逐条，有动画）

        private void PlayPendingDialogues(int npcId)
        {
            var mgr = NPCChatManager.Instance;

            if (mgr.TryGetNextPendingParagraphId(npcId, out int paraId))
            {
                Debug.Log($"[ChatViewController] 开始播放待触发段落 {paraId}。");

                _playbackCoroutine = StartCoroutine(PlayParagraphRoutine(paraId, npcId, () =>
                {
                    mgr.MarkParagraphFinished(npcId, paraId);
                    PlayPendingDialogues(npcId);
                }));
            }
            else
            {
                Debug.Log($"[ChatViewController] NPC {npcId} 当前无待触发段落。");
                _onNoPendingDialogue?.Invoke();
            }
        }

        private IEnumerator PlayParagraphRoutine(int paraId, int npcId, UnityAction onComplete)
        {
            var mgr = NPCChatManager.Instance;

            if (!mgr.paragraphsDic.TryGetValue(paraId, out var para))
            {
                Debug.LogError($"[ChatViewController] 段落 {paraId} 不存在。");
                onComplete?.Invoke();
                yield break;
            }

            int resumeFromId = mgr.GetLastDialogueId(npcId);
            int startDialogueId = para.FirstDialogueId;
            if (resumeFromId != -1)
            {
                if (para.TryGetDialogueById(resumeFromId, out _))
                {
                    startDialogueId = resumeFromId;
                }
                else
                {
                    Debug.LogWarning($"[ChatViewController] NPC {npcId} 的 lastDialogueId={resumeFromId} 不属于段落 {paraId}，将从段落首句 {startDialogueId} 开始播放。");
                }
            }

            if (!para.TryGetDialogueById(startDialogueId, out var dialogue))
            {
                Debug.LogError($"[ChatViewController] 段落 {paraId} 的起始对话 {startDialogueId} 不存在，终止播放。");
                yield break;
            }

            while (dialogue != null)
            {
                if (dialogue.isOptionDialogue)
                    yield return PlayOptionRoutine(dialogue, npcId);
                else
                    yield return PlayNormalRoutine(dialogue, npcId);

                mgr.RecordProgress(npcId, dialogue.dialogueId);

                int nextId = _pendingNextDialogueId != -1 ? _pendingNextDialogueId : dialogue.nextDialogueId;
                _pendingNextDialogueId = -1;

                if (nextId == -1)
                {
                    dialogue = null;
                }
                else if (!para.TryGetDialogueById(nextId, out dialogue))
                {
                    Debug.LogError($"[ChatViewController] 段落 {paraId} 中不存在下一句对话 id={nextId}，终止播放。");
                    yield break;
                }
            }

            onComplete?.Invoke();
        }

        private IEnumerator PlayNormalRoutine(DialogueModel dialogue, int npcId)
        {
            bool isChar = dialogue.speaker == EnumDialogueSpeaker.Character;
            string name = isChar ? "小苔" : NPCManager.Instance.NPC_Info_Dic[npcId]._npc_Base.npc_name;
            Sprite icon = ResolveHeadIcon(isChar, name);

            _messageFinished = false;
            if (isChar)
                CharacterSendMsg(icon, name, dialogue.contents, dialogue.chatHints, OnMessageFinished);
            else
                NPCSendMsg(icon, name, dialogue.contents, dialogue.chatHints, OnMessageFinished);

            yield return new WaitUntil(() => _messageFinished);
        }

        private IEnumerator PlayOptionRoutine(DialogueModel option, int npcId)
        {
            var mgr = NPCChatManager.Instance;
            _pendingNextDialogueId = -1;

            if (mgr.TryGetChoiceRecord(npcId, option.dialogueId, out int recordedChoiceIndex))
            {
                _messageFinished = false;
                if (option.optionsLookup.TryGetValue(recordedChoiceIndex, out var recordedOpt))
                    _pendingNextDialogueId = recordedOpt.nextDialogueId;

                CharacterSendMsg(
                    xiaoTaiHeadIcon,
                    "小苔",
                    option.optionsLookup[recordedChoiceIndex].content,
                    option.chatHints,
                    OnMessageFinished);

                yield return new WaitUntil(() => _messageFinished);
                yield break;
            }

            yield return WaitForOptionChoice(option, npcId);
        }

        private IEnumerator WaitForOptionChoice(DialogueModel option, int npcId)
        {
            var handlers = new List<UnityAction>();

            for (int i = 0; i < option.options.Count; i++)
            {
                var opt = option.options[i];
                handlers.Add(() =>
                {
                    StartCoroutine(HandleOptionSelected(opt, option, npcId));
                });
            }

            EvtDsp.TriggerEvt<EvtData_OpenOption>(
                EvtNames.Evt_NPCChat_OpenOption,
                new EvtData_OpenOption(option, handlers));

            yield return new WaitUntil(() => _optionHandled);
            _optionHandled = false;
        }

        private IEnumerator HandleOptionSelected(OptionInfo optionInfo, DialogueModel dialogue, int npcId)
        {
            var mgr = NPCChatManager.Instance;

            mgr.RecordChoice(npcId, dialogue.dialogueId, optionInfo.optionIndex);
            _pendingNextDialogueId = optionInfo.nextDialogueId;

            EvtDsp.TriggerEvt(EvtNames.Evt_NPCChat_CloseOption);

            _messageFinished = false;
            CharacterSendMsg(
                xiaoTaiHeadIcon,
                "小苔",
                optionInfo.content,
                dialogue.chatHints,
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
