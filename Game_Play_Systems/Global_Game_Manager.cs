using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Kernel.Social;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using Google.Protobuf;
using Newtonsoft.Json;
using Cmd;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;
using PM_RM = CLIP.Framework_Unity.Asset.GameAssets;


namespace CLIP.Project_Mouse.Game_Play_System
{
    public class Global_Game_Manager : SingletonMono<Global_Game_Manager>
    {
        protected override bool PersistAcrossScenes => true;

        [Header("Global_State")]
        public string _current_player_id = "Default_Player_ID";
        public string _current_player_name = "Default_Player";

        [HideInInspector]
        public Common.ServerRoleInfo _current_player_role_info;

        /// <summary>1332 小猫信息（对应 <see cref="Cmd.GetCatInfoRes.Cat"/>）。</summary>
        public CatInfo _current_cat_info;
        public Weather_State _weather_state;
        public Player_Social_Setting _player_brief;
        [Header("Config")]
        public global_const _global_const;
        public List<avatar_icon_info> _avatar_icon_list;
        public List<avatar_icon_info> _avatar_icon_frame_list;
        public TextAsset _avatar_icon_json;

        [Header("Inventory")]
        public List<Game_Item_Info> _gameItem_db;
        public Dictionary<int, Game_Item_Info> _gameItem_idDic = new Dictionary<int, Game_Item_Info>();
        public Dictionary<string, Game_Item_Info> _gameItem_nameDic = new Dictionary<string, Game_Item_Info>();

        [Header("Debug")]
        [Tooltip("勾选后在 Console 输出小猫在家/外出判定与 MapPanel 头像显隐，便于排查；上线请关闭。")]
        [SerializeField] bool _debugLogCatPresence = true;

        [Header("JSON_Data")]
        public bool _global_const_load_from_json = true;
        public TextAsset _global_const_json;

        [Header("Event")]
        public UnityEvent _on_quit_game = new UnityEvent();
        public UnityEvent _update_main_character_cloth_from_server = new UnityEvent();
        public UnityEvent _upload_main_character_cloth_to_server = new UnityEvent();
        public UnityEvent _update_weather_state_from_server = new UnityEvent();
        public UnityEvent _upload_weather_state_to_server = new UnityEvent();

        public UnityEvent _on_update_player_brief_from_server = new UnityEvent();
        public UnityEvent _on_upload_player_brief_to_server = new UnityEvent();

        /// <summary>登录后必拉数据：低 32 位为各 index 已回包标记，高 32 位为各 index 已登记待拉取标记。</summary>
        public long LoginProcessFlag;

        /// <summary>登录后待发送的请求（msg_id + proto 请求体）。</summary>
        public readonly List<IMessage> InitDatas = new List<IMessage>();

        /// <summary>1308 <see cref="Cmd.GetRoomDataRes"/> 解析后的全屋存档；由 <see cref="RoomSystem"/> 在首帧初始化后取出并清空。</summary>
        RoomSaveData _pendingRoomSaveDataFromServer;
        RoomSaveData _cachedRoomSaveData;

        /// <summary>暂存 <see cref="RoomSaveData"/>（登录场景先于 MainScene 收到 1308 时使用）。</summary>
        public void SetPendingRoomSaveDataFromServer(RoomSaveData data)
        {
            _pendingRoomSaveDataFromServer = data;
            CacheRoomSaveData(data);
        }

        public void CacheRoomSaveData(RoomSaveData data)
        {
            if (data == null || data.rooms == null || data.rooms.Count == 0)
                return;

            _cachedRoomSaveData = GF_SP.DeserializeObject<RoomSaveData>(GF_SP.SerializeObject(data));
        }

        public bool TryGetCachedRoomSaveData(out RoomSaveData data)
        {
            data = _cachedRoomSaveData != null
                ? GF_SP.DeserializeObject<RoomSaveData>(GF_SP.SerializeObject(_cachedRoomSaveData))
                : null;
            return data != null && data.rooms != null && data.rooms.Count > 0;
        }

        /// <summary>取出并清空暂存房间存档；若无有效数据返回 false。</summary>
        public bool TryTakePendingRoomSaveData(out RoomSaveData data)
        {
            data = _pendingRoomSaveDataFromServer;
            _pendingRoomSaveDataFromServer = null;
            return data != null && data.rooms != null && data.rooms.Count > 0;
        }

