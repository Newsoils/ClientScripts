using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// 服装表现资源索引（懒加载单例）。只负责从 Resources 建立 SKM 模板和材质模板查询表，不保存角色穿戴状态。
    /// </summary>
    public class CharacterClothesResourceLibrary
    {
        private static CharacterClothesResourceLibrary _instance;
        public static CharacterClothesResourceLibrary Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CharacterClothesResourceLibrary();
                    _instance.LoadFromResources();
                }
                return _instance;
            }
        }

        // key: suitName → (slotKey → [(partIndex, SKM模板)])
        // suitName  = 套装名，对应 SKM 文件名第一段，如 "maid01"、"default"
        // slotKey   = 槽位名，对应文件名第三段，如 "head"、"body"、"leg"
        // partIndex = 同槽位内的部件序号，对应文件名第四段，如 "02"
        // 文件命名规范：{suitName}_{?}_{slotKey}_{partIndex}（SKM 组件名）
        private readonly Dictionary<string, Dictionary<string, List<(int partIndex, SkinnedMeshRenderer skm)>>> rendererTemplates = new Dictionary<string, Dictionary<string, List<(int, SkinnedMeshRenderer)>>>();

        // key: suitName → (colorKey → [(materialIndex, 材质)])
        // suitName      = 套装名，对应材质文件名第一段，如 "maid01"
        // colorKey      = 颜色/配色方案名，对应文件名第三段，如 "red"、"blue"
        // materialIndex = 同配色内的材质序号，对应文件名第二段
        // 文件命名规范：{suitName}_{materialIndex}_{colorKey}.mat
        private readonly Dictionary<string, Dictionary<string, List<(int materialIndex, Material mat)>>> materialTemplates = new Dictionary<string, Dictionary<string, List<(int, Material)>>>();

        private void LoadFromResources()
        {
            LoadRendererTemplates();
            LoadMaterialTemplates();
        }

    public bool TryGetRendererParts(string suitName, string slot, out List<(int, SkinnedMeshRenderer)> parts)
    {
        parts = null;
        return rendererTemplates.TryGetValue(suitName, out var suitData)
            && suitData.TryGetValue(slot, out parts);
    }

    public bool TryGetDefaultRendererParts(string slot, out List<(int, SkinnedMeshRenderer)> parts)
    {
        return TryGetRendererParts("default", slot, out parts);
    }

    public bool TryGetMaterialData(string suitName, string colorKey, out List<(int, Material)> materials)
    {
        materials = null;
        return materialTemplates.TryGetValue(suitName, out var suitData)
            && suitData.TryGetValue(colorKey, out materials);
    }

    private void LoadRendererTemplates()
    {
        rendererTemplates.Clear();
        var sources = Resources.LoadAll<GameObject>("Models/Clothes").ToList();

        foreach (var source in sources)
        {
            var parts = source.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var part in parts)
            {
                string[] names = part.name.Split('_');
                if (names.Length < 3)
                    continue;

                string indexToken = names.Length >= 4 ? names[3] : "02";
                if (!int.TryParse(indexToken, out int partIndex))
                    continue;

                string suitKey = names[0];
                string slotKey = names[2];
                if (!rendererTemplates.TryGetValue(suitKey, out var suitData))
                {
                    suitData = new Dictionary<string, List<(int, SkinnedMeshRenderer)>>();
                    rendererTemplates[suitKey] = suitData;
                }

                if (!suitData.TryGetValue(slotKey, out var slotData))
                {
                    slotData = new List<(int, SkinnedMeshRenderer)>();
                    suitData[slotKey] = slotData;
                }

                slotData.Add((partIndex, part));
            }
        }
    }

    private void LoadMaterialTemplates()
    {
        materialTemplates.Clear();
        var sources = Resources.LoadAll<Material>("Materials/Clothes").ToList();

        foreach (var source in sources)
        {
            string[] names = source.name.Split('_');
            if (names.Length < 3)
                continue;

            string suitKey = names[0];
            string colorKey = names[2];
            int materialIndex = int.Parse(names[1]);

            if (!materialTemplates.TryGetValue(suitKey, out var suitData))
            {
                suitData = new Dictionary<string, List<(int, Material)>>();
                materialTemplates[suitKey] = suitData;
            }

            if (!suitData.TryGetValue(colorKey, out var colorData))
            {
                colorData = new List<(int, Material)>();
                suitData[colorKey] = colorData;
            }

            colorData.Add((materialIndex, source));
        }
    }
}
}
