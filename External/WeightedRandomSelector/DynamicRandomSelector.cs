using System.Collections;
using System.Collections.Generic;
using System;

namespace DataStructures.RandomSelector
{
    using DataStructures.RandomSelector.Math;

    /// <summary>
    /// DynamicRandomSelector（动态随机选择器）
    /// 允许在运行时动态添加或移除元素。
    /// 在完成修改后必须调用 Build() 来重建内部数据结构。
    /// 每次 Build() 后会根据元素数量自动选择线性搜索或二分搜索，
    /// 以在通用使用场景下获得更好的性能。
    /// </summary>
    /// <typeparam name="T">希望返回的元素类型</typeparam>
    public class DynamicRandomSelector<T> : IRandomSelector<T>, IRandomSelectorBuilder<T>
    {

        Random random; // 内部随机数生成器

        // 内部数据缓存

        /// <summary>
        /// 元素列表
        /// </summary>
        List<T> itemsList;
        /// <summary>
        /// 对应权重列表（未归一化）
        /// </summary>
        List<float> weightsList;
        /// <summary>
        /// 累积分布列表（Cumulative Distribution List）
        /// </summary>
        List<float> CDL;         

        /// <summary>
        /// 内部函数指针，根据Build时的结果在 线性/二分 搜索之间切换
        /// </summary>
        private Func<List<float>, float, int> selectFunction;

        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="seed">随机数种子。如果为 -1，则使用系统随机种子</param>
        /// <param name="expectedNumberOfItems">预期元素数量，用于减少GC开销</param>
        public DynamicRandomSelector(int seed = -1, int expectedNumberOfItems = 32)
        {

            if (seed == -1)
                random = new System.Random();
            else
                random = new System.Random(seed);

            itemsList = new List<T>(expectedNumberOfItems);
            weightsList = new List<float>(expectedNumberOfItems);
            CDL = new List<float>(expectedNumberOfItems);
        }

        /// <summary>
        /// 构造函数：通过数组直接初始化元素与权重
        /// </summary>
        /// <param name="items">要被随机选取的元素数组</param>
        /// <param name="weights">每个元素对应的非归一化权重，长度需与items一致</param>
        /// <param name="seed">随机数种子，-1为自动生成</param>
        /// <param name="expectedNumberOfItems">预期元素数量</param>
        public DynamicRandomSelector(T[] items, float[] weights, int seed = -1, int expectedNumberOfItems = 32) : this()
        {

            for (int i = 0; i < items.Length; i++)
                Add(items[i], weights[i]);

            Build();
        }

        /// <summary>
        /// 构造函数：通过 List 初始化元素与权重
        /// </summary>
        /// <param name="items">要被随机选取的元素列表</param>
        /// <param name="weights">对应的权重列表（未归一化）</param>
        /// <param name="seed">随机数种子，-1为自动生成</param>
        /// <param name="expectedNumberOfItems">预期元素数量</param>
        public DynamicRandomSelector(List<T> items, List<float> weights, int seed = -1, int expectedNumberOfItems = 32) : this()
        {

            for (int i = 0; i < items.Count; i++)
                Add(items[i], weights[i]);

            Build();
        }

        /// <summary>
        /// 清空内部数据缓存。
        /// 除非内部List中的对象仍被外部引用，否则不会产生额外垃圾。
        /// </summary>
        public void Clear()
        {
            itemsList.Clear();
            weightsList.Clear();
            CDL.Clear();
        }

        /// <summary>
        /// 添加一个新元素及其权重。
        /// 权重为零的元素会被忽略。
        /// 注意不要添加重复元素，否则在移除时可能导致逻辑错误。
        /// 添加完成后必须调用 Build() 来更新分布。
        /// </summary>
        /// <param name="item">将被随机选取返回的元素</param>
        /// <param name="weight">该元素的非归一化权重，不能为0</param>
        public void Add(T item, float weight)
        {
            // 忽略权重为0的元素
            if (weight == 0)
                return;

            itemsList.Add(item);
            weightsList.Add(weight);
        }

        public void AddRange(T[] items, float[] weights)
        {
            for (int i = 0; i < items.Length; i++)
                Add(items[i], weights[i]);
        }

        public void AddRange(List<T> items, List<float> weights)
        {
            for (int i = 0; i < items.Count; i++)
                Add(items[i], weights[i]);
        }

        public void AddRange(Dictionary<T, float> itemWeightMap)
        {
            foreach (var kvp in itemWeightMap)
                Add(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// 从集合中移除一个元素。
        /// 移除后需要调用 Build() 重建分布。
        /// </summary>
        /// <param name="item">要移除的元素</param>
        public void Remove(T item)
        {

            int index = itemsList.IndexOf(item);

            // 如果未找到该元素则直接返回
            if (index == -1)
                return;

            itemsList.RemoveAt(index);
            weightsList.RemoveAt(index);
            // CDL 会在 Build() 时重建，因此此处无需处理
        }

        /// <summary>
        /// 构建或重建内部的累积分布列表（CDL）。
        /// 必须在添加或移除元素后调用。
        /// 会根据元素数量自动切换为线性或二分搜索模式。
        /// 注意：首次构建可能会有轻微GC（List扩容）。
        /// </summary>
        /// <param name="seed">可选：指定随机数种子；-1表示保持当前随机对象，-2表示重新随机化种子</param>
        /// <returns>返回自身（方便链式调用）</returns>
        public IRandomSelector<T> Build(int seed = -1)
        {

            if (itemsList.Count == 0)
                throw new Exception("无法构建：没有任何元素。");

            // 清空CDL后填入所有权重
            CDL.Clear();
            for (int i = 0; i < weightsList.Count; i++)
                CDL.Add(weightsList[i]);

            // 构建累积分布
            RandomMath.BuildCumulativeDistribution(CDL);

            // 若指定了新的seed则重设随机数发生器
            if (seed != -1)
            {

                // 若输入为 -2，则重新随机化一个种子
                if (seed == -2)
                {
                    seed = random.Next();
                    random = new Random(seed);
                }
                else
                {
                    random = new Random(seed);
                }
            }

            // 根据列表长度决定使用线性还是二分搜索
            // ListBreakpoint 是 RandomMath 内部设定的分界点
            if (CDL.Count < RandomMath.ListBreakpoint)
                selectFunction = RandomMath.SelectIndexLinearSearch;
            else
                selectFunction = RandomMath.SelectIndexBinarySearch;

            return this;
        }

        /// <summary>
        /// 根据给定的随机值（0~1）选取一个元素。
        /// 内部会自动使用线性或二分查找。
        /// </summary>
        /// <param name="randomValue">手动提供的随机值</param>
        /// <returns>被选中的元素</returns>
        public T SelectRandomItem(float randomValue)
        {
            return itemsList[selectFunction(CDL, randomValue)];
        }

        /// <summary>
        /// 使用内部随机数生成器选取一个元素。
        /// 内部会自动使用线性或二分查找。
        /// </summary>
        /// <returns>被选中的元素</returns>
        public T SelectRandomItem()
        {
            float randomValue = (float)random.NextDouble();
            return itemsList[selectFunction(CDL, randomValue)];
        }
    }
}