        /// <summary>由 HotUpdate 侧（如 <c>RoomSystem_Receiver</c>）注册：在暂存数据被取出后调用 <see cref="RoomSystem.ResetStateFromData"/>。</summary>
        Action<RoomSaveData> _applyRoomSaveDataFromServerHandler;

        public void RegisterApplyRoomSaveDataFromServerHandler(Action<RoomSaveData> handler)
        {
            _applyRoomSaveDataFromServerHandler = handler;
        }

        /// <summary>
        /// 由 <see cref="RoomSystem"/> 的 <see cref="RoomSystem.Awake"/> 调用：下一帧取出暂存数据并交给已注册的处理器（避免与 HotUpdate 程序集循环引用）。
        /// </summary>
        public void ScheduleApplyPendingRoomSaveAfterRoomSystemStart()
        {
            StartCoroutine(ApplyPendingRoomSaveAfterRoomSystemStartCoroutine());
        }

        IEnumerator ApplyPendingRoomSaveAfterRoomSystemStartCoroutine()
        {
            yield return null;
            bool hasPending = TryTakePendingRoomSaveData(out var saveData);
            if (!hasPending && (!SceneLoadHelper.IsMainScene || !TryGetCachedRoomSaveData(out saveData)))
                yield break;
            if (_applyRoomSaveDataFromServerHandler == null)
            {
                Debug.LogWarning("[Global_Game_Manager] 有待应用房间存档但未注册 ApplyRoomSaveDataFromServerHandler（RoomSystem_Receiver 尚未 Start），已放回暂存。");
                if (hasPending)
                    SetPendingRoomSaveDataFromServer(saveData);
                yield break;
            }
            _applyRoomSaveDataFromServerHandler.Invoke(saveData);
        }

        /// <summary>登录必拉数据已全部回包并就绪（<see cref="EvtNames.Login_Mandatory_Data_Ready"/>），用于区分「尚未拉到小猫」与「必拉后仍无 Cat」。</summary>
        bool _mandatoryLoginDataReady;

        string _debugLastPresenceSig;
        string _debugLastMapPanelSig;

        /// <summary>Inspector：是否打印小猫状态调试日志。</summary>
        public bool DebugLogCatPresence => _debugLogCatPresence;

        /// <summary>单行摘要：当前场景、必拉标记、CatInfo.Status（显隐仅由此字段决定）、ShouldShow、IsTraveling。</summary>
        public string FormatCatPresenceDebugLine()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("scene=").Append(SceneManager.GetActiveScene().name);
            sb.Append(" mandatoryReady=").Append(_mandatoryLoginDataReady);
            if (_current_cat_info == null)
            {
                sb.Append(" cat=null");
            }
            else
            {
                var c = _current_cat_info;
                sb.Append(" Status=").Append(c.Status);
                if (c.CurrentTravel != null)
                    sb.Append(" [dbg]CurrentTravel(dest=").Append(c.CurrentTravel.Destination).Append(')');
            }

            sb.Append(" ShouldShow=").Append(ShouldShowIndoorMainCharacter());
            sb.Append(" IsTraveling=").Append(IsCatCurrentlyTraveling());
            return sb.ToString();
        }

        /// <summary>调试：同一快照只打一条，避免刷屏。</summary>
        public void LogCatPresenceIfChanged(string reason)
        {
            if (!_debugLogCatPresence)
                return;
            string sig = FormatCatPresenceDebugLine();
            if (sig == _debugLastPresenceSig)
                return;
            _debugLastPresenceSig = sig;
            Debug.Log($"[CatPresenceDebug] {sig} | trigger={reason}");
        }

        /// <summary>调试：每次调用都打（用于网络回包等明确时点）。</summary>
        public void LogCatPresenceAlways(string reason)
        {
            if (!_debugLogCatPresence)
                return;
            _debugLastPresenceSig = FormatCatPresenceDebugLine();
            Debug.Log($"[CatPresenceDebug] {_debugLastPresenceSig} | trigger={reason}");
        }

        /// <summary>MapPanel 应用头像显隐后调用：仅在组合状态变化时打印。</summary>
        public void LogCatPresenceMapPanelIfChanged(bool intendedShow, bool iconGameObjectActiveSelf)
        {
            if (!_debugLogCatPresence)
                return;
            string key = intendedShow + "|" + iconGameObjectActiveSelf + "|" + FormatCatPresenceDebugLine();
            if (key == _debugLastMapPanelSig)
                return;
            _debugLastMapPanelSig = key;
            Debug.Log($"[CatPresenceDebug] MapPanel intendedShow={intendedShow} iconActiveSelf={iconGameObjectActiveSelf} | {FormatCatPresenceDebugLine()} | trigger=MapPanel");
        }

