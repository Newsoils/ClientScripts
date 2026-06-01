namespace CLIP.Framework_Unity.Asset
{
    /// <summary>
    /// 游戏资产路径统一定义。
    /// 所有资源扫描路径、生成路径、运行时加载路径均在此集中管理。
    ///
    /// 【跨平台说明】
    /// - Resources 目录：Unity 编译后任意平台均可用 Resources.Load 读取，跨平台最可靠。
    /// </summary>
    public static class GameAssetsPathDefine
    {
        // ============================================================
        // 资源扫描根目录（Editor）
        // ============================================================

        /// <summary>Editor 下扫描 Runtime 资源的根目录。</summary>
        public const string RuntimeAssetsRoot = "Assets/Resources/RuntimeAssets";

        // ============================================================
        // 生成文件路径（Editor → Resources 发布路径）
        // ============================================================

        /// <summary>
        /// 资源索引 JSON 发布路径（Editor 生成写入的目标路径）。
        /// 使用 Resources 目录，发布后可通过 Resources.Load 跨平台读取。
        /// </summary>
        public const string ResourceIndexJsonPublishPath = "Assets/Resources/Config/resource_index.json";

        /// <summary>
        /// Proto all.json 发布路径（Editor 生成写入的目标路径）。
        /// 使用 Resources 目录，发布后可通过 Resources.Load 跨平台读取。
        /// </summary>
        public const string ProtoAllJsonPublishPath = "Assets/Resources/all.json";

        // ============================================================
        // Resources.Load 使用的资源路径（不含扩展名）
        // ============================================================

        /// <summary>
        /// resource_index.json 的 Resources.Load 路径（不含扩展名）。
        /// 加载方式：Resources.Load&lt;TextAsset&gt;(ResourceIndexJsonResourcesPath)
        /// </summary>
        public const string ResourceIndexJsonResourcesPath = "Config/resource_index";

        /// <summary>
        /// all.json 的 Resources.Load 路径（不含扩展名）。
        /// 加载方式：Resources.Load&lt;TextAsset&gt;(ProtoAllJsonResourcesPath)
        /// </summary>
        public const string ProtoAllJsonResourcesPath = "all";

        // ============================================================
        // Editor 源码路径（ProtoAllJsonGenerator 写入位置）
        // ============================================================

        /// <summary>Proto all.json Editor 源码路径（ProtoAllJsonGenerator 生成到此，Editor 开发时使用）。</summary>
        public const string ProtoAllJsonSourcesPath = "Assets/Scripts/Proto/Sources/all.json";

        // ============================================================
        // Editor 工具内部使用的默认输出路径（菜单初始值）
        // ============================================================

        /// <summary>ResourceIndexGenerator 的 ScriptableObject 默认输出路径。</summary>
        public const string ResourceIndexSOEditorDefaultPath = "Assets/GameConfig/Generated/ResourceIndex.asset";

        /// <summary>ResourceIndexGenerator 的 ResKeys.cs 默认输出路径。</summary>
        public const string ResKeysEditorDefaultPath = "Assets/Scripts/Game_Play_Systems/ResKeys.cs";
    }
}
