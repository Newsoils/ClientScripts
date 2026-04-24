using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 单角色手持物：使用独立 <see cref="CharacterSkinnedMesh"/>（建议单独 <c>modelTarget</c>，与服装解耦）。
/// 槽位名与 <see cref="HandHeldSlot"/> 一致。
/// </summary>
public class CharacterHandHeldController : MonoBehaviour
{
    public CharacterType type;
    public CharacterSkinnedMesh handheldMesh;

    /// <summary>槽位 → 当前手持配置（无手持为 null）。</summary>
    public Dictionary<string, HandHeldInfo> handheldBySlot = new Dictionary<string, HandHeldInfo>();

    private void Start()
    {
        if (handheldMesh != null)
            handheldMesh.mergeMissingBones = true;
        if (CharacterHandHeldManager.Instance != null)
            CharacterHandHeldManager.Instance.controllers[type] = this;
    }

    private void OnDestroy()
    {
        if (CharacterHandHeldManager.Instance != null &&
            CharacterHandHeldManager.Instance.controllers.TryGetValue(type, out var c) && c == this)
            CharacterHandHeldManager.Instance.controllers.Remove(type);
    }

    /// <summary>按配置显示手持物（占用 <see cref="HandHeldInfo.slotsOccupied"/> 中的槽位）。</summary>
    public void ShowHandheld(HandHeldInfo info)
    {
        if (info == null || handheldMesh == null || string.IsNullOrEmpty(info.suitName)) return;
        if (info.slotsOccupied == null || info.slotsOccupied.Count == 0) return;
        var mgr = CharacterHandHeldManager.Instance;
        if (mgr == null || mgr.handHeldData == null || !mgr.handHeldData.TryGetValue(info.suitName, out var datas)) return;

        foreach (var slot in info.slotsOccupied)
        {
            if (string.IsNullOrEmpty(slot)) continue;

            if (handheldBySlot.TryGetValue(slot, out var cur) && cur != null && cur.itemName != info.itemName)
                RemoveHandheldInternal(cur.itemName, false);

            handheldBySlot[slot] = info;
            if (!datas.TryGetValue(slot, out var skmList)) continue;

            if (!string.IsNullOrEmpty(info.colorKey) &&
                mgr.TryGetHandHeldMaterialData(info.suitName, info.colorKey, out var materials))
                handheldMesh.ChangePart(slot, skmList, materials);
            else
                handheldMesh.ChangePart(slot, skmList);
        }
    }

    /// <summary>按物品名卸下（双手同名会同时清掉）。</summary>
    public void HideHandheld(string itemName)
    {
        RemoveHandheldInternal(itemName, true);
    }

    public void ClearAllHandheld()
    {
        handheldMesh?.RemoveAllClothes();
        handheldBySlot.Clear();
    }

    private void RemoveHandheldInternal(string itemName, bool clearEmptySlotEntry)
    {
        if (handheldMesh == null || string.IsNullOrEmpty(itemName)) return;

        foreach (var kv in handheldBySlot.ToList())
        {
            if (kv.Value == null || kv.Value.itemName != itemName) continue;
            handheldMesh.RemovePart(kv.Key);
            if (clearEmptySlotEntry)
                handheldBySlot.Remove(kv.Key);
            else
                handheldBySlot[kv.Key] = null;
        }
    }
}