        /// <summary>CatInfo.Status：0 在家，1 出游（simglecmd.proto）。</summary>
        public const int CatStatusAtHome = 0;
        public const int CatStatusTraveling = 1;

        /// <summary>小猫是否处于出游（出门）状态；仅看 <see cref="CatInfo.Status"/>（与 <see cref="CatStatusTraveling"/> 一致）。</summary>
        public bool IsCatCurrentlyTraveling()
        {
            return _current_cat_info != null && _current_cat_info.Status == CatStatusTraveling;
        }

        /// <summary>
        /// 室内小苔根节点 / 房间小地图头像是否应显示。
        /// 仅当存在 <see cref="_current_cat_info"/> 且 <see cref="CatInfo.Status"/> 为 <see cref="CatStatusAtHome"/>（0）。
        /// <see cref="CatInfo.CurrentTravel"/>、<see cref="CatInfo.Wears"/> 等不参与显隐判断。
        /// </summary>
        public bool ShouldShowIndoorMainCharacter()
        {
            return _current_cat_info != null && _current_cat_info.Status == CatStatusAtHome;
        }

        /// <summary>由 HotUpdate 在收到 <see cref="EvtNames.Login_Mandatory_Data_Ready"/> 时调用。</summary>
        public void NotifyMandatoryLoginDataReady()
        {
            _mandatoryLoginDataReady = true;
            LogCatPresenceAlways("NotifyMandatoryLoginDataReady");
        }

        /// <summary>确保存在 <see cref="_current_cat_info"/> 并设置 <see cref="CatInfo.Status"/>（与本地派遣流程对齐）；不修改 <see cref="CatInfo.CurrentTravel"/>。</summary>
        public void EnsureCatInfoAndSetStatus(int status)
        {
            if (_current_cat_info == null)
                _current_cat_info = new CatInfo();
            _current_cat_info.Status = status;
            ApplyIndoorMainCharacterVisibilityFromCatStatus();
            LogCatPresenceAlways($"EnsureCatInfoAndSetStatus({status})");
        }

        /// <summary>按 <see cref="ShouldShowIndoorMainCharacter"/> 同步室内小苔根节点显隐；显示时顺带刷新渲染状态。</summary>
        public void ApplyIndoorMainCharacterVisibilityFromCatStatus()
        {
            if (IndoorMainCharacter._instance == null)
                return;
            bool show = ShouldShowIndoorMainCharacter();
            var root = IndoorMainCharacter._instance.gameObject;
            if (root.activeSelf == show)
                return;
            root.SetActive(show);
            if (_debugLogCatPresence)
                Debug.Log($"[CatPresenceDebug] IndoorMainCharacter root SetActive({show}) | {FormatCatPresenceDebugLine()} | trigger=ApplyIndoorVisibility");
            if (show)
                EvtDsp.TriggerEvt(EvtNames.ReSetMainCharacterRenderer);
        }

        /// <summary>仅在 MainScene 每帧根据 <see cref="_current_cat_info"/>.Status 对齐室内小猫显隐（替代仅靠派遣/CatInfo 事件刷新）。</summary>
        void Update()
        {
            if (!SceneLoadHelper.IsMainScene)
                return;
            ApplyIndoorMainCharacterVisibilityFromCatStatus();
        }

        /// <summary>
        /// 用 <see cref="CatInfo"/> 的 Bag1~Bag3 覆盖派遣三格背包（与 <see cref="DispatchBagInfo"/> 的 food/snack/tape 对应）。
        /// </summary>
        public void ReplaceDispatchBagsFromCurrentCatInfo()
        {
            if (_current_cat_info == null || Dispatch_Manager._instance == null)
                return;

            var cat = _current_cat_info;
            Dispatch_Manager._instance.dispatch_Bags = new List<DispatchBagInfo>(3)
            {
                DispatchBagFromCatTravel(cat.Bag1),
                DispatchBagFromCatTravel(cat.Bag2),
                DispatchBagFromCatTravel(cat.Bag3),
            };
        }

        static DispatchBagInfo DispatchBagFromCatTravel(CatTravelBagInfo src)
        {
            if (src == null)
                return new DispatchBagInfo();

            string food = ResolveDispatchItemNameByConfigId(src.FoodItem);
            string snack = ResolveDispatchItemNameByConfigId(src.LuckyItem);
            string tape = ResolveTapeDisplayNameByRecordUid(src.RecordUID);
            var bag = new DispatchBagInfo
            {
                foodName = food,
                snackName = snack,
                tapeName = tape,
            };
            bag.SyncPackedFlag();
            return bag;
        }

