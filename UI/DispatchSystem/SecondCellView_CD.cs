using System;
using System.Collections;
using System.Xml.Serialization;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.LYC.UI;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using CLIP.Project_Mouse.ENUM;
using RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;
using System.Linq;

public class SecondCellView_CD : MonoBehaviour
{
    public Image cd_Icon;
    public ZButton zButton;
    

    public int DataIndex { get; private set; }

    private void Start()
    {
        //高亮放大一下
        zButton.enableHoverScale = true;
        //zButton.onClick.RemoveAllListeners();
    }

    public void SetData(int dataIndex, ScrollData_GameItem data)
    {
        DataIndex = dataIndex;
        if(data == null)
        {
            cd_Icon. gameObject.SetActive(false);
            zButton.onClick.RemoveAllListeners();
            return;
        }

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

        SetClickEvent(() =>
        {
            UIManager.Instance.GetPanel<DispatchPanel>().SetDispatchItem(Item_Type.Tape, data.name);
            if(!string.IsNullOrEmpty(wav_name)) AudioManager.Instance.PlayAduioByResKey(wav_name, 0.5f, 0.5f);
        });

    }

   

    public void SetClickEvent(Action action)
    {
        zButton.onClick.RemoveAllListeners();
        zButton.onClick.AddListener(() => action?.Invoke());
    }


}
