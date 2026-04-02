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
            EvtDsp.AddEvt<NPC_Info>(EvtNames.Check_Chat_Play, CheakChatPlay);
            EvtDsp.AddEvt(EvtNames.Stop_Chat_Coroutine, StopChatPanelCoroutine);
        }

        private void OnDestroy()
        {
            EvtDsp.RemoveEvt<NPC_Info>(EvtNames.Check_Chat_Play, CheakChatPlay);
            EvtDsp.RemoveEvt(EvtNames.Stop_Chat_Coroutine, StopChatPanelCoroutine);
        }

        private void Init()
        {
            // 初始化 NPCFavorController 所需数据
            if (npcChatPanelController != null)
            {
                this.npcChatPanelController.InitFavorDatas(NPCManager.instance.NPC_Info_Dict.Values.ToList());
            }

            // 加载 NPCChatPanelController 所需数据
            if (npcChatPanelController != null)
            {
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



        public void CheakChatPlay(NPC_Info info)
        {
            // 这里应该要先清空窗口
            npcChatPanelController.chatViewController.ClearAllMessages();

            // 接着立刻进行聊天历史的加载
            npcChatPanelController.LoadPastChatHistory(info._npc_RuntimeData);

            // 如果存在待触发的对话
            if (npcChatPanelController.TryGetPendingParaIdByNPCId(info._npc_RuntimeData, out int paraId, out int nextDiaAtFavorLevel))
            {
                // 就播放该对话内容
                Debug.Log("当前存在还没有触发的对话剧情");
                npcChatPanelController.StartDisplayParagragh(paraId, info._npc_Base.npc_id, () =>
                {
                    npcChatPanelController.TriggeredDialogueAt(info._npc_Base.npc_id, nextDiaAtFavorLevel);
                });
            }
            else
            {
                Debug.Log("当前没有要触发的对话剧情");
            }
        }
        // 打开与 NPC 的聊天面板（这里还需要为每一个不同的 NPC 定制一个 ChatViewController，以及他们各自的 滚动条物体）
        public void OpenChatPanel(NpcChatUnit npcChatUnit)
        {

            // 先设置当前对话窗口的滑动条控制器
            //dialogueController.SetChatViewController(npcChatPanelController.chatViewController);


        }

        // 事件 handler：关闭可能正在播放的聊天面板的协程
        private void StopChatPanelCoroutine()
        {
            npcChatPanelController.StopChatCoroutine();
        }


        #region 测试

        // 获取 NPCChatUnit (模拟)
        private NpcChatUnit GetNPCChatUnit(int unitId)
        {
            // 1.获取所有 NPC 信息 [npc_id, NPC_Info]
            Dictionary<int, NPC_Info> npcDict = NPCManager.instance.NPC_Info_Dict;


            // 2.创建测试用例数据
            // 群聊数据
            NpcChatUnit groupChat = gameObject.AddComponent<NpcChatUnit>();
            groupChat.isGroupChat = true;
            groupChat.info = null;

            // NPC 个人数据
            Dictionary <int, NpcChatUnit> npcChatUnitList = new Dictionary<int, NpcChatUnit>();
            foreach(var info in npcDict)
            {
                NpcChatUnit tmp = gameObject.AddComponent<NpcChatUnit>();
                tmp.isGroupChat = false;
                tmp.info = info.Value;
                tmp.info._npc_RuntimeData = new NPC_RuntimeData()
                {
                    favor_level = 5,
                    favor_Value = 20,
                    encounter_count = 2,
                    is_met = true,
                    is_acquainted = true,
                    last_interaction_time = DateTime.MinValue,
                    npc_id = info.Key
                };

                npcChatUnitList.Add(info.Key, tmp);
            }

            if(unitId == 0)
            {
                return groupChat;
            }

            return npcChatUnitList.ContainsKey(unitId) ? npcChatUnitList[unitId] : null;
        }

        //public void Test_OnClickOpenChatPanel()
        //{
        //    NpcChatUnit unit = GetNPCChatUnit(1);

        //    npcChatPanelController.OpenChatPanel(unit);

        //    // 如果存在待触发的对话
        //    if (npcChatPanelController.TryGetPendingParaIdByNPCId(unit.info._npc_RuntimeData, out int paraId, out int favorLevel))
        //    {
        //        // 就播放该对话内容
        //        Debug.Log("当前存在还没有触发的对话剧情");
        //        npcChatPanelController.StartDisplayParagragh(paraId, unit.info._npc_Base.npc_id, () =>
        //        {
        //            npcChatPanelController.TriggeredDialogueAt(unit.info._npc_Base.npc_id, favorLevel);
        //        });
        //    }

        //    Debug.Log("当前没有要触发的对话剧情");
        //}

        //public void TestLoadPastChatHistory()
        //{
        //    // 创建测试数据
        //    NPC_RuntimeData runtimeData = new NPC_RuntimeData()
        //    {
        //        favor_level = 5,
        //        favor_Value = 20,
        //        encounter_count = 2,
        //        is_met = true,
        //        is_acquainted = true,
        //        last_interaction_time = DateTime.MinValue,
        //        npc_id = 1
        //    };
        //    NpcChatUnit unit = GetNPCChatUnit(1);

        //    npcChatPanelController.OpenChatPanel(unit);
        //    npcChatPanelController.Test_LoadPastChatHistory(runtimeData);
        //}

        #endregion
    }
}
