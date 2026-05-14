using System;

namespace CLIP.Project_Mouse.Kernel
{
    [System.Serializable]
    public readonly struct Int2 : IEquatable<Int2>
    {
        public readonly int x;
        public readonly int y;

        public static readonly Int2 zero = new Int2(0, 0);
        public static readonly Int2 one = new Int2(1, 1);
        public static readonly Int2 up = new Int2(0, 1);
        public static readonly Int2 right = new Int2(1, 0);

        public Int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Int2 operator +(Int2 a, Int2 b)
            => new(a.x + b.x, a.y + b.y);

        public static Int2 operator -(Int2 a, Int2 b)
            => new(a.x - b.x, a.y - b.y);

        public static bool operator ==(Int2 a, Int2 b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(Int2 a, Int2 b) => !(a == b);

        public bool Equals(Int2 other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is Int2 other && Equals(other);
        }

        public override string ToString()
            => $"({x},{y})";

        public override int GetHashCode()
        {
            // 这是一个经典的哈希算法，性能好且冲突少
            unchecked
            {
                return (x * 397) ^ y;
            }
        }
    }
}