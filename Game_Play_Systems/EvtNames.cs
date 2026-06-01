
using System;

public static class EvtNames
{
    //登录
    public static string Login_Messsage = "Login_Messsage";

    ////家具系统
    //public static string On_Placement_Move_Start = "On_Placement_Move_Start";
    //public static string On_Placement_Move_Over = "On_Placement_Move_Over";
    //public static string On_Placement_Rotate = "On_Placement_Rotate";
    //public static string On_All_Placement_Deslect = "On_All_Placement_Deslect";

    //NPC Events
    public static string Meet_NPC = "On_Meet_NPC";
    public static string Give_Gift_TO_NPC = "Give_Gift_TO_NPC";
    public static string On_All_NPC_Data_Received = "On_All_NPC_Data_Received";
    public static string On_NPC_Data_Update = "On_NPC_Data_Update";
    public static string Receive_NPC_Gift_TO_Player = "Receive_NPC_Gift_TO_Player";
    public static string Updata_NPC_Favor_Level = "Updata_NPC_Favor_Level";

    // NPC 对话系统事件（数据层 → 业务层广播）
    public static string Evt_NPCFavorUpdated = "Evt_NPCFavorUpdated";
    public static string Evt_AllNPCFavorReceived = "Evt_AllNPCFavorReceived";
    public static string Evt_DialogueLoaded = "Evt_DialogueLoaded";

    // NPC 聊天面板事件（View ↔ Controller 解耦）
    /// <summary>Controller → View：打开选项面板，payload 为 <see cref="EvtData_OpenOption"/>。</summary>
    public static string Evt_NPCChat_OpenOption = "Evt_NPCChat_OpenOption";
    /// <summary>Controller → View：关闭选项面板，无 payload。</summary>
    public static string Evt_NPCChat_CloseOption = "Evt_NPCChat_CloseOption";
    /// <summary>Controller → View：打开聊天面板，携带 <see cref="NPC_Info"/> 初始化视图。</summary>
    //public static string Evt_NPCChat_Open = "Evt_NPCChat_Open";
    /// <summary>View → Controller：消息滚动播放完毕，推进对话状态机，无 payload。</summary>
    public static string Evt_NPCChat_MessageFinished = "Evt_NPCChat_MessageFinished";


    public static string On_Get_Exp = "On_GetExp";

    public static string Check_Chat_Play = "Check_Chat_Play";
    public static string Stop_Chat_Coroutine = "Stop_Chat_Coroutine";

    public static string Reconnect = "Reconnect";
    public static string Resume_Silent_Relogin = "Resume_Silent_Relogin";
    public static string Resume_TryLogin_From_Cache = "Resume_TryLogin_From_Cache";

    #region //UI
    //---------------------------------------------------------------------------------------------

    public static string RefreshUI = "RefreshUI";

    public static string ShowPop = "ShowPop";

    public static string ReloadPlacementData = "ReloadPlacementData";
    public static string ReloadDispatchData = "ReloadDispatchData";
    public static string ReloadClothData = "ReloadClothData";
    public static string ReloadPlantData = "ReloadPlantData";
    public static string ReloadRecycleData = "ReloadRecycleData";


    //家具UI
    public static string Open_Edit_Placement_Panel = "Open_Edit_Placement_Panel";
    public static string Open_Edit_Pot_Panel = "Open_Edit_Pot_Panel";
    public static string Close_Edit_Placement_Panel = "Close_Edit_Placement_Panel";
    public static string Move_Placement_Panel = "Move_Placement_Panel";

    //Warning面板
    public static string Show_Warning_Panel = "Show_Warning_Panel";

    //MainPanel
    public static string Set_PhoneButton_Enable = "Set_PhoneButton_Enable";
    public static string Set_MainFunction_Active = "Set_MainFunction_Active";
    public static string Set_TopPanel_Active = "Set_TopPanel_Active";
    public static string Set_MainPanel_All_Active = "Set_MainPanel_All_Active";
    public static string Show_TopPanel_Close_Other = "Show_TopPanel_Close_Other";
    

    //派遣 DispatchUI
    public static string Dispatch_On_Start = "Dispatch_On_Start";
    public static string Dispatch_On_End = "Dispatch_On_End";

