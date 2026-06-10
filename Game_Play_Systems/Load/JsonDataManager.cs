using System;
using System.Collections.Generic;
using System.Linq;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Kernel.Dispatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using SP = CLIP.Framework_Core.Serialization.Serialization_Provider;


namespace CLIP.Project_Mouse.Game_Play_System
{
    public class JsonDataManager
    {
        private static string map_info_fileName = "project_mouse_tb_map_info";

        private static string photo_info_fileName = "project_mouse_tb_photo_info";

        private static string npc_info_fileName = "project_mouse_tb_npc_info";

        private static string npc_favor_exp_fileName = "project_mouse_tb_npc_favor_levelup_exp";

        private static string cloth_info_fileName = "project_mouse_tb_cloth_info";

        private static string currency_fileName = "project_mouse_tb_currency_info";

        private static string first_time_login_reward_fileName = "project_mouse_tb_firsttime_login_reward";

        private const string TaskDataPath = "Json/project_mouse_tb_task";

        private static string tape_info_json_file_name = "project_mouse_tb_tape_info";

        private static string item_group_fileName = "project_mouse_tb_item_group";

        private static string item_info_fileName = "project_mouse_tb_game_item";


        private static string seed_info_fileName = "project_mouse_tb_seedinfo";

        #region NPC 对话系统静态数据路径

        private const string PathDialogue = "Json/project_mouse_tb_dialogue";
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

            var diaJson = Resources.Load<TextAsset>(PathDialogue);
            if (diaJson != null)
            {
                var normalList = JsonConvert.DeserializeObject<List<DialogueModel>>(diaJson.text);
                if (normalList != null)
                {
                    foreach (var dia in normalList)
                    {
                        dia.BuildLookupDict();
                        dialoguesDic[dia.dialogueId] = dia;
                    }
                }
            }
            else
            {
                Debug.LogError($"[JsonDataManager] 对话资源未找到：{PathDialogue}");
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
                Debug.LogError($"[JsonDataManager] 段落资源未找到：{PathParagraph}");
            }

            Debug.Log($"[JsonDataManager] 对话加载完毕，共 {dialoguesDic.Count} 条；段落共 {paragraphsDic.Count} 个。");

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
                    Debug.Log($"[JsonDataManager] 好感度关联数据加载完毕，共 {dataList.Count} 个 NPC。");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JsonDataManager] 解析好感度关联数据失败：{ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"[JsonDataManager] 好感度关联数据未找到：{PathFavorCorrelative}");
            }
        }

        #endregion

        #region 其他数据

        public static void LoadClothData(out Dictionary<int, ClothInfo> clothInfoDic)
        {
            clothInfoDic = new Dictionary<int, ClothInfo>();
            var json = Load_Single_JsonData(cloth_info_fileName);
            var cloths = SP.DeserializeObject<List<ClothInfo>>(json);
            clothInfoDic = cloths.ToDictionary(c => c.clothId, c => c);
        }


        public static void LoadTaskData(out Dictionary<int, MissionStaticData> taskModelsDic)
        {
            taskModelsDic = new Dictionary<int, MissionStaticData>();
            var jsonFile = Resources.Load<TextAsset>(TaskDataPath);
            var taskModels = JsonConvert.DeserializeObject<List<MissionStaticData>>(jsonFile.text);
            foreach (var task in taskModels)
            {
                taskModelsDic[task.taskId] = task;
            }
        }

        public static List<Photo_Info> LoadPhotoInfo()
        {
            string info = Load_Single_JsonData(photo_info_fileName);
            return JsonConvert.DeserializeObject<List<Photo_Info>>(info);
        }

        public static void LoadMapInfo(out Dictionary<int, Map_Info> mapInfo)
        {
            mapInfo = new Dictionary<int, Map_Info>();
            var json = Load_Single_JsonData(map_info_fileName);
            var maps = SP.DeserializeObject<List<Map_Info>>(json);
            mapInfo = maps.ToDictionary(m => m.map_id, m => m);
        }

        public static List<Map_Info> LoadMapInfo()
        {
            var json = Load_Single_JsonData(map_info_fileName);
            return JsonConvert.DeserializeObject<List<Map_Info>>(json);
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

        public static void GetSeedInfo(out Dictionary<int,SeedInfo> seedDic)
        {
             seedDic = new Dictionary<int, SeedInfo>();
            var json = Load_Single_JsonData(seed_info_fileName);
            var list = SP.DeserializeObject<List< SeedInfo>>(json);
            foreach(var each in list)
            {
                seedDic[each.seedID] = each;        
            }
        }

        public class ItemNameCount
        {
            public string name;
            public int num;
        }

        #endregion  
        public static void LoadItemGroupData(out Dictionary<int, ItemGroupInfo> itemGroupDic,out Dictionary<GroupType,List<ItemGroupInfo>> itemGroupDicByGroup )
        {
            itemGroupDic = new Dictionary<int, ItemGroupInfo>();
            itemGroupDicByGroup = new Dictionary<GroupType, List<ItemGroupInfo>>();
            var json = Load_Single_JsonData(item_group_fileName);
            var groups = SP.DeserializeObject<List<ItemGroupInfo>>(json);
            if (groups != null)
            {
                foreach (var group in groups)
                {
                    itemGroupDic[group.group_id] = group;
                    if (!itemGroupDicByGroup.ContainsKey(group.groupType))
                    {
                        itemGroupDicByGroup[group.groupType] = new List<ItemGroupInfo>();
                    }
                    itemGroupDicByGroup[group.groupType].Add(group);
                }
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

        #region 物品静态数据

        /// <summary>
        /// 加载物品静态配置数据，构建字典缓存。
        /// </summary>
        public static void LoadGameItemData(
            out List<Game_Item_Info> gameItemDb,
            out Dictionary<int, Game_Item_Info> idDic,
            out Dictionary<string, Game_Item_Info> nameDic)
        {
            gameItemDb = new List<Game_Item_Info>();
            idDic = new Dictionary<int, Game_Item_Info>();
            nameDic = new Dictionary<string, Game_Item_Info>();

            var json = Load_Single_JsonData(item_info_fileName);
            if (string.IsNullOrEmpty(json))
                return;

            var list = SP.DeserializeObject<List<Game_Item_Info>>(json);
            if (list == null)
                return;

            foreach (var item in list)
            {
                gameItemDb.Add(item);

                if (!idDic.ContainsKey(item.item_id))
                    idDic.Add(item.item_id, item);

                if (!nameDic.ContainsKey(item.name))
                    nameDic.Add(item.name, item);
            }

            Debug.Log($"[JsonDataManager] 物品数据加载完毕，共 {gameItemDb.Count} 条。");
        }

        #endregion
    }
}