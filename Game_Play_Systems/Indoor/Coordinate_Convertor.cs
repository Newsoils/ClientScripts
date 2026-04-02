using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Game_Play_System {
              namespace Indoor_System
            {
            public class Coordinate_Convertor
            {
                public static Vector3 game_to_ground_cell(Vector3 input)
                {
                    return new Vector3(input.y*-1f , input.x * -1f, input.z);

                }
                public static Vector3 game_to_left_cell(Vector3 input)
                {
                    return new Vector3(input.z, input.y, input.x);

                }
                public static Vector3 game_to_right_cell(Vector3 input)
                {
                    return new Vector3(input.x * -1f, input.z * -1f, input.y);

                }
            }

        }
    }
}
}