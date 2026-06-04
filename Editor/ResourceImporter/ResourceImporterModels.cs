using System;
using System.Collections.Generic;

namespace CLIP.Project_Mouse.EditorTools.ResourceImporter
{
    public enum ResourceType
    {
        Cloth,
        Furniture,
        Pot,
        Interact,
        Search
    }

    public enum ImportMode
    {
        Import,
        Modify
    }

    // ==================== 服装 ====================

    public class ClothFbxEntry
    {
        public string FileName;
        public List<ClothChildEntry> Children = new List<ClothChildEntry>();
    }

    public class ClothChildEntry
    {
        public string ChildName;
        public bool IsValid;
        public string SuitName;
        public string PartName;
        public string Slot;
        public string MatId;
        public string ErrorMessage;
    }

    [Serializable]
    public class ClothRowDraft
    {
        public int clothId = 20100;
        public string clothName = "";
        public int firstCategory;
        public int secondCategory;
        public string slotsOccupied = "";
        public string suitName = "";
        public string colorKey = "default";
        public string partName = "";
    }

    // ==================== 查找 ====================

    public class SearchTableResult
    {
        public string TableName;
        public string JsonPath;
        public string XlsxPath;
        public List<SearchRowResult> Rows = new List<SearchRowResult>();
        public List<RelatedItemResult> RelatedItems = new List<RelatedItemResult>();
    }

    public class SearchRowResult
    {
        public string Summary;
        public string JsonPath;
    }

    public class SearchAssetResult
    {
        public string AssetType;
        public string AssetPath;
    }

    public class RelatedItemResult
    {
        public string DisplayName;
        public string RelationType;
        public string TargetQuery;
    }

    // ==================== 服装修改模式 ====================

    public class ExistingClothConfig
    {
        public int ClothId;
        public string ClothName;
        public string SuitName;
        public string ColorKey;
        public string SlotsOccupied;
        public int FirstCategory;
        public int SecondCategory;
    }

    public class ClothMaterialEntry
    {
        public string FileName;
        public string AssetPath;
        public string ColorKey;
        public bool IsTexture;
    }

    public class GameItemDraft
    {
        public int itemId;
        public string itemName = "";
        public string desc = "";
        public string resUrl = "";
        public int itemType = 12;
        public int sellPrice;
        public int rarity = 1;
        public string currencyUnit = "";
        public bool canBePresent;
    }

    public class FuzzyResult
    {
        public string TableName;
        public string DisplayName;
        public string IdValue;
        public string QueryForExact;
        public string JsonPath;
    }
}
