using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Network;
using Newtonsoft.Json;
using UnityEngine;

public class CharacterClothesManager : SingletonMono<CharacterClothesManager>
{
    public List<ClothInfo> clothInfos;
    public Dictionary<string, ClothInfo> clothNameDic;

    private Dictionary<int, ClothInfo> clothIdDic;
    private Dictionary<CharacterType, Dictionary<string, ClothInfo>> clothesDataByType = new Dictionary<CharacterType, Dictionary<string, ClothInfo>>();

    public Dictionary<CharacterType, CharacterClothesController> controllers = new Dictionary<CharacterType, CharacterClothesController>();

    protected override void Awake()
    {
        base.Awake();
        InitItemData();
    }

    #region Public outfit operations
    /// <summary>在指定角色数据上穿戴服装，自动处理多槽位冲突，并立即刷新该角色表现。</summary>
    public void ChangeClothes(CharacterType type, string clothesName)
    {
        if (!TryGetClothInfo(clothesName, out var info)) return;
        ApplyClothToData(GetClothesData(type), info);
        ApplyDataToController(type);
    }

    /// <summary>从指定角色数据中卸下服装，并立即刷新该角色表现。</summary>
    public void RemoveClothes(CharacterType type, string clothesName)
    {
        if (!TryGetClothInfo(clothesName, out var info)) return;
        RemoveClothFromData(GetClothesData(type), info);
        ApplyDataToController(type);
    }

    /// <summary>复制一个角色的服装数据到另一个角色，并刷新目标角色表现。</summary>
    public void SyncClothes(CharacterType from, CharacterType to)
    {
        SetClothesData(to, GetClothesData(from));
        ApplyDataToController(to);
    }

    /// <summary>打开换装面板时调用：用 Main 的权威数据初始化 Target 预览数据。</summary>
    public void BeginPreview()
    {
        SyncClothes(CharacterType.Main, CharacterType.Target);
    }

    /// <summary>换装面板点击服装时调用：只修改 Target 预览数据，不影响 Main 权威数据。</summary>
    public void PreviewCloth(string clothesName)
    {
        ChangeClothes(CharacterType.Target, clothesName);
    }

    public void PreviewCloth(ClothInfo info)
    {
        ApplyClothToData(GetClothesData(CharacterType.Target), info);
        ApplyDataToController(CharacterType.Target);
    }

    /// <summary>取消换装时调用：丢弃 Target 预览数据，恢复为 Main 权威数据。</summary>
    public void CancelPreview()
    {
        SyncClothes(CharacterType.Main, CharacterType.Target);
    }

    /// <summary>确认换装时调用：Target 预览数据写回 Main 权威数据，再上传服务器。</summary>
    public void ConfirmPreview()
    {
        SetClothesData(CharacterType.Main, GetClothesData(CharacterType.Target));
        ApplyDataToController(CharacterType.Main);
        SaveClothesData();
    }

    /// <summary>兼容旧调用：把 Main 权威数据应用到指定角色表现。</summary>
    public void ApplyCachedClothesToController(CharacterType type)
    {
        SyncClothes(CharacterType.Main, type);
    }

    /// <summary>临时只改主角表现，不写入 Main 权威数据，也不参与存档。</summary>
    public void ApplyMainCharacterSuitVisualOnly(string suitName)
    {
        if (clothInfos == null || !controllers.TryGetValue(CharacterType.Main, out var ctrl))
            return;

        var data = BuildSuitSlotDictionary(suitName);
        if (data.Count == 0)
            return;

        ctrl.ApplyClothes(data);
    }

    public void RestoreMainCharacterOutfitFromRecordedData()
    {
        ApplyDataToController(CharacterType.Main);
    }
    /// <summary>把服务器下发的穿戴快照直接应用到指定 Controller，只改表现，不写入任何角色缓存。</summary>
    public void ApplyClothesSnapshotToController(CharacterClothesController controller, string wearsJson)
    {
        if (string.IsNullOrEmpty(wearsJson))
            return;

        controller.ApplyClothes(DeserializeClothesData(wearsJson));
    }
    #endregion

    #region Controller lifecycle
    /// <summary>Controller 注册后调用：确保该类型有数据缓存，并把缓存应用到表现层。</summary>
    public void InitCharacter(CharacterType type)
    {
        if (!clothesDataByType.ContainsKey(type))
            clothesDataByType[type] = BuildDefaultClothesData();

        ApplyDataToController(type);

        if (type == CharacterType.Main)
            LoadClothesData();
    }

    private void ApplyDataToController(CharacterType type)
    {
        if (controllers.TryGetValue(type, out var controller))
            controller.ApplyClothes(CopyClothesData(GetClothesData(type)));
    }
    #endregion

