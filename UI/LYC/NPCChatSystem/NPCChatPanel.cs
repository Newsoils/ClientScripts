using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.LYC.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    public class NPCChatPanel :  MonoBehaviour
    {
        public GameObject obj;

        [Header("UI显示")]
        [SerializeField] private TMP_Text npcName;

        public Button exit;

        public OptionPanel optionPanelView;

        private void Start()
        {
            exit.onClick.AddListener(()=> {
                ClosePanel();
                 UIManager.Instance.GetPanel<SocialPanel>().npcPanel.RefreshNPCList();
            });
        }

        public void OnDestroy()
        {
            exit.onClick.RemoveAllListeners();
        }

        // 更新 UI 上有关 NPC 的基础信息显示
        public void UpdateInfo(NPC_Base NPC_Base,bool isGroupChat)
        {
            if (isGroupChat)
            {
                npcName.text = "小苔的打工群";
            }
            else
            {
                npcName.text = NPC_Base.npc_name;
            }
        }

        public void OpenPanel(NPC_Base NPC_Base, bool isGroupChat)
        {
            obj.SetActive(true);
            UpdateInfo(NPC_Base, isGroupChat);
        }
        public void ClosePanel()
        {
            obj.SetActive(false);
            // 同时关闭可能正在播放的聊天面板的协程
            EvtDsp.TriggerEvt(EvtNames.Stop_Chat_Coroutine);
        }


        // ========== 选项面板操作 ==========
        public void OpenOptionPanel(OptionDialogueModel optionDialogue, List<UnityAction> buttonClickHandlers)
        {
            // 1. 让 optionPanelView 显示
            optionPanelView.gameObject.SetActive(true);
            // 2. 设置数据，并且展示选项
            optionPanelView.DisplayOptions(optionDialogue, buttonClickHandlers);

        }

        // 失活选项面板，然后清除选项
        public void CloseOptionPanel()
        {
            // 1. 让 optionPanelView 关闭
            optionPanelView.gameObject.SetActive(false);
            // 2. 清除选项数据，销毁选项
            optionPanelView.ClearOptions();
        }

        // 清除选项数据，销毁选项
        public void ClearOptions()
        {
            optionPanelView.ClearOptions();
        }
    }

}
