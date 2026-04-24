using UnityEngine;

namespace CLIP.Project_Mouse.Scene_View_Control
{
    public class Hide_Wall : MonoBehaviour
    {
        public Vector3 _wall_front;
        public float _disappear_value = -0.8f;
        public bool _force_hide = false;
        [Header("Dot 淡出区间（dot 从 0.1 降到 0 时逐渐透明）")]
        public float _fade_start_dot_ = 0.12f;
        public float _fade_end_dot = 0f;

        //public Transform _camera_root;
        public Camera _camera;

        private Renderer _renderer;
        private MaterialPropertyBlock _mpb;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private float _baseAlpha = 1f;

        void Start()
        {
            _wall_front = _wall_front.normalized;

            _renderer = GetComponent<Renderer>();
            _mpb = new MaterialPropertyBlock();

            if (_renderer != null && _renderer.sharedMaterial != null)
            {
                if (_renderer.sharedMaterial.HasProperty(BaseColorId))
                {
                    _baseAlpha = _renderer.sharedMaterial.GetColor(BaseColorId).a;
                }
                else if (_renderer.sharedMaterial.HasProperty(ColorId))
                {
                    _baseAlpha = _renderer.sharedMaterial.GetColor(ColorId).a;
                }
            }

            if (_camera == null)
                _camera = Camera.main;

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
            if (dot < _disappear_value)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);
            UpdateAlpha(dot);
        }

        void SetVisible(bool visible)
        {
            if (_renderer != null && _renderer.enabled != visible)
                _renderer.enabled = visible;
        }

        private void UpdateAlpha(float dot)
        {
            if (_renderer == null)
                return;

            float startDot = Mathf.Max(_fade_start_dot_, _fade_end_dot);
            float endDot = Mathf.Min(_fade_start_dot_, _fade_end_dot);

            float alpha;
            if (dot >= startDot)
            {
                alpha = _baseAlpha;
            }
            else if (dot <= endDot)
            {
                alpha = 0f;
            }
            else
            {
                float t = Mathf.InverseLerp(endDot, startDot, dot);
                alpha = Mathf.Lerp(0f, _baseAlpha, t);
            }
            if (_mpb == null) return;
            _renderer?.GetPropertyBlock(_mpb);
            if (_renderer.sharedMaterial != null && _renderer.sharedMaterial.HasProperty(BaseColorId))
            {
                var c = _renderer.sharedMaterial.GetColor(BaseColorId);
                c.a = alpha;
                _mpb.SetColor(BaseColorId, c);
            }
            if (_renderer.sharedMaterial != null && _renderer.sharedMaterial.HasProperty(ColorId))
            {
                var c = _renderer.sharedMaterial.GetColor(ColorId);
                c.a = alpha;
                _mpb.SetColor(ColorId, c);
            }
            _renderer.SetPropertyBlock(_mpb);
        }
    }
}