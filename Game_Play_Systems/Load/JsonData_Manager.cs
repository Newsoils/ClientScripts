using System;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Serialization;
using CLIP.Project_Mouse.Kernel;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using SP = CLIP.Framework_Core.Serialization.Serialization_Provider;


namespace CLIP.Project_Mouse.Game_Play_System
{
    public class JsonData_Manager
    {

        private static string tape_info_json_file_name = "project_mouse_tb_tape_info";

        private static string npc_info_fileName = "project_mouse_tb_npc_info";

        private static string npc_favor_exp_fileName = "project_mouse_tb_npc_favor_levelup_exp";

        private static string cloth_info_fileName = "project_mouse_tb_cloth_info";

        private static string currency_fileName = "project_mouse_tb_currency_info";

        private static string first_time_login_reward_fileName = "project_mouse_tb_firsttime_login_reward";

        /// 加载npc静态数据
        /// </summary>
        public static void Load_NPC_static_Data(out Dictionary<int, NPC_Info> NPC_Info_Dict,
            out Dictionary<int, NPC_Base> NPC_Base_Dict, out Dictionary<int, int> NPCFavor_LevelUp_neededExp)
        {
            NPC_Base_Dict = new Dictionary<int, NPC_Base>();
            NPC_Info_Dict = new Dictionary<int, NPC_Info>();
            NPCFavor_LevelUp_neededExp = new Dictionary<int, int>();

            var jsonText = Load_Single_JsonData(npc_info_fileName);
            var list = SP.DeserializeObject<List<NPC_Base>>(jsonText);
            if (list != null)
            {

                foreach (var p in list)
                {
                    NPC_Base_Dict[p.npc_id] = p;
                    NPC_Info_Dict[p.npc_id] = new NPC_Info()
                    {
                        _npc_Base = p,
                    };
                }
            }

            jsonText = Load_Single_JsonData(npc_favor_exp_fileName);
            var expList = JsonConvert.DeserializeObject<List<JObject>>(jsonText);
            if (expList != null)
            {
                foreach (var item in expList)
                {
                    int level = item["favor_lv"].Value<int>();
                    int neededExp = item["levelUp_ExpNeed"].Value<int>();
                    NPCFavor_LevelUp_neededExp[level] = neededExp;
                }
            }
        }

        public static List<cloth_info> Load_ClothInfo_JsonData()
        {
            var jsonText = Load_Single_JsonData(cloth_info_fileName);
            var clothInfo = SP.DeserializeObject<List<cloth_info>>(jsonText);

            return clothInfo;
        }

        public static void Load_Currency_Data(out Dictionary<int, GameCurrency> idDic, out Dictionary<string, GameCurrency> nameDic)
        {
            idDic = new Dictionary<int, GameCurrency>();
            nameDic = new Dictionary<string, GameCurrency>();

            var json = Load_Single_JsonData(currency_fileName);
            var list = SP.DeserializeObject<List<GameCurrency>>(json);
            foreach (var gameCurrency in list)
            {
                idDic[gameCurrency.item_id] = gameCurrency;
                nameDic[gameCurrency.currency_name] = gameCurrency;
            }
        }

        public static string Load_Single_JsonData(string fileName)
        {
            TextAsset ta = Resources.Load<TextAsset>("Json/" + fileName);
            if (ta != null)
            {
                return ta.text;
            }
            else
            {
                Debug.LogError("无法加载路径：" + fileName + " 下的Json数据");
                return null;
            }
        }

        public static void Load_FirstTimeLoginReward_Data(out List<(string, int)> rewardList)
        {
            rewardList = new List<(string, int)>();
            var json = Load_Single_JsonData(first_time_login_reward_fileName);
            var list = SP.DeserializeObject<List<ItemNameCount>> (json);
            if (list != null)
            {
                foreach(var each in list)
                {
                    rewardList.Add((each.name, each.num));
                }
            }
        }



        public static string  GetRoomJsonData()
        {
            // 构建存档对象
            RoomSaveData saveData = new RoomSaveData
            {
                rooms = RoomSystem.Instance.RoomDatas
            };

            // 序列化
            return  SP.SerializeObject(saveData);
        }

        public class ItemNameCount
      {
            public string name;
            public int num;
        }
    }
}
