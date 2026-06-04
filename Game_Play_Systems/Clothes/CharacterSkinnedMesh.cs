using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 将服装 / 手持 SkinnedMesh 挂到 <see cref="modelTarget"/> 下，骨骼从 <see cref="boneParent"/> 按名字解析。
/// <see cref="mergeMissingBones"/> 为 true 时，会把手持等资源里主骨架不存在的骨骼链按 prefab 局部变换挂到最近同名父骨骼下，并在 <see cref="RemovePart"/> 时销毁撤回。
/// </summary>
public class CharacterSkinnedMesh : MonoBehaviour
{
    [Header("配置")]
    public GameObject modelTarget;
    public GameObject boneParent;
    public LayerMask layer = 1 << 9;

    [Tooltip("为 true 时，源 SMR 上主骨架缺少的骨骼会按 prefab 层级与局部 TRS 动态挂到主骨架，卸下对应部位时删除。")]
    public bool mergeMissingBones;

    [Tooltip("合并/撤回动态骨骼后调用 Animator.Rebind 刷新绑定；Rebind 会重置运行时参数，本脚本会在 Rebind 前后快照并恢复 Bool/Int/Float 与 LayerWeight。状态机当前 State 仍可能被重置。")]
    public bool rebindAnimatorAfterSkeletonChange = true;

    private Dictionary<string, List<SkinnedMeshRenderer>> characterRenderers = new Dictionary<string, List<SkinnedMeshRenderer>>();
    private Dictionary<string, Transform> characterBones;

    /// <summary>部位名（槽位）→ 该部位合并骨骼时创建的、直接挂在已有骨架下的根 Transform（用于撤回）。</summary>
    private readonly Dictionary<string, List<Transform>> mergedBoneRootsByPart = new Dictionary<string, List<Transform>>();

    private void Start()
    {
        InitBones();
    }


    private void EnsureRendererDict()
    {
        if (characterRenderers == null)
            characterRenderers = new Dictionary<string, List<SkinnedMeshRenderer>>();
    }

    public void InitCharacter()
    {
        EnsureRendererDict();
        foreach (Transform child in modelTarget.transform)
        {
            Destroy(child.gameObject);
        }
    }
    private void InitBones()
    {
        RebuildBoneMap();
    }

    private void RebuildBoneMap()
    {
        if (boneParent == null)
        {
            characterBones = new Dictionary<string, Transform>();
            return;
        }

        characterBones = boneParent.GetComponentsInChildren<Transform>(true)
            .ToDictionary(t => t.name, t => t);
    }

    private void EnsureBonesReady()
    {
        if (characterBones == null)
            RebuildBoneMap();
    }

    public void RemoveAllClothes()
    {
        foreach (Transform child in modelTarget.transform)
        {
            Destroy(child.gameObject);
        }
        EnsureRendererDict();
        characterRenderers.Clear();
        RevertAllMergedBones();
        RebuildBoneMap();

        if (mergeMissingBones && rebindAnimatorAfterSkeletonChange)
            TryRebindAnimator();
    }

    public void ChangePart(string slotName, List<(int, SkinnedMeshRenderer)> skmDatas, List<(int, Material)> materialData = null)
    {
        EnsureRendererDict();
        RemovePart(slotName);
        if (!characterRenderers.TryGetValue(slotName, out var slotList))
        {
            slotList = new List<SkinnedMeshRenderer>();
            characterRenderers[slotName] = slotList;
        }

        if (skmDatas == null)
            return;

        if (mergeMissingBones)
        {
            foreach (var part in skmDatas)
            {
                if (part.Item2 != null)
                    MergeMissingBonesFromSource(part.Item2, slotName);
            }
        }

        foreach (var part in skmDatas)
        {
            if (part.Item2 == null)
                continue;
            SkinnedMeshRenderer skm = SetSKM(part.Item2, part.Item2.name);
            if (skm == null)
                continue;
            if (materialData != null)
            {
                var found = materialData.Find(x => x.Item1 == part.Item1);
                Material mat = found.Item2;
                if (mat != null)
                    skm.material = mat;
            }
            slotList.Add(skm);
        }

        if (mergeMissingBones && rebindAnimatorAfterSkeletonChange)
            TryRebindAnimator();
    }

