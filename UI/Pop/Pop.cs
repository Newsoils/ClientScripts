using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace CLIP.Project_Mouse.UI
{
    public class Pop : MonoBehaviour
    {
        public Vector3 offset;
        public TMP_Text text;
        public CanvasGroup group;
        public GameObject parent;
        public void Init(string content, GameObject parent)
        {
            text.text = content;
            group.alpha = 0;
            this.parent = parent;
            UpdatePosition();
            Sequence sequence = DOTween.Sequence();
            sequence.Append(group.DOFade(1, 0.5f));
            sequence.Append(DOVirtual.DelayedCall(2f, null));
            sequence.Append(group.DOFade(0, 0.5f).OnComplete(() => Destroy(gameObject)));
        }
        public void UpdatePosition()
        {
            Vector3 position = Camera.main.WorldToScreenPoint(parent.transform.position + offset);
            transform. position = position;
        }
        private void Update()
        {
            UpdatePosition();
        }
    }
}
