namespace DataStructures.RandomSelector
{
    using DataStructures.RandomSelector.Math;

    /// <summary>
    /// 使用「线性搜索（Linear Search）」的随机选择器。
    /// 适用于数据量较小的情况（例如几十个元素以内）。
    /// 当元素数量较少时，线性搜索比二分搜索在 CPU 分支预测上更有优势。
    /// </summary>
    /// <typeparam name="T">要被选择的元素类型。</typeparam>
    public class StaticRandomSelectorLinear<T> : IRandomSelector<T>
    {

        System.Random random;

        // 内部缓存：待选择的元素和对应的累积分布数组（CDA）
        T[] items;
        float[] CDA;

        /// <summary>
        /// 构造函数（通常由 StaticRandomSelectorBuilder 调用）。
        /// 需要传入待选项数组以及对应的「累积分布数组」。
        /// </summary>
        /// <param name="items">存放待选元素的数组。</param>
        /// <param name="CDA">累积分布数组（Cumulative Distribution Array）。</param>
        /// <param name="seed">随机数种子，用于内部随机生成器。</param>
        public StaticRandomSelectorLinear(T[] items, float[] CDA, int seed)
        {
            this.items = items;
            this.CDA = CDA;
            this.random = new System.Random(seed);
        }

        /// <summary>
        /// 根据权重随机选择一个元素。
        /// 使用线性搜索方式在 CDA 中查找对应区间。
        /// </summary>
        /// <returns>返回一个被选中的元素。</returns>
        public T SelectRandomItem(float randomValue)
        {
            // 使用线性搜索查找 randomValue 对应的区间索引
            return items[CDA.SelectIndexLinearSearch(randomValue)];
        }

        /// <summary>
        /// 根据权重随机选择一个元素。
        /// 自动生成 [0,1) 的随机值，然后使用线性搜索选择元素。
        /// </summary>
        /// <param name="randomValue">（可选）外部提供的随机值。</param>
        /// <returns>返回一个被选中的元素。</returns>
        public T SelectRandomItem()
        {
            float randomValue = (float)random.NextDouble();
            return items[CDA.SelectIndexLinearSearch(randomValue)];
        }
    }
}
