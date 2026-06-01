using System;
using CLIP.Framework_Unity.Asset;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    public class CellView_Task : EnhancedScrollerCellView
    {
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Transform _rewardField;
        [SerializeField] private Image _mainSlider;
        [SerializeField] private Image _leftSlider;
        [SerializeField] private Image _rightSlider;
        public TMP_Text progressText;

        [SerializeField] private Button _confirmButton;
        [SerializeField] private Image _rewardIconPrefab;
        [SerializeField] private Sprite _greenSprite;
        [SerializeField] private Sprite _whiteSprite;
        [SerializeField] private TMP_Text _buttonText;
        [SerializeField] private Image _transparentMask;
        [SerializeField] private Image _buttonImage;

        public TMP_Text unlockText;

        private ScrollData_Task mData;

        public void OnEnable()
        {
            if (mData != null)
                RefreshTaskState(mData);
        }

        public void SetData(ScrollData_Task data, Action<ulong> claimCallback)
        {
            if (data == null)
                return;
            mData = data;
            if (data.isLocked)
            {
                progressText.text = "??/??";
            }
            else
            {
                progressText.text = data.runtime.current.ToString() + "/" + data.runtime.target.ToString();
            }
            _confirmButton.onClick.RemoveAllListeners();
            if (!data.isLocked)
                _confirmButton.onClick.AddListener(() => claimCallback?.Invoke(data.runtime.missionUId));

            SetRewardIcons();

            RefreshTaskState(data);
        }

        private void RefreshTaskState(ScrollData_Task data)
        {
            if (data.isLocked)
            {
                _transparentMask.gameObject.SetActive(true);
                _confirmButton.interactable = false;
                _descriptionText.text = data.model.desc;
                unlockText.text = data.model.unlockExp.ToString();
                _buttonText.text = "未解锁";
                UpdateProgress(0f);
                return;
            }

            _transparentMask.gameObject.SetActive(false);
            _confirmButton.interactable = true;
            _descriptionText.text = data.model.desc;
            unlockText.text = data.model.unlockExp.ToString();
            //任务状态 0：进行中 1：已完成 2：已领取奖励
            switch (data.runtime.status)
            {
                case 0:
                    _buttonText.text = "未完成";
                    break;
                case 1:
                    _buttonText.text = "可领取";
                    break;
                case 2:
                    _buttonText.text = "已领取";
                    break;
            }
            UpdateProgress(data.runtime.Progress);
        }


        public void UpdateProgress(float progress)
        {
            _mainSlider.fillAmount = progress;

            bool hasProgress = progress > 0;
            _leftSlider.gameObject.SetActive(hasProgress);
            _rightSlider.gameObject.SetActive(hasProgress);

            if (progress >= 1f)
            {
                _buttonImage.sprite = _greenSprite;
            }
            else
            {
                _buttonImage.sprite = _whiteSprite;
            }
        }

        private void OnDisable()
        {
            _confirmButton.onClick.RemoveAllListeners();
        }

        public void SetRewardIcons()
        {
            Unity_Tools.ClearAllChildren(_rewardField);

            for (int i = 0; i < mData.model.Rewards.Count; i++)
            {
                var reward = mData.model.Rewards[i];
                var iconName = mData.model.RewardNames[i];

                var rewardCell = Instantiate(_rewardIconPrefab, _rewardField).GetComponent<TaskRewardCell>();
                GameAssets.Instance.LoadAndSetByKey<Sprite>(iconName, sp => rewardCell.itemImage.sprite = sp);
                rewardCell.itemCount.text = reward.amount.ToString();
                Debug.Log($"Reward: {reward}");
            }

        }
    }
}
