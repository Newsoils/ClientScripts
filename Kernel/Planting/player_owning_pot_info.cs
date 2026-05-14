//using System;
//using System.Collections;
//using System.Collections.Generic;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        /// <summary>
//        /// 表示玩家拥有的一种花盆的信息。
//        /// 每个花盆在使用多次后会“损耗”，因此需要记录不同剩余可使用次数的花盆数量。
//        /// </summary>
//        [System.Serializable]
//        public class player_owning_pot_info
//        {
//            /// <summary>
//            /// 花盆最多可以使用的次数（例如一个花盆最多可使用7次）
//            /// </summary>
//            public static int _MAX_POTS_USE_COUNT = 7;

//            /// <summary>
//            /// 花盆名称，例如 “陶瓷花盆”、“塑料花瓶”
//            /// </summary>
//            public string _pot_name;

//            /// <summary>
//            /// 记录当前不同“剩余可使用次数”的花盆数量
//            /// 数组下标 +1 = 花盆剩余可使用次数  
//            /// 例如：
//            /// _pot_count_of_remained_use_count[0] 表示“只剩1次可用”的花盆数量  
//            /// _pot_count_of_remained_use_count[6] 表示“可用7次的全新花盆数量”
//            /// </summary>
//            public int[] _pot_count_of_remained_use_count = new int[_MAX_POTS_USE_COUNT];

//            /// <summary>
//            /// 使用一个花盆，使其剩余可使用次数 -1。
//            /// </summary>
//            /// <param name="using_pot_have_used_count">当前正在使用的花盆的“剩余使用次数”</param>
//            /// <returns>如果成功使用则返回 true，否则 false（例如没有该类花盆或已损坏）</returns>
//            public bool using_pot(int using_pot_have_used_count)
//            {
//                // 防御性检查：超出最大使用次数
//                if (using_pot_have_used_count >= _MAX_POTS_USE_COUNT)
//                    return false;

//                // 检查该类型花盆是否有库存
//                if (_pot_count_of_remained_use_count[using_pot_have_used_count - 1] <= 0)
//                    return false;

//                // 使用花盆后，剩余次数 -1
//                if (using_pot_have_used_count >= 2)
//                {
//                    // 例如：原来是可使用3次的花盆 -> 使用后变成可使用2次的花盆
//                    _pot_count_of_remained_use_count[using_pot_have_used_count - 1]--;
//                    _pot_count_of_remained_use_count[using_pot_have_used_count - 2]++;
//                }
//                else
//                {
//                    // 只剩1次使用机会的花盆，用完后就消失
//                    _pot_count_of_remained_use_count[using_pot_have_used_count - 1]--;
//                }

//                return true;
//            }

//            /// <summary>
//            /// 静态方法：批量使用指定数量的花盆（用于多次种植）
//            /// </summary>
//            /// <param name="_list">所有花盆信息列表</param>
//            /// <param name="pot_name">目标花盆名称</param>
//            /// <param name="use_count">要使用的花盆总数量</param>
//            public static void _using_pot(List<player_owning_pot_info> _list, string pot_name, int use_count)
//            {
//                var _info = _list.Find(_info => _info._pot_name == pot_name);
//                if (_info == null) return;

//                int remain_use_count = use_count;

//                // 从可使用次数少的花盆开始使用
//                for (int i = 0; i < _MAX_POTS_USE_COUNT; i++)
//                {
//                    int _current_pot_count = _info._pot_count_of_remained_use_count[i];
//                    if (_current_pot_count == 0) continue;

//                    // 本轮可以用的数量
//                    int _actual_use_pot_count = Math.Min(_current_pot_count, remain_use_count);

//                    // 对每一个使用中的花盆调用 using_pot() 来减少寿命
//                    for (int pot_count = 1; pot_count <= _actual_use_pot_count; pot_count++)
//                        _info.using_pot(pot_count);

//                    if (_current_pot_count >= remain_use_count)
//                        break; // 用够了就退出

//                    remain_use_count -= _actual_use_pot_count;
//                }

//                // 调试查看状态
//                int[] _data_after = _info._pot_count_of_remained_use_count;
//            }

//            /// <summary>
//            /// 获取指定花盆中“剩余可使用次数最少”的花盆的剩余次数。
//            /// 例如返回1表示还有一个“快坏掉”的花盆。
//            /// </summary>
//            public static int get_pot_least_use_count(in List<player_owning_pot_info> _list, string pot_name)
//            {
//                var _info = _list.Find(_info => _info._pot_name == pot_name);
//                int ans = -1;
//                if (_info == null) return ans;

//                // 找到第一个还有数量的剩余次数档位
//                for (int i = 0; i < _MAX_POTS_USE_COUNT; i++)
//                {
//                    if (_info._pot_count_of_remained_use_count[i] != 0)
//                    {
//                        ans = i + 1;
//                        break;
//                    }
//                }
//                return ans;
//            }

//            /// <summary>
//            /// 归还花盆（例如完成种植任务后退回空花盆）
//            /// </summary>
//            /// <param name="_list">玩家的所有花盆列表</param>
//            /// <param name="pot_name">花盆名称</param>
//            /// <param name="return_count">归还数量</param>
//            /// <param name="use_count">归还的花盆的“剩余使用次数”</param>
//            public static void return_pot(
//                List<player_owning_pot_info> _list,
//                string pot_name,
//                int return_count,
//                int use_count)
//            {
//                var _info = _list.Find(_info => _info._pot_name == pot_name);
//                if (_info == null) return;
//                if (use_count == 0) return;

//                // 归还一定数量的“可使用 use_count 次”的花盆
//                _info._pot_count_of_remained_use_count[use_count - 1] += return_count;
//            }
//        }
//    }
//}
