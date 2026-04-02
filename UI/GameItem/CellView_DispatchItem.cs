using System;
using System.Collections.Generic;
using CLIP.Project_Mouse;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.UI;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;


public class CellView_DispatchItem : EnhancedScrollerCellView
{
    public TMP_Text gameItem_Name;
    public Button button;
    public Image gameItem_Icon;
    public TMP_Text gameItem_Count;
    public Button detailButton;

    // 在管理这些 CellView 的地方存储 List<Sprite>，在这里也可以
    // 然后添加该 CellView 与稀有度相关的 UI 组件的引用

    public void OnDestory()
    {
        button.onClick.RemoveAllListeners();
        detailButton.onClick.RemoveAllListeners();
    }

    public void SetData(ScrollData_GameItem data,Action<ScrollData_GameItem> clickEvent = null)
    {
        string name = data.name;
        int count = data.count;
        int id = data.id;
        // 稀有度，用于判断应该使用什么颜色的框来表示
        Enum_RarityType rarity = data.rarity;

        gameItem_Name.text = name;
        gameItem_Count.text = count.ToString();

        // 在此处进行替换

        var inventoryDB = Global_Inventory_Manager.GameItem_DB;
        var url =  inventoryDB.Find(item => item.item_id == id).res_url;
        if (!string.IsNullOrEmpty(url))
        {
            var _image_url_data = url.Split("#");
            if (_image_url_data.Length == 2)
            {
                RM.load_sub_sprite(_image_url_data[0], _image_url_data[1], (sp)=> gameItem_Icon.sprite = sp);
            }
            else
            {
                RM.load_sprite_async(_image_url_data[0], sp=> gameItem_Icon.sprite = sp);
            }
        }
        if( clickEvent != null) SetClickEvent(()=> clickEvent?.Invoke(data)); 
        

        SetDetailClickEvent(()=>
        {
            var item = Global_Inventory_Manager.GetItem(id);
            ItemDescriptionPanel.Instance.OpenPanel(item);
        });
    }

    public void SetClickEvent(Action action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(()=> action?.Invoke());
    }

    public void SetDetailClickEvent(Action action)
    {
        detailButton.onClick.RemoveAllListeners();
        detailButton.onClick.AddListener(()=> action?.Invoke());
    }
}