    //教程
    public static string Guide_Open_Panel = "Guide_Open_Panel";
    public static string Guide_Close_Panel = "Guide_Close_Panel";
    public static string Guide_Next_Step = "Guide_Next_Step";
    public static string Guide_Start_Move_Furniture = "Guide_Start_Move_Furniture";
    public static string Guide_Start_Shop = "Guide_Start_Shop";
    public static string Guide_Step_Changed = "Guide_Step_Changed";
    public static string Guide_Clear_View = "Guide_Clear_View";
    public static string Guide_Is_View_Ready = "Guide_Is_View_Ready";
    /// <summary>服端拉取/清除派遣后，仅把展示层与 player_state 对齐；勿当作 Dispatch_On_End 用（后者会触发回家拍照等）。</summary>
    public static string Dispatch_VisualsSync = "Dispatch_VisualsSync";

    public static string Dispatch_Text_Notice = "Dispatch_Text_Notice";
    public static string Dispatch_Text_Clear = "Dispatch_Text_Clear";

    public static string Dispatch_Switch_Procudure = "Dispatch_Switch_Procudure";

    public static string Dispatch_Switch_Bag = "Dispatch_Switch_Bag";
    public static string Dispatch_Change_Item = "Dispatch_Change_Item";
    public static string Dispatch_Refresh_Model = "Dispatch_Refresh_Model";
    /// <summary>旅行背包打包成功（<see cref="PackageBagRes"/>），参数为 0-based 背包下标。</summary>
    public static string Dispatch_Package_Bag_Res = "Dispatch_Package_Bag_Res";
    public static string Dispatch_Show_Reward = "Dispatch_Get_Reward";
    public static string Dispatch_Get_Photo = "Dispatch_Get_Photo";

    public static string CD_Player_Playing = "CD_Player_Playing";
    public static string CD_Player_Toggle = "CD_Player_Toggle";
    public static string CD_Play_Next = "CD_Play_Next";
    public static string CD_Play_Before = "CD_Play_Before";

    public static string OnPlacementPanelOpen = "OnPlacementPanelOpen";
    public static string OnPlacementPanelClose = "OnPlacementPanelClose";

    public static string OnClothPanelOpen = "OnClothPanelOpen";
    public static string OnClothPanelClose = "OnClothPanelClose";

    public static string OnPlantPanelOpen = "OnPlantPanelOpen";
    public static string OnPlantPanelClose = "OnPlantPanelClose";

    public static string OnRecyclePanelOpen = "OnRecyclePanelOpen";
    public static string OnRecyclePanelClose = "OnRecyclePanelClose";

    public static string OnDispatchPanelOpen = "OnDispatchPanelOpen";
    public static string OnDispatchPanelClose = "OnDispatchPanelClose";

    public static string OnShoppingPanelOpen = "OnShoppingPanelOpen";
    public static string OnShoppingPanelClose = "OnShoppingPanelClose";
    public static string OpenShoppingPanel = "OpenShoppingPanel";
    public static string OnShopInfoReceived = "OnShopInfoReceived";
    public static string OnManualRefreshShopReceived = "OnManualRefreshShopReceived";
    public static string OnBuyGoodsReceived = "OnBuyGoodsReceived";
    public static string OnSoldItemReceived = "OnSoldItemReceived";
    public static string OnBuyTicketsReceived = "OnBuyTicketsReceived";
    public static string OpenPayPanel = "OpenPayPanel";

    public static string OnPhonePanelOpen = "OnPhonePanelOpen";
    public static string OnPhonePanelClose = "OnPhonePanelClose";

    public static string OnPerseonBriefOpen = "OnPerseonBriefOpen";
    public static string OnPerseonBriefClose = "OnPerseonBriefClose";

    public static string OnLevelPanelOpen = "OnLevelPanelOpen";
    public static string OnLevelPanelClose = "OnLevelPanelClose";

    public static string OnTakePhotoPanelOpen = "OnTakePhotoPanelOpen";
    public static string OnTakePhotoPanelClose = "OnTakePhotoPanelClose";

    public static string ShowItemDetail = "ShowItemDetail";
    //---------------------------------------------------------------------------------------------
    #endregion


    //相机
    public static string Update_Wall_Visible = "Update_Wall_Visible";

    //经济
    public static string ClickBuyButton = "ClickBuyButton";
    public static string OnGetCoin = "OnGetCoin";
    public static string OnGetDiamond = "OnGetDiamond";
    public static string Show_Reward = "Show_Reward";

