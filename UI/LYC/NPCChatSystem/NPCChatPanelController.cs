using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.LYC.DialogueSystem;
using CLIP.Project_Mouse.LYC.NPCDialogueSystem;
using CLIP.Project_Mouse.LYC.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace CLIP.Project_Mouse.UI
{
    public class NPCChatPanelController : MonoBehaviour
    {
        // ====================================== 原 NPCChatPanelController =============================================
        [Header("View")]
        [SerializeField] private NPCChatPanel npcChatPanel;
        [SerializeField] public ChatViewController chatViewController;
        [Header("Model")]
        private Dictionary<int, NPC_Info> AllNPCChatDataDic;
        private NPC_Info _currentNPCChatData;
        [Header("Test")]
        [SerializeField] private NPCChatPanel testNPCChatPanel;
        // ====================================== 原 NPCChatPanelController =============================================

        // ======================================   原 DialogueController   =============================================
        // 选项面板界面
        [SerializeField] private OptionPanel optionPanelView;
        // 所有对话 [DialogueId, IDialogueModel]
        private Dictionary<int, IDialogueModel> _allDialoguesDic;
        // 所有段落 [ParaId, ParagraphModel]
        private Dictionary<int, ParagraphModel> _allParagraghsDic;
        // 当前正在播放的段落
        private ParagraphModel _currentPlayingParagragh;
        // 当前正在播放的对话
        private IDialogueModel _currentPlayingDialogue;
        // 聊天界面滚动控制器
        //[SerializeField] public ChatViewController _chatViewController;
        // 当前正在播放对话的协程
        private Coroutine _currentCoroutine;
        // 所有 NPC 的头像 Sprite 
        [SerializeField] private List<Sprite> NPCHeadIcons;
        // 小苔头像
        [SerializeField] private Sprite XiaoTaiHeadIcon;
        // 在 Resources 文件夹下的路径
        private string _optionDialoguePath = "Json/project_mouse_lyc_dialoguesystem_tb_option_dialogue";
        private string _normalDialoguePath = "Json/project_mouse_lyc_dialoguesystem_tb_normal_dialogue";
        private string _paragraghPath = "Json/project_mouse_lyc_dialoguesystem_tb_paragragh";
        // ======================================   原 DialogueController   =============================================

        // ======================================   原 NPCFavorController   =============================================
        // 本地存储的好感度数据 [npc_id, LocalNPCFavorData]
        private Dictionary<int, LocalNPCFavorData> LocalFavorDatas = null;
        // 记录各个 NPC ，在各自到达一定好感度等级时，其所对应要触发剧情的 paraId
        // Dictionary<npc_id, Dictionary<favor_level, paraId>>
        private Dictionary<int, Dictionary<int, int>> ParaIdOfEachFavorLevelDic = null;
        // 绝对路径
        private string _absolutePath;
        // NPC 好感度在 Resources 下的加载地址
        private string _npcFavorSavePath = "NPCFavorDatas/LocalFavorDatas";
        // Resources 路径下的加载地址
        private string _paraIdOfEachFavorLevelLoadPath = "Json/project_mouse_lyc_dialoguesystem_tb_npc_favor_correlative_data";
        // ======================================   原 NPCFavorController   =============================================

        private void Start()
        {
            EvtDsp.AddEvt<NPC_RuntimeData>(EvtNames.On_Single_NPC_Data_Updated, OnReceiveNPCFavorUpdate);
        }

        // --------------------------------------------------------------------------------------------------------------

        // ====================================== 原 NPCChatPanelController =============================================
        #region 业务数据加载

        public void LoadDataFromSO()
        {
            AllNPCChatDataDic = new Dictionary<int, NPC_Info>();

            foreach (var info in NPCManager.instance.NPC_Info_Dict.Values)
            {
                AllNPCChatDataDic[info._npc_Base.npc_id] = info;
            }
        }

        #endregion


        // ======================================   原 DialogueController   =============================================
        #region Dialogue 业务数据操作

        // ========== 加载部分 ==========
        /// <summary>
        /// 加载所有对话
        /// </summary>
        public void LoadAllDialogues()
        {
            _allDialoguesDic = new Dictionary<int, IDialogueModel>();

            List<IDialogueModel> tempDialogueDatas = new List<IDialogueModel>();

            // 加载 NormalDialogueModel
            tempDialogueDatas = LoadDialoguesByPath<NormalDialogueModel>(_normalDialoguePath);
            foreach (IDialogueModel dia in tempDialogueDatas)
            {
                _allDialoguesDic[dia.DialogueId] = dia;
            }
            // 加载 OptionDialogueModel
            tempDialogueDatas = LoadDialoguesByPath<OptionDialogueModel>(_optionDialoguePath);
            foreach (IDialogueModel dia in tempDialogueDatas)
            {
                _allDialoguesDic[dia.DialogueId] = dia;
            }

            Debug.Log("已完成所有对话的加载");
        }

        // 通过 Resources 路径加载对话数据
        private List<IDialogueModel> LoadDialoguesByPath<T>(string filePath) where T : IDialogueModel
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(filePath);

            return LoadDialoguesBySingleLine<T>(jsonFile);
        }

        // 逐条读取对话数据
        private List<IDialogueModel> LoadDialoguesBySingleLine<T>(TextAsset fileName) where T : IDialogueModel
        {
            using (var reader = new JsonTextReader(new StringReader(fileName.text)))
            {
                var dialogues = new List<IDialogueModel>();

                // 把整个文件当成 JArray 一次性加载，再逐条转实体
                JArray arr = JArray.Load(reader);
                foreach (JToken tok in arr)
                {
                    dialogues.Add(tok.ToObject<T>());
                }
                return dialogues;
            }
        }

        // ========== 加载部分 ==========

        private bool HasOptionDialogue(IDialogueModel dialogue)
        {
            if (dialogue.HasOptionDialogue) return true;
            return false;
        }

        public IDialogueModel GetDialogueById(int diaId)
        {
            if (_allDialoguesDic.ContainsKey(diaId))
            {
                return _allDialoguesDic[diaId];
            }

            Debug.LogWarning($"不存在 id 为 {diaId} 的 Dialogue");
            return null;
        }

        #endregion


        #region Paragragh 业务数据操作

        // ========== 加载部分 ==========
        /// <summary>
        /// 加载所有段落消息
        /// </summary>
        public void LoadAllParagraghs()
        {
            _allParagraghsDic = new Dictionary<int, ParagraphModel>();

            List<ParagraphModel> temp = LoadParagraghsByPath(_paragraghPath);
            foreach (var para in temp)
            {
                para.InitAllDialoguesInPara(_allDialoguesDic);  // 在加载完Json之后，这里还要初始化一下，以填充 _allDialoguesInPara 信息
                _allParagraghsDic[para.ParaId] = para;
            }

            Debug.Log("已完成所有段落的加载");
        }

        // 通过 Resources 路径加载段落数据
        private List<ParagraphModel> LoadParagraghsByPath(string filePath)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(filePath);

            return JsonConvert.DeserializeObject<List<ParagraphModel>>(jsonFile.text);
        }




        // ========== 操作部分 ==========
        /// <summary>
        /// 开始播放段落，顺序推进段落中的对话
        /// </summary>
        public void StartDisplayParagragh(int paraId, int npcId, UnityAction afterDisplay = null)
        {
            if (!_allParagraghsDic.ContainsKey(paraId))
            {
                Debug.LogError($"id为 {paraId} 的段落不存在");
                return;
            }

            _currentCoroutine = StartCoroutine(EnumStartDisplayParagragh());

            IEnumerator EnumStartDisplayParagragh()
            {
                _currentPlayingParagragh = _allParagraghsDic[paraId];
                _currentPlayingDialogue = _currentPlayingParagragh.GetDialogueById(_currentPlayingParagragh.FirstDialogueId);
                IDialogueModel dialogue = _currentPlayingDialogue;

                //bool canTurnToNextDialogue = false;
                // 等待当前 Dialogue 处理
                bool waiting = true;

                while (_currentPlayingDialogue != null)
                {
                    // 1.对当前正在播放的对话进行操作（判断当前对话类型，然后更新并显示 View 层内容）
                    if (!dialogue.HasOptionDialogue)
                    {
                        // 正常展示对话内容（可能是以动画的形式展现，展示选项同理）
                        OnNormalDialogueProcess(dialogue, npcId, () => { waiting = false; });
                    }
                    else
                    {
                        // 展示选项
                        OnOptionProcess(dialogue as OptionDialogueModel, () => { waiting = false; }, npcId);
                    }

                    // 2.等待一段时间，或等待前面内容完成（就是等待一定的动画时间，或者等待 View 层传来结束信号）
                     //yield return new WaitForSeconds(1f);

                    yield return new WaitForEndOfFrame();
                    while (waiting)
                    {
                        yield return null;
                    }
                    waiting = true;

                    // 3.转到下一句对话
                    if (dialogue.NextDialogueId != -1)
                    {
                        _currentPlayingDialogue = _currentPlayingParagragh.GetDialogueById(dialogue.NextDialogueId);
                        dialogue = _currentPlayingDialogue;
                    }
                    else
                    {
                        Debug.Log("当前段落结束");
                        break;
                    }
                }

                afterDisplay?.Invoke();
            }
        }

        /// <summary>
        /// 开始播放段落，顺序推进段落中的对话
        /// </summary>
        public void StartDisplayParagragh(ParagraphModel para, UnityAction afterDisplay, int npcId)
        {
            if (para == null)
            {
                Debug.LogError("段落为空");
                return;
            }

            StartDisplayParagragh(para.ParaId, npcId, afterDisplay);
        }


        // 处理一般对话
        private void OnNormalDialogueProcess(IDialogueModel dialogue, int npcId, UnityAction OnCanTurnToNextDialogue)
        {
            Sprite sprite = NPCHeadIcons[npcId - 1];
            string speakerName = null;
            string someText = dialogue.Contents;
            string chatHint = dialogue.ChatHints;

            if (dialogue.Speaker == EnumDialogueSpeaker.Character)
            {
                speakerName = "小苔";
                chatViewController.CharacterSendMsg(sprite, speakerName, someText, chatHint, OnCanTurnToNextDialogue);
            }
            else if (dialogue.Speaker == EnumDialogueSpeaker.NPC)
            {
                //speakerName = "NPC 1";
                speakerName = AllNPCChatDataDic[npcId]._npc_Base.npc_name;
                chatViewController.NPCSendMsg(sprite, speakerName, someText, chatHint, OnCanTurnToNextDialogue);
            }
        }

        // 处理选项选择对话
        private void OnOptionProcess(OptionDialogueModel optionDialogue, UnityAction OnCanTurnToNextDialogue, int npcId)
        {
            Sprite sprite = XiaoTaiHeadIcon;
            string speakerName = null;
            string someText = null;
            string chatHint = optionDialogue.ChatHints;

            List<UnityAction> buttonHandlers = new List<UnityAction>();
            foreach (var op in optionDialogue.Options)
            {
                buttonHandlers.Add(() =>
                {
                    speakerName = "小苔";
                    someText = op.Key;   // 随不同选项而改变
                    LocalFavorDatas[npcId].AddChoiceRecord(optionDialogue.DialogueId, op.Key);
                    optionDialogue.SetNextDialogueId(op.Value);
                    chatViewController.CharacterSendMsg(sprite, speakerName, someText, chatHint, OnCanTurnToNextDialogue);

                    // 选择选项后关闭选项面板
                    UIManager.Instance.GetPanel<SocialPanel>().npcChatPanel.CloseOptionPanel();
                });
            }

            //OpenOptionPanel(optionDialogue, buttonHandlers);
            UIManager.Instance.GetPanel<SocialPanel>().npcChatPanel.OpenOptionPanel(optionDialogue, buttonHandlers);
        }

        // 关闭聊天面板协程
        public void StopChatCoroutine()
        {
            StopCoroutine(_currentCoroutine);
        }



        #endregion


        #region UI交互

        // 激活选项面板，然后创建选项
        //private void OpenOptionPanel(OptionDialogueModel optionDialogue, List<UnityAction> buttonClickHandlers)
        //{
        //    // 先设置数据
        //    optionPanelView.SetOptionDatas(optionDialogue, buttonClickHandlers);
        //    // 然后让 UI 层显示
        //    optionPanelView.gameObject.SetActive(true);
        //    optionPanelView.DisplayOptions();
        //}

        //private void OpenOptionPanel(OptionDialogueModel optionDialogue, List<UnityAction> buttonClickHandlers)
        //{
        //    //EventCenter.Publish<SetOptionDataAndDisplayEvent>(new SetOptionDataAndDisplayEvent());
        //    EvtDsp.TriggerEvt<>("SetOptionDataAndDisplayEvent");
        //}



        #endregion


        #region 测试

        /// <summary>
        /// 加载并且打印所有对话内容
        /// </summary>
        //public void TestLoadAndPrintAllDialogues()
        //{
        //    LoadAllDialogues();

        //    Debug.Log("====== 测试对话 ======");
        //    foreach (var diaPair in _allDialoguesDic)
        //    {
        //        if (diaPair.Value.HasOptionDialogue)
        //        {
        //            List<string> options = (diaPair.Value as OptionDialogueModel).GetOptionList();
        //            Debug.Log($"id: {diaPair.Key}, 该 Dialogue 为选项对话，共展示 {options.Count} 个选项内容: ");
        //            foreach (var option in options)
        //            {
        //                Debug.Log($"content: {option}");
        //            }
        //        }
        //        else
        //        {
        //            Debug.Log($"id: {diaPair.Key}, content: {diaPair.Value.Contents}");
        //        }
        //    }
        //    Debug.Log("====== 测试对话 ======");
        //}

        /// <summary>
        /// 加载并且打印所有对话内容（这里已经包含了 段落启动 + 对话循环播放的内容）
        /// </summary>
        //public void TestLoadAndDisplayParagragh(int paraId)
        //{
        //    LoadAllDialogues();
        //    LoadAllParagraghs();

        //    // 初始化段落信息
        //    // StartParagragh(paraId);

        //    Debug.Log("====== 开始播放对话 ======");
        //    //DisplayDialogueById(_currentPlayingDialogue.DialogueId, normalDialogueProcesser, optionProcesser);
        //    StartDisplayParagragh(paraId, 1, () =>
        //    {
        //        Debug.Log("====== 对话播放结束 ======");
        //    });
        //}

        // 测试播放长度不同的对话
        //public void TestDisplayDialoguesWithDifferentLength(int paraId)
        //{
        //    LoadAllDialogues();
        //    LoadAllParagraghs();

        //    if (!_allParagraghsDic.ContainsKey(paraId))
        //    {
        //        Debug.LogError($"id为 {paraId} 的段落不存在");
        //        return;
        //    }

        //    StartCoroutine(EnumStartDisplayParagragh());

        //    IEnumerator EnumStartDisplayParagragh()
        //    {
        //        _currentPlayingParagragh = _allParagraghsDic[paraId];
        //        _currentPlayingDialogue = _currentPlayingParagragh.GetDialogueById(_currentPlayingParagragh.FirstDialogueId);
        //        IDialogueModel dialogue = _currentPlayingDialogue;

        //        bool canTurnToNextDialogue = false;

        //        while (_currentPlayingDialogue != null)
        //        {
        //            // 这里进行定制
        //            switch (dialogue.DialogueId)
        //            {
        //                case 10001:
        //                    (dialogue as NormalDialogueModel).SetContents("Character1111111111111111111111111" +
        //                        "111111111111111111111");
        //                    break;
        //                case 10002:
        //                    (dialogue as NormalDialogueModel).SetContents("Character222222");
        //                    break;
        //                case 10003:
        //                    (dialogue as NormalDialogueModel).SetContents("Character3333333333333333333333333" +
        //                        "3333333333333333333333333333333333333333333333333333333333333333333333333333" +
        //                        "3333333333333333333333333333333333333333333333333333333333333333333333333333" +
        //                        "3333333333333333333333333333333333333333333333333333333333333333333333333333" +
        //                        "3333333333333333333333333333333333333333333333333333333333333333333333333333");
        //                    break;
        //                case 20001:
        //                    (dialogue as NormalDialogueModel).SetContents("NPC1111111111111111111111111111111" +
        //                        "1111111111111111");
        //                    break;
        //                case 20002:
        //                    (dialogue as NormalDialogueModel).SetContents("NPC222222");
        //                    break;
        //                case 10004:
        //                    (dialogue as NormalDialogueModel).SetContents("Character1");
        //                    break;
        //                default:
        //                    break;
        //            }


        //            // 1.对当前正在播放的对话进行操作（判断当前对话类型，然后更新并显示 View 层内容）
        //            if (!dialogue.HasOptionDialogue)
        //            {
        //                // 正常展示对话内容（可能是以动画的形式展现，展示选项同理）
        //                //Debug.Log($"id: {dialogue.DialogueId}, content: {dialogue.Contents}");
        //                OnNormalDialogueProcess(dialogue, 1, () => { canTurnToNextDialogue = true; });
        //            }
        //            else
        //            {
        //                // 展示选项
        //                //List<string> options = (dialogue as OptionDialogueModel).GetOptionList();
        //                //Debug.Log($"id: {dialogue.DialogueId}, 该 Dialogue 为选项对话，共展示 {options.Count} 个选项内容: ");
        //                //foreach (var option in options)
        //                //{
        //                //    Debug.Log($"content: {option}");
        //                //}
        //                OnOptionProcess(dialogue as OptionDialogueModel, () => { canTurnToNextDialogue = true; }, 1);
        //            }

        //            // 2.等待一段时间，或等待前面内容完成（就是等待一定的动画时间，或者等待 View 层传来结束信号）
        //            // yield return new WaitForSeconds(1f);

        //            yield return new WaitForNextFrameUnit();
        //            while (!canTurnToNextDialogue)
        //            {
        //                yield return null;
        //            }
        //            canTurnToNextDialogue = false;

        //            // 3.转到下一句对话
        //            if (dialogue.NextDialogueId != -1)
        //            {
        //                _currentPlayingDialogue = _currentPlayingParagragh.GetDialogueById(dialogue.NextDialogueId);
        //                dialogue = _currentPlayingDialogue;
        //            }
        //            else
        //            {
        //                Debug.Log("当前段落结束");
        //                break;
        //            }
        //        }

        //        Debug.Log("====== 对话播放结束 ======");
        //    }
        //}


        #endregion

        private string GetFavorSavePath()
        {
            string dir = Path.Combine(
                Application.persistentDataPath,
                "NPCFavorDatas"
            );

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return Path.Combine(dir, "npc_favor_save.json");
        }





        // ======================================   原 NPCFavorController   =============================================
        #region 好感度相关数据操作（初始化，查，存，加载）

        // ========== 初始化 ==========

        /// <summary>
        /// 初始化好感度相关数据（主要是创建 LocalFavorDatas，检测本地文件是否存在）
        /// </summary>
        public void InitFavorDatas(List<NPC_Info> infos)
        {
            LocalFavorDatas = new Dictionary<int, LocalNPCFavorData>();
            //_absolutePath = Application.dataPath + "/LYC/Resources/NPCFavorDatas/npcFavorDatas.json";
            //_absolutePath = Application.dataPath + "/LYC/Resources/" + _npcFavorSavePath + ".json";

            var path = GetFavorSavePath();

            // 先要加载所有的 NPC 以及他们的好感度相关信息
            LoadParaIdOfEachFavorLevelDic();

            //Debug.Log($"NPC 好感度数据 将要存储的路径是：{_npcFavorSavePath}");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                LocalFavorDatas = JsonConvert.DeserializeObject<Dictionary<int, LocalNPCFavorData>>(json);

                Debug.Log($"检测到 NPC 好感度数据 在存储路径 {_absolutePath} 上存在");

                CheckAndFilterUnexistDataFile(infos);

                CheckAndInitAllDialoguesToTrigger(infos);

                return;
            }

            Debug.Log($"NPC 好感度数据 在存储路径上不存在，开始进行初始化操作");
            CreateAndFilterAllDataFile(infos);
        }

        // 创建并且填充所有 LocalFavorDatas 文件
        private void CreateAndFilterAllDataFile(List<NPC_Info> infos)
        {
            Dictionary<int, LocalNPCFavorData> datas = new Dictionary<int, LocalNPCFavorData>();
            foreach (var info in infos)
            {
                datas[info._npc_Base.npc_id] = new LocalNPCFavorData(info._npc_Base.npc_id,
                    info._npc_RuntimeData.favor_level, info._npc_RuntimeData.favor_Value);
                // 初始化 DialoguesToTrigger 数据
                if (ParaIdOfEachFavorLevelDic.ContainsKey(info._npc_Base.npc_id))
                {
                    datas[info._npc_Base.npc_id].InitDialoguesToTrigger
                        (ParaIdOfEachFavorLevelDic[info._npc_Base.npc_id]);
                }
            }

            SaveNPCFavorToLocal(datas);
        }

        // 检查并且填充不存在于 LocalFavorDatas 里面的 NPC 好感度数据文件
        private void CheckAndFilterUnexistDataFile(List<NPC_Info> infos)
        {
            Debug.Log($"开始检查是否存在 未添加的 NPC，并且对该 NPC 进行信息添加");
            foreach (var info in infos)
            {
                if (!LocalFavorDatas.ContainsKey(info._npc_Base.npc_id))
                {
                    LocalFavorDatas[info._npc_Base.npc_id] = new LocalNPCFavorData(info._npc_Base.npc_id,
                info._npc_RuntimeData.favor_level, info._npc_RuntimeData.favor_Value);
                    // 初始化 DialoguesToTrigger 数据
                    if (ParaIdOfEachFavorLevelDic.ContainsKey(info._npc_Base.npc_id))
                    {
                        LocalFavorDatas[info._npc_Base.npc_id].InitDialoguesToTrigger
                            (ParaIdOfEachFavorLevelDic[info._npc_Base.npc_id]);
                    }
                }
            }

            SaveNPCFavorToLocal(LocalFavorDatas);
        }

        // 检查并且初始化 LocalFavorDatas 里面的 DialoguesToTrigger
        private void CheckAndInitAllDialoguesToTrigger(List<NPC_Info> infos)
        {
            Debug.Log($"开始检查是否存在 未添加的 NPC，并且对该 NPC 进行信息添加");
            foreach (var info in infos)
            {
                // 初始化 DialoguesToTrigger 数据
                if (ParaIdOfEachFavorLevelDic.ContainsKey(info._npc_Base.npc_id))
                {
                    LocalFavorDatas[info._npc_Base.npc_id].InitDialoguesToTrigger
                        (ParaIdOfEachFavorLevelDic[info._npc_Base.npc_id]);
                }
            }

            SaveNPCFavorToLocal(LocalFavorDatas);
        }

        private void UpdateDialogueUnlockStatus(NPC_RuntimeData newData)
        {
            if (LocalFavorDatas.TryGetValue(newData.npc_id, out LocalNPCFavorData data))
            {
                data.UpdateUnlockStatus(newData.favor_level);
            }
        }

        // ========== 初始化 ==========


        // ========== 查 ==========

        /// <summary>
        /// 尝试获取目标 NPC 在打开对话窗口时，需要播放的段落Id
        /// </summary>
        public bool TryGetPendingParaIdByNPCId(NPC_RuntimeData newData, out int paraId, out int nextDiaAtFavorLevel)
        {
            // 打印调试数据
            Debug.Log($"当前好感度等级为：{newData.favor_level}");
            LocalFavorDatas[newData.npc_id].PrintDialogueToTriggerData();

            // 首先检查并更新解锁状态
            UpdateDialogueUnlockStatus(newData);
            // 然后判断是否存在待触发的对话
            if (HasPendingDialogue(newData))
            {
                Debug.Log("检测到当前有要播放的好感度对话");
                paraId = LocalFavorDatas[newData.npc_id].GetNextPendingDialogue(newData.favor_level, out nextDiaAtFavorLevel).ParagraphId;
                return true;
            }

            //Debug.Log("当前没有好感度对话要播放");
            paraId = -1;
            nextDiaAtFavorLevel = -1;
            return false;
        }

        /// <summary>
        /// 在点击 NPC 聊天格子时，首先调用这个方法，对是否触发剧情进行判断
        /// </summary>
        /// <param name="newData"></param>
        /// <returns></returns>
        public bool HasPendingDialogue(NPC_RuntimeData newData)
        {
            if (!LocalFavorDatas.ContainsKey(newData.npc_id))
            {
                // 主动更新当前 NPC 的数据
                UpdateFavorInMemory(newData);
                SaveNPCFavorToLocal(LocalFavorDatas);
            }

            if (LocalFavorDatas == null || !LocalFavorDatas.ContainsKey(newData.npc_id))
            {
                Debug.Log($"当前 LocalFavorDatas 不存在 id 为 {newData.npc_id} 的对话");
                return false;
            }

            return LocalFavorDatas[newData.npc_id].HasPendingDialogues;
        }

        // ========== 查 ==========


        // ========== 存 ==========

        // 收到单个 NPC favor 变动时，对数据进行备份和保存


        private void OnReceiveNPCFavorUpdate(NPC_RuntimeData newData)
        {
            // 确保内存数据存在
            if (LocalFavorDatas == null)
            {
                LocalFavorDatas = LoadFavorFromDisk();
            }

            // 更新内存数据
            UpdateFavorInMemory(newData);

            // 保存到磁盘
            SaveNPCFavorToLocal(LocalFavorDatas);
        }

        private Dictionary<int, LocalNPCFavorData> LoadFavorFromDisk()
        {
            var path = GetFavorSavePath();

            if (!File.Exists(path))
            {
                return new Dictionary<int, LocalNPCFavorData>();
            }

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Dictionary<int, LocalNPCFavorData>>(json)
                   ?? new Dictionary<int, LocalNPCFavorData>();
        }

        private void UpdateFavorInMemory(NPC_RuntimeData newData)
        {
            if (!LocalFavorDatas.TryGetValue(newData.npc_id, out var favorData))
            {
                favorData = new LocalNPCFavorData(
                    newData.npc_id,
                    newData.favor_level,
                    newData.favor_Value
                );

                LocalFavorDatas[newData.npc_id] = favorData;
            }

            favorData.UpdateUnlockStatus(newData.favor_level);
            favorData.SetCurrentFavorLevel(newData.favor_level);
            favorData.SetCurrentFavorValue(newData.favor_Value);
        }



        //private void OnReceiveNPCFavorUpdate(NPC_RuntimeData newData)
        //{
        //    // 首先判断是否存在保存的地址
        //    if (!File.Exists(_absolutePath))
        //    {
        //        // 如果不存在就创建初始化数据
        //        Dictionary<int, LocalNPCFavorData> initDatas = new Dictionary<int, LocalNPCFavorData>();
        //        initDatas[newData.npc_id] = new LocalNPCFavorData(newData.npc_id, newData.favor_level, newData.favor_Value);
        //        UpdateLocalFavorDatas(newData);
        //        SaveNPCFavorToLocal(initDatas);
        //        return;
        //    }

        //    // 更新 类中的 LocalFavorDatas
        //    UpdateLocalFavorDatas(newData);

        //    // 主动保存 NPC 数据到硬盘
        //    SaveNPCFavorToLocal(LocalFavorDatas);
        //}

        //private void UpdateLocalFavorDatas(NPC_RuntimeData newData)
        //{
        //    // 加载存储在本地的 NPC 数据
        //    //TextAsset jsonFile = Resources.Load<TextAsset>(_npcFavorSavePath);

        //    var json = GetFavorSavePath();

        //    bool isNull = true;
        //    if (jsonFile == null)
        //    {
        //        // 如果不存在就创建初始化数据
        //        Dictionary<int, LocalNPCFavorData> initDatas = new Dictionary<int, LocalNPCFavorData>();
        //        initDatas[newData.npc_id] = new LocalNPCFavorData(newData.npc_id, newData.favor_level, newData.favor_Value);
        //        SaveNPCFavorToLocal(initDatas);
        //        // 然后再次加载
        //        StartCoroutine(CheckIfJsonFileCanLoad());
        //        return;
        //    }

        //    IEnumerator CheckIfJsonFileCanLoad()
        //    {

        //        while (isNull)
        //        {
        //            jsonFile = Resources.Load<TextAsset>(_npcFavorSavePath);
        //            if (jsonFile != null)
        //            {
        //                isNull = false;
        //            }
        //            yield return null;
        //        }

        //        UpdateLocalFavorDatas(newData);
        //    }


        //    // 先创建一个
        //    Dictionary<int, LocalNPCFavorData> oldDatas
        //        = JsonConvert.DeserializeObject<Dictionary<int, LocalNPCFavorData>>(jsonFile.text);

        //    if (oldDatas == null)
        //    {
        //        oldDatas = new Dictionary<int, LocalNPCFavorData>();
        //        oldDatas[newData.npc_id] = new LocalNPCFavorData(newData.npc_id, newData.favor_level, newData.favor_Value);
        //        Debug.Log($"这是 {_npcFavorSavePath} 路径下文件的 第一次初始化");
        //    }
        //    else if (!oldDatas.ContainsKey(newData.npc_id))
        //    {
        //        // 如果进到这里，表示这是该 NPC 第一次检测到被修改，需要添加到本地数据中
        //        oldDatas[newData.npc_id] = new LocalNPCFavorData(newData.npc_id, newData.favor_level, newData.favor_Value);
        //    }

        //    // 更新数据
        //    oldDatas[newData.npc_id].UpdateUnlockStatus(newData.favor_level);
        //    oldDatas[newData.npc_id].SetCurrentFavorLevel(newData.favor_level);
        //    oldDatas[newData.npc_id].SetCurrentFavorValue(newData.favor_Value);

        //    // 这里需要进行判断：是否需要新添加

        //    // 然后覆盖为新数据
        //    LocalFavorDatas = oldDatas;
        //}

        // 将 NPC 当前的 favor 值保存到本地
        private void SaveNPCFavorToLocal(Dictionary<int, LocalNPCFavorData> newData)
        {
            // Formatting.Indented 可以使得生成的 Json 文本可读性好一些
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                // NullValueHandling = NullValueHandling.Ignore, // 可选：去掉 null
            };
            string json = JsonConvert.SerializeObject(newData, settings);

            var path = GetFavorSavePath();
            File.WriteAllText(path, json);
            Debug.Log($"[Save] → {path}");
        }

        // ========== 存 ==========


        // ========== 加载 ==========

        // 加载 ParaIdOfEnchFavorLevelDic
        public void LoadParaIdOfEachFavorLevelDic()
        {
            ParaIdOfEachFavorLevelDic = new Dictionary<int, Dictionary<int, int>>();

            TextAsset jsonFile = Resources.Load<TextAsset>(_paraIdOfEachFavorLevelLoadPath);

            if (jsonFile != null)
            {
                // 逐条读取文件信息
                using (var reader = new JsonTextReader(new StringReader(jsonFile.text)))
                {
                    // 把整个文件当成 JArray 一次性加载，再逐条转实体
                    JArray arr = JArray.Load(reader);
                    foreach (JToken tok in arr)
                    {
                        NPCFavorCorrelativeData data = tok.ToObject<NPCFavorCorrelativeData>();
                        ParaIdOfEachFavorLevelDic.Add(data.npcId, data.paraIdOfEachFavorLevel);
                    }
                    Debug.Log("ParaIdOfEnchFavorLevelDic 加载完毕");
                    return;
                }
            }
            else
            {
                Debug.Log($"请检查 {_paraIdOfEachFavorLevelLoadPath} 是否为有效路径");
            }

        }

        /// <summary>
        /// 加载与该 NPC 进行的所有过往聊天记录
        /// </summary>
        public void LoadPastChatHistory(NPC_RuntimeData newData)
        {
            UIManager.Instance.GetPanel<SocialPanel>().npcChatPanel.ClearOptions();
            if (LocalFavorDatas.TryGetValue(newData.npc_id, out LocalNPCFavorData data))
            {
                List<int> paraIds;
                // 获取要提前加载的聊天记录对应的 paraId
                paraIds = data.GetParaIdListTriggered(newData.favor_level);
                foreach (int id in paraIds)
                {
                    // 逐 Paragragh 加载
                    LoadChatHistoryOfPara(id, newData.npc_id);
                }
            }
        }

        // 加载单个 Paragragh 的聊天记录
        private void LoadChatHistoryOfPara(int paraId, int npcId)
        {
            _currentPlayingParagragh = _allParagraghsDic[paraId];
            _currentPlayingDialogue = _currentPlayingParagragh.GetDialogueById(_currentPlayingParagragh.FirstDialogueId);
            IDialogueModel dialogue = _currentPlayingDialogue;

            while (_currentPlayingDialogue != null)
            {
                // 1.对当前正在播放的对话进行操作（判断当前对话类型，然后更新并显示 View 层内容）
                if (!dialogue.HasOptionDialogue)
                {
                    // 正常展示对话内容（可能是以动画的形式展现，展示选项同理）
                    OnNormalDialogueLoad(dialogue, npcId);
                }
                else
                {
                    // 展示过往选项内容
                    OnOptionLoad(dialogue as OptionDialogueModel, npcId);
                }

                // 2.等待一段时间，或等待前面内容完成（就是等待一定的动画时间，或者等待 View 层传来结束信号）
                // yield return new WaitForSeconds(1f);

                // 3.转到下一句对话
                if (dialogue.NextDialogueId != -1)
                {
                    _currentPlayingDialogue = _currentPlayingParagragh.GetDialogueById(dialogue.NextDialogueId);
                    dialogue = _currentPlayingDialogue;
                }
                else
                {
                    Debug.Log("当前段落加载结束");
                    break;
                }
            }
        }

        // 加载一般对话
        private void OnNormalDialogueLoad(IDialogueModel dialogue, int npcId)
        {
            Sprite sprite = NPCHeadIcons[npcId - 1];
            string speakerName = null;
            string someText = dialogue.Contents;
            string chatHint = dialogue.ChatHints;

            if (dialogue.Speaker == EnumDialogueSpeaker.Character)
            {
                speakerName = "小苔";
                chatViewController.LoadSingleCharacterMsg(sprite, speakerName, someText, chatHint);
            }
            else if (dialogue.Speaker == EnumDialogueSpeaker.NPC)
            {
                //speakerName = "NPC 1";
                speakerName = AllNPCChatDataDic[npcId]._npc_Base.npc_name;
                chatViewController.LoadSingleNPCMsg(sprite, speakerName, someText, chatHint);
            }
        }

        // 加载选项选择对话
        private void OnOptionLoad(OptionDialogueModel optionDialogue, int npcId)
        {
            Sprite sprite = XiaoTaiHeadIcon;
            string speakerName = null;
            string someText = null;
            string chatHint = optionDialogue.ChatHints;

            if(LocalFavorDatas[npcId].OptionChoicesDic == null || 
                !LocalFavorDatas[npcId].OptionChoicesDic.ContainsKey(optionDialogue.DialogueId))
            {
                // 不存在则代表出现异常，默认显示第一句对话
                if (LocalFavorDatas[npcId].OptionChoicesDic == null)
                {
                    LocalFavorDatas[npcId].OptionChoicesDic = new Dictionary<int, string>();
                }

                LocalFavorDatas[npcId].OptionChoicesDic[optionDialogue.DialogueId] = optionDialogue.Options.Keys.First();

                SaveNPCFavorToLocal(LocalFavorDatas);
            }

            // 接着获取该对话的过往选择记录
            string optionText = LocalFavorDatas[npcId].OptionChoicesDic[optionDialogue.DialogueId];
            // 并且设置下一句对话的 id
            optionDialogue.SetNextDialogueId(optionDialogue.Options[optionText]);
            // 然后得到该选相对应的文字
            //someText = optionDialogue.Options[optionDialogue.OptionKeys[optionIndex]];
            // 然后直接加载该条消息
            chatViewController.LoadSingleCharacterMsg(sprite, speakerName, optionText, chatHint);
        }


        public void TriggeredDialogueAt(int npcId, int favorLevel)
        {
            LocalFavorDatas[npcId].TriggeredDialogueAt(favorLevel);

            // 然后将信息保存到硬盘
            SaveNPCFavorToLocal(LocalFavorDatas);
        }

        // ========== 加载 ==========

        #endregion


        #region 测试

        public void Test_LoadPastChatHistory(NPC_RuntimeData runtimeData)
        {
            // 创建测试数据
            LocalNPCFavorData testData = new LocalNPCFavorData(1, 2, 30);
            testData.InitDialoguesToTrigger(ParaIdOfEachFavorLevelDic[runtimeData.npc_id]);

            // 解锁两个剧情片段，并且设置为已触发
            int count = 0;
            foreach (var dic in ParaIdOfEachFavorLevelDic)
            {
                if(count < 2)
                {
                    testData.UnlockDialogueAt(dic.Key);
                    testData.TriggeredDialogueAt(dic.Key);
                    count++;
                }
                else
                {
                    break;
                }
            }

            // 这里直接设置为已选择
            testData.AddChoiceRecord(30001, testData.OptionChoicesDic.Values.First());

            // 获取要提前加载的聊天记录对应的 paraId
            List<int> paraIds = testData.GetParaIdListTriggered(runtimeData.favor_level);
            foreach (int id in paraIds)
            {
                // 逐 Paragragh 加载
                LoadChatHistoryOfPara(id, runtimeData.npc_id);
            }
        }

        #endregion
        // ======================================   原 NPCFavorController   =============================================
    }
}
