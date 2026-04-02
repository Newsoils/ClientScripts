using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MaterialPrefabGenerator : EditorWindow
{
    private string modelFolderRootPath = "Assets/Models";
    private string modelFolderPath = "Assets/Models";
    private string textureFolderPath = "Assets/Art/Res/Textures";
    private string materialOutputPath = "Assets/Art/Res/Materials";
    private string prefabOutputPath = "Assets/Art/Prefabs";
    private string extraPrefabFolder = "Extra"; // 额外Prefab的文件夹名
    private string extraSuffix = "_Photo";
    private bool generateExtraPrefabs = true;
    private bool addNumberPrefix = false;
    private string numberPrefixFormat = "{0}_";

    // URP Shader路径
    private string urpLitShaderPath = "Universal Render Pipeline/Lit";

    // 支持的模型格式
    private readonly string[] modelExtensions = { ".fbx", ".obj", ".blend", ".max", ".mb", ".ma" };

    [MenuItem("Tools/批量生成材质和Prefab")]
    private static void ShowWindow()
    {
        var window = GetWindow<MaterialPrefabGenerator>("材质Prefab生成器");
        window.minSize = new Vector2(400, 450);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("路径设置", EditorStyles.boldLabel);

        EditorGUILayout.Space(5);
        modelFolderPath = EditorGUILayout.TextField("模型文件夹:", modelFolderPath);
        textureFolderPath = EditorGUILayout.TextField("贴图文件夹:", textureFolderPath);
        materialOutputPath = EditorGUILayout.TextField("材质输出:", materialOutputPath);
        prefabOutputPath = EditorGUILayout.TextField("Prefab输出:", prefabOutputPath);

        EditorGUILayout.Space(10);
        GUILayout.Label("URP设置", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("使用Universal Render Pipeline/Lit Shader", MessageType.Info);

        EditorGUILayout.Space(10);
        GUILayout.Label("Prefab设置", EditorStyles.boldLabel);

        generateExtraPrefabs = EditorGUILayout.Toggle("生成额外Prefab", generateExtraPrefabs);
        if (generateExtraPrefabs)
        {
            extraPrefabFolder = EditorGUILayout.TextField("额外Prefab文件夹:", extraPrefabFolder);
            extraSuffix = EditorGUILayout.TextField("额外后缀:", extraSuffix);
        }

        EditorGUILayout.Space(10);
        GUILayout.Label("序号前缀设置", EditorStyles.boldLabel);

        addNumberPrefix = EditorGUILayout.Toggle("添加序号前缀", addNumberPrefix);
        if (addNumberPrefix)
        {
            EditorGUILayout.HelpBox("注意：将在模型文件和贴图文件夹前添加1_, 2_, 3_...这样的序号前缀", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("格式:");
            GUILayout.Label(numberPrefixFormat, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(20);

        if (GUILayout.Button("开始批量生成", GUILayout.Height(30)))
        {
            GenerateMaterialsAndPrefabs();
        }

        EditorGUILayout.Space(10);

        if (GUILayout.Button("仅重命名文件添加序号", GUILayout.Height(25)))
        {
            AddNumberPrefixToFiles();
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "说明:\n" +
            "1. 使用URP Lit Shader创建材质\n" +
            "2. 根据模型名在贴图路径下查找同名文件夹\n" +
            "3. 查找Base_color和Roughness贴图\n" +
            "4. 根据模型位置创建对应目录结构\n" +
            "5. 生成材质和Prefab\n" +
            "6. 额外Prefab存放在单独的文件夹内\n" +
            "7. 可选：添加1_, 2_, 3_...序号前缀",
            MessageType.Info
        );
    }

    private void GenerateMaterialsAndPrefabs()
    {
        if (!Directory.Exists(modelFolderPath))
        {
            EditorUtility.DisplayDialog("错误", "模型文件夹不存在！", "确定");
            return;
        }

        if (!Directory.Exists(textureFolderPath))
        {
            EditorUtility.DisplayDialog("错误", "贴图文件夹不存在！", "确定");
            return;
        }

        // 检查URP Lit Shader是否存在
        Shader urpLitShader = Shader.Find(urpLitShaderPath);
        if (urpLitShader == null)
        {
            EditorUtility.DisplayDialog("错误",
                $"找不到URP Lit Shader: {urpLitShaderPath}\n请确保已安装Universal RP包",
                "确定");
            return;
        }

        // 如果开启序号前缀，先重命名文件
        if (addNumberPrefix)
        {
            if (!AddNumberPrefixToFiles())
            {
                return;
            }
        }

        // 创建输出目录
        Directory.CreateDirectory(materialOutputPath);
        Directory.CreateDirectory(prefabOutputPath);

        // 获取所有模型文件
        var modelFiles = GetAllModelFiles(modelFolderPath);
        int processedCount = 0;
        int successCount = 0;

        EditorUtility.DisplayProgressBar("批量生成", "正在搜索模型...", 0);

        foreach (var modelFile in modelFiles)
        {
            processedCount++;
            float progress = (float)processedCount / modelFiles.Count;
            EditorUtility.DisplayProgressBar("批量生成", $"处理: {Path.GetFileName(modelFile)}", progress);

            try
            {
                ProcessModel(modelFile, urpLitShader);
                successCount++;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"处理模型 {modelFile} 失败: {e.Message}");
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("完成",
            $"处理完成!\n总计: {modelFiles.Count}\n成功: {successCount}\n失败: {modelFiles.Count - successCount}",
            "确定"
        );
    }

    private bool AddNumberPrefixToFiles()
    {
        // 获取所有模型文件
        var modelFiles = GetAllModelFiles(modelFolderPath);

        // 按字母排序，确保一致的序号分配
        modelFiles.Sort();

        int index = 1;
        bool hasRenamed = false;

        EditorUtility.DisplayProgressBar("添加序号前缀", "正在处理模型文件...", 0);

        foreach (var modelFile in modelFiles)
        {
            string modelName = Path.GetFileNameWithoutExtension(modelFile);
            string extension = Path.GetExtension(modelFile);
            string directory = Path.GetDirectoryName(modelFile);

            // 检查是否已有数字前缀
            if (HasNumberPrefix(modelName))
            {
                // 如果已有数字前缀，更新序号为当前index
                string nameWithoutPrefix = RemoveNumberPrefix(modelName);
                string newModelName = string.Format(numberPrefixFormat, index) + nameWithoutPrefix;
                string newModelPath = Path.Combine(directory, newModelName + extension);

                if (modelFile != newModelPath)
                {
                    string error = AssetDatabase.RenameAsset(modelFile, newModelName);
                    if (string.IsNullOrEmpty(error))
                    {
                        Debug.Log($"重命名模型: {Path.GetFileName(modelFile)} -> {newModelName}{extension}");
                        hasRenamed = true;

                        // 查找对应的贴图文件夹
                        string originalTextureFolder = FindOriginalTextureFolder(modelName);
                        if (!string.IsNullOrEmpty(originalTextureFolder) && Directory.Exists(originalTextureFolder))
                        {
                            string newTextureFolderName = newModelName;
                            string newTextureFolderPath = Path.Combine(Path.GetDirectoryName(originalTextureFolder), newTextureFolderName);

                            if (originalTextureFolder != newTextureFolderPath)
                            {
                                // 重命名贴图文件夹
                                string folderRenameError = AssetDatabase.RenameAsset(originalTextureFolder, newTextureFolderName);
                                if (string.IsNullOrEmpty(folderRenameError))
                                {
                                    Debug.Log($"重命名贴图文件夹: {Path.GetFileName(originalTextureFolder)} -> {newTextureFolderName}");
                                }
                                else
                                {
                                    Debug.LogWarning($"重命名贴图文件夹失败: {folderRenameError}");
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"重命名模型失败: {error}");
                    }
                }
            }
            else
            {
                // 如果没有数字前缀，添加新的前缀
                string newModelName = string.Format(numberPrefixFormat, index) + modelName;
                string newModelPath = Path.Combine(directory, newModelName + extension);

                if (modelFile != newModelPath)
                {
                    string error = AssetDatabase.RenameAsset(modelFile, newModelName);
                    if (string.IsNullOrEmpty(error))
                    {
                        Debug.Log($"重命名模型: {Path.GetFileName(modelFile)} -> {newModelName}{extension}");
                        hasRenamed = true;

                        // 查找对应的贴图文件夹
                        string originalTextureFolder = FindOriginalTextureFolder(modelName);
                        if (!string.IsNullOrEmpty(originalTextureFolder) && Directory.Exists(originalTextureFolder))
                        {
                            string newTextureFolderName = newModelName;
                            string newTextureFolderPath = Path.Combine(Path.GetDirectoryName(originalTextureFolder), newTextureFolderName);

                            if (originalTextureFolder != newTextureFolderPath)
                            {
                                // 重命名贴图文件夹
                                string folderRenameError = AssetDatabase.RenameAsset(originalTextureFolder, newTextureFolderName);
                                if (string.IsNullOrEmpty(folderRenameError))
                                {
                                    Debug.Log($"重命名贴图文件夹: {Path.GetFileName(originalTextureFolder)} -> {newTextureFolderName}");
                                }
                                else
                                {
                                    Debug.LogWarning($"重命名贴图文件夹失败: {folderRenameError}");
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"重命名模型失败: {error}");
                    }
                }
            }

            index++;
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();

        if (hasRenamed)
        {
            EditorUtility.DisplayDialog("完成", "已添加序号前缀到文件和文件夹", "确定");
        }

        return hasRenamed;
    }

    private string FindOriginalTextureFolder(string modelName)
    {
        // 尝试查找贴图文件夹（支持多种命名格式）
        string[] possiblePaths = {
            Path.Combine(textureFolderPath, modelName),
            Path.Combine(textureFolderPath, RemoveNumberPrefix(modelName))
        };

        foreach (var path in possiblePaths)
        {
            if (Directory.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    private bool HasNumberPrefix(string name)
    {
        // 检查是否以数字+下划线开头，如 "1_", "10_", "001_"
        return Regex.IsMatch(name, @"^\d+_");
    }

    private string RemoveNumberPrefix(string name)
    {
        // 移除数字前缀，如 "1_angel" -> "angel"
        if (HasNumberPrefix(name))
        {
            return Regex.Replace(name, @"^\d+_", "");
        }
        return name;
    }

    private List<string> GetAllModelFiles(string folderPath)
    {
        var modelFiles = new List<string>();
        var allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

        foreach (var file in allFiles)
        {
            foreach (var ext in modelExtensions)
            {
                if (file.EndsWith(ext, System.StringComparison.OrdinalIgnoreCase))
                {
                    modelFiles.Add(file);
                    break;
                }
            }
        }

        return modelFiles;
    }

    private void ProcessModel(string modelPath, Shader urpLitShader)
    {
        // 获取模型名（不带扩展名）
        string modelName = Path.GetFileNameWithoutExtension(modelPath);
        string modelNameWithoutPrefix = RemoveNumberPrefix(modelName);

        // 获取模型相对路径（用于确定分类）
        string relativePath = GetRelativePath(modelPath, modelFolderRootPath);
        string category = GetCategoryFromPath(relativePath);

        // 查找贴图文件夹（支持带前缀和不带前缀）
        string textureFolder = Path.Combine(textureFolderPath, modelName);
        if (!Directory.Exists(textureFolder))
        {
            // 尝试不带前缀的名称
            textureFolder = Path.Combine(textureFolderPath, modelNameWithoutPrefix);
            if (!Directory.Exists(textureFolder))
            {
                Debug.LogWarning($"未找到贴图文件夹: {modelName} 或 {modelNameWithoutPrefix}，跳过模型: {modelName}");
                return;
            }
        }

        // 查找贴图文件（支持多种命名格式）
        string baseColorPath = FindTexture(textureFolder, "Base_color", modelName, modelNameWithoutPrefix);
        string roughnessPath = FindTexture(textureFolder, "Roughness", modelName, modelNameWithoutPrefix);

        if (string.IsNullOrEmpty(baseColorPath))
        {
            Debug.LogWarning($"未找到BaseColor贴图，跳过模型: {modelName}");
            return;
        }

        // 创建URP材质
        Material material = CreateURPMaterial(category, modelName, baseColorPath, roughnessPath, urpLitShader);
        if (material == null) return;

        // 创建Prefab
        GameObject prefab = CreatePrefab(modelPath, category, modelName, material);
        if (prefab == null) return;

        // 如果需要，生成额外Prefab到单独文件夹
        if (generateExtraPrefabs && !string.IsNullOrEmpty(extraSuffix))
        {
            CreateExtraPrefabInFolder(prefab, category, modelName);
        }
    }

    private string GetRelativePath(string fullPath, string basePath)
    {
        fullPath = fullPath.Replace("\\", "/");
        basePath = basePath.Replace("\\", "/");

        if (fullPath.StartsWith(basePath))
        {
            return fullPath.Substring(basePath.Length).TrimStart('/');
        }
        return string.Empty;
    }

    private string GetCategoryFromPath(string relativePath)
    {
        // 获取第一个文件夹名作为分类，如果没有则返回"Common"
        if (string.IsNullOrEmpty(relativePath))
            return "Common";

        string[] parts = relativePath.Split('/');
        return parts.Length > 1 ? parts[0] : "Common";
    }

    private string FindTexture(string folderPath, string textureType, string modelName, string modelNameWithoutPrefix)
    {
        var files = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly);

        // 尝试几种可能的命名格式（带前缀和不带前缀）
        List<string> searchPatterns = new List<string>
        {
            $"{modelName}_*{textureType}*",
            $"{modelNameWithoutPrefix}_*{textureType}*",
            $"*{textureType}*",
            $"*{textureType.ToLower()}*",
            $"*{textureType.ToUpper()}*"
        };

        // 添加更多可能的命名格式
        searchPatterns.Add($"{modelName}*{textureType}*");
        searchPatterns.Add($"{modelNameWithoutPrefix}*{textureType}*");
        searchPatterns.Add($"{textureType}*{modelName}*");
        searchPatterns.Add($"{textureType}*{modelNameWithoutPrefix}*");

        // 尝试所有搜索模式
        foreach (var pattern in searchPatterns)
        {
            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string fileNameLower = fileName.ToLower();
                string textureTypeLower = textureType.ToLower();

                // 检查是否包含贴图类型关键词
                if (fileNameLower.Contains(textureTypeLower))
                {
                    // 检查是否包含模型名（带前缀或不带前缀）
                    if (fileNameLower.Contains(modelName.ToLower()) ||
                        fileNameLower.Contains(modelNameWithoutPrefix.ToLower()) ||
                        (modelNameWithoutPrefix != modelName && fileNameLower.Contains(modelNameWithoutPrefix.ToLower())))
                    {
                        return file;
                    }
                }
            }
        }

        // 如果上面的都没找到，再尝试宽松匹配
        foreach (var file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file).ToLower();
            if (fileName.Contains(textureType.ToLower()))
            {
                return file;
            }
        }

        return null;
    }

    private Material CreateURPMaterial(string category, string modelName, string baseColorPath, string roughnessPath, Shader urpLitShader)
    {
        // 创建材质输出目录
        string materialCategoryPath = Path.Combine(materialOutputPath, category);
        Directory.CreateDirectory(materialCategoryPath);

        string materialPath = Path.Combine(materialCategoryPath, $"{modelName}.mat");

        // 检查是否已存在材质
        Material existingMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (existingMaterial != null)
        {
            Debug.Log($"材质已存在: {materialPath}");
            return existingMaterial;
        }

        // 创建新的URP材质
        Material material = new Material(urpLitShader);
        material.name = modelName;

        // 加载贴图
        Texture2D baseColorTex = AssetDatabase.LoadAssetAtPath<Texture2D>(baseColorPath);
        if (baseColorTex != null)
        {
            // URP Lit Shader使用 _BaseMap 作为基础贴图
            material.SetTexture("_BaseMap", baseColorTex);
            material.SetColor("_BaseColor", Color.white);
        }

        // 处理粗糙度贴图
        if (!string.IsNullOrEmpty(roughnessPath))
        {
            Texture2D roughnessTex = AssetDatabase.LoadAssetAtPath<Texture2D>(roughnessPath);
            if (roughnessTex != null)
            {
                // URP中通常使用 _MetallicGlossMap 或 _MaskMap
                // 对于简单的Lit Shader，我们可以将粗糙度贴图设置为 _MetallicGlossMap
                material.SetTexture("_MetallicGlossMap", roughnessTex);

                // 启用贴图关键字
                material.EnableKeyword("_METALLICSPECGLOSSMAP");

                // 设置金属度和光滑度
                material.SetFloat("_Metallic", 0.5f);

                // 粗糙度贴图通常需要反转作为光滑度，这里设为1表示使用贴图值
                material.SetFloat("_Smoothness", 1.0f);

                // 设置贴图通道（粗糙度通常在Alpha通道）
                material.SetFloat("_SmoothnessTextureChannel", 0); // 0 = Alpha通道
            }
        }
        else
        {
            // 如果没有粗糙度贴图，设置默认值
            material.SetFloat("_Metallic", 0.0f);
            material.SetFloat("_Smoothness", 0.5f);
        }

        // 设置URP Lit材质的一些常用属性
        material.SetFloat("_WorkflowMode", 1.0f); // 1 = Metallic工作流
        material.SetFloat("_Surface", 0); // 0 = Opaque, 1 = Transparent

        // 保存材质
        AssetDatabase.CreateAsset(material, materialPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"创建URP材质: {materialPath}");
        return material;
    }

    private GameObject CreatePrefab(string modelPath, string category, string modelName, Material material)
    {
        // 创建Prefab输出目录
        string prefabCategoryPath = Path.Combine(prefabOutputPath, category);
        Directory.CreateDirectory(prefabCategoryPath);

        string prefabPath = Path.Combine(prefabCategoryPath, $"{modelName}.prefab");

        // 检查是否已存在Prefab
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
        {
            Debug.Log($"Prefab已存在: {prefabPath}");
            return existingPrefab;
        }

        // 加载模型
        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (modelPrefab == null)
        {
            Debug.LogWarning($"无法加载模型: {modelPath}");
            return null;
        }

        // 实例化模型
        GameObject instance = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
        if (instance == null)
        {
            instance = GameObject.Instantiate(modelPrefab);
        }

        instance.name = modelName;

        // 应用材质到所有Renderer组件（包括MeshRenderer和SkinnedMeshRenderer）
        ApplyMaterialToRenderers(instance, material);

        // 创建Prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        GameObject.DestroyImmediate(instance);

        Debug.Log($"创建Prefab: {prefabPath}");
        return prefab;
    }

    private void ApplyMaterialToRenderers(GameObject gameObject, Material material)
    {
        // 获取所有Renderer组件
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            // 为每个Renderer设置材质
            Material[] materials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }
            renderer.sharedMaterials = materials;

            // 如果是SkinnedMeshRenderer，还需要处理材质
            if (renderer is SkinnedMeshRenderer skinnedRenderer)
            {
                // SkinnedMeshRenderer使用相同的方式处理材质
                Material[] skinnedMaterials = new Material[skinnedRenderer.sharedMaterials.Length];
                for (int i = 0; i < skinnedMaterials.Length; i++)
                {
                    skinnedMaterials[i] = material;
                }
                skinnedRenderer.sharedMaterials = skinnedMaterials;
            }
        }
    }

    private void CreateExtraPrefabInFolder(GameObject originalPrefab, string category, string modelName)
    {
        if (originalPrefab == null) return;

        // 创建额外Prefab文件夹路径
        string extraFolderPath = Path.Combine(prefabOutputPath, extraPrefabFolder, category);
        Directory.CreateDirectory(extraFolderPath);

        string extraPrefabPath = Path.Combine(extraFolderPath, $"{modelName}{extraSuffix}.prefab");

        // 检查是否已存在额外Prefab
        GameObject existingExtraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(extraPrefabPath);
        if (existingExtraPrefab != null)
        {
            Debug.Log($"额外Prefab已存在: {extraPrefabPath}");
            return;
        }

        // 复制Prefab到额外文件夹
        string originalPrefabPath = AssetDatabase.GetAssetPath(originalPrefab);

        // 确保原始路径和额外路径不同
        if (originalPrefabPath != extraPrefabPath)
        {
            // 复制Prefab到新路径
            bool success = AssetDatabase.CopyAsset(originalPrefabPath, extraPrefabPath);

            if (success)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // 加载新创建的Prefab
                GameObject newPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(extraPrefabPath);
                if (newPrefab != null)
                {
                    // 如果需要，可以对新Prefab进行特殊处理
                    Debug.Log($"创建额外Prefab: {extraPrefabPath}");
                }
            }
            else
            {
                Debug.LogError($"复制Prefab失败: {originalPrefabPath} -> {extraPrefabPath}");
            }
        }
        else
        {
            Debug.LogWarning($"原始Prefab路径和额外Prefab路径相同，跳过: {originalPrefabPath}");
        }
    }
}