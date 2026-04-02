using System;
using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.NewFrame.UI;
using CLIP.Project_Mouse.UI;
using UnityEngine;
using UnityEngine.UI;

public class PhonePanel : UIPanelBase
{

    public Button btn_Exit;

    public Button BackToRoomButton;
    public Button ChangeClothButton;
    public Button PersonalBriefButton;
    public Button DispatchButton;
    public Button ChatButton;
    public Button IllustrateButton;
    public Button EmailButton;
    public Button PhotoButton;
    public Button ShopButton;

    public Transform root;



    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        btn_Exit.onClick.AddListener(() => ClosePhone());

        ChangeClothButton.onClick.AddListener(
            () =>
            ClosePhone(() => UIManager.Instance.OpenPanel<ChangeClothPanel>())
            );
        BackToRoomButton.onClick.AddListener(BackToRoom);

        PersonalBriefButton.onClick.AddListener(
            () =>
            {
                //PersonalBriefCanvasInteractManager.Instance.OpenPersonalBrief(); 
                ClosePhone(() =>
               UIManager.Instance.GetPanel<PersonalBriefPanel>()?.OpenPanel());
            });

        DispatchButton.onClick.AddListener(Dispatch);

        ChatButton.onClick.AddListener(
            () =>
            {
                ClosePhone(() =>
                UIManager.Instance.GetPanel<SocialPanel>()?.OpenPanel());
            });
        IllustrateButton.onClick.AddListener(
            () =>
            {
                //IllustrateInteractManager.Instance.OpenIllustrate();
                ClosePhone(() =>
                UIManager.Instance.OpenPanel<IllustratePanel>());
            });
        EmailButton.onClick.AddListener(
            () =>
            {
                //EmailCanvasInteracterManager.Instance.OpenEmailCanvas();
                ClosePhone(() =>
                UIManager.Instance.OpenPanel<EmailPanel>());
            }
            );
        PhotoButton.onClick.AddListener(() =>
        {
            //  DispatchPhotoPanel.Instance.OpenDispatchPhotoCanvas();
            //  });
            ClosePhone(() => UIManager.Instance.OpenPanel<DispatchPhotoPanel>());
        });

        ShopButton.onClick.AddListener(
            () =>
            {
                //ShoppingCanvasInteractManager.Instance.OpenShop();
                ClosePhone(() => UIManager.Instance.OpenPanel<ShoppingPanel>());
            });

    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        btn_Exit.onClick.RemoveAllListeners();

        ChangeClothButton.onClick.RemoveAllListeners();
        BackToRoomButton.onClick.RemoveAllListeners();
        PersonalBriefButton.onClick.RemoveAllListeners();
        DispatchButton.onClick.RemoveAllListeners();
        ChatButton.onClick.RemoveAllListeners();
        IllustrateButton.onClick.RemoveAllListeners();
        EmailButton.onClick.RemoveAllListeners();
        PhotoButton.onClick.RemoveAllListeners();
        ShopButton.onClick.RemoveAllListeners();
    }


    public void OpenPanel()
    {
        root?.gameObject.SetActive(true);
    }

    public override void OpenPanel(object[] objects)
    {
        root?.gameObject.SetActive(true);
    }

    public override void ClosePanel()
    {
        root?.gameObject.SetActive(false);
    }

    public override void UpdatePanel(params object[] data)
    {
    }


    public void OpenPhone()
    {
    }


    public void ClosePhone(Action afterClose = null)
    {
        PhoneButton.Instance.ClosePhone(afterClose);
    }

    public void BackToRoom()
    {
        if (SceneLoadHelper.IsMainScene)
        {
            ClosePhone();
        }
        else
        {
            ClosePhone(() => SceneLoadHelper.Load_MainScene());
            MainPanel.OpenMainFuncP();
        }
    }

    public void Dispatch()
    {
        if (SceneLoadHelper.IsDispatchScene)
        {
            ClosePhone();
        }
        else
        {
            ClosePhone(() => SceneLoadHelper.Load_DispatchScene());
            MainPanel.CloseMainFuncP();
        }
    }

}