    /// <summary>
    /// 对所有已应用部件中 partIndex 等于 targetIndex 的 SMR 替换材质。
    /// 用于换装场景中对"身体部位"（材质id=01）做特殊材质覆盖。
    /// </summary>
    public void ApplyMaterialByPartIndex(int targetIndex, Material mat)
    {
        if (mat == null || characterRenderers == null)
            return;
        foreach (var kvp in characterRenderers)
        {
            foreach (var skm in kvp.Value)
            {
                if (skm == null)
                    continue;
                string[] names = skm.name.Split('_');
                if (names.Length < 4)
                    continue;
                if (!int.TryParse(names[3], out int partIndex))
                    continue;
                if (partIndex == targetIndex)
                    skm.material = mat;
            }
        }
    }

    public void RemovePart(string partName)
    {
        if (characterRenderers != null && characterRenderers.TryGetValue(partName, out var list) && list != null)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] != null)
                    Destroy(list[i].gameObject);
                list.RemoveAt(i);
            }
        }

        RevertMergedBonesForPart(partName);
        RebuildBoneMap();

        if (mergeMissingBones && rebindAnimatorAfterSkeletonChange)
            TryRebindAnimator();
    }

    /// <summary>
    /// 运行时增删骨骼后刷新 Animator 绑定。Unity 的 <see cref="Animator.Rebind"/> 会重建内部图，常把参数重置为控制器默认值，故先快照 Bool/Int/Float 与各层 Weight 再恢复。
    /// 当前播放状态、Trigger、根运动等仍可能与 Rebind 前不完全一致，属引擎限制。
    /// </summary>
    private void TryRebindAnimator()
    {
        if (boneParent == null)
            return;
        var animator = boneParent.GetComponentInParent<Animator>(true);
        if (animator == null || !animator.isActiveAndEnabled)
            return;

        var parameters = animator.parameters;
        var boolSnap = new Dictionary<int, bool>(parameters.Length);
        var intSnap = new Dictionary<int, int>(parameters.Length);
        var floatSnap = new Dictionary<int, float>(parameters.Length);
        foreach (var p in parameters)
        {
            switch (p.type)
            {
                case AnimatorControllerParameterType.Bool:
                    boolSnap[p.nameHash] = animator.GetBool(p.nameHash);
                    break;
                case AnimatorControllerParameterType.Int:
                    intSnap[p.nameHash] = animator.GetInteger(p.nameHash);
                    break;
                case AnimatorControllerParameterType.Float:
                    floatSnap[p.nameHash] = animator.GetFloat(p.nameHash);
                    break;
            }
        }

        int layerCount = animator.layerCount;
        var layerWeights = new float[layerCount];
        for (int i = 0; i < layerCount; i++)
            layerWeights[i] = animator.GetLayerWeight(i);

        animator.Rebind();

        foreach (var kv in boolSnap)
            animator.SetBool(kv.Key, kv.Value);
        foreach (var kv in intSnap)
            animator.SetInteger(kv.Key, kv.Value);
        foreach (var kv in floatSnap)
            animator.SetFloat(kv.Key, kv.Value);
        for (int i = 0; i < layerCount; i++)
            animator.SetLayerWeight(i, layerWeights[i]);
    }

    /// <summary>
    /// 将源 SMR 骨骼链上主骨架没有的节点挂到主骨架（从最近同名父骨骼下开始复制局部 TRS）。
    /// </summary>
    private void MergeMissingBonesFromSource(SkinnedMeshRenderer sourceSmr, string partName)
    {
        EnsureBonesReady();
        if (characterBones == null || sourceSmr == null)
            return;

        if (!mergedBoneRootsByPart.TryGetValue(partName, out var roots))
        {
            roots = new List<Transform>();
            mergedBoneRootsByPart[partName] = roots;
        }

        var seen = new HashSet<Transform>();
        if (sourceSmr.bones != null)
        {
            foreach (var b in sourceSmr.bones)
            {
                if (b == null || !seen.Add(b))
                    continue;
                MergeBoneChainFromLeaf(b, roots);
            }
        }

        if (sourceSmr.rootBone != null && seen.Add(sourceSmr.rootBone))
            MergeBoneChainFromLeaf(sourceSmr.rootBone, roots);
    }

    private void MergeBoneChainFromLeaf(Transform leafInPrefab, List<Transform> rootsRegistry)
    {
        var upward = new List<Transform>();
        Transform t = leafInPrefab;
        Transform anchorOnCharacter = null;

        while (t != null)
        {
            if (characterBones.TryGetValue(t.name, out var onCharacter))
            {
                anchorOnCharacter = onCharacter;
                break;
            }

            upward.Add(t);
            t = t.parent;
        }

        if (anchorOnCharacter == null)
        {
            Debug.LogWarning($"[CharacterSkinnedMesh] 骨骼「{leafInPrefab.name}」向上找不到主骨架同名锚点，跳过合并。");
            return;
        }

        Transform parent = anchorOnCharacter;
        for (int i = upward.Count - 1; i >= 0; i--)
        {
            Transform src = upward[i];
            if (characterBones.TryGetValue(src.name, out var existing))
            {
                parent = existing;
                continue;
            }

            var go = new GameObject(src.name);
            Transform tr = go.transform;
            tr.SetParent(parent, false);
            tr.localPosition = src.localPosition;
            tr.localRotation = src.localRotation;
            tr.localScale = src.localScale;
            characterBones[src.name] = tr;
            if (i == upward.Count - 1)
                rootsRegistry.Add(tr);
            parent = tr;
        }
    }

    private void RevertMergedBonesForPart(string partName)
    {
        if (!mergedBoneRootsByPart.TryGetValue(partName, out var roots) || roots == null)
            return;
        for (int i = 0; i < roots.Count; i++)
        {
            if (roots[i] != null)
                Destroy(roots[i].gameObject);
        }

        roots.Clear();
        mergedBoneRootsByPart.Remove(partName);
    }

    private void RevertAllMergedBones()
    {
        foreach (var kv in mergedBoneRootsByPart.ToList())
            RevertMergedBonesForPart(kv.Key);
    }

    private SkinnedMeshRenderer SetSKM(SkinnedMeshRenderer renderer, string name)
    {
        EnsureBonesReady();
        var bones = new List<Transform>();
        if (renderer.bones != null)
        {
            foreach (var bone in renderer.bones)
            {
                if (bone == null)
                {
                    Debug.LogWarning($"[CharacterSkinnedMesh] SMR「{name}」含空骨骼引用。");
                    bones.Add(null);
                    continue;
                }

                if (!characterBones.TryGetValue(bone.name, out var mapped))
                {
                    Debug.LogError($"[CharacterSkinnedMesh] 主骨架缺少骨骼「{bone.name}」（SMR「{name}」）。可开启 mergeMissingBones 或检查命名。");
                    return null;
                }

                bones.Add(mapped);
            }
        }

        GameObject skmObj = new GameObject();
        skmObj.name = name;
        skmObj.transform.SetParent(modelTarget.transform, false);
        SkinnedMeshRenderer newRenderer = skmObj.AddComponent<SkinnedMeshRenderer>();
        newRenderer.bones = bones.ToArray();
        newRenderer.sharedMaterials = renderer.sharedMaterials;
        newRenderer.sharedMesh = renderer.sharedMesh;

        if (renderer.rootBone != null && characterBones.TryGetValue(renderer.rootBone.name, out var rootBt))
            newRenderer.rootBone = rootBt;
        else if (bones.Count > 0 && bones[0] != null)
            newRenderer.rootBone = bones[0];

        newRenderer.gameObject.layer = (int)Mathf.Log(layer.value, 2);
        return newRenderer;
    }
}


