using System.Collections;
using System.Collections.Generic;
using System;

using Stopwatch = System.Diagnostics.Stopwatch;

namespace DataStructures.RandomSelector.Math
{

    /// <summary>
    /// 提供与随机选择算法相关的数学工具函数。
    /// 包含生成累积分布、线性/二分查找、随机权重等功能。
    /// </summary>
    public static class RandomMath
    {

        /// <summary>
        /// 当元素数量小于该值时，静态选择器使用线性查找；
        /// 当元素数量大于该值时，改用二分查找。
        /// 该数值通过实验确定。
        /// </summary>
        public static readonly int ArrayBreakpoint = 51;

        /// <summary>
        /// 当元素数量小于该值时，动态选择器使用线性查找；
        /// 当元素数量大于该值时，改用二分查找。
        /// 该数值通过实验确定。
        /// </summary>
        public static readonly int ListBreakpoint = 26;

        /// <summary>
        /// 在原地构建“累积分布列表（CDL）”，输入为未归一化的权重。
        /// 会自动进行归一化并将每个元素转为累积概率。
        /// </summary>
        /// <param name="CDL">权重列表（未归一化）</param>
        public static void BuildCumulativeDistribution(List<float> CDL)
        {

            int Length = CDL.Count;

            // 使用 double 提高精度，避免浮点误差
            double Sum = 0;

            // 计算权重总和
            for (int i = 0; i < Length; i++)
                Sum += CDL[i];

            // 计算归一化常数 k = 1 / Sum
            // 乘法比除法更快，所以后面直接用乘法
            double k = (1f / Sum);

            Sum = 0;

            // 构建累积分布
            for (int i = 0; i < Length; i++)
            {

                Sum += CDL[i] * k; // 将当前权重累加，并归一化
                CDL[i] = (float)Sum;
            }

            // 最后一个元素强制设为 1，防止累计误差导致略小于 1
            CDL[Length - 1] = 1f;
        }

        /// <summary>
        /// 与上一个函数相同，但操作对象是数组。
        /// 在原地构建“累积分布数组（CDA）”，输入为未归一化的权重。
        /// </summary>
        /// <param name="CDA">权重数组（未归一化）</param>
        public static void BuildCumulativeDistribution(float[] CDA)
        {

            int Length = CDA.Length;
            double Sum = 0;

            // 计算权重总和
            for (int i = 0; i < Length; i++)
                Sum += CDA[i];

            double k = (1f / Sum);
            Sum = 0;

            // 构建累积分布数组
            for (int i = 0; i < Length; i++)
            {

                Sum += CDA[i] * k;
                CDA[i] = (float)Sum;
            }

            CDA[Length - 1] = 1f;
        }

        /// <summary>
        /// 线性查找法 —— 适合小型数组。
        /// 通过线性扫描找到随机值对应的区间索引。
        /// </summary>
        /// <param name="CDA">累积分布数组</param>
        /// <param name="randomValue">[0,1) 的随机值</param>
        /// <returns>返回随机命中的索引</returns>
        public static int SelectIndexLinearSearch(this float[] CDA, float randomValue)
        {

            int i = 0;

            // 最后一个元素应为 1
            while (CDA[i] < randomValue)
                i++;

            return i;
        }

        /// <summary>
        /// 二分查找法 —— 适合大型数组。
        /// 修改自 C# 内部的 Array.BinarySearch()。
        /// </summary>
        /// <param name="CDA">累积分布数组</param>
        /// <param name="randomValue">[0,1) 的随机值</param>
        /// <returns>返回随机命中的索引</returns>
        public static int SelectIndexBinarySearch(this float[] CDA, float randomValue)
        {

            int lo = 0;
            int hi = CDA.Length - 1;
            int index;

            while (lo <= hi)
            {

                // 计算中间索引（防止溢出的安全写法）
                index = lo + ((hi - lo) >> 1);

                if (CDA[index] == randomValue)
                {
                    return index;
                }
                if (CDA[index] < randomValue)
                {
                    // 向右查找
                    lo = index + 1;
                }
                else
                {
                    // 向左查找
                    hi = index - 1;
                }
            }

            index = lo;
            return index;
        }

