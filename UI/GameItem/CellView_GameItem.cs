using System;
using System.IO;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellView_GameItem : MonoBehaviour
{
    public GameObject obj;
    public GameObject newRedDot;

    public TMP_Text gameItem_Name;
    public ZButton button;
    public Image gameItemBg_Icon;
    public Image gameItem_Icon;
    public TMP_Text gameItem_Count;
    public Button detailButton;

    private string _currentIconUrl;

    public void OnDestory()
    {
        button.onClick.RemoveAllListeners();
        button.onPointDown.RemoveAllListeners();
        button.onLongPress.RemoveAllListeners();
        detailButton.onClick.RemoveAllListeners();
    }

    public void SetData(ScrollData_GameItem data, 
    Action<ScrollData_GameItem> clickEvent = null, 
    Action<ScrollData_GameItem> pointDownEvent = null,
     Action<ScrollData_GameItem> longPressEvent = null)
    {
        if (data == null)
        {
            obj.SetActive(false);
            if (newRedDot != null) newRedDot.SetActive(false);
            return;
        }
        else
        {
            obj.SetActive(true);
        }

        if (newRedDot != null) newRedDot.SetActive(data.isNew);

        string name = data.name;
        int count = data.count;
        int id = data.id;
        gameObject.name = data.name;

        gameItem_Name.text = name;
        gameItem_Count.text = count.ToString();

        gameItem_Icon.sprite = null; // 先清空，避免残影

        var mImage = gameItem_Icon;
        var url = Global_Inventory_Manager.GetItemInfo(id).res_url;
        var rarity = (int)Global_Inventory_Manager.GetItemInfo(id).rarity;


        _currentIconUrl = url; // 记录最新请求的 URL
        if (!string.IsNullOrEmpty(url))
        {
            string spritePath = Path.Combine("Textures\\UI\\House", "rarity_" + rarity);
            Debug.Log(spritePath);
            LoadSpriteInVoid(spritePath);

            LoadItemIcon(url, mImage);
        }
        if (clickEvent != null) SetClickEvent(() => clickEvent?.Invoke(data));

        if (pointDownEvent != null) SetPointDownEvent(() => pointDownEvent?.Invoke(data));

        if (longPressEvent != null) SetLongPressEvent(() => longPressEvent?.Invoke(data));

        SetDetailClickEvent(() =>
        {
            var item = Global_Inventory_Manager.GetItem(id);
            UIManager.Instance.OpenPanel<ItemDescriptionPanel>(item);
        });
    }
    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
        button.onPointDown.RemoveAllListeners();
        button.onLongPress.RemoveAllListeners();
        detailButton.onClick.RemoveAllListeners();
        gameItem_Icon.sprite = null;
    }

    // 2. 异步方法（async void 适配 Unity 回调）
    private void LoadItemIcon(string url, Image targetImage)
    {
        GameAssets.LoadSpriteByUrl(url, sp =>
        {
            if (_currentIconUrl == url && targetImage != null)
                targetImage.sprite = sp;
        });
    }

    private async void LoadSpriteInVoid(string path)
    {
        try
        {
            // 调用 Task 并 await 等待完成
            Sprite loadedSprite = await GameAssets.LoadAsync<Sprite>(path);

            // 等待完成后，在主线程操作 UI（安全）
            if (gameItemBg_Icon != null && loadedSprite != null)
            {
                gameItemBg_Icon.sprite = loadedSprite;
            }
        }
        catch (System.Exception e)
        {
            Log.Error("加载失败：" + e.Message);
        }
    }


    public void SetClickEvent(Action action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => action?.Invoke());
    }

    public void SetPointDownEvent(Action action)
    {
        button.onPointDown.RemoveAllListeners();
        button.onPointDown.AddListener(() => action?.Invoke());
    }

    public void SetLongPressEvent(Action action)
    {
        button.onLongPress.RemoveAllListeners();
        button.onLongPress.AddListener(() => action?.Invoke());
    }

    public void SetDetailClickEvent(Action action)
    {
        detailButton.onClick.RemoveAllListeners();
        detailButton.onClick.AddListener(() => action?.Invoke());
    }



}
