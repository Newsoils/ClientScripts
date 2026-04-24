using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.LYC.DialogueSystem;
using CLIP.Project_Mouse.LYC.NPCDialogueSystem;
using UnityEngine;


namespace CLIP.Project_Mouse.UI
{
    // 协调各个 Controller 的行为
    public class NPCChatPanelMgr : MonoBehaviour
    {
        public static NPCChatPanelMgr Instance;

        [Header("Controllers")]
        [SerializeField] private NPCChatPanelController npcChatPanelController;
        //[SerializeField] private DialogueController dialogueController;
        //[SerializeField] private NPCFavorController npcFavorController;

        // 这里是放置具体数据的地方，估计是以 SO 的形式存储在这里 
        //[Header("Datas")]
        //[SerializeField] public NPC_Data_SO npcDatas;

        private bool hasInit = false;


        protected  void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            EvtDsp.AddEvt<NPC_Info>(EvtNames.Check_Chat_Play, CheckChatPlay);
            EvtDsp.AddEvt(EvtNames.Stop_Chat_Coroutine, StopChatPanelCoroutine);
        }

        private void OnDestroy()
        {
            EvtDsp.RemoveEvt<NPC_Info>(EvtNames.Check_Chat_Play, CheckChatPlay);
            EvtDsp.RemoveEvt(EvtNames.Stop_Chat_Coroutine, StopChatPanelCoroutine);
        }

        private void Init()
        {
            // 初始化 NPCFavorController 所需数据
            if (npcChatPanelController != null)
            {
                // 本地数据
                this.npcChatPanelController.InitFavorDatas(NPCManager.instance.NPC_Info_Dict.Values.ToList());
            }

            // 加载 NPCChatPanelController 所需数据
            if (npcChatPanelController != null)
            {
                // Json 数据
                npcChatPanelController.LoadDataFromSO();
                npcChatPanelController.LoadAllDialogues();
                npcChatPanelController.LoadAllParagraghs();
            }
            else
            {
                Debug.LogError("npcChatPanelController 为空，无法加载所需数据");
            }
        }

        // 让外界在合适的时候调用方法进行初始化
        public void InitNPCFavorRuntimeDatas()
        {
            // 如果还没有初始化过
            if (!hasInit)
            {
                Init();
                hasInit = true;
            }
        }


        /// <summary>
        /// 在打开 NPC 聊天面板的时候检测是否有对话要触发
        /// </summary>
        /// <param name="info"></param>
        public void CheckChatPlay(NPC_Info info)
        {
            PlayPendingDialogues(info, reloadChatHistory: true);
        }

        private void PlayPendingDialogues(NPC_Info info, bool reloadChatHistory)
        {
            if (info == null || npcChatPanelController == null)
            {
                return;
            }

            if (reloadChatHistory)
            {
                // 首次进入聊天时先清空窗口，再加载已经触发过的历史记录
                npcChatPanelController.chatViewController.ClearAllMessages();
                npcChatPanelController.LoadPastChatHistory(info._npc_RuntimeData);
            }

            if (npcChatPanelController.TryGetPendingParaIdByNPCId(info._npc_RuntimeData, out int paraId, out int nextDiaAtFavorLevel))
            {
                Debug.Log("当前存在还没有触发的对话剧情");
                npcChatPanelController.StartDisplayParagragh(paraId, info._npc_Base.npc_id, () =>
                {
                    npcChatPanelController.TriggeredDialogueAt(info._npc_Base.npc_id, nextDiaAtFavorLevel);
                    // 一个段落结束后立刻检查是否还有下一个待触发段落，无需退出聊天界面
                    PlayPendingDialogues(info, reloadChatHistory: false);
                });
            }
            else
            {
                Debug.Log("当前没有要触发的对话剧情");
            }
        }

        // 事件 handler：关闭可能正在播放的聊天面板的协程
        private void StopChatPanelCoroutine()
        {
            npcChatPanelController.StopChatCoroutine();
        }

    }
}
