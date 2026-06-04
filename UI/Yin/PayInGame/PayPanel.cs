using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{

    public class PayPanel : UIPanelBase
    {
        public GameObject  panelObj;
        public Button btnClose;
        public Transform diamondFrame;
        public Transform coinFrame;
        public GameObject payButtonObj;
        public GameObject payDiamondButtonObj;
        public Sprite diamondIcon;
        public Sprite coinIcon;

        public List<MoneyPrice> diamondList;
        public List<MoneyPrice> coinList;
        private void Start()
        {
            EvtDsp.AddEvt(EvtNames.OpenPayPanel, OpenPanel);
            btnClose.onClick.AddListener(ClosePanel);
            foreach (Transform child in diamondFrame)
            {
                Destroy(child.gameObject);
            }
            foreach (var price in diamondList)
            {
                GameObject obj = Instantiate(payButtonObj, diamondFrame);
                PayButton button = obj.GetComponent<PayButton>();
                button.Init(price.num, price.reward, price.price, price.icon, true);
            }
            foreach (Transform child in coinFrame)
            {
                Destroy(child.gameObject);
            }
            foreach (var price in coinList)
            {
                GameObject obj = Instantiate(payDiamondButtonObj, coinFrame);
                PayButton button = obj.GetComponent<PayButton>();
                button.Init(price.num, price.reward, price.price, price.icon, false);
            }
        }
        public override void OnDestroy()
        {
            EvtDsp.RemoveEvt(EvtNames.OpenPayPanel, OpenPanel);
        }

        private void OpenPanel()
        {
            OpenPanel();
        }

        public override void OpenPanel(params object[] data)
        {
            panelObj.SetActive(true);
        }
        public override void ClosePanel()
        {
            panelObj.SetActive(false);
        }
       
    }
    [Serializable]
    public class MoneyPrice
    {
        public int num;
        public int reward;
        public float price;
        public Sprite icon;
    }

}

