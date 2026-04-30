using System;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using UnityEngine;
using UnityEngine.UI;
using RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;
using System.Linq;

public class SecondCellView_CD : MonoBehaviour
{
    public Image cd_Icon;
    public Image highLight;
    public ZButton zButton;
    
    public int DataIndex { get; private set; }

    private string _currentDataName;

    private void Start()
    {
        //高亮放大一下
        zButton.enableHoverScale = true;
        //zButton.onClick.RemoveAllListeners();

        EvtDsp.AddEvt<Item_Type, string>(EvtNames.Dispatch_Change_Item, OnDispatchItemChanged);
        EvtDsp.AddEvt<int>(EvtNames.Dispatch_Refresh_Model, OnDispatchRefreshModel);
        RefreshHighLight();
    }

    private void OnDestroy()
    {
        EvtDsp.RemoveEvt<Item_Type, string>(EvtNames.Dispatch_Change_Item, OnDispatchItemChanged);
        EvtDsp.RemoveEvt<int>(EvtNames.Dispatch_Refresh_Model, OnDispatchRefreshModel);
    }

    private void OnDispatchItemChanged(Item_Type type, string name)
    {
        if (type != Item_Type.Tape) return;
        RefreshHighLight();
    }

    private void OnDispatchRefreshModel(int bagIndex)
    {
        RefreshHighLight();
    }

    // 高亮条件：此 cell 对应的 CD 是当前正在编辑的背包已装填的 tape
    private void RefreshHighLight()
    {
        if (highLight == null) return;

        if (string.IsNullOrEmpty(_currentDataName))
        {
            highLight.gameObject.SetActive(false);
            return;
        }

        var dispatchMgr = Dispatch_Manager._instance;
        if (dispatchMgr == null || dispatchMgr.dispatch_Bags == null)
        {
            highLight.gameObject.SetActive(false);
            return;
        }

        int bagIndex = -1;
        var panel = UIManager.Instance != null ? UIManager.Instance.GetPanel<DispatchPanel>() : null;
        if (panel != null)
        {
            bagIndex = panel.CurrentBagIndex;
        }

        if (bagIndex < 0 || bagIndex >= dispatchMgr.dispatch_Bags.Count)
        {
            highLight.gameObject.SetActive(false);
            return;
        }

        var info = panel != null ? panel.GetPreviewBagInfo(bagIndex) : dispatchMgr.dispatch_Bags[bagIndex];
        bool isSelected = info != null
                          && !string.IsNullOrEmpty(info.tapeName)
                          && info.tapeName == _currentDataName;
        highLight.gameObject.SetActive(isSelected);
    }

    public void SetData(int dataIndex, ScrollData_GameItem data, Action action)
    {
        DataIndex = dataIndex;
        if(data == null)
        {
            cd_Icon. gameObject.SetActive(false);
            zButton.onClick.RemoveAllListeners();
            _currentDataName = null;
            RefreshHighLight();
            return;
        }

        _currentDataName = data.name;

        var id = data.id;
        var inventoryDB = Global_Inventory_Manager.GameItem_DB;
        var url = inventoryDB.Find(item => item.item_id == id).res_url;
        if (!string.IsNullOrEmpty(url))
        {
            var _image_url_data = url.Split("#");
            if (_image_url_data.Length == 2)
            {
                RM.load_sub_sprite(_image_url_data[0], _image_url_data[1], (sp) => cd_Icon.sprite = sp);
            }
            else
            {
                RM.load_sprite_async(_image_url_data[0], sp => cd_Icon.sprite = sp);
            }
        }

        var dispatch_config = Dispatch_Manager._instance._dispatch_configuration_so._dispatch_config;
        
        var wav_name = dispatch_config.tape_info_list.FirstOrDefault(t => t.item_id == id)?.wav_resource_name;

        if (wav_name != null && action != null)
        {
            SetClickEvent(action);
        }

        RefreshHighLight();
        //SetClickEvent(() =>
        //{
        //    UIManager.Instance.GetPanel<DispatchPanel>().SetDispatchItem(Item_Type.Tape, data.name);
        //    if(!string.IsNullOrEmpty(wav_name)) AudioManager.Instance.PlayAduioByResKey(wav_name, 0.5f, 0.5f);
        //    EvtDsp.TriggerEvt(EvtNames.Dispatch_Change_Item, Item_Type.Tape, data.name);
        //});

    }


    private Action _clickAction;

    public void SetClickEvent(Action action)
    {
        _clickAction = action;
        // 只绑定自己的逻辑，不再 RemoveAllListeners
        //zButton.onClick.RemoveListener(OnButtonClick);
        //zButton.onClick.AddListener(OnButtonClick);

        zButton.onPointDown.RemoveListener(OnButtonClick);
        zButton.onPointDown.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        _clickAction?.Invoke();
    }
    //public void SetClickEvent(Action action)
    //{
    //    zButton.onClick.RemoveAllListeners();
    //    zButton.onClick.AddListener(() => action?.Invoke());

    //    zButton.onPointDown.RemoveAllListeners();
    //    zButton.onPointDown.AddListener(() => action.Invoke());


    //}


}
