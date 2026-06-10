using System;
using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;

[Serializable]
public class SeedInfo
{
    /// <summary>
    /// 种子对应的物品 ID（seedID）
    /// </summary>
    public int seedID;

    /// <summary>
    /// 该种子可生成的植物 ID 列表（对应 PlantData.plantId）
    /// </summary>
    public List<int> plantIDs = new List<int>();

    /// <summary>
    /// 植物属性（花卉/果实/绿植），从 Game_Item_Info 移动到这里
    /// </summary>
    public PlantType plantType = PlantType.None;
}