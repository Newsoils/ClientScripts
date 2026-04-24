using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
using UnityEngine;

public class CharacterClothesManager : SingletonMono<CharacterClothesManager>
{
    public Dictionary<string, Dictionary<string, List<(int, SkinnedMeshRenderer)>>> clothesData;//服装-部位-(材质编号,部件skm)
    public Dictionary<string, Dictionary<string, List<(int,Material)>>> materialData; //服装-颜色key-(材质编号，材质)
    public List<ClothInfo> clothInfos;
    public Dictionary<string, ClothInfo> clothNameDic;

    public Dictionary<CharacterType, CharacterClothesController> controllers = new Dictionary<CharacterType, CharacterClothesController>();

    protected override void Awake()
    {
        base.Awake();
        InitSKMData();
        InitMaterialData();
        InitItemData();
    }
    #region 外部调用
    public void ChangeClothes(CharacterType type, string clothesName)
    {
        if (clothInfos == null || !controllers.TryGetValue(type, out var ctrl)) return;
        var info = clothInfos.Find(x => x.clothName == clothesName);
        if (info != null)
            ctrl.ChangeClothes(info);
    }

    /// <summary>按 <c>cloth_info_v2</c> 表中的 <c>clothName</c> 卸下该件（槽位回默认 SKM）。</summary>
    public void RemoveClothes(CharacterType type, string clothesName)
    {
        if (clothInfos == null || !controllers.TryGetValue(type, out var ctrl)) return;
        var info = clothInfos.Find(x => x.clothName == clothesName);
        if (info != null)
            ctrl.RemoveClothes(info);
    }

    public void SyncClothes(CharacterType from, CharacterType to)
    {
        if (!controllers.TryGetValue(from, out var fromCtrl) || !controllers.TryGetValue(to, out var toCtrl)) return;
        toCtrl.ChangeAllClothes(fromCtrl.characterClothesData);
    }

    /// <summary>主角仅显示某套装，不改动 <see cref="CharacterClothesController.characterClothesData"/>。</summary>
    public void ApplyMainCharacterSuitVisualOnly(string suitName)
    {
        if (clothInfos == null || !controllers.TryGetValue(CharacterType.Main, out var ctrl))
            return;
        var dic = BuildSuitSlotDictionary(suitName);
        if (dic.Count == 0)
            return;
        ctrl.ApplySuitVisualOnly(dic);
    }

    /// <summary>按当前已记录的服装数据重刷主角外观（用于结束临时显示套装）。</summary>
    public void RestoreMainCharacterOutfitFromRecordedData()
    {
        if (!controllers.TryGetValue(CharacterType.Main, out var ctrl))
            return;
        ctrl.ChangeAllClothes(ctrl.characterClothesData);
    }

    private Dictionary<string, ClothInfo> BuildSuitSlotDictionary(string suitName)
    {
        var dic = new Dictionary<string, ClothInfo>();
        if (clothInfos == null || string.IsNullOrEmpty(suitName))
            return dic;
        foreach (var info in clothInfos.Where(x => x.suitName == suitName))
        {
            if (info.slotsOccupied == null)
                continue;
            foreach (var slot in info.slotsOccupied)
                dic[slot] = info;
        }
        return dic;
    }
    #endregion

