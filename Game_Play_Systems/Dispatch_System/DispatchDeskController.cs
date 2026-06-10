using System.Threading.Tasks;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System.Dispatch_System
{
    public class DispatchDeskController : MonoBehaviour
    {
        [Header("Page")]
        public DispatchDeskPageController pageController;

        [Header("Model Roots")]
        public Transform foodRoot;
        public Transform snackRoot;

        [Header("CD")]
        public CD3DCarousel cdCarousel;
        public CDLidController cdLid;
        public MeshRenderer cdPlayerMainCdRenderer;

        [Header("State")]
        public bool clearRootsBeforeInstantiate = true;
        public bool useLegacyDispatchPanelLists = true;

        [Header("Swipe")]
        public float swipeThreshold = 80f;
        public float swipeDirectionDominance = 1.2f;

        private int foodLoadVersion;
        private int snackLoadVersion;
        private int tapeLoadVersion;
        private int cdDisplayLoadVersion;
        private IReadOnlyList<Material> cdDisplayMaterials;
        private int currentPreviewBagIndex = -1;
        private string currentPreviewTapeName;
        private Vector2 swipeStartPosition;
        private bool isTrackingSwipe;
        private bool isTrackingCdGridSwipe;
        private Procedure_Dispatch currentProcedure = Procedure_Dispatch.SelectBag;

        private void Awake()
        {
            DisableLegacySwitcher(foodRoot);
            DisableLegacySwitcher(snackRoot);
        }

        private void Start()
        {
            BindInput();
            _ = RefreshCDDisplayAsync();
        }

        private void OnEnable()
        {
            EvtDsp.AddEvt<Procedure_Dispatch>(EvtNames.Dispatch_Switch_Procudure, OnProcedureChanged);
            EvtDsp.AddEvt<int>(EvtNames.Dispatch_Refresh_Model, RefreshModel);
            EvtDsp.AddEvt<Item_Type, string>(EvtNames.Dispatch_Change_Item, OnDispatchItemChanged);
            BindInput();
        }

        private void OnDisable()
        {
            EvtDsp.RemoveEvt<Procedure_Dispatch>(EvtNames.Dispatch_Switch_Procudure, OnProcedureChanged);
            EvtDsp.RemoveEvt<int>(EvtNames.Dispatch_Refresh_Model, RefreshModel);
            EvtDsp.RemoveEvt<Item_Type, string>(EvtNames.Dispatch_Change_Item, OnDispatchItemChanged);
            UnbindInput();
        }

        private void OnProcedureChanged(Procedure_Dispatch procedure)
        {
            currentProcedure = procedure;
            switch (procedure)
            {
                case Procedure_Dispatch.SelectBag:
                    pageController.ShowBagCamera();
                    ClearDeskModels();
                    cdLid.SetOpen(false);
                    break;
                case Procedure_Dispatch.SelectFood:
                    pageController.SwitchToPage(0);
                    break;
                case Procedure_Dispatch.SelectCD:
                    pageController.SwitchToPage(1);
                    cdLid.SetOpen(true);
                    _ = RefreshCDDisplayAsync();
                    break;
            }
        }

        public void HandleDeskClick(DispatchDeskClickAction action)
        {
            switch (action)
            {
                case DispatchDeskClickAction.SelectFood:
                    SwitchToLeftPage();
                    if (useLegacyDispatchPanelLists)
                        EvtDsp.TriggerEvt(EvtNames.DispatchDesk_Click_Action, action);
                    break;
                case DispatchDeskClickAction.SelectSnack:
                    SwitchToLeftPage();
                    if (useLegacyDispatchPanelLists)
                        EvtDsp.TriggerEvt(EvtNames.DispatchDesk_Click_Action, action);
                    break;
                case DispatchDeskClickAction.SwitchToLeftPage:
                    SwitchToLeftPage();
                    break;
                case DispatchDeskClickAction.SwitchToRightPage:
                    SwitchToRightPage();
                    break;
                case DispatchDeskClickAction.ToggleCdList:
                    SwitchToRightPage();
                    cdCarousel.ToggleLayout();
                    BindCdClickTargets();
                    if (useLegacyDispatchPanelLists)
                        EvtDsp.TriggerEvt(EvtNames.DispatchDesk_Click_Action, action);
                    break;
            }
        }

        public void SwitchToLeftPage()
        {
            pageController.SwitchToPage(0);
            cdLid.SetOpen(false);
        }

        public void SwitchToRightPage()
        {
            pageController.SwitchToPage(1);
            cdLid.SetOpen(true);
        }

        public void RefreshModel(int bagIndex)
        {
            Debug.Log($"[DispatchDeskController] RefreshModel bagIndex={bagIndex}");

            var mgr = Dispatch_Manager._instance;
            if (mgr == null)
                return;

            var bag = mgr.GetBagForModelPreview(bagIndex);
            if (bag == null)
            {
                ShowDefaultModels();
                return;
            }

            if (string.IsNullOrEmpty(bag.foodName))
                ShowFood(null, ShouldAnimateModelSwap(foodRoot, "bread"));
            else
                ShowFood(bag.foodName, ShouldAnimateModelSwap(foodRoot, ResolveFoodPrefabName(bag.foodName)));

            if (string.IsNullOrEmpty(bag.snackName))
                ShowSnack(null, ShouldAnimateModelSwap(snackRoot, "Default_leaf"));
            else
                ShowSnack(bag.snackName, ShouldAnimateModelSwap(snackRoot, ResolveSnackPrefabName(bag.snackName)));

            if (string.IsNullOrEmpty(bag.tapeName))
            {
                var defaultTape = GetDefaultTapeInfo();
                if (defaultTape != null)
                    bag.tapeName = defaultTape.tape_name;
            }

            bool shouldPlayTape = currentPreviewBagIndex != bagIndex || currentPreviewTapeName != bag.tapeName;
            currentPreviewBagIndex = bagIndex;
            if (!string.IsNullOrEmpty(bag.tapeName))
                _ = ShowTapeAsync(bag.tapeName, shouldPlayTape);
        }

        private void OnDispatchItemChanged(Item_Type type, string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
                return;

            switch (type)
            {
                case Item_Type.Food:
                    ShowFood(itemName, true);
                    break;
                case Item_Type.Snack:
                    ShowSnack(itemName, true);
                    break;
                case Item_Type.Tape:
                    _ = ShowTapeAsync(itemName, false);
                    break;
            }
        }

        private void ShowFood(string itemName, bool animate = false)
        {
            string prefabName = ResolveFoodPrefabName(itemName);
            ShowDispatchItemPrefab(foodRoot, prefabName, itemName, "food", animate);
        }

        private void ShowSnack(string itemName, bool animate = false)
        {
            string prefabName = ResolveSnackPrefabName(itemName);
            ShowDispatchItemPrefab(snackRoot, prefabName, itemName, "snack", animate);
        }

        private async Task ShowTapeAsync(string itemName, bool playAudio = true)
        {
            var info = Dispatch_Manager.GetCDInfo(itemName);
            if (info == null)
                info = GetDefaultTapeInfo();

            if (info == null)
            {
                Debug.LogWarning($"[DispatchDeskController] Tape config missing: {itemName}");
                return;
            }

            await ShowTapeInfoAsync(info, playAudio);
        }

        private async Task ShowTapeInfoAsync(Tape_Info info, bool playAudio)
        {
            if (string.IsNullOrEmpty(info.mat_resource_name))
                return;

            int version = ++tapeLoadVersion;
            var mat = await GameAssets.Instance.LoadAsycByKey<Material>(info.mat_resource_name);
            if (version != tapeLoadVersion)
                return;

            if (mat == null)
            {
                Debug.LogWarning($"[DispatchDeskController] Tape material load failed: {info.tape_name}, key={info.mat_resource_name}");
                return;
            }

            cdPlayerMainCdRenderer.gameObject.SetActive(true);
            cdPlayerMainCdRenderer.sharedMaterial = mat;
            cdLid.SetOpen(true);
            currentPreviewTapeName = info.tape_name;
            if (playAudio)
                PlayTapeAudio(info.wav_resource_name);
        }

        private static Tape_Info GetDefaultTapeInfo()
        {
            var tapeInfos = Dispatch_Manager._instance?._dispatch_configuration_so?._dispatch_config?.tape_info_list;
            return tapeInfos != null && tapeInfos.Count > 0 ? tapeInfos[0] : null;
        }

        private static void PlayTapeAudio(string wavResourceName)
        {
            if (string.IsNullOrEmpty(wavResourceName) || AudioManager.Instance == null)
                return;

            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlayAduioByResKey(wavResourceName, 0.5f, 0.5f);
            EvtDsp.TriggerEvt<bool>(EvtNames.CD_Player_Playing, true);
        }

        private async Task RefreshCDDisplayAsync()
        {
            int version = ++cdDisplayLoadVersion;
            var config = Dispatch_Manager._instance?._dispatch_configuration_so?._dispatch_config;
            var tapeInfos = config?.tape_info_list;
            if (cdCarousel == null || tapeInfos == null || tapeInfos.Count == 0 || GameAssets.Instance == null)
                return;

            var orderedMaterials = new List<Material>(tapeInfos.Count);
            foreach (var tapeInfo in tapeInfos)
            {
                if (version != cdDisplayLoadVersion)
                    return;

                Material material = null;
                if (!string.IsNullOrEmpty(tapeInfo.mat_resource_name))
                    material = await GameAssets.Instance.LoadAsycByKey<Material>(tapeInfo.mat_resource_name);

                orderedMaterials.Add(material);
            }

            if (version != cdDisplayLoadVersion)
                return;

            cdDisplayMaterials = orderedMaterials;
            cdCarousel.ConfigureOrderedDisplay(orderedMaterials, PlayCDAtOrderedIndex);
            BindCdClickTargets();
        }

        private void BindCdClickTargets()
        {
            foreach (var model in cdCarousel.cdModels)
            {
                if (model == null)
                    continue;

                var target = model.GetComponent<DispatchCdClickTarget>();
                if (target == null)
                    target = model.gameObject.AddComponent<DispatchCdClickTarget>();

                target.carousel = cdCarousel;
                target.index = model.GetComponent<CDItem>().index;
            }
        }

        private void PlayCDAtOrderedIndex(int orderedIndex)
        {
            var tapeInfos = Dispatch_Manager._instance?._dispatch_configuration_so?._dispatch_config?.tape_info_list;
            if (tapeInfos == null || tapeInfos.Count == 0)
                return;

            int index = ((orderedIndex % tapeInfos.Count) + tapeInfos.Count) % tapeInfos.Count;
            var tapeInfo = tapeInfos[index];
            string wavResourceName = tapeInfos[index].wav_resource_name;
            if (string.IsNullOrEmpty(wavResourceName) || AudioManager.Instance == null)
                return;

            if (cdPlayerMainCdRenderer != null && cdDisplayMaterials != null &&
                index < cdDisplayMaterials.Count && cdDisplayMaterials[index] != null)
            {
                cdPlayerMainCdRenderer.gameObject.SetActive(true);
                cdPlayerMainCdRenderer.sharedMaterial = cdDisplayMaterials[index];
            }

            currentPreviewTapeName = tapeInfo.tape_name;
            PlayTapeAudio(wavResourceName);
            EvtDsp.TriggerEvt<string>(EvtNames.DispatchDesk_Select_Tape, tapeInfo.tape_name);
        }

        public void ClearDeskModels()
        {
            foodLoadVersion++;
            snackLoadVersion++;
            foodRoot?.GetComponent<FoodSwitcher>()?.CancelModelSwitch();
            snackRoot?.GetComponent<FoodSwitcher>()?.CancelModelSwitch();
            ClearChildren(foodRoot);
            ClearChildren(snackRoot);
        }

        private void ShowDefaultModels()
        {
            ShowFood(null);
            ShowSnack(null);
        }

        private void ShowDispatchItemPrefab(Transform root, string prefabName, string itemName, string itemType, bool animate)
        {
            var prefab = Resources.Load<GameObject>($"Prefabs/DispatchItem/{prefabName}");
            if (prefab == null)
            {
                Debug.LogError($"[DispatchDeskController] Dispatch item prefab missing: {prefabName}");
                return;
            }

            void ReplaceModel()
            {
                if (clearRootsBeforeInstantiate)
                    ClearChildren(root);

                var instance = Instantiate(prefab, root, false);
                instance.name = prefab.name;

                if (!string.IsNullOrEmpty(itemName) && IsDefaultPrefab(prefabName))
                    Debug.LogWarning($"[DispatchDeskController] No dedicated {itemType} prefab for '{itemName}', using {prefabName}.");
            }

            var switcher = root != null ? root.GetComponent<FoodSwitcher>() : null;
            if (animate && switcher != null)
                switcher.SwitchModel(ReplaceModel);
            else
                ReplaceModel();
        }

        private static bool ShouldAnimateModelSwap(Transform root, string prefabName)
        {
            return root != null && root.childCount > 0 && root.GetChild(root.childCount - 1).name != prefabName;
        }

        private void BindInput()
        {
            if (InputManager.Instance == null)
                return;

            InputManager.Instance.OnDragBegin -= OnDragBegin;
            InputManager.Instance.OnSingleDrag -= OnSingleDrag;
            InputManager.Instance.OnDragRelease -= OnDragRelease;
            InputManager.Instance.OnDragBegin += OnDragBegin;
            InputManager.Instance.OnSingleDrag += OnSingleDrag;
            InputManager.Instance.OnDragRelease += OnDragRelease;
        }

        private void UnbindInput()
        {
            if (InputManager.Instance == null)
                return;

            InputManager.Instance.OnDragBegin -= OnDragBegin;
            InputManager.Instance.OnSingleDrag -= OnSingleDrag;
            InputManager.Instance.OnDragRelease -= OnDragRelease;
        }

        private void OnDragBegin(Vector2 screenPosition)
        {
            if (currentProcedure == Procedure_Dispatch.SelectBag || IsItemWarehouseOpen())
                return;

            swipeStartPosition = screenPosition;
            isTrackingSwipe = true;
            isTrackingCdGridSwipe = pageController != null && pageController.pageProgress >= 0.5f &&
                                    cdCarousel != null && cdCarousel.isGridLayout;
        }

        private void OnSingleDrag(Vector2 delta, Vector2 screenPosition)
        {
            if (isTrackingCdGridSwipe)
                cdCarousel.DragGrid(delta.x);
        }

        private void OnDragRelease(Vector2 screenPosition)
        {
            if (!isTrackingSwipe)
                return;

            isTrackingSwipe = false;
            if (isTrackingCdGridSwipe)
            {
                isTrackingCdGridSwipe = false;
                cdCarousel.EndGridDrag();
                return;
            }

            Vector2 delta = screenPosition - swipeStartPosition;
            if (delta.magnitude < swipeThreshold)
                return;

            float absX = Mathf.Abs(delta.x);
            float absY = Mathf.Abs(delta.y);
            if (absX > absY * swipeDirectionDominance)
            {
                if (delta.x < 0f)
                    SwitchToRightPage();
                else
                    SwitchToLeftPage();
            }
            else if (pageController != null && pageController.pageProgress >= 0.5f &&
                     absY > absX * swipeDirectionDominance)
            {
                cdCarousel?.SnapByStep(delta.y > 0f ? 1 : -1);
            }
        }

        private static bool IsItemWarehouseOpen()
        {
            return GameObject.Find("GameItem_Scroller") != null;
        }

        private static string ResolveFoodPrefabName(string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
                return "bread";

            var info = Dispatch_Manager.GetFoodInfo(itemName);
            switch (info?.model_resource_name)
            {
                case "PREFAB_ODENFINAL":
                    return "Food_0";
                case "PREFAB_FOOD3_FLOSS_RICE_BALL":
                    return "Food_1";
                case "PREFAB_FOOD2_DEEP_SEA_FISH_ONIGIRI":
                    return "Food_2";
                default:
                    return "bread";
            }
        }

        private static string ResolveSnackPrefabName(string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
                return "Default_leaf";

            var info = Dispatch_Manager.GetSnackInfo(itemName);
            switch (info?.model_resource_name)
            {
                case "PREFAB_ITEM1_BEAR_KEYRING":
                    return "item1";
                case "PREFAB_GAMECONSOLE2":
                    return "gameBoy_2";
                default:
                    return "Default_leaf";
            }
        }

        private static bool IsDefaultPrefab(string prefabName)
        {
            return prefabName == "bread" || prefabName == "Default_leaf";
        }

        private static void DisableLegacySwitcher(Transform root)
        {
            if (root == null)
                return;

            var switcher = root.GetComponent<FoodSwitcher>();
            if (switcher != null)
                switcher.enabled = false;
        }

        private static void ClearChildren(Transform root)
        {
            if (root == null)
                return;

            for (int i = root.childCount - 1; i >= 0; i--)
                Destroy(root.GetChild(i).gameObject);
        }
    }
}
