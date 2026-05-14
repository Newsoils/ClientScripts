using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Kernel;
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
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Image _rewardIconPrefab;
        [SerializeField] private Sprite _greenSprite;
        [SerializeField] private Sprite _whiteSprite;
        [SerializeField] private TMP_Text _buttonText;
        [SerializeField] private Image _transparentMask;
        [SerializeField] private Image _buttonImage;
        public GameObject unlockLevelObj;
        public TMP_Text unlockLevelText;

        private int _currentTaskId = -1;

        private ScrollData_Task mData;

        public void OnEnable()
        {
            if(mData!=null)
            RefreshTaskState(mData);
        }

        public void SetData(ScrollData_Task data, Action<int> claimCallback)
        {
            if (data == null)
                return;
            mData = data;
            _currentTaskId = data.model.taskId;
            _descriptionText.text = data.model.desc;

            _confirmButton.onClick.RemoveAllListeners();
            _confirmButton.onClick.AddListener(() => claimCallback?.Invoke(data.model.taskId));

            _transparentMask.gameObject.SetActive(false);

            RefreshTaskState(data);

            data.runtime.OnProgressChange -= OnProgressChanged;
            data.runtime.OnProgressChange += OnProgressChanged;
            data.runtime.OnTaskFinished -= OnTaskFinished;
            data.runtime.OnTaskFinished += OnTaskFinished;
        }

        private void RefreshTaskState(ScrollData_Task data)
        {
            //if (data.runtime.IsFinish)
            //{
            //    SetAsFinished();
            //    return;
            //}

            if (!data.runtime.IsAccept)
            {
                SetAsLocked(data.model.unlockExp);
                return;
            }

            _confirmButton.interactable = true;
            _transparentMask.gameObject.SetActive(false);
            unlockLevelObj?.SetActive(false);
            UpdateProgress(data.runtime.Progress);
        }

        private void OnProgressChanged(float progress)
        {
            UpdateProgress(progress);
        }

        private void OnTaskFinished()
        {
            _confirmButton.interactable = true;
            _transparentMask.gameObject.SetActive(false);
            _buttonImage.sprite = _greenSprite;
            _buttonText.text = "已领取";
            _mainSlider.fillAmount = 1f;
            _leftSlider.gameObject.SetActive(false);
            _rightSlider.gameObject.SetActive(false);
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
                _buttonText.text = "可领取";
            }
            else
            {
                _buttonText.text = "未完成";
                _buttonImage.sprite = _whiteSprite;
            }
        }

        //public void SetAsFinished()
        //{
        //    gameObject.SetActive(false);
        //}

        private void SetAsLocked(int unlockExp)
        {
            _confirmButton.interactable = false;
            _transparentMask.gameObject.SetActive(true);
            _buttonImage.sprite = _whiteSprite;
            _buttonText.text = "未解锁";
            _mainSlider.fillAmount = 0f;
            _leftSlider.gameObject.SetActive(false);
            _rightSlider.gameObject.SetActive(false);
            _descriptionText.color = Color.gray;
            unlockLevelObj?.SetActive(true);
            unlockLevelText.text = unlockExp.ToString();
        }

        private void OnDisable()
        {
            _confirmButton.onClick.RemoveAllListeners();
            if (mData != null)
            {
                mData.runtime.OnProgressChange -= OnProgressChanged;
                mData.runtime.OnTaskFinished -= OnTaskFinished;
            }
        }

        public async Task SetRewardIcons(List<string> iconNames)
        {
            foreach (Transform child in _rewardField)
                Destroy(child.gameObject);

            foreach (var iconName in iconNames)
            {
                var icon = Instantiate(_rewardIconPrefab, _rewardField);
                var sprite = await GameAssets.Instance.LoadAsycByKey<Sprite>(iconName);
                icon.transform.Find("ItemImage").GetComponent<Image>().sprite = sprite;
            }
        }
    }
}