    #region 数据
    public bool TryGetMaterialData(string suitName, string colorKey, out List<(int, Material)> materials)
    {
        materials = null;
        if (suitName == null || colorKey == null)
        {
            Log.Error("suitName或colorKey为null");
            return false;
        }

        if(materialData.TryGetValue(suitName, out var dic))
        {
            if(dic.TryGetValue(colorKey, out materials))
            {
                return true;
            }
        }
        return false;
    }
    private void InitItemData()
    {
        string text = JsonData_Manager.Load_Single_JsonData("project_mouse_tb_cloth_info");
        clothInfos = JsonConvert.DeserializeObject<List<ClothInfo>>(text);
        clothNameDic = clothInfos.ToDictionary(x => x.clothName);
    }
    private void InitSKMData()
    {
        List<GameObject> sources = Resources.LoadAll<GameObject>("Models/Clothes").ToList();
        clothesData = new Dictionary<string, Dictionary<string, List<(int, SkinnedMeshRenderer)>>>();
        foreach (var source in sources)
        {
            SkinnedMeshRenderer[] parts = source.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var part in parts)
            {
                string[] names = part.name.Split('_');
                if (names.Length < 3)//该部件命名不规范，或者不是服装
                {
                    continue;
                }
                // 约定：Suit_X_Slot_Index；仅三段时默认 Index 为 "02"（Append 不会扩容数组，不能写 names[3]）
                string indexToken = names.Length >= 4 ? names[3] : "02";
                if (!int.TryParse(indexToken, out int partIndex))
                {
                    continue;
                }
                string suitKey = names[0];
                string slotKey = names[2];
                if (clothesData.ContainsKey(suitKey))
                {
                    if (clothesData[suitKey].ContainsKey(slotKey))
                    {
                        clothesData[suitKey][slotKey].Add((partIndex, part));
                    }
                    else
                    {
                        clothesData[suitKey].Add(slotKey, new List<(int, SkinnedMeshRenderer)> { (partIndex, part) });
                    }
                }
                else
                {
                    clothesData.Add(suitKey, new Dictionary<string, List<(int, SkinnedMeshRenderer)>> { { slotKey, new List<(int, SkinnedMeshRenderer)> { (partIndex, part) } } });
                }

            }
        }

    }
    private void InitMaterialData()
    {
        List<Material> sources = Resources.LoadAll<Material>("Materials/Clothes").ToList();
        materialData = new Dictionary<string, Dictionary<string, List<(int, Material)>>>();
        foreach (var source in sources)
        {
            string[] names = source.name.Split('_');
            if(names.Length < 3)//该材质命名不规范
            {
                continue;
            }
            if (materialData.ContainsKey(names[0]))
            {
                if (materialData[names[0]].ContainsKey(names[2]))
                {
                    materialData[names[0]][names[2]].Add((int.Parse(names[1]), source));
                }
                else
                {
                    materialData[names[0]].Add(names[2], new List<(int, Material)> { (int.Parse(names[1]), source) });
                }
            }
            else
            {
                materialData.Add(names[0], new Dictionary<string, List<(int, Material)>> { { names[2], new List<(int, Material)> { (int.Parse(names[1]), source) } } });
            }
        }
    }
    #endregion

    #region 角色
    public void InitCharacter(CharacterType type)
    {
        List<ClothInfo> infos = clothInfos.Where(x => x.suitName == "default").ToList();
        Dictionary<string, ClothInfo> dic = infos.ToDictionary(x => x.slotsOccupied[0]);
        if(controllers.TryGetValue(type, out var controller))
        {
            controller.ChangeAllClothes(dic);
        }
        if(type == CharacterType.Main)
        {
            LoadClothesData();
        }
    }
    #endregion

    #region 服务器
    public void SaveClothesData()
    {
        EvtDsp.ReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, "SaveClothes", SaveClothesDataTask(), null);
    }
    public ServerTask SaveClothesDataTask()
    {
        var infos = controllers[CharacterType.Main].characterClothesData;
        string send = "";
        if (infos != null)
        {
            send = JsonConvert.SerializeObject(infos);
        }
        ServerTask task = new ServerTask(send, (string receive, ServerTask task) =>
        {
            if(receive == "success")
            {
                Log.Info("已保存服装数据");
            }
            else
            {
                Log.Error("保存服装数据失败");
            }
        });
        return task;
    }
    public void LoadClothesData()
    {
        EvtDsp.ReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, "LoadClothes", LoadClothesDataTask(), null);
    }
    public ServerTask LoadClothesDataTask()
    {
        ServerTask task = new ServerTask("LoadClothes", (string receive, ServerTask task) =>
        {
            if(receive != "nodata")
            {
                Dictionary<string, ClothInfo> infos = JsonConvert.DeserializeObject<Dictionary<string, ClothInfo>>(receive);
                controllers[CharacterType.Main].ChangeAllClothes(infos);
            }
        });
        return task;
    }
    #endregion
}
