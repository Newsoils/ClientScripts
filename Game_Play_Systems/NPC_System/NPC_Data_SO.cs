using System.Collections.Generic;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    [CreateAssetMenu(fileName = "NPC_Data_SO ", menuName = "Project_Mouse/NPC_Data_SO")]
    public class NPC_Data_SO : ScriptableObject
    {
        [SerializeField]
        public List<NPC_Info> npc_info_list;


        public void OnEnable()
        {
            RefreshData();
        }

        private void OnValidate()
        {
            RefreshData();
        }

        [ContextMenu("加载数据")]
        public void RefreshData()
        {
            npc_info_list.Clear();
            TextAsset json_file = Resources.Load<TextAsset>("Json/project_mouse_tb_npc_info");
            if (json_file != null)
            {
                var data = JsonConvert.DeserializeObject<List<NPC_Base>>(json_file.text);
                foreach (var npc in data)
                {
                    npc_info_list.Add(new NPC_Info() { _npc_Base = npc });
                }
                //Debug.Log("NPC Data Updated From JSON");
            }
            else
            {
                Log.Error("NPC Data JSON file not found in Resources/NPC/npc_info_db");
            }
        }
    }

}

     



