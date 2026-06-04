namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// 弹窗文本 ID 常量（对应 project_mouse_tb_prompt_text）。
    /// 用法：PromptManager.Instance.GetText(PromptId.FurnitureCrossRoom) 。
    /// </summary>
    public static class PromptId
    {
        // --- 警告面板 (Show_Warning_Panel) ---
        public const int FurnitureCrossRoom = 1;
        public const int FurnitureNoSpace = 2;
        public const int FurnitureRotateNoSpace = 3;
        public const int FurnitureMoveCrossRoom = 4;
        public const int FurnitureMoveNoSpace = 5;
        public const int FurnitureRotateInvalid = 6;
        public const int CatNotAway = 7;
        public const int FurnitureWrongRoom = 8;

        // --- 上方提示 (ShowUpPrompt) ---
        public const int FertilizerNotEnough = 9;
        public const int SeedNotEnough = 10;
        public const int PlantTypeLimit = 11;
        public const int Watered = 12;
        public const int PhotoSaved = 13;
        public const int PotHasPlant = 14;
        public const int DispatchItemNotEnough = 15;
        public const int WaterGuide = 16;
        public const int FriendRequestSent = 29;
        public const int FriendRequestAccepted = 30;
        public const int FriendRequestRejected = 31;
        public const int PlayerIdInvalid = 32;
        public const int FriendNotFound = 33;
        public const int FriendInfoInvalid = 34;
        public const int MessageEmpty = 35;
        public const int RecycleItemError = 37;
        public const int NetworkDisconnected = 38;
        public const int PlayerNotFound = 39;
        public const int RechargeSuccess = 40;
        public const int LoveTicketConfigMissing = 41;
        public const int PurchaseSuccess = 43;
        public const int ShopDataOutOfSync = 44;
        public const int ColorMintConfigMissing = 48;
        public const int GachaPoolOutOfSync = 50;
        public const int ShopRefreshSuccess = 54;
        public const int RechargeOptionInvalid = 55;
        public const int RechargeDailyLimit = 56;
        public const int PurchaseOptionInvalid = 57;
        public const int RechargeNetworkError = 58;
        public const int RechargeCheckFailed = 59;
        public const int RechargeSubmitFailed = 60;
        public const int DispatchPacked = 61;
        public const int PotPlacementHint = 25;
        public const int GachaTimesExhausted = 27;
        public const int GachaTimesLow = 28;

        // --- 确认弹窗 (ShowPrompt) ---
        public const int ServerDisconnected = 17;
        public const int ServerConnectFailed = 18;
        public const int ReconnectFailed = 19;
        public const int ReconnectSuccess = 20;
        public const int DebugFormatError = 21;
        public const int DebugNotConnected = 22;
        public const int DebugInputInteger = 23;
        public const int CurrencyNotEnough = 24;
        public const int LoveTicketNotEnough = 26;
        public const int BuyColorMintConfirm = 47;
        public const int BuySuitConfirm = 49;
        public const int GachaConfirm = 51;
        public const int GachaMintShortage = 52;
        public const int BuyItemConfirm = 53;
        public const int BuyTicketConfirm = 42;
        public const int RechargeDiamondConfirm = 70;
        public const int ExchangeCoinConfirm = 71;
        public const int ShopFreeRefreshConfirm = 45;
        public const int ShopCoinRefreshConfirm = 46;
        public const int GoHomeConfirm = 63;

        // --- Prefab 静态 ---
        public const int BtnConfirm = 64;
        public const int BtnCancel = 65;
        public const int TitleHint = 66;
        public const int CheckDontRemind = 67;
        public const int ResetDontRemind = 68;
        public const int PlaceholderText = 69;

        // --- 回收 ---
        public const int RecycleConfirm = 72;

        // --- 其他 ---
        public const int NPCNoNewDialogue = 36;
        public const int PlacementCancelNoBackup = 62;
        //
    }
}
