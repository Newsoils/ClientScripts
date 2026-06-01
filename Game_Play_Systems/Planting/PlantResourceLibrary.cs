using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// 植物表现资源索引（懒加载单例）。只负责从 Resources 建立模型和材质查询表，不保存运行时状态。
    /// </summary>
    public class PlantResourceLibrary
    {
        private static PlantResourceLibrary _instance;
        public static PlantResourceLibrary Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PlantResourceLibrary();
                    _instance.LoadFromResources();
                }
                return _instance;
            }
        }

        // key: (modelKey, stage, variantKey) → 模型预制体
        // modelKey  = PlantData.modelKey，对应 Resources/Models/Plant 下文件名第一段，如 "rose"
        // stage     = 生长阶段（1-based），对应文件名第二段，如 "1"、"2"
        // variantKey= 变体名，对应文件名第三段，无第三段时默认 "default"
        // 文件命名规范：{modelKey}_{stage}.prefab 或 {modelKey}_{stage}_{variantKey}.prefab
        private readonly Dictionary<(string modelKey, int stage, string variantKey), GameObject> modelTemplates = new Dictionary<(string, int, string), GameObject>();

        // key: (modelKey, matKey) → 材质
        // modelKey = PlantData.modelKey，对应文件名第一段
        // matKey   = PlantData.matKey 列表中的某一项，对应文件名第二段
        // 文件命名规范：{modelKey}_{matKey}.mat
        private readonly Dictionary<(string modelKey, string matKey), Material> materialTemplates = new Dictionary<(string, string), Material>();

        private void LoadFromResources()
        {
            LoadModelTemplates();
            LoadMaterialTemplates();
        }

        public bool TryGetModel(string modelKey, int stage, string variantKey, out GameObject model)
        {
            string variant = string.IsNullOrEmpty(variantKey) ? "default" : variantKey;

            if (modelTemplates.TryGetValue((modelKey, stage, variant), out model))
                return true;

            if (modelTemplates.TryGetValue((modelKey, stage, "default"), out model))
                return true;

            return modelTemplates.TryGetValue(("default", stage, variant), out model);
        }

        public bool TryGetMaterial(string modelKey, string matKey, out Material material)
        {
            return materialTemplates.TryGetValue((modelKey, matKey), out material);
        }

        private void LoadModelTemplates()
        {
            modelTemplates.Clear();
            var sources = Resources.LoadAll<GameObject>("Models/Plant").ToList();

            foreach (var source in sources)
            {
                string[] names = source.name.Split('_');
                if (names.Length < 2)
                    continue;

                string modelKey = names[0];
                if (!int.TryParse(names[1], out int stage))
                    continue;

                string variantKey = names.Length >= 3 ? names[2] : "default";
                modelTemplates[(modelKey, stage, variantKey)] = source;
            }
        }

        private void LoadMaterialTemplates()
        {
            materialTemplates.Clear();
            var sources = Resources.LoadAll<Material>("Materials/Plant").ToList();

            foreach (var source in sources)
            {
                string[] names = source.name.Split('_');
                if (names.Length < 2)
                    continue;

                string modelKey = names[0];
                string matKey = names[1];
                materialTemplates[(modelKey, matKey)] = source;
            }
        }
    }
}
