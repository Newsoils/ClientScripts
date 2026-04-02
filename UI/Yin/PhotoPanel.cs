using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.UI;
using UnityEngine;
using UnityEngine.UI;

public class PhotoPanel : UIPanelBase
{
    public GameObject panelObj;
    public RawImage photo1;
    public RawImage photo2;
    public Button photo1Button;
    public Button photo2Button;
    public Button lastPageButton;
    public Button nextPageButton;
    public Button exitButton;
    private int pageIndex;

    [Header("放大缩小")]
    private Vector2 originPos;
    public GameObject transObj;
    public float scaleSpeed;
    public float moveSpeed;

    private void Start()
    {
        lastPageButton.onClick.AddListener(LastPage);
        nextPageButton.onClick.AddListener(NextPage);
        exitButton.onClick.AddListener(ClosePanel);
        photo1Button.onClick.AddListener(() => ShowPhoto(pageIndex * 2));
        photo2Button.onClick.AddListener(() => ShowPhoto(pageIndex * 2 + 1));
        originPos = transObj.transform.position;
        InitPage();
    }
    private void Update()
    {
        CheckMove();
        CheckScale();
    }
    private void InitPage()
    {
        pageIndex = 0;
        RefreshPanel();
        ResetObj();
    }
    private void ShowPhoto(int index)
    {
        PhotoData data = PhotoManager.Instance.localData[index];
        UIManager.Instance.OpenPanel<ShowPhotoPanel>(data);
    }
    private void LastPage()
    {
        pageIndex--;
        RefreshPanel();
    }
    private void NextPage()
    {
        pageIndex++;
        RefreshPanel();
    }
    private void RefreshPanel()
    {
        photo1.gameObject.SetActive(true);
        photo2.gameObject.SetActive(true);
        if(PhotoManager.Instance.TryGetPhotoByIndex(pageIndex * 2,out Texture2D photo1Tex))
        {
            photo1.texture = photo1Tex;
        }
        else
        {
            photo1.gameObject.SetActive(false);
        }
        if(PhotoManager.Instance.TryGetPhotoByIndex(pageIndex * 2 + 1, out Texture2D photo2Tex))
        {
            photo2.texture = photo2Tex;
        }
        else
        {
            photo2.gameObject.SetActive(false);
        }
    }

    private void CheckMove()
    {
        transObj.transform.position += (Vector3)InputManager.Instance.SingleDragDelta * moveSpeed;
    }
    private void CheckScale()
    {
        transObj.transform.localScale *= InputManager.Instance.PinchRatio;
    }
    private void ResetObj()
    {
        transObj.transform.localScale = new Vector3(1, 1, 1);
        transObj.transform.position = originPos;
    }
    public override void ClosePanel()
    {
        panelObj.SetActive(false);
    }

    public override void OpenPanel(params object[] data)
    {
        panelObj.SetActive(true);
        InitPage();
    }
}
