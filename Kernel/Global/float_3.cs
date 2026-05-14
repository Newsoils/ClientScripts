namespace CLIP.Project_Mouse
{

    [System.Serializable]
    public struct float_3
    {
        public float _x;
        public float _y;
        public float _z;
        public float_3(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        // 向量加法
        public static float_3 operator +(float_3 a, float_3 b)
        {
            return new float_3(a._x + b._x, a._y + b._y, a._z + b._z);
        }

        // 向量减法
        public static float_3 operator -(float_3 a, float_3 b)
        {
            return new float_3(a._x - b._x, a._y - b._y, a._z - b._z);
        }

        // 向量乘法（分量乘法）
        public static float_3 operator *(float_3 a, float_3 b)
        {
            return new float_3(a._x * b._x, a._y * b._y, a._z * b._z);
        }

        // 向量除法（分量除法）
        public static float_3 operator /(float_3 a, float_3 b)
        {
            return new float_3(
                b._x != 0 ? a._x / b._x : 0,
                b._y != 0 ? a._y / b._y : 0,
                b._z != 0 ? a._z / b._z : 0
            );
        }

        // 标量乘法
        public static float_3 operator *(float_3 a, float d)
        {
            return new float_3(a._x * d, a._y * d, a._z * d);
        }

        public static float_3 operator *(float d, float_3 a)
        {
            return new float_3(a._x * d, a._y * d, a._z * d);
        }

        // 标量除法
        public static float_3 operator /(float_3 a, float d)
        {
            return new float_3(
                d != 0 ? a._x / d : 0,
                d != 0 ? a._y / d : 0,
                d != 0 ? a._z / d : 0
            );
        }
    }
}