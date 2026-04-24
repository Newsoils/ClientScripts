
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
    public static string On_Single_NPC_Data_Updated = "On_Single_NPC_Data_Updated";
    public static string On_All_NPC_Data_Received = "On_All_NPC_Data_Received";
    public static string On_NPC_Data_Update = "On_NPC_Data_Update";
    public static string Receive_NPC_Gift_TO_Player = "Receive_NPC_Gift_TO_Player";
    public static string Updata_NPC_Favor_Level = "Updata_NPC_Favor_Level";


    public static string On_Get_Exp = "On_GetExp";

    public static string Check_Chat_Play = "Check_Chat_Play";
    public static string Stop_Chat_Coroutine = "Stop_Chat_Coroutine";

    public static string Reconnect = "Reconnect";

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

    public static string Dispatch_Text_Notice = "Dispatch_Text_Notice";
    public static string Dispatch_Text_Clear = "Dispatch_Text_Clear";

    public static string Dispatch_Switch_Procudure = "Dispatch_Switch_Procudure";

    public static string Dispatch_Switch_Bag = "Dispatch_Switch_Bag";
    public static string Dispatch_Change_Item = "Dispatch_Change_Item";
    public static string Dispatch_Refresh_Model = "Dispatch_Refresh_Model";
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
    public static string SceneLoading_Progress = "SceneLoading_Progress";
    public static string SceneLoading_Close = "SceneLoading_Close";
}
