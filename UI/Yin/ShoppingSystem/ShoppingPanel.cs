using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Network;
using Cmd;
using UnityEngine;
using UnityEngine.UI;


namespace CLIP.Project_Mouse.UI
{
    public class ShoppingPanel : UIPanelBase
    {
        [Header("Render Texture")]
        public RenderTexture characterRT;
        public RenderTexture furnitureRT;

        [Header("UI")]
        public GameObject obj;

        [Header("选择杂志")]
        public Button dailyMagazine;
        public Button limitMagazine;
        public GameObject chooseMagazine;

        public Button exitButton;

        [Header("面板")]
        public DailyMagazinePanel dailyMagazinePanel;
        public LimitMagazineUI limitMagazineUI;

        private GameObject dailyMagazineObj;
        private GameObject limitMagazineObj;

        private GetAllShopInfoRes _cachedShopData;

        private void Start()
        {
            characterRT = RenderTextureCompatUtility.EnsureCompatible(characterRT, "ShoppingCharacterRT");
            furnitureRT = RenderTextureCompatUtility.EnsureCompatible(furnitureRT, "ShoppingFurnitureRT");

            dailyMagazineObj = dailyMagazinePanel.gameObject;
            limitMagazineObj = limitMagazineUI.gameObject;

            OpenChooseMagazine();

            EvtDsp.AddEvt<GetAllShopInfoRes>(EvtNames.OnShopInfoReceived, OnGetAllShopInfoRes);
            EvtDsp.AddEvt<ManualRefreshShopRes>(EvtNames.OnManualRefreshShopReceived, OnManualRefreshShopRes);
            EvtDsp.AddEvt<BuyGoodsRes>(EvtNames.OnBuyGoodsReceived, OnBuyGoodsRes);
            EvtDsp.AddEvt(EvtNames.OpenShoppingPanel, OnOpenShoppingPanelEvent);

            dailyMagazine.onClick.AddListener(OpenOrChooseDailyMagazine);
            limitMagazine.onClick.AddListener(OpenOrChooseLimitMagazine);

            exitButton.onClick.AddListener(ClosePanel);
        }


        public override void OnDestroy()
        {
            base.OnDestroy();
            EvtDsp.RemoveEvt<GetAllShopInfoRes>(EvtNames.OnShopInfoReceived, OnGetAllShopInfoRes);
            EvtDsp.RemoveEvt<ManualRefreshShopRes>(EvtNames.OnManualRefreshShopReceived, OnManualRefreshShopRes);
            EvtDsp.RemoveEvt<BuyGoodsRes>(EvtNames.OnBuyGoodsReceived, OnBuyGoodsRes);
            EvtDsp.RemoveEvt(EvtNames.OpenShoppingPanel, OnOpenShoppingPanelEvent);
            dailyMagazine.onClick.RemoveListener(OpenOrChooseDailyMagazine);
            limitMagazine.onClick.RemoveListener(OpenOrChooseLimitMagazine);
            exitButton.onClick.RemoveAllListeners();
        }

        #region 接口方法实现

        public override void OpenPanel(params object[] data)
        {
            obj.SetActive(true);
            RequestShopData();
            GuideManager.Instance.CheckShopFinished();
            EvtDsp.TriggerEvt(EvtNames.OnShoppingPanelOpen);
            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        }

        public override void ClosePanel()
        {
            obj.SetActive(false);
            EvtDsp.TriggerEvt(EvtNames.OnShoppingPanelClose);
            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        }

        private void OnOpenShoppingPanelEvent()
        {
            OpenPanel();
        }

        #endregion

        #region 服务器数据

        public void RequestShopData()
        {
            var req = new GetAllShopInfoReq();
            for (long shopID = 1; shopID <= 10; shopID++)
                req.ShopIDs.Add(shopID);
            NetWork_Center_WSS.SendMsg(req);
        }

        public void OnGetAllShopInfoRes(GetAllShopInfoRes res)
        {
            _cachedShopData = res;
            ApplyShopList(res.Shops);
        }

        private void OnManualRefreshShopRes(ManualRefreshShopRes res)
        {
            if (res == null)
                return;

            var roleInfo = Global_Game_Manager.Instance.get_current_player_role_info();
            if (roleInfo != null)
                roleInfo.TodayShopRefreshTimes = res.RefreshTimes;

            ApplyShopList(res.Shop);
            PromptMessage.Instance.ShowUpPrompt("刷新成功");
        }

        private void ApplyShopList(IEnumerable<UserShop> shops)
        {
            if (shops == null)
                return;

            dailyMagazinePanel.ApplyServerShopData(shops);

            UserShop limitedClothShop1 = null;
            UserShop limitedClothShop2 = null;
            bool hasLimitedShop = false;
            foreach (var shop in shops)
            {
                if (shop.ShopID == 9)
                {
                    limitedClothShop1 = shop;
                    hasLimitedShop = true;
                }
                else if (shop.ShopID == 10)
                {
                    limitedClothShop2 = shop;
                    hasLimitedShop = true;
                }
            }

            if (hasLimitedShop)
                limitMagazineUI.ApplyServerShopData(limitedClothShop1, limitedClothShop2);
        }

        public GetAllShopInfoRes CachedShopData => _cachedShopData;

        private void OnBuyGoodsRes(BuyGoodsRes res)
        {
            PromptMessage.Instance.ShowUpPrompt("购买成功");
            RequestShopData();
        }

        #endregion

        public void ChangeCharacterCloth(string clothName)
        {
            CharacterClothesManager.Instance.ChangeClothes(CharacterType.Shopping, clothName);
        }

        public void InitCharacterCloth()
        {
            CharacterClothesManager.Instance.InitCharacter(CharacterType.Shopping);
        }

        public void OpenOrChooseDailyMagazine()
        {
            if (BringButtonToFront(dailyMagazine))
            {
                dailyMagazineObj.SetActive(true);
                chooseMagazine.SetActive(false);
                dailyMagazinePanel.OnTabButtonClick(DailyShoppingTab.Clothes);
                InitCharacterCloth();
            }
        }

        public void OpenOrChooseLimitMagazine()
        {
            if (BringButtonToFront(limitMagazine))
            {
                chooseMagazine.SetActive(false);
                limitMagazineUI.OpenPanel();
            }
        }

        public void OpenChooseMagazine()
        {
            chooseMagazine.SetActive(true);
            dailyMagazineObj.SetActive(false);
            limitMagazineObj.SetActive(false);
        }

        public void ChooseMagazineOrCategory(Button button)
        {
            BringButtonToFront(button);
        }

        public bool BringButtonToFront(Button button)
        {
            RectTransform buttonTransform = button.GetComponent<RectTransform>();
            Transform parentTransform = buttonTransform.parent;

            if (buttonTransform.GetSiblingIndex() == parentTransform.childCount - 1)
                return true;

            buttonTransform.SetSiblingIndex(parentTransform.childCount - 1);
            return false;
        }
    }
}
