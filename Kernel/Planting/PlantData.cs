using System;
using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;
/// <summary>
/// 植物本地数据,存储植物状态以及和服务器交互
/// </summary>
public class PlantLocalData
{
    public string uid;
    public string potUid;
    public int plantId;
    public string variantKey;

    #region 生长
    public int growStage;
    public int growStage2;
    public double plantTime;
    public double lastUpdateTime;
    public double curGrowTime;
    #endregion

    #region 状态
    public float water;
    public int harvestTime;
    public float variantProbability;
    public bool isFertilize;
    #endregion
}
/// <summary>
/// 植物静态数据
/// </summary>
public class PlantData
{
    /// <summary>
    /// 植物ID
    /// </summary>
    public int plantId;
    /// <summary>
    /// 植物名称
    /// </summary>
    public string plantName;
    /// <summary>
    /// 模型关键字
    /// </summary>
    public string modelKey;
    /// <summary>
    /// 材质关键字
    /// </summary>
    public List<string> matKey;
    /// <summary>
    /// 植物种类（普通/爬藤）
    /// </summary>
    public Plant_Second_Category plantType;
    /// <summary>
    /// 种子名称
    /// </summary>
    public string plantSeed;
    /// <summary>
    /// 植物变体
    /// </summary>
    public List<string> plantVariant;
    public List<string> variantKey;
    /// <summary>
    /// 收获物品
    /// </summary>
    public List<string> harvestItem;
    /// <summary>
    /// 收获物品数量范围
    /// </summary>
    public (int, int) harvestRange;
    /// <summary>
    /// 成熟时间
    /// </summary>
    public List<float> lifeCycle;
    /// <summary>
    /// 收获物种类
    /// </summary>
    public PlantHarvestType harvestType;
    public int harvestTime;
    public int plantSize;
}
public enum PlantHarvestType
{
    CanCycle = 0,
    CannotCycle = 1
}