        /// <summary>
        /// 线性查找法 —— 适合小型 List。
        /// 通过线性扫描找到随机值对应的区间索引。
        /// </summary>
        /// <param name="CDL">累积分布列表</param>
        /// <param name="randomValue">[0,1) 的随机值</param>
        /// <returns>返回随机命中的索引</returns>
        public static int SelectIndexLinearSearch(this List<float> CDL, float randomValue)
        {

            int i = 0;

            // 最后一个元素应为 1
            while (CDL[i] < randomValue)
                i++;

            return i;
        }

        /// <summary>
        /// 二分查找法 —— 适合大型 List。
        /// 修改自 C# 内部的 Array.BinarySearch()。
        /// </summary>
        /// <param name="CDL">累积分布列表</param>
        /// <param name="randomValue">[0,1) 的随机值</param>
        /// <returns>返回随机命中的索引</returns>
        public static int SelectIndexBinarySearch(this List<float> CDL, float randomValue)
        {

            int lo = 0;
            int hi = CDL.Count - 1;
            int index;

            while (lo <= hi)
            {

                // 计算中间索引
                index = lo + ((hi - lo) >> 1);

                if (CDL[index] == randomValue)
                {
                    return index;
                }
                if (CDL[index] < randomValue)
                {
                    lo = index + 1;
                }
                else
                {
                    hi = index - 1;
                }
            }

            index = lo;
            return index;
        }

        /// <summary>
        /// 创建一个“恒等数组”：array[i] = i
        /// </summary>
        /// <param name="length">数组长度</param>
        /// <returns>返回一个恒等数组</returns>
        public static float[] IdentityArray(int length)
        {

            float[] array = new float[length];

            for (int i = 0; i < array.Length; i++)
                array[i] = i;

            return array;
        }

        /// <summary>
        /// 使用随机数生成器，为数组中的每个元素生成 [0,1) 的随机值。
        /// </summary>
        /// <param name="array">目标数组</param>
        /// <param name="r">随机数生成器</param>
        public static void RandomWeightsArray(ref float[] array, System.Random r)
        {

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (float)r.NextDouble();

                // 如果生成的是 0，则重新生成（避免无效权重）
                if (array[i] == 0)
                    i--;
            }
        }

        /// <summary>
        /// 创建一个新的随机权重数组（每个值 ∈ (0,1)）。
        /// </summary>
        /// <param name="r">随机数生成器</param>
        /// <param name="length">数组长度</param>
        /// <returns>随机权重数组</returns>
        public static float[] RandomWeightsArray(System.Random r, int length)
        {

            float[] array = new float[length];

            for (int i = 0; i < length; i++)
            {
                array[i] = (float)r.NextDouble();

                if (array[i] == 0)
                    i--;
            }
            return array;
        }

        /// <summary>
        /// 创建一个“恒等列表”：list[i] = i
        /// </summary>
        /// <param name="length">列表长度</param>
        /// <returns>返回一个恒等列表</returns>
        public static List<float> IdentityList(int length)
        {

            List<float> list = new List<float>(length);

            for (int i = 0; i < length; i++)
                list.Add(i);

            return list;
        }

        /// <summary>
        /// 使用随机数生成器，为列表中的每个元素生成 [0,1) 的随机值。
        /// </summary>
        /// <param name="list">目标列表</param>
        /// <param name="r">随机数生成器</param>
        public static void RandomWeightsList(ref List<float> list, System.Random r)
        {

            for (int i = 0; i < list.Count; i++)
            {
                list[i] = (float)r.NextDouble();

                if (list[i] == 0)
                    i--;
            }
        }
    }
}
