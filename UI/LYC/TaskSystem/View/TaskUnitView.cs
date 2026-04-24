using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CLIP.Framework_Unity.Asset;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    public class TaskUnitView : MonoBehaviour
    {
        [SerializeField] public TMP_Text descriptionText;

        [SerializeField] public Transform rewardField;

        [SerializeField] public List<Image> RewardsIcons;

        //[SerializeField] public Slider taskProgressSlider;
        [SerializeField] public Image mainSlider;

        [SerializeField] public Image leftSlider;

        [SerializeField] public Image rightSlider;

        [SerializeField] public Button confirmButton;

        [SerializeField] public Image buttonImage;

        [SerializeField] public Image rewardIconPrefab;

        [SerializeField] public Sprite greenSprite;

        [SerializeField] public Sprite whiteSprite;

        [SerializeField] public TMP_Text notFinishText;

        [SerializeField] public TMP_Text finishedText;

        [SerializeField] public Image transparentMask;


        // 添加单个奖励图标
        public async Task AddRewardIcon(string iconName)
        {
            if (RewardsIcons == null)
            {
                RewardsIcons = new List<Image>();
            }
            Image newIcon = Instantiate(rewardIconPrefab, rewardField);
            var iconSprite = await GameAssets.Instance.LoadAsycByKey<Sprite>(iconName);
            newIcon.transform.Find("ItemImage").GetComponent<Image>().sprite = iconSprite;

            RewardsIcons.Add(newIcon);
        }

        // 添加多个奖励图标
        public async Task AddRewardIcons(List<string> IconNames)
        {
            if (RewardsIcons == null)
            {
                RewardsIcons = new List<Image>();
            }
            foreach (string iconName in IconNames)
            {
                Image newIcon = Instantiate(rewardIconPrefab, rewardField);
                var iconSprite = await GameAssets.Instance.LoadAsycByKey<Sprite>(iconName);
                newIcon.transform.Find("ItemImage").GetComponent<Image>().sprite = iconSprite;

                RewardsIcons.Add(newIcon);
            }
        }

        // 添加多个奖励图标
        public async Task SetRewardIcons(List<string> IconNames)
        {
            ClearRewardIcons();
            foreach (string iconName in IconNames)
            {
                Image newIcon = Instantiate(rewardIconPrefab, rewardField);
                var iconSprite = await GameAssets.Instance.LoadAsycByKey<Sprite>(iconName);
                if(iconSprite == null)
                {
                    Debug.LogWarning("图标资源加载失败");
                }
                newIcon.transform.Find("ItemImage").GetComponent<Image>().sprite = iconSprite;

                RewardsIcons.Add(newIcon);
            }
        }


        // 更新任务完成进度的显示
        public void UpdateView(float percentageOfProgress)
        {
            this.mainSlider.fillAmount = percentageOfProgress;

            // 进度条是否显示为 零进度（不显示任何进度条状态）
            if (percentageOfProgress == 0)
            {
                leftSlider.gameObject.SetActive(false);
                rightSlider.gameObject.SetActive(false);
            }
            else
            {
                leftSlider.gameObject.SetActive(true);
                rightSlider.gameObject.SetActive(true);
            }

            // 按键是否表示为绿色可交互状态
            if (percentageOfProgress == 1)
            {
                buttonImage.sprite = greenSprite;
                notFinishText.gameObject.SetActive(false);
                finishedText.gameObject.SetActive(true);
            }
            else
            {
                buttonImage.sprite = whiteSprite;
            }
        }

        // 事件 handler：设置为已完成状态
        public void SetAsFinished()
        {
            confirmButton.interactable = false;
            buttonImage.sprite = whiteSprite;
            
            transparentMask.gameObject.SetActive(true);
            finishedText.text = "已领取";

            descriptionText.color = Color.gray;
        }

        // 事件 handler：自我销毁
        public void SelfDestroy()
        {
            Destroy(gameObject);
        }


        #region 内部方法

        private void ClearRewardIcons()
        {
            if (RewardsIcons != null)
            {
                foreach (Image image in RewardsIcons)
                {
                    Destroy(image);
                }
            }
            RewardsIcons = new List<Image>();
        }

        #endregion
    }
}
