using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class NPCManager : SingletonMono<NPCManager>
    {

        //NPC数据
        [SerializeField]
        public Dictionary<int, NPC_Info> NPC_Info_Dict = new Dictionary<int, NPC_Info>();
        public Dictionary<int, NPC_Base> NPC_Base_Dict = new Dictionary<int, NPC_Base>();

        /// <summary>
        /// 好感度升级所需经验（NPC通用），
        /// Key = 好感度等级，Value = 升级所需经验值
        /// </summary>
        public Dictionary<int, int> NPCFavor_LevelUp_neededExp = new Dictionary<int, int>();

        //传给TS层的事件
        public UnityEvent<string> on_Meet_NPC;
        public UnityEvent<string> on_Give_Gift_ToNPC;

        /// <summary>
        /// 请求所有NPC相关数据，接给TS层调用
        /// </summary>
        public UnityEvent ask_All_NPC_Data;

        /// <summary>
        /// 请求NPC相关数据，接给TS层调用
        /// </summary>
        public UnityEvent<string> ask_Single_NPC_Data;

        public UnityEvent<string> receive_NPC_Gift_TO_Player;


        void Start()
        {
            DontDestroyOnLoad(this.gameObject);

            JsonData_Manager.LoadNPCData(out NPC_Info_Dict, out NPC_Base_Dict, out NPCFavor_LevelUp_neededExp);
            NPCChatManager.Instance.Initialize();

            EvtDsp.AddEvt<int>(EvtNames.Meet_NPC, Meet_NPC);
            EvtDsp.AddEvt<int, Item_Type>(EvtNames.Give_Gift_TO_NPC, Give_Gift_To_NPC);

            receive_NPC_Gift_TO_Player.AddListener(p => Receive_NPC_Gift_TO_Player(p));

            StartCoroutine(waitForAskNPCData());
        }

        void Update()
        {
            //测试代码

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.mKey.wasPressedThisFrame)
            {
                int randomNPCId = Random.Range(1, NPC_Base_Dict.Count + 1);
                EvtDsp.TriggerEvt<int>(EvtNames.Meet_NPC, randomNPCId);
            }

            if (keyboard.gKey.wasPressedThisFrame)
            {
                EvtDsp.TriggerEvt<int, Item_Type>(EvtNames.Give_Gift_TO_NPC, 1, Item_Type.Pot);
            }

            if (keyboard.aKey.wasPressedThisFrame)
            {
                Ask_For_All_NPC_Data();
            }
        }


        public IEnumerator waitForAskNPCData()
        {
            yield return new WaitForSeconds(1f);
            Ask_For_All_NPC_Data();
        }
        public void Meet_NPC(int npc_id)
        {
            Debug.Log("遇见NPC：" + npc_id + "\t" + NPC_Base_Dict[npc_id].npc_name);
            //序列化
            var json = GF_SP.SerializeObject(npc_id);
            //传给TS层
            on_Meet_NPC?.Invoke(json);
        }

        public void Give_Gift_To_NPC(int npc_id, Item_Type item_type)
        {
            Debug.Log("给NPC送礼物：" + npc_id + "\t" + NPC_Base_Dict[npc_id].npc_name + "\t" + "礼物类型：" + item_type.ToString());
            //序列化

            var data = new Gift_To_NPC_Msg(npc_id, item_type);
            var json = GF_SP.SerializeObject(data);

            //传给TS层
            on_Give_Gift_ToNPC?.Invoke(json);
        }

        public void Ask_For_Single_NPC_Data(int npc_ID)
        {
            var data = GF_SP.SerializeObject(npc_ID);
            ask_Single_NPC_Data?.Invoke(data);
        }

        public void Ask_For_All_NPC_Data()
        {
            ask_All_NPC_Data?.Invoke();
        }


        public string get_receiver_name()
        {
            return "NPC_Manager";
        }

        public void Receive_NPC_Data(string json)
        {
            var newData = JsonConvert.DeserializeObject<NPC_RuntimeData>(json);

            //更新数据
            var npc_Info = NPC_Info_Dict[newData.npc_id];

            npc_Info._npc_RuntimeData = newData;
            npc_Info.npc_Next_Favor_Level = Next_Level_Favor_Exp_Needed(npc_Info._npc_RuntimeData.favor_level);

            EvtDsp.TriggerEvt<NPC_RuntimeData>(EvtNames.On_Single_NPC_Data_Updated, newData);
            EvtDsp.TriggerEvt(EvtNames.On_NPC_Data_Update);
            Debug.Log("收到NPC数据：" + newData.npc_id + "\t" + NPC_Base_Dict[newData.npc_id].npc_name
                + "\t" + "好感度等级：" + newData.favor_level + "\t" + "好感度：" + newData.favor_Value);
        }

        public void Receive_All_NPC_Data(string json)
        {
            var list = JsonConvert.DeserializeObject<List<NPC_RuntimeData>>(json);

            //更新数据
            if (list != null)
            {
                foreach (var p in list)
                {
                    NPC_Info_Dict.TryGetValue(p.npc_id, out NPC_Info npc_Info);
                    if (npc_Info._npc_RuntimeData == null)
                    {
                        NPC_RuntimeData data = new NPC_RuntimeData();
                        npc_Info._npc_RuntimeData = p;
                    }
                    else
                    {
                        npc_Info._npc_RuntimeData = p;
                    }

                    npc_Info.npc_Next_Favor_Level = Next_Level_Favor_Exp_Needed(npc_Info._npc_RuntimeData.favor_level);

                }
            }
            Debug.Log("收到所有NPC数据，数量：" + list.Count);

            EvtDsp.TriggerEvt(EvtNames.On_All_NPC_Data_Received, list);
            EvtDsp.TriggerEvt(EvtNames.On_NPC_Data_Update);
            //foreach (var newData in datas)
            //{
            //    Debug.Log("收到NPC数据：" + newData.npc_id + "\t" + NPC_Base_Dict[newData.npc_id].npc_name
            //        + "\t" + "好感度等级：" + newData.favor_level + "\t" + "好感度：" + newData.favor_Value);
            //}
        }

        /// <summary>
        /// 收到NPC送给玩家的礼物，触发事件
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public void Receive_NPC_Gift_TO_Player(string json)
        {
            var data = GF_SP.DeserializeObject<NPC_Gift_To_PLayer_Info>(json);
            EvtDsp.TriggerEvt(EvtNames.Receive_NPC_Gift_TO_Player, data);
        }

        /// <summary>
        /// 计算到下级好感度需要的经验值
        /// </summary>
        /// <param name="current_Level"></param>
        /// <returns></returns>
        public int Next_Level_Favor_Exp_Needed(int current_Level)
        {
            if (NPCFavor_LevelUp_neededExp.TryGetValue(current_Level, out int neededExp))
            {
                return neededExp;
            }
            else
            {
                Debug.LogWarning("没有找到好感度等级 " + (current_Level) + " 所需经验数据，可能已达最高等级");
                return -1;
            }
        }


    }
}
