using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class RotationController : MonoBehaviour
    {
        [SerializeField] private Vector3 rotationAxis = new Vector3(0, 0, 1);
        [SerializeField] private float speed = 0.1f;

        private void Update()
        {
            transform.Rotate(rotationAxis, speed, Space.Self);
        }
    }
}
