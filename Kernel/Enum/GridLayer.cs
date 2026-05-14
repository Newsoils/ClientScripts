using System;
using System.Collections.Generic;
using System.Numerics;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;


namespace CLIP.Project_Mouse.Kernel
{
    [System.Serializable]
    public class GridLayer
    {
        public int id;
        public string name;
        public string uid;

        public GridLayerType LayerType;
        public int Width { get; private set; }
        public int Height { get; private set; }

        // 存储格子的数据
        private Dictionary<Int2, MGrid> mGrids = new();

        public IEnumerable<MGrid> Grids => mGrids.Values;

        // 使用静态实例，避免频繁创建导致的随机数重复问题
        private static readonly Random _random = new Random();

        public string GetUId(Int2 position)
        {
            if (mGrids.TryGetValue(position, out var mGrid))
            {
                return mGrid.UId;
            }
            return null;
        }

        public List<string> GetUIds(IEnumerable<Int2> positions)
        {
            var list = new List<string>();
            foreach (var position in positions)
            {
                if (mGrids.TryGetValue(position, out var mGrid))
                {
                    list.Add(mGrid.UId);
                }
            }
            return list;
        }
        public bool TryGetUId(Int2 position, out string uid)
        {
            uid = null;

            if (mGrids.TryGetValue(position, out var grid))
            {
                uid = grid.UId;
                return true;
            }

            return false;
        }

        public bool TryGetUIds(IEnumerable<Int2> positions, out List<string> uids)
        {
            uids = new List<string>();
            bool foundAny = false;

            foreach (var position in positions)
            {
                if (mGrids.TryGetValue(position, out var grid))
                {
                    uids.Add(grid.UId);
                    foundAny = true;
                }
            }

            return foundAny;
        }
        public string SetOccupied(Int2 position, bool isOccupied)
        {
            if (mGrids.TryGetValue(position, out var grid))
            {
                grid.SetOccupied(isOccupied);
                return grid.UId;
            }
            return null;
        }

        public bool CheckVaild(Int2 pos)
        {
            if (mGrids.TryGetValue(pos, out var grid))
            {
                if (grid.isOccpuied) return false;
                else return true;
            }
            return false;
        }

        public bool CheckVaild(IEnumerable<Int2> positions)
        {
            foreach (var position in positions)
            {
                if (mGrids.TryGetValue(position, out var grid))
                {
                    if (grid.isOccpuied) return false;
                }
                else return false;
            }
            return true;
        }

        /// <summary>
        /// 初始化生成grid信息
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="type"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public GridLayer(int ID, string Name, string UId, GridLayerType type, int w, int h)
        {
            this.id = ID;
            this.name = Name;
            this.LayerType = type;
            this.Width = w;
            this.Height = h;
            this.uid = UId;
            // 生成逻辑
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Int2 pos = new Int2(x, y);
                    mGrids[pos] = new MGrid(type, pos);
                }
            }
        }



        public Int2 GetRandomPosition()
        {
            // System.Random.Next(min, max) 包含下限，不包含上限
            // 正好符合数组/网格索引的需求
            int x = _random.Next(0, Width);
            int y = _random.Next(0, Height);

            return new Int2(x, y);
        }

        public bool IsPoiontIn(Vector2 position)
        {
            if (0 < position.X && position.X < Width && position.Y < Height && position.Y > 0) return true;

            else return false;

        }

        /// <summary>
        /// 清空所有格子的占据状态
        /// </summary>
        public void ClearAllOccupied()
        {
            foreach (var grid in mGrids.Values)
            {
                grid.SetOccupied(false);
            }
        }

        /// <summary>
        /// 获取所有格子的UID列表
        /// </summary>
        public List<string> GetAllGridUids()
        {
            var list = new List<string>();
            foreach (var grid in mGrids.Values)
            {
                list.Add(grid.UId);
            }
            return list;
        }

    }
}