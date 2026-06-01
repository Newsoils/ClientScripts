using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.UI;
using TMPro;
using UnityEngine;


namespace CLIP.Project_Mouse.UI
{
    public class AnnouncementPanel : UIPanelBase
    {
        [Header("UI")]
        public GameObject announcementCanvas;
        public GameObject background;
        public GameObject announcementPanel;
        public GameObject announcementDetailPanel;

        [Header("Annoucement Unit")]
        public Transform announcementUnitRoot;
        public GameObject announcementUnitPrefab;

        [Header("Announcement Detail")]
        public Announcement_Record selectRecord;
        public TMP_Text title;
        public TMP_Text content;

        private void Start()
        {
            Email_And_Announcement_Manager.instance._on_refresh_announcement.AddListener(RefreshUI);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            Email_And_Announcement_Manager.instance._on_refresh_announcement.RemoveListener(RefreshUI);
        }

        public void RefreshUI()
        {
            InitAnnouncementDetail();
            InitAnnouncementPanel();
        }

        // 初始化公告详情
        public void InitAnnouncementDetail()
        {
            if (selectRecord != null)
            {
                title.text = selectRecord.anno_title;
                content.text = selectRecord.anno_content;

                var height = content.preferredHeight;
                content.rectTransform.sizeDelta = new Vector2(content.rectTransform.sizeDelta.x, height);

            }
        }

        // 初始化公告列表
        public void InitAnnouncementPanel()
        {
            var announcementList = Email_And_Announcement_Manager.instance._announcement_record;
            int announcementCount = 0;
            int childCount = announcementUnitRoot.childCount;

            for (int i = 0; i < announcementList.Count; i++)
            {
                GameObject announcementUnitObj;
                if (i < childCount)
                {
                    announcementUnitObj = announcementUnitRoot.GetChild(announcementCount).gameObject;
                    announcementUnitObj.SetActive(true);
                }
                else
                {
                    announcementUnitObj = Instantiate(announcementUnitPrefab, announcementUnitRoot);
                }

                var announcementUnit = announcementUnitObj.GetComponent<AnnouncementUnit>();
                if (announcementUnit != null)
                {
                    announcementUnit.InitAnnouncementUnit(announcementList[i]);
                }
                announcementCount++;
            }

            for (int i = announcementCount; i < childCount; i++)
            {
                announcementUnitRoot.GetChild(i).gameObject.SetActive(false);
            }
        }

        // 选择公告
        public void SelectAnnouncement(Announcement_Record record)
        {
            announcementPanel.SetActive(false);
            announcementDetailPanel.SetActive(true);
            selectRecord = record;

            InitAnnouncementDetail();
        }

        public override void ClosePanel()
        {
            announcementCanvas.SetActive(false);
            background.SetActive(false);
        }

        public override void OpenPanel(params object[] data)
        {
            announcementCanvas.SetActive(true);
            background.SetActive(true);
            announcementPanel.SetActive(true);
            announcementDetailPanel.SetActive(false);

            InitAnnouncementPanel();
        }

        public void OpenPanel()
        {
            OpenPanel(null);
        }
    }
}