    public static string ShowFirstLoginReward = "ShowFirstLoginReward";
    public static string GetFirstLoginReward = "GetFirstLoginReward";



    public static string Audio_Change_Play = "Audio_Change_Play";
    public static string Audio_Play = "Audio_Play";
    public static string Audio_Stop = "Audio_Stop";

    public static string On_Set_Auto_Change_Cloth = "On_Set_Auto_Change_Cloth";
    public static string On_Save_Current_Character_Clothes = "On_Save_Current_Character_Clothes";

    //开关人物（小苔）可见性
    /// <summary>
    /// Reset一下主角状态（让Character自己根据房间自行决定是否显示）
    /// </summary>
    public static string ReSetMainCharacterRenderer = "ReSetMainCharacterRenderer";
    /// <summary>
    /// 控制主角是否显示（强制）
    /// </summary>
    public static string SetMainCharacterState = "SetMainCharacterState";


    //服务器
    public static string Save_Data_To_Server = "Save_Data_To_Server";
    public static string Get_Data_From_Server = "Get_Data_From_Server";
    public static string Excute_Server_Task = "ExcuteServerTask";
    public static string OnWSLError = "OnWSLError";

    public static string Network_Disconnect = "Network_Disconnect";


    //时间
    public static string On_Set_Time = "OnSetTime";

    public static string Excute_Server_Tasks = "ExcuteServerTasks";



    //种植
    public static string InitPlant = "InitPlant";
    public static string ShowSeedPop = "ShowSeedPop";
    public static string ShowFertilizerPop = "ShowFertilizerPop";
    public static string ShowHarvestPop = "ShowHarvestPop";
    public static string ShowRemovePop = "ShowRemovePop";
    public static string ShowPotState = "ShowPotState";

    public static string ClosePlantPop = "ClosePlantPop";



    //家具系统
    /// <summary>
    /// 网格实际占用
    /// </summary>
    public static string Update_GridView_Occupy = "Update_GridView_Occupy";
    /// <summary>
    /// 网格预览占用，家具拖动的时候使用
    /// </summary>
    public static string Update_GridView_Preview_Occupy = "Update_GridView_Preview_Occupy";
    public static string OnPutPlacement = "OnPutPlacement";
    //房间
    public static string SwitchRoom = "SwitchRoom";
    //点击
    public static string OnClickNothing = "OnClickNothing";
    public static string ShowUpPrompt = "ShowUpPrompt";
    public static string ShowPrompt = "ShowPrompt";
    //刷新
    public static string RefreshGridView = "RefreshGridView";
    public static string RefreshPlacementPanel = "RefreshPlacementPanel";

    //场景 Loading 进度条
    public static string SceneLoading_Open = "SceneLoading_Open";
    /// <summary>遮罩完全不透明后发出；SceneLoadingHelper 收到后再开始 LoadSceneAsync。</summary>
    public static string SceneLoading_MaskReady = "SceneLoading_MaskReady";
    public static string SceneLoading_Progress = "SceneLoading_Progress";
    public static string SceneLoading_Close = "SceneLoading_Close";


    /// <summary>任务系统：是否存在「可领取」任务状态变化（用于主界面任务按钮红点）。</summary>
    public static string Task_ClaimableChanged = "Task_ClaimableChanged";

    /// <summary>任务系统：刷新面板。</summary>
    public static string OnMissionRefresh = "OnMissionRefresh";

    /// <summary>
    /// 收到任务变动通知，更新任务数据
    /// </summary>
    public static string OnMissionUpdate = "OnMissionUpdate";
    /// <summary>经验系统：玩家等级数据从服务器加载完成。</summary>
    public static string PlayerLevelDataLoaded = "PlayerLevelDataLoaded";

    /// <summary>场景切换视频播放完成，可触发 SceneLoadingHelper.Load_MainScene()。</summary>
    public static string OnTransitionVideoFinished = "OnTransitionVideoFinished";

    /// <summary>
    /// 通知UI播放视频
    /// </summary>
    public static string PlayTransitionVideo = "PlayTransitionVideo";

    public static string WS_Open = "WS_Open";
    public static string Send_Req_To_Server = "Send_Req_To_Server";
    public static string Login_Mandatory_Data_Ready = "Login_Mandatory_Data_Ready";
    public static string Receive_Msg_From_Server = "Receive_Msg_From_Server";
    public static string Guide_Skip_Current = "Guide_Skip_Current";
}