    #region Runtime data cache
    private Dictionary<string, ClothInfo> GetClothesData(CharacterType type)
    {
        if (!clothesDataByType.TryGetValue(type, out var data))
        {
            data = BuildDefaultClothesData();
            clothesDataByType[type] = data;
        }
        return data;
    }

    private void SetClothesData(CharacterType type, Dictionary<string, ClothInfo> source)
    {
        clothesDataByType[type] = CopyClothesData(source);
    }

    private Dictionary<string, ClothInfo> CopyClothesData(Dictionary<string, ClothInfo> source)
    {
        return source == null ? new Dictionary<string, ClothInfo>() : new Dictionary<string, ClothInfo>(source);
    }

    private Dictionary<string, ClothInfo> BuildDefaultClothesData()
    {
        return clothInfos
            .Where(x => x.suitName == "default" && x.slotsOccupied != null && x.slotsOccupied.Count > 0)
            .ToDictionary(x => x.slotsOccupied[0], x => x);
    }

    private Dictionary<string, ClothInfo> BuildSuitSlotDictionary(string suitName)
    {
        var data = new Dictionary<string, ClothInfo>();
        if (clothInfos == null || string.IsNullOrEmpty(suitName))
            return data;

        foreach (var info in clothInfos.Where(x => x.suitName == suitName))
        {
            if (info.slotsOccupied == null)
                continue;
            foreach (var slot in info.slotsOccupied)
                data[slot] = info;
        }
        return data;
    }

    /// <summary>纯数据穿戴逻辑：先清掉所有槽位冲突服装，再写入新服装占据的全部槽位。</summary>
    private void ApplyClothToData(Dictionary<string, ClothInfo> data, ClothInfo info)
    {
        if (data == null || info?.slotsOccupied == null)
            return;

        var conflictNames = new HashSet<string>();
        foreach (string slot in info.slotsOccupied)
        {
            if (data.TryGetValue(slot, out var existing) && existing != null && existing.clothName != info.clothName)
                conflictNames.Add(existing.clothName);
        }

        foreach (string slot in data.Keys.ToList())
        {
            if (data[slot] != null && conflictNames.Contains(data[slot].clothName))
                data[slot] = null;
        }

        foreach (string slot in info.slotsOccupied)
            data[slot] = info;
    }

    private void RemoveClothFromData(Dictionary<string, ClothInfo> data, ClothInfo info)
    {
        if (data == null || info == null)
            return;

        foreach (string slot in data.Keys.ToList())
        {
            if (data[slot] != null && data[slot].clothName == info.clothName)
                data[slot] = null;
        }
    }
    #endregion

    #region Config lookup
    private bool TryGetClothInfo(string clothesName, out ClothInfo info)
    {
        info = null;
        if (clothInfos == null || string.IsNullOrEmpty(clothesName))
            return false;
        if (clothNameDic != null && clothNameDic.TryGetValue(clothesName, out info))
            return true;
        info = clothInfos.Find(x => x.clothName == clothesName);
        return info != null;
    }

    private void InitItemData()
    {
        string text = JsonDataManager.Load_Single_JsonData("project_mouse_tb_cloth_info");
        clothInfos = JsonConvert.DeserializeObject<List<ClothInfo>>(text);
        clothNameDic = clothInfos.ToDictionary(x => x.clothName);
        clothIdDic = clothInfos.ToDictionary(x => x.clothId);
    }
    #endregion

    #region Server sync
    public void SaveClothesData()
    {
        var req = new Cmd.SaveClothesReq
        {
            ClothData = SerializeMainClothesData()
        };
        NetWork_Center_WSS.SendMsg(req);
    }

    public void LoadClothesData()
    {
        ApplyCatInfoWears(Global_Game_Manager.Instance?._current_cat_info);
    }

    public void ApplyCatInfoWears(Cmd.CatInfo catInfo)
    {
        if (catInfo == null || string.IsNullOrEmpty(catInfo.Wears))
            return;

        SetClothesData(CharacterType.Main, DeserializeClothesData(catInfo.Wears));
        ApplyDataToController(CharacterType.Main);
    }

    private string SerializeMainClothesData()
    {
        var slotClothIds = new Dictionary<string, int>();
        foreach (var kv in GetClothesData(CharacterType.Main))
        {
            if (kv.Value != null)
                slotClothIds[kv.Key] = kv.Value.clothId;
        }
        return JsonConvert.SerializeObject(slotClothIds);
    }

    private Dictionary<string, ClothInfo> DeserializeClothesData(string receive)
    {
        var slotClothIds = JsonConvert.DeserializeObject<Dictionary<string, int>>(receive);
        var infos = new Dictionary<string, ClothInfo>();
        foreach (var kv in slotClothIds)
        {
            if (clothIdDic.TryGetValue(kv.Value, out var info))
                infos[kv.Key] = info;
        }
        return infos;
    }
    #endregion
}
