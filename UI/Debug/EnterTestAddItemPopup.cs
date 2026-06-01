using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using Cmd;
using Common;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    /// <summary>
    /// 登录成功进入主界面后按 Enter 弹出的全屏调试弹窗：
    /// 搜索道具 -> 选择数量 -> 生成“添加道具:id:数量,...” -> 校验 -> 发送 cmd.TestAddItemReq。
    /// </summary>
    public class EnterTestAddItemPopup : MonoBehaviour
    {
        private const int TestAddItemReqMsgId = 1021;

        private static readonly int[] QtyOptions = { 1, 10, 100, 1000, 10000, 100000, 1000000 };
        private Canvas _popupCanvas;
        private GameObject _root;
        private InputField _searchInput;
        private RectTransform _listContent;
        private InputField _commandInput;
        private Button _sendButton;
        private Button _closeButton;

        private readonly Dictionary<int, int> _selected = new Dictionary<int, int>(); // configID -> count
        private readonly List<int> _order = new List<int>(); // keep append order
        private int _lastSelectedConfigId = 0;

        private sealed class ItemEntry
        {
            public int item_id;
            public string name;
        }

        private readonly List<ItemEntry> _allItems = new List<ItemEntry>();
        private List<ItemEntry> _filteredItems = new List<ItemEntry>();
        private bool _loadedFromJson = false;

        private static readonly Regex CmdRegex = new Regex(
            @"^\s*添加道具:(\d+):(\d+)\s*$",
            RegexOptions.Compiled
        );

        private void Awake()
        {
            CacheItems();
        }

        private void Update()
        {
            // 任意界面按 Enter 都可触发
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return;
            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
            {
                Toggle();
            }
        }

        private void CacheItems()
        {
            _allItems.Clear();
            _loadedFromJson = false;
            try
            {
                // 以 Resources/Json/project_mouse_tb_game_item.json 为准
                var ta = Resources.Load<TextAsset>("Json/project_mouse_tb_game_item");
                if (ta == null || string.IsNullOrEmpty(ta.text))
                {
                    Debug.LogError("[EnterTestAddItemPopup] Resources/Json/project_mouse_tb_game_item.json not found or empty.");
                    _filteredItems = _allItems;
                    return;
                }

                // 只读取 item_id / name，避免其他字段类型差异导致整体反序列化失败
                var arr = JArray.Parse(ta.text);
                foreach (var token in arr)
                {
                    if (token == null) continue;
                    int id = token.Value<int?>("item_id") ?? 0;
                    string name = token.Value<string>("name") ?? string.Empty;
                    if (id <= 0) continue;
                    _allItems.Add(new ItemEntry { item_id = id, name = name });
                }
                _loadedFromJson = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EnterTestAddItemPopup] parse json failed: {ex.Message}");
            }

            // 兜底：若上面异常或结果为空，再用正则从原文提取 item_id/name
            if (_allItems.Count == 0)
            {
                try
                {
                    var ta = Resources.Load<TextAsset>("Json/project_mouse_tb_game_item");
                    if (ta != null && !string.IsNullOrEmpty(ta.text))
                    {
                        var pairRegex = new Regex("\"item_id\"\\s*:\\s*(\\d+)[\\s\\S]*?\"name\"\\s*:\\s*\"([^\"]*)\"");
                        var matches = pairRegex.Matches(ta.text);
                        foreach (Match m in matches)
                        {
                            if (!m.Success) continue;
                            if (!int.TryParse(m.Groups[1].Value, out int id) || id <= 0) continue;
                            string nm = m.Groups[2].Value ?? string.Empty;
                            _allItems.Add(new ItemEntry { item_id = id, name = nm });
                        }
                        _loadedFromJson = _allItems.Count > 0;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EnterTestAddItemPopup] fallback parse failed: {ex.Message}");
                }
            }

            _filteredItems = _allItems;
            Debug.Log($"[EnterTestAddItemPopup] items loaded from json: {_allItems.Count}, loaded={_loadedFromJson}");
        }

        public void Toggle()
        {
            if (_root == null)
                BuildUI();

            bool nextActive = !_root.activeSelf;
            _root.SetActive(nextActive);
            if (nextActive)
            {
                ClearSelectedItemsCache();
                CacheItems();
                // 打开时默认展示全部道具
                if (_searchInput != null) _searchInput.text = string.Empty;
                ApplyFilter(string.Empty);
                _searchInput?.ActivateInputField();
            }
        }

        /// <summary>清空已选道具缓存与命令框（打开界面、发送成功后调用）。</summary>
        private void ClearSelectedItemsCache()
        {
            _selected.Clear();
            _order.Clear();
            _lastSelectedConfigId = 0;
            if (_commandInput != null)
                _commandInput.text = string.Empty;
        }

        private void BuildUI()
        {
            EnsureTopCanvas();

            _root = new GameObject("EnterTestAddItemPopup", typeof(RectTransform));
            _root.transform.SetParent(_popupCanvas.transform, false);
            var rt = (RectTransform)_root.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(_root.transform, false);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            // 全屏背景板：黑色 50% 透明
            bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            panel.transform.SetParent(_root.transform, false);
            var panelRt = (RectTransform)panel.transform;
            panelRt.anchorMin = new Vector2(0.03f, 0.03f);
            panelRt.anchorMax = new Vector2(0.97f, 0.97f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            // 面板本身保持透明，视觉上仅用背景板
            panel.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
            var vlg = panel.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.spacing = 16;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            panel.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            // top row (search). close button at top-right overlay like sketch.
            var topRow = new GameObject("TopRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            topRow.transform.SetParent(panel.transform, false);
            topRow.AddComponent<LayoutElement>().preferredHeight = 96;
            var hlg = topRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            _searchInput = CreateInputField("SearchInput", topRow.transform, "搜索道具名或ID…");
            var searchLe = _searchInput.gameObject.AddComponent<LayoutElement>();
            searchLe.flexibleWidth = 1;
            searchLe.preferredHeight = 96;
            _searchInput.onValueChanged.AddListener(ApplyFilter);

            var overlay = new GameObject("Overlay", typeof(RectTransform));
            overlay.transform.SetParent(_root.transform, false);
            var overlayRt = (RectTransform)overlay.transform;
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.offsetMin = Vector2.zero;
            overlayRt.offsetMax = Vector2.zero;
            overlayRt.SetAsLastSibling();

            _closeButton = CreateButton("CloseBtn", overlay.transform, "X");
            var closeRt = (RectTransform)_closeButton.transform;
            closeRt.anchorMin = new Vector2(1, 1);
            closeRt.anchorMax = new Vector2(1, 1);
            closeRt.pivot = new Vector2(1, 1);
            closeRt.anchoredPosition = new Vector2(0, 0);
            closeRt.sizeDelta = new Vector2(88, 88);
            _closeButton.onClick.AddListener(() => _root.SetActive(false));

            // middle list
            var listRoot = new GameObject("ItemList", typeof(RectTransform), typeof(Image));
            listRoot.transform.SetParent(panel.transform, false);
            listRoot.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 0.92f);
            var listLe = listRoot.AddComponent<LayoutElement>();
            listLe.flexibleHeight = 1;
            listLe.minHeight = 900;

            var scroll = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            scroll.transform.SetParent(listRoot.transform, false);
            var scrollRt = (RectTransform)scroll.transform;
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = new Vector2(10, 10);
            scrollRt.offsetMax = new Vector2(-10, -10);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewport.transform.SetParent(scroll.transform, false);
            var vpRt = (RectTransform)viewport.transform;
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = Vector2.zero;
            // 避免全透明 Mask 导致子元素可点但不可见
            viewport.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.14f, 0.35f);

            var content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            _listContent = (RectTransform)content.transform;
            _listContent.anchorMin = new Vector2(0, 1);
            _listContent.anchorMax = new Vector2(1, 1);
            _listContent.pivot = new Vector2(0.5f, 1);
            _listContent.anchoredPosition = Vector2.zero;
            _listContent.sizeDelta = new Vector2(0, 0);

            var grid = content.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.cellSize = new Vector2(300, 110);
            grid.spacing = new Vector2(8, 8);
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.childAlignment = TextAnchor.UpperLeft;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            content.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var sr = scroll.GetComponent<ScrollRect>();
            sr.viewport = vpRt;
            sr.content = _listContent;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;

            // qty row
            var qtyRow = new GameObject("QtyRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            qtyRow.transform.SetParent(panel.transform, false);
            qtyRow.AddComponent<LayoutElement>().preferredHeight = 92;
            var qtyHlg = qtyRow.GetComponent<HorizontalLayoutGroup>();
            qtyHlg.spacing = 8;
            qtyHlg.childForceExpandWidth = true;
            qtyHlg.childForceExpandHeight = false;

            foreach (var q in QtyOptions)
            {
                var b = CreateButton($"Qty_{q}", qtyRow.transform, q.ToString());
                b.onClick.AddListener(() => ApplyQtyToLast(q));
                b.gameObject.AddComponent<LayoutElement>().preferredHeight = 92;
            }

            // bottom command row (input + send)
            var cmdRow = new GameObject("CmdRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cmdRow.transform.SetParent(panel.transform, false);
            cmdRow.AddComponent<LayoutElement>().preferredHeight = 120;
            var cmdHlg = cmdRow.GetComponent<HorizontalLayoutGroup>();
            cmdHlg.spacing = 12;
            cmdHlg.childForceExpandHeight = false;
            cmdHlg.childForceExpandWidth = false;
            cmdHlg.childControlHeight = true;
            cmdHlg.childControlWidth = true;

            _commandInput = CreateInputField("CommandInput", cmdRow.transform, "文本框");
            var cmdLe = _commandInput.gameObject.AddComponent<LayoutElement>();
            cmdLe.flexibleWidth = 1;
            cmdLe.preferredHeight = 120;

            _sendButton = CreateButton("SendBtn", cmdRow.transform, "发送");
            var sendLe = _sendButton.gameObject.AddComponent<LayoutElement>();
            sendLe.preferredWidth = 140;
            sendLe.preferredHeight = 120;
            _sendButton.onClick.AddListener(() => _ = SendAsync());

            // 关闭按钮层级始终置顶
            overlayRt.SetAsLastSibling();

            _root.SetActive(false);
        }

        private void EnsureTopCanvas()
        {
            if (_popupCanvas != null) return;

            var go = new GameObject("EnterTestAddItemPopupCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _popupCanvas = go.GetComponent<Canvas>();
            _popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _popupCanvas.overrideSorting = true;
            _popupCanvas.sortingOrder = 32767;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void ApplyFilter(string text)
        {
            text = text ?? string.Empty;
            string t = text.Trim();
            if (string.IsNullOrEmpty(t))
            {
                _filteredItems = _allItems;
            }
            else
            {
                _filteredItems = _allItems.Where(i =>
                {
                    if (i == null) return false;
                    return (i.name != null && i.name.Contains(t, StringComparison.OrdinalIgnoreCase))
                        || i.item_id.ToString().Contains(t, StringComparison.OrdinalIgnoreCase);
                }).ToList();
            }
            RebuildList();
        }

        private void RebuildList()
        {
            if (_listContent == null) return;
            foreach (Transform c in _listContent) Destroy(c.gameObject);

            foreach (var item in _filteredItems)
            {
                if (item == null) continue;
                var b = CreateButton($"Item_{item.item_id}", _listContent, $"{item.name}\n{item.item_id}");
                b.onClick.AddListener(() => OnClickItem(item.item_id));
            }

            if (_filteredItems.Count == 0)
            {
                var tip = CreateButton("EmptyTip", _listContent, "未读取到道具配置");
                tip.interactable = false;
            }
        }

        private void OnClickItem(int configId)
        {
            if (configId <= 0) return;
            _lastSelectedConfigId = configId;

            if (_selected.ContainsKey(configId))
            {
                // keep old count
            }
            else
            {
                _selected[configId] = 1;
                _order.Add(configId);
            }

            SyncCommandTextFromSelected();
        }

        private void ApplyQtyToLast(int qty)
        {
            if (_lastSelectedConfigId <= 0) return;
            if (!_selected.ContainsKey(_lastSelectedConfigId)) return;
            _selected[_lastSelectedConfigId] = qty;
            SyncCommandTextFromSelected();
        }

        private void SyncCommandTextFromSelected()
        {
            var parts = new List<string>();
            foreach (var id in _order.Distinct())
            {
                if (!_selected.TryGetValue(id, out var cnt)) continue;
                parts.Add($"添加道具:{id}:{cnt}");
            }
            _commandInput.text = string.Join(",", parts);
        }

        private async Task SendAsync()
        {
            if (_sendButton != null) _sendButton.interactable = false;
            bool requestDispatched = false;
            try
            {
                if (_commandInput == null) return;
                string raw = _commandInput.text ?? string.Empty;
                var (valid, cleaned, parsed, hasInvalid) = ParseCommand(raw);

                if (hasInvalid)
                {
                    EvtDsp.TriggerEvt<string, Action>(EvtNames.ShowPrompt, "格式错误,删除错误内容", null);
                    _commandInput.text = cleaned;
                    return;
                }
                if (!valid || parsed.Count == 0)
                {
                    EvtDsp.TriggerEvt<string, Action>(EvtNames.ShowPrompt, "格式错误,删除错误内容", null);
                    _commandInput.text = string.Empty;
                    return;
                }

                var req = new TestAddItemReq
                {
                    Token = string.Empty
                };
                foreach (var (cfgId, count) in parsed)
                {
                    req.Items.Add(new ItemInfo
                    {
                        ConfigID = (ulong)cfgId,
                        Count = count,
                        TypeID = 1
                    });
                }

                var task = new ServerTask(req, (string receive, ServerTask t) =>
                {
                    Debug.Log($"[TestAddItemRes] receive={receive}");
                });

                await EvtDsp.ReturnEvt<ServerTask, Action<string>, Task>(
                    EvtNames.Excute_Server_Task,
                    task,
                    null
                );

                requestDispatched = true;
            }
            finally
            {
                if (requestDispatched)
                    ClearSelectedItemsCache();
                if (_sendButton != null) _sendButton.interactable = true;
            }
        }

        private static (bool ok, string cleaned, List<(int cfgId, long count)> parsed, bool hasInvalid) ParseCommand(string raw)
        {
            raw = raw ?? string.Empty;
            var tokens = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            var parsed = new List<(int, long)>();
            var cleanedParts = new List<string>();
            bool hasInvalid = false;

            foreach (var tok in tokens)
            {
                var m = CmdRegex.Match(tok);
                if (!m.Success)
                {
                    hasInvalid = true;
                    continue;
                }
                if (!int.TryParse(m.Groups[1].Value, out int id) || id <= 0)
                {
                    hasInvalid = true;
                    continue;
                }
                if (!long.TryParse(m.Groups[2].Value, out long cnt) || cnt <= 0)
                {
                    hasInvalid = true;
                    continue;
                }
                parsed.Add((id, cnt));
                cleanedParts.Add($"添加道具:{id}:{cnt}");
            }

            string cleaned = string.Join(",", cleanedParts);
            bool ok = parsed.Count > 0;
            return (ok, cleaned, parsed, hasInvalid);
        }

        private static InputField CreateInputField(string name, Transform parent, string placeholder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = Color.white;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.GetComponent<Text>();
            text.text = "";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 34;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleLeft;
            var textRt = (RectTransform)textGo.transform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(20, 10);
            textRt.offsetMax = new Vector2(-20, -10);

            var phGo = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
            phGo.transform.SetParent(go.transform, false);
            var ph = phGo.GetComponent<Text>();
            ph.text = placeholder;
            ph.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ph.fontSize = 30;
            ph.color = new Color(0, 0, 0, 0.35f);
            ph.alignment = TextAnchor.MiddleLeft;
            var phRt = (RectTransform)phGo.transform;
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = new Vector2(20, 10);
            phRt.offsetMax = new Vector2(-20, -10);

            var input = go.GetComponent<InputField>();
            input.textComponent = text;
            input.placeholder = ph;
            input.contentType = InputField.ContentType.Standard;
            input.lineType = InputField.LineType.SingleLine;
            return input;
        }

        private static Button CreateButton(string name, Transform parent, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.88f, 0.9f, 0.94f, 1f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var txt = textGo.GetComponent<Text>();
            txt.text = label;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 30;
            txt.color = new Color(0.08f, 0.09f, 0.12f, 1f);
            txt.alignment = TextAnchor.MiddleCenter;
            var rt = (RectTransform)textGo.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var btn = go.GetComponent<Button>();
            return btn;
        }
    }
}

