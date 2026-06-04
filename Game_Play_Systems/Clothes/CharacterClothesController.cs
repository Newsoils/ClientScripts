using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;

public class CharacterClothesController : MonoBehaviour
{
    public CharacterType type;
    public CharacterSkinnedMesh clothes;

    private bool initialized;

    private void OnEnable()
    {
        if (!initialized)
        {
            clothes.InitCharacter();
            initialized = true;
        }

        var mgr = CharacterClothesManager.Instance;
        mgr.controllers[type] = this;
        mgr.InitCharacter(type);
    }

    private void OnDisable()
    {
        UnregisterController();
    }

    private void OnDestroy()
    {
        UnregisterController();
    }

    private void UnregisterController()
    {
        if (CharacterClothesManager.Instance.controllers.TryGetValue(type, out var c))
        {
            if (c == this)
                CharacterClothesManager.Instance.controllers.Remove(type);
        }
    }

    public bool useWoodBodyPreview;

    public void ApplyClothes(Dictionary<string, ClothInfo> infos)
    {
        var lib = CharacterClothesResourceLibrary.Instance;
        clothes.RemoveAllClothes();
        foreach (var info in infos)
        {
            if (info.Value == null)
                continue;
            if (!lib.TryGetRendererParts(info.Value.suitName, info.Key, out var skmList))
                continue;
            if (lib.TryGetMaterialData(info.Value.suitName, info.Value.colorKey, out var materials))
                clothes.ChangePart(info.Key, skmList, materials);
            else
                clothes.ChangePart(info.Key, skmList);
        }
        ApplyDefaultSlots(lib, infos);

        // 换装场景：对所有已应用部件中材质id为 01 的身体部位覆盖 wood 材质
        if (useWoodBodyPreview && lib.TryGetMaterialData("wood", "default", out var woodMats))
        {
            var woodMat = woodMats.Find(x => x.Item1 == 1).Item2;
            if (woodMat != null)
                clothes.ApplyMaterialByPartIndex(1, woodMat);
        }
    }

    private void ApplyDefaultSlots(CharacterClothesResourceLibrary lib, Dictionary<string, ClothInfo> infos)
    {
        List<string> basicSlot = new List<string> { "head", "body", "leg", "shoes" };
        foreach (var slot in basicSlot)
        {
            if (infos.TryGetValue(slot, out var cur) && cur != null)
            {
                // 有服装数据且能加载到对应资源，才跳过兜底
                if (lib.TryGetRendererParts(cur.suitName, slot, out _))
                    continue;
                // 有数据但资源加载失败（如 default fbx 解析/命名问题），继续走兜底
            }
            if (!lib.TryGetDefaultRendererParts(slot, out var defaultSkm))
                continue;
            clothes.ChangePart(slot, defaultSkm);
        }
    }
}
