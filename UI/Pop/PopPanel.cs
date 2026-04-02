using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using UnityEngine;

namespace CLIP.Project_Mouse.UI
{
    public class PopPanel : UIPanelBase
    {
        public GameObject popPrefab;

        private void Start()
        {
            EvtDsp.AddEvt<string, GameObject>(EvtNames.ShowPop, CreatePop);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            EvtDsp.RemoveEvt<string, GameObject>(EvtNames.ShowPop, CreatePop);
        }
        public void CreatePop(string content, GameObject parent)
        {
            GameObject obj = Instantiate(popPrefab, transform);
            Pop pop = obj.GetComponent<Pop>();
            pop.Init(content, parent);
        }

        public override void OpenPanel(params object[] data)
        {
        }

        public override void ClosePanel()
        {
        }
    }
}

