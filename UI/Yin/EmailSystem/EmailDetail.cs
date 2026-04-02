using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.NewFrame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class EmailDetail : MonoBehaviour
            {
                public Mail_Record record;

                public TMP_Text titleText;
                public TMP_Text contentText;
                public Button receive;

                public Transform awardRoot;
                public GameObject awardUnitPrefab;

                public void InitEmailDatail(Mail_Record record)
                {
                    this.record = record;
                    titleText.text = record.mail_title;
                    contentText.text = record.mail_text;

                    if (record.item_list.Count > 0 && !record.isGetReward)
                    {
                        receive.interactable = true;
                    }
                    else
                    {
                        receive.interactable = false;
                    }

                    foreach(Transform child in awardRoot)
                    {
                        Destroy(child.gameObject);
                    }
                    foreach(var item in record.item_list)
                    {
                        var obj = Instantiate(awardUnitPrefab, awardRoot);
                        var itemInfo = Global_Inventory_Manager.GetItemInfo(item.item_name);
                        obj.GetComponent<EmailItem>().InitItem(itemInfo, item.item_quantity);
                    }
                }

                public void Receive()
                {
                    //EmailCanvasInteracterManager.Instance.ReceiveAwards(new List<Mail_Record> { record });
                    //EmailCanvasInteracterManager.Instance.UploadAfterOpenGifts();
                    //EmailCanvasInteracterManager.Instance.generalInteractionEventHub._invoke_on_refresh_gift();
                    UIManager.Instance.GetPanel<EmailPanel>().ReceiveAwards(new List<Mail_Record> { record });
                    UIManager.Instance.GetPanel<EmailPanel>().UploadAfterOpenGifts();
                    UIManager.Instance.GetPanel<EmailPanel>().generalInteractionEventHub._invoke_on_refresh_gift();
                    receive.interactable = false;
                    record.isGetReward = true;

                    // 重新显示一键领取按钮
                    UIManager.Instance.GetPanel<EmailPanel>().ShowGetAllRewardsButton();

                    // 隐藏ReceiveOrDelete按钮
                    UIManager.Instance.GetPanel<EmailPanel>().HideReceiveOrDelete();
                }
                

                public void Delete()
                {
                    if (record.item_list.Count > 0 && !record.isGetReward)
                    {
                        Receive();
                    }

                    List<int> readId = new List<int>
                    {
                        record.mail_id
                    };
                    Email_And_Announcement_Manager.instance.on_delete_mail(readId);
                    //EmailCanvasInteracterManager.Instance.OpenEmailCanvas();
                    UIManager.Instance.OpenPanel<EmailPanel>();

                    // 重新显示一键领取按钮
                    UIManager.Instance.GetPanel<EmailPanel>().ShowGetAllRewardsButton();

                    // 隐藏ReceiveOrDelete按钮
                    UIManager.Instance.GetPanel<EmailPanel>().HideReceiveOrDelete();
                }
            }
        }
    }
}