using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.LYC.UI;
using CLIP.Project_Mouse.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.NewFrame.UI
{
    public class EmailPanel : UIPanelBase
    {
        [Header("UI")]
        public GameObject obj;
        public GameObject allEmailPanel;
        public GameObject emailDetailPanel;
        public GameObject awardPanel;

        public Button getAllRewardsButton;

        public GameObject receiveOrDelete;

        [Header("AllEmail")]
        public Transform emailRoot;
        public GameObject emailUnitPrefab;

        [Header("EmailDetail")]
        public EmailDetail emailDetail;

        [Header("Award")]
        public Transform awardRoot;
        public GameObject awardUnitPrefab;
        public int waitPhotoCount = 0;

        private void Start()
        {
            Email_And_Announcement_Manager.instance._on_refresh_mail.AddListener(InitAllEmail);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Email_And_Announcement_Manager.instance._on_refresh_mail.RemoveListener(InitAllEmail);
        }


        #region 接口方法实现

        // 打开邮箱面板
        public override void OpenPanel(params object[] data)
        {
            obj.SetActive(true);
            allEmailPanel.SetActive(true);
            emailDetailPanel.SetActive(false);
            awardPanel.SetActive(false);

            InitAllEmail();

            // 显示一键领取按钮
            ShowGetAllRewardsButton();

            // 隐藏ReceiveOrDelete按钮
            HideReceiveOrDelete();
        }

        // 关闭邮箱面板
        public override void ClosePanel()
        {
            obj.SetActive(false);
        }

        #endregion


        #region 内部方法

        // 初始化所有邮件
        private void InitAllEmail()
        {
            var mailList = Email_And_Announcement_Manager.instance._mail_record;
            int mailCount = 0;
            int childCount = emailRoot.childCount;

            for (int i = 0; i < mailList.Count; i++)
            {
                if (mailList[i].mail_state == "deleted")
                {
                    continue;
                }

                GameObject emailUnitObj;
                if (i < childCount)
                {
                    emailUnitObj = emailRoot.GetChild(mailCount).gameObject;
                    emailUnitObj.SetActive(true);
                }
                else
                {
                    emailUnitObj = Instantiate(emailUnitPrefab, emailRoot);
                }

                var emailUnit = emailUnitObj.GetComponent<EmailUnit>();
                if (emailUnit != null)
                {
                    emailUnit.InitEmailUnit(mailList[i]);
                }
                mailCount++;
            }

            for (int i = mailCount; i < emailRoot.childCount; i++)
            {
                emailRoot.GetChild(i).gameObject.SetActive(false);
            }

            // 然后刷新邮箱中 EmailUnit 的布局大小
            emailRoot.GetComponent<VerticalSpacer>().UpdateLayout();
        }

        #endregion

        // 外部挂载
        public void OpenPanel()
        {
            OpenPanel(null);
        }


        // 一键领取所有奖励
        public void ReceiveAllAwards()
        {
            var allMails = Email_And_Announcement_Manager.instance._mail_record;
            var toReceive = new List<Mail_Record>();
            List<int> mailId = new List<int>();
            foreach (var mail in allMails)
            {
                if (!mail.isGetReward && mail.item_list.Count > 0)
                {
                    toReceive.Add(mail);
                }

                if (mail.mail_state == "unread")
                {
                    mailId.Add(mail.mail_id);
                }
            }
            Email_And_Announcement_Manager.instance.on_read_mail(mailId);
            ReceiveAwards(toReceive);
            UploadAfterOpenGifts();
        }

        // 显示奖励面板
        public void ReceiveAwards(List<Mail_Record> mailRecords)
        {
            awardPanel.SetActive(true);
            var itemDb = Global_Inventory_Manager.GameItem_DB;

            var allAwards = new List<item_in_mail>();
            var presentRecords = Player_Social_Manager._instance._current_social_info._present_records;
            List<int> mailId = new List<int>();
            foreach (var mail in mailRecords)
            {
                allAwards.AddRange(mail.item_list);

                if (mail._send_present_msg_id_related != -1)
                {
                    presentRecords.RemoveAll(r => r._msg_id == mail._send_present_msg_id_related);
                }

                if (!mail.isGetReward)
                {
                    mailId.Add(mail.mail_id);
                }
            }
            Email_And_Announcement_Manager.instance.on_get_mail_reward(mailId);

            int awardCount = 0;
            int childCount = awardRoot.childCount;
            List<(string, int)> list = new List<(string, int)>();
            foreach (var item in allAwards)
            {
                GameObject awardObj;
                if (awardCount < childCount)
                {
                    awardObj = awardRoot.GetChild(awardCount).gameObject;
                    awardObj.SetActive(true);
                }
                else
                {
                    awardObj = Instantiate(awardUnitPrefab, awardRoot);
                }

                var awardUnit = awardObj.GetComponent<EmailItem>();
                if (awardUnit != null)
                {
                    var dbItem = itemDb.Find(db => db.name == item.item_name);
                    awardUnit.InitItem(dbItem, item.item_quantity);
                    list.Add((item.item_name, item.item_quantity));
                }
                awardCount++;
            }
            Global_Inventory_Manager.Change_Items_Count(list, "邮件");
            for (int i = awardCount; i < childCount; i++)
            {
                awardRoot.GetChild(i).gameObject.SetActive(false);
            }
        }

        // 奖励面板关闭后上传数据
        public void UploadAfterOpenGifts()
        {
            StartCoroutine(UploadAfterOpenGiftsCo());

            IEnumerator UploadAfterOpenGiftsCo()
            {
                Player_Social_Manager._instance.upload_present_records_to_server();

                yield return new WaitForSeconds(1f);

                Global_Inventory_Manager._instance.Send_inventory_to_server();
            }
        }

        // 打开邮件详情
        public void OpenEmailDetail(Mail_Record record)
        {
            allEmailPanel.SetActive(false);
            emailDetailPanel.SetActive(true);
            awardPanel.SetActive(false);

            emailDetail.InitEmailDatail(record);

            // 同时将一键领取按钮隐藏
            HideGetAllRewardsButton();

            // 并显示ReceiveOrDelete按钮
            ShowReceiveOrDelete();
        }

        // 显示一键领取按钮
        public void ShowGetAllRewardsButton()
        {
            getAllRewardsButton.gameObject.SetActive(true);
        }

        // 隐藏一键领取按钮
        public void HideGetAllRewardsButton()
        {
            getAllRewardsButton.gameObject.SetActive(false);
        }

        // 显示ReceiveOrDelete按钮
        public void ShowReceiveOrDelete()
        {
            receiveOrDelete.SetActive(true);
        }

        // 隐藏ReceiveOrDelete按钮
        public void HideReceiveOrDelete()
        {
            receiveOrDelete.SetActive(false);
        }


    }
}
