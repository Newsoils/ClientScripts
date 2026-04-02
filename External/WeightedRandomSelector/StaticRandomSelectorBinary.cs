namespace DataStructures.RandomSelector
{
    using DataStructures.RandomSelector.Math;

    /// <summary>
    /// 使用二分查找法进行加权随机选择。
    /// 适用于元素数量较多的情况，效率较高。
    /// </summary>
    /// <typeparam name="T">要返回的元素类型</typeparam>
    public class StaticRandomSelectorBinary<T> : IRandomSelector<T>
    {

        System.Random random; // 内部随机数生成器
        T[] items;            // 元素数组
        float[] CDA;          // 累积分布数组（Cumulative Distribution Array）

        /// <summary>
        /// 构造函数 —— 通常由 StaticRandomSelectorBuilder 调用。
        /// 需要传入元素数组和累积分布数组。
        /// </summary>
        /// <param name="items">存储元素的数组</param>
        /// <param name="CDA">累积分布数组（Cumulative Distribution Array）</param>
        /// <param name="seed">内部随机数生成器的种子</param>
        public StaticRandomSelectorBinary(T[] items, float[] CDA, int seed)
        {

            this.items = items;
            this.CDA = CDA;
            this.random = new System.Random(seed);
        }

        /// <summary>
        /// 按元素的权重随机选择一个元素。
        /// 使用二分查找来确定被选中的索引。
        /// </summary>
        /// <returns>返回被选中的元素</returns>
        public T SelectRandomItem()
        {

            float randomValue = (float)random.NextDouble(); // 获取0~1之间的随机浮点数

            // 根据随机值在累积分布数组中查找对应索引
            return items[CDA.SelectIndexBinarySearch(randomValue)];
        }

        /// <summary>
        /// 按元素的权重随机选择一个元素。
        /// 可手动传入随机值（0~1）。
        /// 使用二分查找确定结果。
        /// </summary>
        /// <param name="randomValue">来自外部均匀分布随机源的值</param>
        /// <returns>返回被选中的元素</returns>
        public T SelectRandomItem(float randomValue)
        {

            return items[CDA.SelectIndexBinarySearch(randomValue)];
        }
    }
}
