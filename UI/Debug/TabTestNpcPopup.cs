using System;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Network;
using Cmd;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Game_Play_System;

namespace CLIP.Project_Mouse.UI
{
    /// <summary>
    /// 按 Tab 弹出的 NPC 调试界面：开放 NPC / 增加亲密度。
    /// NPC 列表来自 Resources/Json/project_mouse_tb_npc_info.json。
    /// </summary>
    public class TabTestNpcPopup : MonoBehaviour
    {
        private const long PinkTulipPotGiftItemId = 8001;

        private Canvas _popupCanvas;
        private GameObject _root;
        private RectTransform _listContent;
        private Button _closeButton;

        private readonly List<NpcEntry> _npcList = new List<NpcEntry>();
        private readonly Dictionary<int, InputField> _expInputs = new Dictionary<int, InputField>();

        private sealed class NpcEntry
        {
            public int npc_id;
            public string npc_name;
        }

        private void Update()
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null)
                return;
            if (kb.tabKey.wasPressedThisFrame)
                Toggle();
        }

        public void Toggle()
        {
            if (_root == null)
                BuildUI();

            bool nextActive = !_root.activeSelf;
            _root.SetActive(nextActive);
            if (nextActive)
            {
                LoadNpcList();
                RebuildNpcRows();
            }
        }

        private void LoadNpcList()
        {
            _npcList.Clear();
            try
            {
                var ta = Resources.Load<TextAsset>("Json/project_mouse_tb_npc_info");
                if (ta == null || string.IsNullOrEmpty(ta.text))
                {
                    Debug.LogError("[TabTestNpcPopup] Resources/Json/project_mouse_tb_npc_info.json not found or empty.");
                    return;
                }

                var arr = JArray.Parse(ta.text);
                foreach (var token in arr)
                {
                    if (token == null)
                        continue;
                    int id = token.Value<int?>("npc_id") ?? 0;
                    string name = token.Value<string>("npc_name") ?? string.Empty;
                    if (id <= 0)
                        continue;
                    _npcList.Add(new NpcEntry { npc_id = id, npc_name = name });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TabTestNpcPopup] parse npc json failed: {ex.Message}");
            }

            Debug.Log($"[TabTestNpcPopup] npc loaded: {_npcList.Count}");
        }

        private void RebuildNpcRows()
        {
            if (_listContent == null)
                return;

            _expInputs.Clear();
            for (int i = _listContent.childCount - 1; i >= 0; i--)
                Destroy(_listContent.GetChild(i).gameObject);

            foreach (var npc in _npcList)
                CreateNpcRow(npc);
        }

        private void CreateNpcRow(NpcEntry npc)
        {
            var row = new GameObject($"Npc_{npc.npc_id}", typeof(RectTransform), typeof(VerticalLayoutGroup));
            row.transform.SetParent(_listContent, false);
            var rowVlg = row.GetComponent<VerticalLayoutGroup>();
            rowVlg.spacing = 8;
            rowVlg.padding = new RectOffset(4, 4, 6, 6);
            rowVlg.childAlignment = TextAnchor.UpperLeft;
            rowVlg.childControlWidth = true;
            rowVlg.childControlHeight = true;
            rowVlg.childForceExpandWidth = true;
            rowVlg.childForceExpandHeight = false;
            var rowLe = row.AddComponent<LayoutElement>();
            rowLe.minHeight = 140;

            var nameGo = new GameObject("Name", typeof(RectTransform), typeof(Text));
            nameGo.transform.SetParent(row.transform, false);
            var nameText = nameGo.GetComponent<Text>();
            nameText.text = $"{npc.npc_name} (id:{npc.npc_id})";
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 32;
            nameText.color = Color.white;
            nameText.alignment = TextAnchor.MiddleLeft;
            nameGo.AddComponent<LayoutElement>().preferredHeight = 44;

            var actionRow = new GameObject("ActionRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            actionRow.transform.SetParent(row.transform, false);
            var hlg = actionRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            var actionLe = actionRow.AddComponent<LayoutElement>();
            actionLe.preferredHeight = 88;
            actionLe.minHeight = 88;

            int npcId = npc.npc_id;

            var btnA = CreateButton("BtnOpen", actionRow.transform, "开启");
            ApplyFixedLayout(btnA.gameObject, 120, 88);

            var btnB = CreateButton("BtnAddExp", actionRow.transform, "+亲密度");
            ApplyFixedLayout(btnB.gameObject, 150, 88);

            var btnGift = CreateButton("BtnGiveGift", actionRow.transform, "送粉色郁金香盆栽");
            ApplyFixedLayout(btnGift.gameObject, 200, 88);

            var expInput = CreatePositiveIntInputField("ExpInput", actionRow.transform, "亲密度");
            var expLe = expInput.gameObject.AddComponent<LayoutElement>();
            expLe.flexibleWidth = 1;
            expLe.minWidth = 140;
            expLe.preferredHeight = 88;
            expLe.minHeight = 88;

            _expInputs[npcId] = expInput;
            btnA.onClick.AddListener(() => SendOpenNpc(npcId));
            btnB.onClick.AddListener(() => SendAddNpcExp(npcId, expInput));
            btnGift.onClick.AddListener(() => SendGiveGiftToNpc(npcId));
        }

        private static void ApplyFixedLayout(GameObject go, float width, float height)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null)
                le = go.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.minWidth = width;
            le.preferredHeight = height;
            le.minHeight = height;

            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(width, height);
        }

        private void SendOpenNpc(int npcId)
        {
            if (!NetWork_Center_WSS.IsConnectedToPlayerServer)
            {
                PromptManager.ShowPrompt(PromptId.DebugNotConnected, null);
                return;
            }

            NetWork_Center_WSS.SendMsg(new OpenNpcReq { NpcId = npcId });
            Debug.Log($"[TabTestNpcPopup] OpenNpcReq npcId={npcId}");
        }

        private void SendAddNpcExp(int npcId, InputField expInput)
        {
            if (!NetWork_Center_WSS.IsConnectedToPlayerServer)
            {
                PromptManager.ShowPrompt(PromptId.DebugNotConnected, null);
                return;
            }

            string raw = expInput != null ? expInput.text : string.Empty;
            if (!TryParsePositiveInt(raw, out long addExp))
            {
                PromptManager.ShowPrompt(PromptId.DebugInputInteger, null);
                return;
            }

            NetWork_Center_WSS.SendMsg(new AddNpcExpReq
            {
                NpcId = npcId,
                AddExp = addExp
            });
            Debug.Log($"[TabTestNpcPopup] AddNpcExpReq npcId={npcId} addExp={addExp}");
        }

        private void SendGiveGiftToNpc(int npcId)
        {
            if (!NetWork_Center_WSS.IsConnectedToPlayerServer)
            {
                PromptManager.ShowPrompt(PromptId.DebugNotConnected, null);
                return;
            }

            NetWork_Center_WSS.SendMsg(new GiveGiftToNpcReq
            {
                NpcId = npcId,
                ItemID = PinkTulipPotGiftItemId
            });
            Debug.Log($"[TabTestNpcPopup] GiveGiftToNpcReq npcId={npcId} itemId={PinkTulipPotGiftItemId}");
        }

        private static bool TryParsePositiveInt(string raw, out long value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(raw))
                return false;
            if (!long.TryParse(raw.Trim(), out value))
                return false;
            return value > 0;
        }

        private void BuildUI()
        {
            EnsureTopCanvas();

            _root = new GameObject("TabTestNpcPopup", typeof(RectTransform));
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
            bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            panel.transform.SetParent(_root.transform, false);
            var panelRt = (RectTransform)panel.transform;
            panelRt.anchorMin = new Vector2(0.05f, 0.05f);
            panelRt.anchorMax = new Vector2(0.95f, 0.95f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.16f, 0.95f);
            var vlg = panel.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.spacing = 16;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleGo.transform.SetParent(panel.transform, false);
            var title = titleGo.GetComponent<Text>();
            title.text = "NPC 调试 (Tab 关闭)";
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.fontSize = 36;
            title.color = Color.white;
            title.alignment = TextAnchor.MiddleLeft;
            titleGo.AddComponent<LayoutElement>().preferredHeight = 64;

            var listRoot = new GameObject("NpcList", typeof(RectTransform), typeof(Image));
            listRoot.transform.SetParent(panel.transform, false);
            listRoot.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.14f, 0.92f);
            var listLe = listRoot.AddComponent<LayoutElement>();
            listLe.flexibleHeight = 1;
            listLe.minHeight = 600;

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
            viewport.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.14f, 0.35f);

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            _listContent = (RectTransform)content.transform;
            _listContent.anchorMin = new Vector2(0, 1);
            _listContent.anchorMax = new Vector2(1, 1);
            _listContent.pivot = new Vector2(0.5f, 1);
            _listContent.anchoredPosition = Vector2.zero;
            _listContent.sizeDelta = new Vector2(0, 0);
            var contentVlg = content.GetComponent<VerticalLayoutGroup>();
            contentVlg.spacing = 10;
            contentVlg.padding = new RectOffset(8, 8, 8, 8);
            contentVlg.childControlWidth = true;
            contentVlg.childControlHeight = true;
            contentVlg.childForceExpandWidth = true;
            contentVlg.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sr = scroll.GetComponent<ScrollRect>();
            sr.viewport = vpRt;
            sr.content = _listContent;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;

            var overlay = new GameObject("Overlay", typeof(RectTransform));
            overlay.transform.SetParent(_root.transform, false);
            var overlayRt = (RectTransform)overlay.transform;
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.offsetMin = Vector2.zero;
            overlayRt.offsetMax = Vector2.zero;

            _closeButton = CreateButton("CloseBtn", overlay.transform, "X");
            var closeRt = (RectTransform)_closeButton.transform;
            closeRt.anchorMin = new Vector2(1, 1);
            closeRt.anchorMax = new Vector2(1, 1);
            closeRt.pivot = new Vector2(1, 1);
            closeRt.anchoredPosition = Vector2.zero;
            closeRt.sizeDelta = new Vector2(88, 88);
            _closeButton.onClick.AddListener(() => _root.SetActive(false));
            overlayRt.SetAsLastSibling();

            _root.SetActive(false);
        }

        private void EnsureTopCanvas()
        {
            if (_popupCanvas != null)
                return;

            var go = new GameObject("TabTestNpcPopupCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _popupCanvas = go.GetComponent<Canvas>();
            _popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _popupCanvas.overrideSorting = true;
            _popupCanvas.sortingOrder = 32766;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private static InputField CreatePositiveIntInputField(string name, Transform parent, string placeholder)
        {
            var input = CreateInputField(name, parent, placeholder);
            input.contentType = InputField.ContentType.IntegerNumber;
            input.onValueChanged.AddListener(text =>
            {
                if (string.IsNullOrEmpty(text))
                    return;
                if (long.TryParse(text, out long v) && v > 0)
                    return;

                int i = 0;
                while (i < text.Length && char.IsDigit(text[i]))
                    i++;
                string digits = i > 0 ? text.Substring(0, i) : string.Empty;
                if (digits == "0")
                    digits = string.Empty;
                if (input.text != digits)
                    input.text = digits;
            });
            return input;
        }

        private static InputField CreateInputField(string name, Transform parent, string placeholder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = Color.white;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 30;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleLeft;
            var textRt = (RectTransform)textGo.transform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(16, 8);
            textRt.offsetMax = new Vector2(-16, -8);

            var phGo = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
            phGo.transform.SetParent(go.transform, false);
            var ph = phGo.GetComponent<Text>();
            ph.text = placeholder;
            ph.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ph.fontSize = 28;
            ph.color = new Color(0, 0, 0, 0.35f);
            ph.alignment = TextAnchor.MiddleLeft;
            var phRt = (RectTransform)phGo.transform;
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = new Vector2(16, 8);
            phRt.offsetMax = new Vector2(-16, -8);

            var input = go.GetComponent<InputField>();
            input.textComponent = text;
            input.placeholder = ph;
            input.lineType = InputField.LineType.SingleLine;
            return input;
        }

        private static Button CreateButton(string name, Transform parent, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.35f, 0.55f, 0.95f, 1f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var txt = textGo.GetComponent<Text>();
            txt.text = label;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 26;
            txt.color = Color.white;
            txt.supportRichText = false;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            txt.alignment = TextAnchor.MiddleCenter;
            var rt = (RectTransform)textGo.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(4, 4);
            rt.offsetMax = new Vector2(-4, -4);

            return go.GetComponent<Button>();
        }
    }
}
