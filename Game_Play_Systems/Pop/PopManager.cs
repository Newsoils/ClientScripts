using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using Newtonsoft.Json;
using UnityEngine;


namespace CLIP.Project_Mouse.Game_Play_System
{
    public class PopManager : SingletonMono<PopManager>
    {
        private List<ClickInteractData> datas;
        private float clickTimer;
        private void Start()
        {
            string data = JsonData_Manager.Load_Single_JsonData("project_mouse_tb_click_info");
            datas = JsonConvert.DeserializeObject<List<ClickInteractData>>(data);
        }
        private void Update()
        {
            clickTimer -= Time.deltaTime;
        }
        public void Pop()
        {
            if (clickTimer > 0) return;
            clickTimer = 5;
            var data = SelectData();
            var player = IndoorMainCharacter._instance;
            EvtDsp.TriggerEvt<string, GameObject>(EvtNames.ShowPop, data.popContent, player.gameObject);
            player.ClickInteract();
        }
        private ClickInteractData SelectData()
        {
            List<ClickInteractData> canSlect = datas.Where(x => x.favorLevel <= ExpManager.instance.curLevel).ToList();
            return canSlect[Random.Range(0, canSlect.Count)];
        }
    }
    public class ClickInteractData
    {
        public string interactName;
        public int favorLevel;
        public string animationName;
        public string popContent;
    }
}

