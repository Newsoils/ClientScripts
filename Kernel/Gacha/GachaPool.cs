using System.Collections.Generic;

namespace CLIP.Project_Mouse.Kernel
{
	public class GachaPool
	{
		/// <summary>
		/// 奖池序号
		/// </summary>
		public int id;
		/// <summary>
		/// 需要加入奖池的物品id(对应gameItem）
		/// </summary>
		public List<int> items;
		/// <summary>
		/// 每种稀有度对应的概率
		/// </summary>
		public List<(int, int)> probs;
	}
}