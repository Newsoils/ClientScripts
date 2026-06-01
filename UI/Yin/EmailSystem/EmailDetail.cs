using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
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

            foreach (Transform child in awardRoot)
            {
                Destroy(child.gameObject);
            }
            foreach (var item in record.item_list)
            {
                var obj = Instantiate(awardUnitPrefab, awardRoot);

                var itemInfo = Global_Inventory_Manager.GetItemInfo(item.item_name);
                obj.GetComponent<EmailItem>().InitItem(itemInfo, item.item_quantity);
            }
        }

        public void Receive()
        {
            var emailPanel = UIManager.Instance.GetPanel<EmailPanel>();

            emailPanel.ReceiveAwards(new List<Mail_Record> { record });
            emailPanel.UploadAfterOpenGifts();
            receive.interactable = false;

            // 重新显示一键领取按钮
            emailPanel.ShowGetAllRewardsButton();

            // 隐藏ReceiveOrDelete按钮
            emailPanel.HideReceiveOrDelete();
        }


        public void Delete()
        {
            if (record.item_list.Count > 0 && !record.isGetReward)
            {
                Receive();
            }

            List<ulong> readId = new List<ulong>
                    {
                        record.mail_id
                    };
            Email_And_Announcement_Manager.instance.on_delete_mail(readId);
            var panel = UIManager.Instance.OpenPanel<EmailPanel>();

            // 重新显示一键领取按钮
            panel.ShowGetAllRewardsButton();

            // 隐藏ReceiveOrDelete按钮
            panel.HideReceiveOrDelete();
        }
    }

}