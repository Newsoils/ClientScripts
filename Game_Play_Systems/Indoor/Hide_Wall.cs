using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
using UnityEngine;

namespace CLIP.Project_Mouse.Scene_View_Control
{
    public class Hide_Wall : MonoBehaviour
    {
        public Vector3 _wall_front;
        public float _disappear_value = -0.8f;
        public bool _force_hide = false;

        //public Transform _camera_root;
        public Camera _camera;

        private Renderer _renderer;

        void Start()
        {
            _wall_front = _wall_front.normalized;

            _renderer = GetComponent<Renderer>();

            //var gm = GetComponentInParent<Indoor_Room_Game_Manager>();
            //if (gm != null)
            //    _camera_root = gm._camera_root;
            //else
            //    _camera_root = Camera.main?.transform.parent;

            //if (_camera_root != null)
            //    _camera = _camera_root.GetComponentInChildren<Camera>();

            if (_camera == null)
                _camera = Camera.main;

            //EvtDsp.AddEvt(EvtNames.Updata)
        }

        private void LateUpdate()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                return;
            }

            if (_force_hide)
            {
                SetVisible(false);
                return;
            }

            UpdateVisible();
        }

        private void UpdateVisible()
        {
            Vector3 camDir = _camera.transform.forward;
            camDir.y = 0;
            camDir.Normalize();

            float dot = Vector3.Dot(_wall_front, camDir);

            SetVisible(dot >= _disappear_value);
        }

        void SetVisible(bool visible)
        {
            if (_renderer != null && _renderer.enabled != visible)
                _renderer.enabled = visible;
        }
    }
}