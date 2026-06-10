using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Network;
using Cmd;
using Common;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    /// <summary>
    /// 按 \ 弹出的测试发奖界面：从 project_mouse_tb_test_item.json 读取 low/middle/high 并发送 TestAddItemReq。
    /// </summary>
    public class BackslashTestRewardPopup : MonoBehaviour
    {
        private const string TestItemJsonResourcePath = "Json/project_mouse_tb_test_item";

        private Canvas _popupCanvas;
        private GameObject _root;
        private Button _closeButton;
        private InputField _roleIdsInput;
        private Button _btnLow;
        private Button _btnMiddle;
        private Button _btnHigh;
        private bool _sending;

        private void Update()
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null)
                return;
            if (kb.backslashKey.wasPressedThisFrame)
                Toggle();
        }

        public void Toggle()
        {
            if (_root == null)
                BuildUI();

            ClearRoleIdsInput();
            _root.SetActive(!_root.activeSelf);
        }

        private void ClearRoleIdsInput()
        {
            if (_roleIdsInput != null)
                _roleIdsInput.text = string.Empty;
        }

        private async void SendTier(string tierField)
        {
            if (_sending)
                return;

            if (!NetWork_Center_WSS.IsConnectedToPlayerServer)
            {
                EvtDsp.TriggerEvt<string, Action>(EvtNames.ShowPrompt, "未连接玩家服务器", null);
                return;
            }

            string roleIdsRaw = _roleIdsInput != null ? _roleIdsInput.text : string.Empty;
            if (!TryParseRoleIds(roleIdsRaw, out var roleIds, out string roleError))
            {
                EvtDsp.TriggerEvt<string, Action>(EvtNames.ShowPrompt, roleError ?? "RoleID 格式错误", null);
                return;
            }

            if (!TryBuildTestAddItemReq(tierField, out var req, out string error))
            {
                EvtDsp.TriggerEvt<string, Action>(EvtNames.ShowPrompt, error ?? "配置解析失败", null);
                return;
            }

            foreach (var roleId in roleIds)
                req.RoleIDs.Add(roleId);

            _sending = true;
            SetButtonsInteractable(false);
            try
            {
                var task = new ServerTask(req, (receive, t) =>
                {
                    Debug.Log(
                        $"[BackslashTestRewardPopup] TestAddItemRes tier={tierField} items={req.Items.Count} roleIds={req.RoleIDs.Count} receive={receive}");
                });

                await EvtDsp.ReturnEvt<ServerTask, Action<string>, Task>(
                    EvtNames.Excute_Server_Task,
                    task,
                    null);
            }
            finally
            {
                _sending = false;
                SetButtonsInteractable(true);
                ClearRoleIdsInput();
            }
        }

        /// <summary>解析以逗号分隔的 RoleID；空表示不填 RoleIDs（发给自己）。</summary>
        private static bool TryParseRoleIds(string raw, out List<ulong> roleIds, out string error)
        {
            roleIds = new List<ulong>();
            error = null;

            if (string.IsNullOrWhiteSpace(raw))
                return true;

            var seen = new HashSet<ulong>();
            var parts = raw.Split(new[] { ',', '，', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                if (!ulong.TryParse(trimmed, out ulong roleId) || roleId == 0)
                {
                    error = $"RoleID 格式错误: {trimmed}";
                    return false;
                }

                if (seen.Add(roleId))
                    roleIds.Add(roleId);
            }

            return true;
        }

        private static bool TryBuildTestAddItemReq(string tierField, out TestAddItemReq req, out string error)
        {
            req = null;
            error = null;

            var ta = Resources.Load<TextAsset>(TestItemJsonResourcePath);
            if (ta == null || string.IsNullOrEmpty(ta.text))
            {
                error = "未找到 test_item 配置";
                return false;
            }

            JArray root;
            try
            {
                root = JArray.Parse(ta.text);
            }
            catch (Exception ex)
            {
                error = $"配置 JSON 解析失败: {ex.Message}";
                return false;
            }

            if (root == null || root.Count == 0)
            {
                error = "test_item 配置为空";
                return false;
            }

            if (root[0] is not JObject firstEntry)
            {
                error = "test_item 首项格式错误";
                return false;
            }

            if (firstEntry[tierField] is not JArray tierItems || tierItems.Count == 0)
            {
                error = $"未找到 {tierField} 道具列表";
                return false;
            }

            req = new TestAddItemReq { Token = string.Empty };
            var missingNames = new List<string>();

            foreach (var token in tierItems)
            {
                if (token is not JObject itemObj)
                    continue;

                string itemName = itemObj.Value<string>("Item1");
                long count = itemObj.Value<long?>("Item2") ?? 0;
                if (string.IsNullOrWhiteSpace(itemName) || count <= 0)
                    continue;

                var info = Global_Inventory_Manager.GetItemInfo(itemName.Trim());
                if (info == null)
                {
                    missingNames.Add(itemName);
                    continue;
                }

                req.Items.Add(new ItemInfo
                {
                    ConfigID = (ulong)info.item_id,
                    Count = count,
                    TypeID = 1
                });
            }

            if (req.Items.Count == 0)
            {
                error = missingNames.Count > 0
                    ? $"道具均未找到: {string.Join(", ", missingNames)}"
                    : "没有可发送的道具";
                req = null;
                return false;
            }

            if (missingNames.Count > 0)
            {
                Debug.LogWarning(
                    $"[BackslashTestRewardPopup] tier={tierField} 跳过未找到道具: {string.Join(", ", missingNames)}");
            }

            Debug.Log($"[BackslashTestRewardPopup] TestAddItemReq tier={tierField} count={req.Items.Count}");
            return true;
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (_btnLow != null) _btnLow.interactable = interactable;
            if (_btnMiddle != null) _btnMiddle.interactable = interactable;
            if (_btnHigh != null) _btnHigh.interactable = interactable;
        }

        private void BuildUI()
        {
            EnsureTopCanvas();

            _root = new GameObject("BackslashTestRewardPopup", typeof(RectTransform));
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
            panelRt.anchorMin = new Vector2(0.15f, 0.35f);
            panelRt.anchorMax = new Vector2(0.85f, 0.75f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.16f, 0.95f);
            var vlg = panel.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(24, 24, 24, 24);
            vlg.spacing = 20;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleGo.transform.SetParent(panel.transform, false);
            var title = titleGo.GetComponent<Text>();
            title.text = "测试发奖 (\\ 关闭)";
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.fontSize = 36;
            title.color = Color.white;
            title.alignment = TextAnchor.MiddleCenter;
            titleGo.AddComponent<LayoutElement>().preferredHeight = 56;

            _roleIdsInput = CreateInputField("RoleIdsInput", panel.transform, "RoleID，多个用英文逗号分隔，留空则发给自己");
            var roleInputLe = _roleIdsInput.gameObject.AddComponent<LayoutElement>();
            roleInputLe.preferredHeight = 88;
            roleInputLe.minHeight = 88;
            roleInputLe.flexibleWidth = 1;

            _btnLow = CreateActionButton(panel.transform, "发送初级道具", () => SendTier("low"));
            _btnMiddle = CreateActionButton(panel.transform, "发送中级道具", () => SendTier("middle"));
            _btnHigh = CreateActionButton(panel.transform, "发送高级道具", () => SendTier("high"));

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
            _closeButton.onClick.AddListener(() =>
            {
                ClearRoleIdsInput();
                _root.SetActive(false);
            });
            overlayRt.SetAsLastSibling();

            _root.SetActive(false);
        }

        private Button CreateActionButton(Transform parent, string label, Action onClick)
        {
            var btn = CreateButton(label, parent, label);
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 96;
            le.minHeight = 96;
            btn.onClick.AddListener(() => onClick?.Invoke());
            return btn;
        }

        private void EnsureTopCanvas()
        {
            if (_popupCanvas != null)
                return;

            var go = new GameObject("BackslashTestRewardPopupCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _popupCanvas = go.GetComponent<Canvas>();
            _popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _popupCanvas.overrideSorting = true;
            _popupCanvas.sortingOrder = 32765;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
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
            text.fontSize = 28;
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
            ph.fontSize = 24;
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
            input.contentType = InputField.ContentType.Standard;
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
            txt.fontSize = 30;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            var rt = (RectTransform)textGo.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            return go.GetComponent<Button>();
        }
    }
}
