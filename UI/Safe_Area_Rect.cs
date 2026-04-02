using UnityEngine;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class Safe_Area_Rect : MonoBehaviour
            {

                public RectTransform _rt;
                void Start()
                {
                    var _sa = Screen.safeArea;

                    //_rt.position = _sa.center;
                    _rt.anchorMax = new Vector2(_sa.xMax / Screen.width, _sa.yMax / Screen.height);
                    _rt.anchorMin = new Vector2(_sa.xMin / Screen.width, _sa.yMin / Screen.height);
                }

            }
        }
    }
}
