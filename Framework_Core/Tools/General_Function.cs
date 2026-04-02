using System.Collections.Generic;

namespace CLIP.Framework_Core.Tools
{
    /// <summary>
    /// 通用工具函数类，提供一些常用的基础功能方法
    /// </summary>
    public class General_Function
    {
        /// <summary>
        /// 将分钟数转换为"X h Y min"格式的字符串
        /// </summary>
        /// <param name="minute_number">需要转换的分钟数</param>
        /// <returns>转换后的字符串，例如：130分钟转换为"2 h 10 min"</returns>
        public static string parse_minutes_int_to_str(int minute_number)
        {
            // 计算小时数（整数除法）和剩余分钟数（取模运算）
            return minute_number / 60 + " h " + minute_number % 60 + " min";
        }

        /// <summary>
        /// 按权重进行多次抽样，获取指定数量的抽样结果
        /// </summary>
        /// <param name="weight">权重列表，正数表示有抽样概率，非正数表示无抽样概率</param>
        /// <param name="count">需要抽样的次数</param>
        /// <returns>包含count个抽样索引的列表</returns>
        public static List<int> weight_sample(in List<float> weight, int count)
        {
            var ans = new List<int>();
            for (int i = 0; i < count; i++)
            {
                // 每次抽样调用单次抽样方法
                ans.Add(weight_sample(weight));
            }
            return ans;
        }

        /// <summary>
        /// 按权重进行单次抽样，返回被选中元素的索引
        /// </summary>
        /// <param name="weight">权重列表，正数表示有抽样概率，非正数表示无抽样概率</param>
        /// <returns>被选中元素的索引，默认返回0（当所有权重均无效时）</returns>
        public static int weight_sample(in List<float> weight)
        {
            // 使用当前时间的毫秒数作为随机数种子
            var rd = new System.Random((int)System.DateTime.Now.Millisecond);
            double total = 0;

            // 计算有效权重总和（仅累加正数权重）
            foreach (var x in weight)
            {
                if (x <= 0) continue;
                total += (double)x;
            }

            // 生成0到总权重之间的随机数
            double random_val = rd.NextDouble() * total;
            int ans = 0;
            double up = 0;

            // 遍历权重列表，累计权重直到超过随机值，返回对应索引
            for (int i = 0; i < weight.Count; i++)
            {
                if (weight[i] <= 0) continue; // 跳过无效权重
                up += weight[i];
                if (up > random_val)
                {
                    return i;
                }
            }

            // 当所有权重均为非正数时，默认返回0
            return ans;
        }
    }
}