        static string ResolveDispatchItemNameByConfigId(long itemId)
        {
            if (itemId == 0)
                return string.Empty;
            var list = Global_Inventory_Manager.GameItem_DB;
            if (list == null)
                return string.Empty;
            int id = unchecked((int)itemId);
            var info = list.FirstOrDefault(x => x != null && x.item_id == id);
            return info != null && !string.IsNullOrEmpty(info.name) ? info.name : string.Empty;
        }

        static string ResolveTapeDisplayNameByRecordUid(ulong recordUid)
        {
            if (recordUid == 0 || Global_Inventory_Manager.Instance == null)
                return string.Empty;
            foreach (var inv in Global_Inventory_Manager.Items)
            {
                if (inv == null || (ulong)inv.uid != recordUid)
                    continue;
                if (inv.item_info != null && inv.item_info.type == Item_Type.Tape)
                    return !string.IsNullOrEmpty(inv.item_name) ? inv.item_name : inv.item_info.name ?? string.Empty;
                return !string.IsNullOrEmpty(inv.item_name) ? inv.item_name : inv.item_info?.name ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// 将派遣 UI 的 <see cref="DispatchBagInfo"/> 转为协议 <see cref="CatTravelBagInfo"/>（便当/小物配置 id + 唱片实例 RecordUID）。
        /// </summary>
        public static CatTravelBagInfo BuildCatTravelBagInfoFromDispatchBag(DispatchBagInfo bag)
        {
            if (bag == null)
                return new CatTravelBagInfo();
            return new CatTravelBagInfo
            {
                FoodItem = ResolveItemConfigIdByDisplayNameAndType(bag.foodName, Item_Type.Food),
                LuckyItem = ResolveItemConfigIdByDisplayNameAndType(bag.snackName, Item_Type.Snack),
                RecordUID = ResolveTapeRecordUidByDisplayName(bag.tapeName)
            };
        }

        static long ResolveItemConfigIdByDisplayNameAndType(string displayName, Item_Type type)
        {
            if (string.IsNullOrEmpty(displayName) || Global_Inventory_Manager.GameItem_DB == null)
                return 0;
            var info = Global_Inventory_Manager.GameItem_DB.FirstOrDefault(x => x != null && x.type == type && x.name == displayName);
            return info != null ? info.item_id : 0L;
        }

        static ulong ResolveTapeRecordUidByDisplayName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName) || Global_Inventory_Manager.Instance == null)
                return 0;
            foreach (var inv in Global_Inventory_Manager.Items)
            {
                if (inv?.item_info == null || inv.item_info.type != Item_Type.Tape)
                    continue;
                string n = !string.IsNullOrEmpty(inv.item_name) ? inv.item_name : inv.item_info.name;
                if (n == displayName)
                    return (ulong)inv.uid;
            }
            return 0;
        }

        /// <summary>
        /// 将 <see cref="PackageBagRes"/> 中的背包写回 <see cref="_current_cat_info"/>（BagIndex 1~3 对应 Bag1~Bag3），并刷新派遣三格 UI 数据。
        /// </summary>
        public void ApplyPackageBagRes(Cmd.PackageBagRes res)
        {
            if (res == null || res.Bag == null)
                return;
            if (_current_cat_info == null)
                _current_cat_info = new CatInfo();

            var bagClone = res.Bag.Clone();
            switch (res.BagIndex)
            {
                case 1:
                    _current_cat_info.Bag1 = bagClone;
                    break;
                case 2:
                    _current_cat_info.Bag2 = bagClone;
                    break;
                case 3:
                    _current_cat_info.Bag3 = bagClone;
                    break;
                default:
                    Debug.LogWarning($"[Global_Game_Manager] ApplyPackageBagRes: invalid BagIndex {res.BagIndex}");
                    return;
            }

            ReplaceDispatchBagsFromCurrentCatInfo();
            int bagIndex0 = res.BagIndex - 1;
            EvtDsp.TriggerEvt<int>(EvtNames.Dispatch_Refresh_Model, bagIndex0);
            EvtDsp.TriggerEvt<int>(EvtNames.Dispatch_Package_Bag_Res, bagIndex0);
        }

        void Start()
        {
            _weather_state = new Weather_State();
            _weather_state._on_weather_change += upload_weather_state;
            load_from_json();
        }

        public void try_update_weahter()
        {
            if (_global_const != null)
            {
                bool _flag = _weather_state.update_weather(DateTime.Now, _global_const);
                if (_flag == true)
                {
                    upload_weather_state();
                }
            }
        }

        public void load_default_character()
        {
            PM_RM.LoadMainCharacter("Default", (go) =>
            {
                var go_in_local = Instantiate(go);
            });
        }

        public void load_from_json()
        {
            if (_global_const_load_from_json == true)
            {
                if (_global_const_json != null)
                {
                    List<global_const> _data = JsonConvert.DeserializeObject<List<global_const>>(_global_const_json.text);
                    if (_data != null)
                    {
                        _global_const = _data[0];
                    }
                }
            }
        }
        public void set_up_player_info(Cmd.UserVerifyLoginRes _msg)
        {
            if (_msg == null)
                return;

            // UserVerifyLoginRes��proto���ֶΣ�UserID, RoleInfo
            _current_player_id = _msg.RoleInfo.RoleID.ToString();
            _current_player_name = _msg.RoleInfo != null && !string.IsNullOrEmpty(_msg.RoleInfo.RoleName)
                ? _msg.RoleInfo.RoleName
                : _current_player_name;
            _current_player_role_info = _msg.RoleInfo;
        }

        public void update_player_info_from_s2c(Cmd.RoleInfoChangeS2C _msg)
        {
            if (_msg == null)
                return;

            // UserVerifyLoginRes��proto���ֶΣ�UserID, RoleInfo
            _current_player_id = _msg.Info.RoleID.ToString();
            _current_player_name = _msg.Info != null && !string.IsNullOrEmpty(_msg.Info.RoleName)
                ? _msg.Info.RoleName
                : _current_player_name;
            var oldLv = _current_player_role_info.Lv;
            var oldExp = _current_player_role_info.Exp;
            _current_player_role_info = _msg.Info;
            if (oldLv != _current_player_role_info.Lv || oldExp != _current_player_role_info.Exp) {
                ExpManager.instance.OnGetExpFromServer();
            }
        }

        public Common.ServerRoleInfo get_current_player_role_info()
        {
            return _current_player_role_info;
        }

        /// <summary>当前展示用昵称，优先取服务端 <see cref="Common.ServerRoleInfo.RoleName"/>。</summary>
        public string GetPlayerNickName()
        {
            if (_current_player_role_info != null && !string.IsNullOrEmpty(_current_player_role_info.RoleName))
                return _current_player_role_info.RoleName;
            if (_player_brief != null && !string.IsNullOrEmpty(_player_brief._player_nick_name))
                return _player_brief._player_nick_name;
            return _current_player_name ?? string.Empty;
        }

        public void update_main_character_cloth()
        {
            _update_main_character_cloth_from_server.Invoke();
        }

        public void upload_main_character_cloth()
        {
            _upload_main_character_cloth_to_server.Invoke();
        }
        public void update_weather_state()
        {
            _update_weather_state_from_server.Invoke();
        }
        public void upload_weather_state()
        {
            _upload_weather_state_to_server.Invoke();
        }

        public void load_weather_state_from_json(string json)
        {
            var _neo_weather = GF_SP.DeserializeObject<Weather_State>(json);
            _weather_state._current_weather = _neo_weather._current_weather;
            _weather_state._last_update_weather_time = _neo_weather._last_update_weather_time;
            _weather_state._return_to_normal_time = _neo_weather._return_to_normal_time;
        }

        public void load_player_brief_from_json(string json)
        {
            var _brief = GF_SP.DeserializeObject<Player_Social_Setting>(json);
            if (_brief != null)
            {
                _player_brief = _brief;
            }
        }
        public void on_update_player_brief_from_server()
        {
            if (_on_update_player_brief_from_server != null) _on_update_player_brief_from_server.Invoke();
        }
        public void on_upload_player_brief_to_server()
        {
            if (_on_upload_player_brief_to_server != null) _on_upload_player_brief_to_server.Invoke();
        }

        /// <summary>新一轮登录前置拉取前清空登记与位标记。<para/><b>此处是唯一将 <see cref="_current_cat_info"/> 置为 null 的代码路径</b>（由 <see cref="Login_Manager"/> 登录成功链调用）。</summary>
        public void ClearLoginInitState()
        {
            if (_debugLogCatPresence)
            {
                Debug.LogWarning(
                    "[CatInfoTrace] ClearLoginInitState → 即将执行 _current_cat_info=null（登录成功后会先发必拉再收 1332 写入 Cat）。\n" +
                    $"此前 _current_cat_info 非空: {_current_cat_info != null}\n" +
                    new System.Diagnostics.StackTrace(true));
            }

            LoginProcessFlag = 0;
            InitDatas.Clear();
            GachaManager.Instance?.ClearServerGachaData();
            _mandatoryLoginDataReady = false;
            _current_cat_info = null;
        }

        /// <summary>登记某 index 的必拉请求：置位 (32+index)，并加入发送队列。</summary>
        public void RegisterInitDataAfterLogin(long index, IMessage req)
        {
            if (index < 0 || index > 31)
            {
                Debug.LogError($"[Global_Game_Manager] RegisterInitDataAfterLogin index out of range: {index}");
                return;
            }

            int i = (int)index;
            LoginProcessFlag |= 1L << (32 + i);
            InitDatas.Add(req);
        }

        /// <summary>按队列发送登录后必拉请求（走 <see cref="EvtNames.Send_Req_To_Server"/>）。</summary>
        public void ExecInitDataReqSend()
        {
            foreach (var entry in InitDatas)
                EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, entry);
            InitDatas.Clear();
            StartCoroutine(WatchLoginMandatoryDataTimeout(20f));
        }

        static readonly string[] LoginInitSlotNames =
        {
            "GetItemBagRes(1012)",
            "GetCatInfoRes(1332)",
            "GetAllNpcInfoRes(1316)",
            null,
            "GetRoomDataRes(1308)",
            "MissionListRes(1056)",
            "GachaListRes(1032)",
            "GetAllPlantRes(1352)",
        };

        IEnumerator WatchLoginMandatoryDataTimeout(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (_mandatoryLoginDataReady)
                yield break;

            var pending = GetPendingLoginInitSlotLabels(LoginProcessFlag);
            if (pending.Count == 0)
                yield break;

            Debug.LogError(
                "[Login] 登录必拉数据超时，仍未进入主场景。缺失回包: " + string.Join(", ", pending) +
                "。请核对服务端是否对上述 Req 均有响应（新角色常见：1308 房间为空/未初始化、1316 NPC 列表为空）。");
        }

        static List<string> GetPendingLoginInitSlotLabels(long loginProcessFlag)
        {
            var pending = new List<string>();
            for (int slot = 0; slot < 32; slot++)
            {
                long need = loginProcessFlag & (1L << (32 + slot));
                if (need == 0)
                    continue;
                long got = loginProcessFlag & (1L << slot);
                if (got != 0)
                    continue;

                string label = slot < LoginInitSlotNames.Length && !string.IsNullOrEmpty(LoginInitSlotNames[slot])
                    ? LoginInitSlotNames[slot]
                    : $"slot[{slot}]";
                pending.Add(label);
            }
            return pending;
        }

        /// <summary>某 index 回包后置位 index；若所有已登记项均已回包，则继续原登录后续流程。</summary>
        public void UpdateLoginProcessFlagAfterDataBack(long index)
        {
            Debug.Log($"[Global_Game_Manager] UpdateLoginProcessFlagAfterDataBack index: {index}");
            if (index < 0 || index > 31)
            {
                Debug.LogError($"[Global_Game_Manager] UpdateLoginProcessFlagAfterDataBack index out of range: {index}");
                return;
            }

            int i = (int)index;
            LoginProcessFlag |= 1L << i;

            bool anyRegistered = false;
            for (int slot = 0; slot < 32; slot++)
            {
                long need = LoginProcessFlag & (1L << (32 + slot));
                if (need == 0)
                    continue;
                anyRegistered = true;
                long got = LoginProcessFlag & (1L << slot);
                if (got == 0)
                {
                    var pending = GetPendingLoginInitSlotLabels(LoginProcessFlag);
                    if (pending.Count > 0)
                        Debug.Log($"[Login] 必拉回包进度 index={index} 已收；仍等待: {string.Join(", ", pending)}");
                    return;
                }
            }

            if (!anyRegistered)
                return;

            LoginProcessFlag = 0;
            Debug.Log("[Login] 登录必拉数据已全部到齐，触发 Login_Mandatory_Data_Ready。");
            EvtDsp.TriggerEvt(EvtNames.Login_Mandatory_Data_Ready);
        }

    }

}
