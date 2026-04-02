using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Kernel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{

    public class NpcDetailPanel : MonoBehaviour
    {
        public GameObject obj;

        [HideInInspector]
        public NPC_Info info;

        [Header("UI")]
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text personalityText;
        public TMP_Text areaText;
        public TMP_Text intimacyLevelText;
        public Slider slider;
        public TMP_Text intimacyValueText;
        public GameObject reward;

        public void OpenPanel(NPC_Info info)
        {
            obj.SetActive(true);
            InitNpcDescription(info);
        }


        public void ClosePanel()
        {
            obj.SetActive(false);
        }

        public void InitNpcDescription(NPC_Info info)
        {
            this.info = info;

            LoadImage(info._npc_Base.icon_resource_name);
            // 模型照片
            nameText.text = info._npc_Base.npc_name;
            personalityText.text = info._npc_Base.npc_personality;
            // npc所在地点
            intimacyLevelText.text = info._npc_RuntimeData.favor_level.ToString();
            intimacyValueText.text = $"{info._npc_RuntimeData.favor_Value}/{info.npc_Next_Favor_Level}";
            slider.value = (float)info._npc_RuntimeData.favor_Value / info.npc_Next_Favor_Level;
            reward.SetActive(false);
        }

        private async void LoadImage(string name)
        {
            Sprite s = await GameAssets.Instance.LoadAsycByKey<Sprite>(name);
            icon.sprite = s;
        }

        public void RewardForNextLevel()
        {
            if (reward.activeSelf)
            {
                reward.SetActive(false);
            }
            else
            {
                reward.SetActive(true);
                // 显示奖励内容
            }
        }
    }

}