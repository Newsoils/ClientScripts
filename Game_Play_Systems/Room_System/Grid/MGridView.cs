using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public enum MGridState
    {
        Normal,
        Highlight,
        Occupied
    }


    public class MGridView : MonoBehaviour
    {
        public string UId;
        public string LayerUid;

        public Material _normal_mat;
        public Material _highlight_mat;
        public Material _occupied_mat;

        private MeshRenderer _meshRenderer;

        private MGridState _state = MGridState.Normal;
        private MGridState? _previewState = null;

        private void Start()
        {
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        public void SetActive(bool active)
        {
            if (_meshRenderer == null) _meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (_meshRenderer == null) return;
            _meshRenderer.enabled = active;
        }

      
        public void SetRealState(MGridState state)
        {
            _state = state;
            Refresh();
        }

        public void SetPreviewState(MGridState? state)
        {
            _previewState = state;
            Refresh();
        }

        private void Refresh()
        {
            var state = _previewState ?? _state;
            SetRenderer(state);
        }


        private void SetRenderer(MGridState state = MGridState.Normal)
        {
            if(_meshRenderer==null) _meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (_meshRenderer == null) return;
            switch (state)
            {
                case MGridState.Normal:
                    _meshRenderer.material = _normal_mat;
                    break;
                case MGridState.Highlight:
                    _meshRenderer.material = _highlight_mat;
                    break;
                case MGridState.Occupied:
                    _meshRenderer.material = _occupied_mat;
                    break;
            }
        }
    }

}
