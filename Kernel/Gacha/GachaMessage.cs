using System.Collections.Generic;

namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// 客户端发起的抽卡请求消息 客户端->服务器
    /// </summary>
    public class GachaRequest
    {
        /// <summary>
        /// 池子ID
        /// </summary>
        public int PoolId { get; set; }

        /// <summary>
        /// 使用哪种货币（比如：金币、钻石、抽卡券）
        /// </summary>
        public int CurrencyId { get; set; }

        /// <summary>
        /// 客户端计算的预期消耗（后端会进行二次校验）
        /// </summary>
        public int CurrencyCount { get; set; }

        /// <summary>
        /// 抽几次（1次、5次、10次等）
        /// </summary>
        public int PullCount { get; set; }
    }

    /// <summary>
    /// 服务器返回消息 服务器->客户端
    /// </summary>
    public class GachaResponse
    {
        /// <summary>
        /// 抽卡是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 抽到的物品 ID 列表
        /// </summary>
        public List<int> ItemIds { get; set; } = new();

        /// <summary>
        /// 剩下的钱，客户端收到后更新 UI
        /// </summary>
        public long NewAmount { get; set; }

        /// <summary>
        /// 如果失败了，告诉客户端为什么（钱不够、池子过期等）
        /// </summary>
        public string Message { get; set; }
    }
}

