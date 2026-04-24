using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// SKM 命名约定与服装相同：<c>SuitName_X_SlotName_Index</c>（三段时 Index 默认 02），资源目录 <c>Resources/Models/HandHeld</c>。
/// </summary>
public class CharacterHandHeldManager : SingletonMono<CharacterHandHeldManager>
{
    public Dictionary<string, Dictionary<string, List<(int, SkinnedMeshRenderer)>>> handHeldData;//物品名-部位-(材质编号,部件skm)
    public Dictionary<string, Dictionary<string, List<(int, Material)>>> handHeldMaterialData;//物品名-颜色key-(材质编号，材质)
    public List<HandHeldInfo> handHeldInfos = new List<HandHeldInfo>();

    public Dictionary<CharacterType, CharacterHandHeldController> controllers = new Dictionary<CharacterType, CharacterHandHeldController>();

    protected override void Awake()
    {
        base.Awake();
        InitHandHeldSkmData();
        InitHandHeldMaterialData();
        InitHandHeldInfos();
    }

    #region 外部调用

    public void ShowHandheld(CharacterType type, string itemName)
    {
        if (handHeldInfos == null || !controllers.TryGetValue(type, out var ctrl)) return;
        var info = handHeldInfos.Find(x => x.itemName == itemName);
        if (info != null)
            ctrl.ShowHandheld(info);
    }

    public void HideHandheld(CharacterType type, string itemName)
    {
        if (!controllers.TryGetValue(type, out var ctrl)) return;
        ctrl.HideHandheld(itemName);
    }

    public void ClearAllHandheld(CharacterType type)
    {
        if (!controllers.TryGetValue(type, out var ctrl)) return;
        ctrl.ClearAllHandheld();
    }

    #endregion

    #region 数据

    public bool TryGetHandHeldMaterialData(string suitName, string colorKey, out List<(int, Material)> materials)
    {
        materials = null;
        if (string.IsNullOrEmpty(suitName) || string.IsNullOrEmpty(colorKey))
            return false;
        if (handHeldMaterialData != null && handHeldMaterialData.TryGetValue(suitName, out var dic) &&
            dic.TryGetValue(colorKey, out materials))
            return true;
        return false;
    }

    private void InitHandHeldInfos()
    {
        handHeldInfos = new List<HandHeldInfo>();
        var ta = Resources.Load<TextAsset>("Json/project_mouse_tb_handheld_info");
        if (ta == null || string.IsNullOrWhiteSpace(ta.text))
            return;
        var list = JsonConvert.DeserializeObject<List<HandHeldInfo>>(ta.text);
        if (list != null)
            handHeldInfos = list;
    }

    private void InitHandHeldSkmData()
    {
        List<GameObject> sources = Resources.LoadAll<GameObject>("Models/HandHeld").ToList();
        handHeldData = new Dictionary<string, Dictionary<string, List<(int, SkinnedMeshRenderer)>>>();
        foreach (var source in sources)
        {
            foreach (var part in source.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                string[] names = part.name.Split('_');
                if (names.Length < 3)
                    continue;
                string indexToken = names.Length >= 4 ? names[3] : "02";
                if (!int.TryParse(indexToken, out int partIndex))
                    continue;
                string suitKey = names[0];
                string slotKey = names[2];
                if (!handHeldData.ContainsKey(suitKey))
                    handHeldData[suitKey] = new Dictionary<string, List<(int, SkinnedMeshRenderer)>>();
                if (!handHeldData[suitKey].ContainsKey(slotKey))
                    handHeldData[suitKey][slotKey] = new List<(int, SkinnedMeshRenderer)>();
                handHeldData[suitKey][slotKey].Add((partIndex, part));
            }
        }
    }

    private void InitHandHeldMaterialData()
    {
        List<Material> sources = Resources.LoadAll<Material>("Materials/HandHeld").ToList();
        handHeldMaterialData = new Dictionary<string, Dictionary<string, List<(int, Material)>>>();
        foreach (var source in sources)
        {
            string[] names = source.name.Split('_');
            if (names.Length < 3)
                continue;
            if (!int.TryParse(names[1], out int matIndex))
                continue;
            string suitKey = names[0];
            string colorKey = names[2];
            if (!handHeldMaterialData.ContainsKey(suitKey))
                handHeldMaterialData[suitKey] = new Dictionary<string, List<(int, Material)>>();
            if (!handHeldMaterialData[suitKey].ContainsKey(colorKey))
                handHeldMaterialData[suitKey][colorKey] = new List<(int, Material)>();
            handHeldMaterialData[suitKey][colorKey].Add((matIndex, source));
        }
    }

    #endregion
}
