using System;
using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
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

        private const string TaskDataPath = "Json/project_mouse_tb_task";

        #region NPC 对话系统静态数据路径

        private const string PathNormalDialogue = "Json/project_mouse_tb_dialogue";
        private const string PathParagraph = "Json/project_mouse_tb_paragragh";
        private const string PathFavorCorrelative = "Json/project_mouse_tb_npc_favor_correlative_data";

        #endregion

        #region NPC 静态数据

        /// <summary>
        /// 加载 NPC 基础信息及好感度升级经验配置。
        /// </summary>
        public static void LoadNPCData(out Dictionary<int, NPC_Info> NPC_Info_Dict,
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

        #endregion

        #region NPC 对话系统静态数据

        /// <summary>
        /// 一次性加载对话系统全部静态数据（对话/段落/好感度等级映射）。
        /// </summary>
        public static void LoadAllDialogueData(
            out Dictionary<int, DialogueModel> dialoguesDic,
            out Dictionary<int, ParagraphModel> paragraphsDic,
            out Dictionary<int, Dictionary<int, int>> favorLevelToParaIdDic)
        {
            // 1. 加载对话
            dialoguesDic = new Dictionary<int, DialogueModel>();

            var diaJson = Resources.Load<TextAsset>(PathNormalDialogue);
            if (diaJson != null)
            {
                var normalList = SP.DeserializeObject<List<DialogueModel>>(diaJson.text);
                if (normalList != null)
                {
                    foreach (var dia in normalList)
                    {
                        dialoguesDic[dia.DialogueId] = dia;
                    }
                }
            }
            else
            {
                Debug.LogError($"[JsonData_Manager] 对话资源未找到：{PathNormalDialogue}");
            }

            // 2. 加载段落并注入对话
            paragraphsDic = new Dictionary<int, ParagraphModel>();

            var paragraphJson = Resources.Load<TextAsset>(PathParagraph);
            if (paragraphJson != null)
            {
                var paraList = SP.DeserializeObject<List<ParagraphModel>>(paragraphJson.text);
                if (paraList != null)
                {
                    foreach (var para in paraList)
                    {
                        para.InjectDialogues(dialoguesDic);
                        paragraphsDic[para.ParaId] = para;
                    }
                }
            }
            else
            {
                Debug.LogError($"[JsonData_Manager] 段落资源未找到：{PathParagraph}");
            }

            Debug.Log($"[JsonData_Manager] 对话加载完毕，共 {dialoguesDic.Count} 条；段落共 {paragraphsDic.Count} 个。");

            // 3. 加载好感度等级 ↔ 段落映射
            favorLevelToParaIdDic = new Dictionary<int, Dictionary<int, int>>();

            var favorJson = Resources.Load<TextAsset>(PathFavorCorrelative);
            if (favorJson != null)
            {
                try
                {
                    var dataList = JsonConvert.DeserializeObject<List<NPCFavorCorrelativeData>>(favorJson.text);
                    foreach (var data in dataList)
                    {
                        favorLevelToParaIdDic[data.npcId] = data.ToDictionary();
                    }
                    Debug.Log($"[JsonData_Manager] 好感度关联数据加载完毕，共 {dataList.Count} 个 NPC。");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JsonData_Manager] 解析好感度关联数据失败：{ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"[JsonData_Manager] 好感度关联数据未找到：{PathFavorCorrelative}");
            }
        }

        #endregion

        #region 其他数据

        public static void LoadTaskData(out Dictionary<int, TaskModel> taskModelsDic, out Dictionary<int, List<TaskModel>> taskGroupByLevel)
        {
            taskModelsDic = new Dictionary<int, TaskModel>();
            taskGroupByLevel = new Dictionary<int, List<TaskModel>>();
            var jsonFile = Resources.Load<TextAsset>(TaskDataPath);
            var taskModels = JsonConvert.DeserializeObject<List<TaskModel>>(jsonFile.text);
            foreach (var task in taskModels)
            {
                taskModelsDic[task.taskId] = task;
                if (!taskGroupByLevel.TryGetValue(task.unlockExp, out var list))
                {
                    list = new List<TaskModel>();
                    taskGroupByLevel[task.unlockExp] = list;
                }
                list.Add(task);
            }
        }

        public static List<cloth_info> Load_ClothInfo_JsonData()
        {
            var jsonText = Load_Single_JsonData(cloth_info_fileName);
            var clothInfo = JsonConvert.DeserializeObject<List<cloth_info>>(jsonText);

            return clothInfo;
        }

        public static void Load_Currency_Data(out Dictionary<int, GameCurrency> idDic, out Dictionary<string, GameCurrency> nameDic)
        {
            idDic = new Dictionary<int, GameCurrency>();
            nameDic = new Dictionary<string, GameCurrency>();

            var json = Load_Single_JsonData(currency_fileName);
            var list = JsonConvert.DeserializeObject<List<GameCurrency>>(json);
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
            var list = JsonConvert.DeserializeObject<List<ItemNameCount>>(json);
            if (list != null)
            {
                foreach (var each in list)
                {
                    rewardList.Add((each.name, each.num));
                }
            }
        }

        public static string GetRoomJsonData()
        {
            // 构建存档对象
            RoomSaveData saveData = new RoomSaveData
            {
                rooms = RoomSystem.Instance.RoomDatas
            };

            // 序列化
            return SP.SerializeObject(saveData);
        }

        public class ItemNameCount
        {
            public string name;
            public int num;
        }

        #endregion
    }
}
