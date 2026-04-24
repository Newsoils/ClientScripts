using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterClothesController : MonoBehaviour
{
    public CharacterType type;
    public CharacterSkinnedMesh clothes;
    public Dictionary<string, ClothInfo> characterClothesData = new Dictionary<string, ClothInfo>();


    private void Start()
    {
        if (CharacterClothesManager.Instance.controllers.TryGetValue(type, out var c))
        {
            return;
        }
        clothes.InitCharacter();
        CharacterClothesManager.Instance.controllers[type] = this;
        CharacterClothesManager.Instance.InitCharacter(type);
    }
    private void OnDestroy()
    {
        if (CharacterClothesManager.Instance.controllers.TryGetValue(type,out var c))
        {
            if(c == this)
            {
                CharacterClothesManager.Instance.controllers.Remove(type);
            }
        }

    }

    public void ChangeAllClothes(Dictionary<string, ClothInfo> infos)
    {
        clothes.RemoveAllClothes();
        characterClothesData = infos;
        foreach (var info in infos)
        {
            if (info.Value == null)
                continue;
            var mgr = CharacterClothesManager.Instance;
            if (!mgr.clothesData.TryGetValue(info.Value.suitName, out var datas))
                continue;
            if (!datas.TryGetValue(info.Key, out var skmList))
                continue;
            if (mgr.TryGetMaterialData(info.Value.suitName, info.Value.colorKey, out var materials))
                clothes.ChangePart(info.Key, skmList, materials);
            else
                clothes.ChangePart(info.Key, skmList);
        }
        CheckSlot();
    }

    /// <summary>仅更新 SkinnedMesh 显示，不修改 <see cref="characterClothesData"/>（临时套装、不参与存档）。</summary>
    public void ApplySuitVisualOnly(Dictionary<string, ClothInfo> suitSlotDic)
    {
        if (suitSlotDic == null || suitSlotDic.Count == 0 || clothes == null)
            return;
        clothes.RemoveAllClothes();
        var mgr = CharacterClothesManager.Instance;
        foreach (var kv in suitSlotDic)
        {
            var info = kv.Value;
            if (info == null)
                continue;
            if (!mgr.clothesData.TryGetValue(info.suitName, out var datas))
                continue;
            if (!datas.TryGetValue(kv.Key, out var skmList))
                continue;
            if (mgr.TryGetMaterialData(info.suitName, info.colorKey, out var materials))
                clothes.ChangePart(kv.Key, skmList, materials);
            else
                clothes.ChangePart(kv.Key, skmList);
        }
    }

    public void ChangeClothes(ClothInfo info)
    {
        if (CharacterClothesManager.Instance.clothesData.TryGetValue(info.suitName, out var datas))
        {
            foreach(string slot in info.slotsOccupied)
            {
                if (characterClothesData.TryGetValue(slot, out var existing) && existing != null &&
                    existing.clothName != info.clothName)
                {
                    RemoveClothes(existing.clothName, false);
                }

                characterClothesData[slot] = info;
                if (datas.TryGetValue(slot, out var data))
                {
                    if (CharacterClothesManager.Instance.TryGetMaterialData(info.suitName, info.colorKey, out var materials))
                    {
                        clothes.ChangePart(slot, data, materials);
                    }
                    else
                    {
                        clothes.ChangePart(slot, data);
                    }

                }
            }
        }
        CheckSlot();
    }
    public void RemoveClothes(ClothInfo info) => RemoveClothes(info.clothName, true);
    private void RemoveClothes(string clothesName, bool checkSlot = true)
    {
        foreach (var kv in characterClothesData.ToList())
        {
            if (kv.Value != null && kv.Value.clothName == clothesName)
            {
                clothes.RemovePart(kv.Key);
                characterClothesData[kv.Key] = null;
            }
        }
        if (checkSlot)
        {
            CheckSlot();
        }
    }
    public void CheckSlot()
    {
        var mgr = CharacterClothesManager.Instance;
        if (mgr == null || !mgr.clothesData.TryGetValue("default", out var defaultSuit))
            return;
        List<string> basicSlot = new List<string> { "head", "body", "leg" };
        foreach (var slot in basicSlot)
        {
            if (characterClothesData.TryGetValue(slot, out var cur) && cur != null)
                continue;
            if (!defaultSuit.TryGetValue(slot, out var defaultSkm))
                continue;
            clothes.ChangePart(slot, defaultSkm);
            characterClothesData[slot] = null;
        }
    }
}

