using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class RotationController : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            transform.Rotate(new Vector3(0, 0, 1), 0.1f, Space.Self);
        }
    }
}
