using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.LYC.Tools;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace CLIP.Project_Mouse.LYC.UI
{
    public class ChatViewController : MonoBehaviour, IEnhancedScrollerDelegate
    {
        /// <summary>
        /// Internal representation of our data. Note that the scroller will never see
        /// this, so it separates the data from the layout using MVC principles.
        /// </summary>
        private List<CellData> _data = new List<CellData>();

        /// <summary>
        /// This stores the total size of all the cells,
        /// plus the scroller's top and bottom padding.
        /// This will be used to calculate the scroller size
        /// </summary>
        private float _totalCellSize = 0;

        /// <summary>
        /// This is our scroller we will be a delegate for
        /// </summary>
        public EnhancedScroller scroller;

        /// <summary>
        /// This will be the prefab of our chat cell
        /// </summary>
        public EnhancedScrollerCellView characterTextCellViewPrefab;

        /// <summary>
        /// This will be the prefab of another person's chat cell
        /// </summary>
        public EnhancedScrollerCellView npcTextCellViewPrefab;

        /// <summary>
        /// The estimated width of each character. Note that this is just an estimate
        /// since most fonts are not mono-spaced.
        /// </summary>
        //public int characterWidth = 25; // 使用字符长度来预测不精确，改用下面的方式

        // 字符资源
        public TMP_FontAsset characterFontAsset;

        // 字符尺寸
        public int fontSize = 25;

        // 默认聊天框高度
        public int defaultTextWinHeight = 60;

        // 默认文字安全区高度
        public int defaultSafeAreaHeight = 30;

        // 一行文字 的高度，供滚动窗口的高度计算（fontSize + 文字的每行行宽text.spacer.line）
        public int lineHeight = 30;

        // 默认 CellView 的高度，供滚动窗口的高度计算
        public int defaultCellViewHeight = 145;

        /// <summary>
        /// Whether to auto scroll to top when new message is added
        /// </summary>
        public bool autoScrollToTop = true;

        /// <summary>
        /// Animation time for scrolling to top
        /// </summary>
        public float scrollAnimationTime = 0.8f;


        void Awake()
        {
            // 初始化数据列表
            _data = new List<CellData>();
        }

        void Start()
        {
            UnityEngine.Application.targetFrameRate = 60;
            scroller.Delegate = this;

        }



        /// <summary>
        /// 创建并且添加新行
        /// </summary>
        public void AddNewRow(CellData.CellType cellType, Sprite headIconSprite
            , string speakerName, string someText, string chatHint = null)
        {
            if (cellType == CellData.CellType.MyText)
            {
                defaultCellViewHeight = (int)characterTextCellViewPrefab.transform.Find("BG_Image").
                    GetComponent<RectTransform>().sizeDelta.y;
                defaultTextWinHeight = (int)characterTextCellViewPrefab.transform.Find("textBG_Image").
                    GetComponent<RectTransform>().sizeDelta.y;
            }
            else
            {
                defaultCellViewHeight = (int)npcTextCellViewPrefab.transform.Find("BG_Image").
                    GetComponent<RectTransform>().sizeDelta.y;
                defaultTextWinHeight = (int)npcTextCellViewPrefab.transform.Find("textBG_Image").
                    GetComponent<RectTransform>().sizeDelta.y;
            }

            Vector2 predictedSize = TMPTextSizePredictor.GetExactTextSize(someText, characterFontAsset, fontSize, new Vector2(400, 30));

            // 计算 文字框 高度
            var textWinHeight = defaultTextWinHeight - fontSize + predictedSize.y;

            // 计算 CellView 高度
            var cellViewHeight = defaultCellViewHeight - fontSize + predictedSize.y;

            // 计算 文字安全区 高度
            var safeTextAreaHeight = defaultSafeAreaHeight - fontSize + predictedSize.y;

            // 保持原来的添加到末尾（新消息在底部）
            _data.Add(new CellData()
            {
                cellType = cellType,
                headIconSprite = headIconSprite,
                speakerName = speakerName,
                someText = someText,
                chatHint = chatHint,
                defaultCellViewHeight = defaultCellViewHeight,
                textWinHeight = textWinHeight,
                cellViewHeight = cellViewHeight,
                safeTextAreaHeight = safeTextAreaHeight,
            });

            float oldTotalCellSize = _totalCellSize;

            // 重新计算总高度
            UpdateTotalCellSize();

            // 重新加载数据（传入参数为 0 时，代表滚动条 Scroll 从上方开始往下滚动；为 1 时，代表滚动条位于底部）
            // 这里可以通过计算加入 CellView 前后总高度之比得到新消息向下滚动前的 Scroll 位置
            float scrollPosPercentage = oldTotalCellSize / _totalCellSize;
            scroller.ReloadData(scrollPosPercentage);

            bool autoScrollToBottom = true;

            // 如果需要，滚动到新消息（底部）
            if (autoScrollToBottom)
            {
                ScrollToBottom();
            }
        }

        private void ResizeScroller()
        {
            // 简化版本：只需更新高度并重新加载
            UpdateTotalCellSize();
            scroller.ReloadData();
        }

        // 计算全部 CellView 的高度
        /// <summary>
        /// Update the total cell size calculation
        /// </summary>
        private void UpdateTotalCellSize()
        {
            _totalCellSize = scroller.padding.top + scroller.padding.bottom;
            for (var i = 0; i < _data.Count; i++)
            {
                _totalCellSize += _data[i].cellViewHeight + (i < _data.Count - 1 ? scroller.spacing : 0);
            }
        }

        /// <summary>
        /// Scroll to the top to show newest message
        /// </summary>
        private void ScrollToTop()
        {
            // Jump to the first cell (index 0) which is the newest message
            scroller.JumpToDataIndex(0, 1f, 1f,
                tweenType: EnhancedScroller.TweenType.easeInOutSine,
                tweenTime: scrollAnimationTime);
        }

        #region EnhancedScroller Handlers

        /// <summary>
        /// This tells the scroller the number of cells that should have roomData allocated. This should be the length of your data array.
        /// </summary>
        /// <param name="scroller">The scroller that is requesting the data size</param>
        /// <returns>The number of cells</returns>
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            // return the number of data elements
            return _data.Count;
        }

        /// <summary>
        /// Gets the cell view size for each cell
        /// </summary>
        /// <param name="scroller"></param>
        /// <param name="dataIndex"></param>
        /// <returns></returns>
        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            // return the cell size for each cell
            return _data[dataIndex].cellViewHeight;
        }

        // 本质就是创建一个 CellView 格子
        /// <summary>
        /// Reuse the appropriate cell
        /// </summary>
        /// <param name="scroller"></param>
        /// <param name="dataIndex"></param>
        /// <param name="cellIndex"></param>
        /// <returns></returns>
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            CellView cellView;

            // 没有Spacer了，直接判断消息类型
            if (_data[dataIndex].cellType == CellData.CellType.MyText)
            {
                cellView = scroller.GetCellView(characterTextCellViewPrefab) as CellView;
            }
            else
            {
                cellView = scroller.GetCellView(npcTextCellViewPrefab) as CellView;
            }

            cellView.name = "Cell " + dataIndex.ToString();
            cellView.SetData(_data[dataIndex]);

            return cellView;
        }

        #endregion

        #region Additional Public Methods

        /// <summary>
        /// Clear all chat messages
        /// </summary>
        public void ClearAllMessages()
        {
            _data.Clear();
            scroller.ReloadData();
        }

        /// <summary>
        /// Get the total number of messages
        /// </summary>
        public int GetMessageCount()
        {
            return _data.Count;
        }

        /// <summary>
        /// Toggle auto scroll to top when new message is added
        /// </summary>
        public void ToggleAutoScroll(bool enable)
        {
            autoScrollToTop = enable;
        }

        /// <summary>
        /// Manually scroll to top
        /// </summary>
        public void ManualScrollToTop()
        {
            ScrollToTop();
        }

        /// <summary>
        /// Manually scroll to bottom (oldest messages)
        /// </summary>
        public void ScrollToBottom(UnityAction onFinishScroll = null)
        {
            if (_data.Count > 0)
            {
                scroller.JumpToDataIndex(_data.Count - 1, 1f, 1f,
                    tweenType: EnhancedScroller.TweenType.easeInOutSine,
                    tweenTime: scrollAnimationTime);
            }

            StartCoroutine(WaitForScroll());
            IEnumerator WaitForScroll()
            {
                yield return new WaitForSeconds(scrollAnimationTime + 0.8f);
                onFinishScroll?.Invoke();
            }
        }

        #endregion

        #region 供外部调用的，用于模拟消息发送等方法

        // 模拟玩家发消息
        // onFinishScroll 会在播放结束后触发回调
        public void CharacterSendMsg(Sprite headIconSprite
            , string speakerName, string someText, string chatHint, UnityAction onFinishScroll)
        {
            AddNewRow(CellData.CellType.MyText, headIconSprite, speakerName, someText, chatHint);

            // 发送后滚动到底部
            ScrollToBottom(onFinishScroll);
        }

        // 模拟 NPC 发消息
        // onFinishScroll 会在播放结束后触发回调
        public void NPCSendMsg(Sprite headIconSprite
            , string speakerName, string someText, string chatHint, UnityAction onFinishScroll)
        {
            AddNewRow(CellData.CellType.OtherText, headIconSprite, speakerName, someText, chatHint);

            // 发送后滚动到底部
            ScrollToBottom(onFinishScroll);
        }

        // 单条加载玩家聊天信息（不带滚动效果）
        public void LoadSingleCharacterMsg(Sprite headIconSprite
            , string speakerName, string someText, string chatHint)
        {
            AddNewRow(CellData.CellType.MyText, headIconSprite, speakerName, someText, chatHint);
        }

        // 单条加载 NPC 聊天信息（不带滚动效果）
        public void LoadSingleNPCMsg(Sprite headIconSprite
            , string speakerName, string someText, string chatHint)
        {
            AddNewRow(CellData.CellType.OtherText, headIconSprite, speakerName, someText, chatHint);
        }

        #endregion


    }
}
