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
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Transform _rewardField;
        [SerializeField] private Image _mainSlider;
        [SerializeField] private Image _leftSlider;
        [SerializeField] private Image _rightSlider;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Image _buttonImage;
        [SerializeField] private Image _rewardIconPrefab;
        [SerializeField] private Sprite _greenSprite;
        [SerializeField] private Sprite _whiteSprite;
        [SerializeField] private TMP_Text _notFinishText;
        [SerializeField] private TMP_Text _finishedText;
        [SerializeField] private Image _transparentMask;

        public Button ConfirmButton => _confirmButton;
        public TMP_Text descriptionText => _descriptionText;
        public TMP_Text finishedText => _finishedText;

        private void Start()
        {
            _transparentMask.gameObject.SetActive(false);
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

        public void UpdateProgress(float progress)
        {
            _mainSlider.fillAmount = progress;

            bool hasProgress = progress > 0;
            _leftSlider.gameObject.SetActive(hasProgress);
            _rightSlider.gameObject.SetActive(hasProgress);

            if (progress >= 1f)
            {
                _buttonImage.sprite = _greenSprite;
                _notFinishText.gameObject.SetActive(false);
                _finishedText.gameObject.SetActive(true);
            }
            else
            {
                _buttonImage.sprite = _whiteSprite;
            }
        }

        public void SetAsFinished()
        {
            _confirmButton.interactable = false;
            _buttonImage.sprite = _whiteSprite;
            _transparentMask.gameObject.SetActive(true);
            _finishedText.text = "已领取";
            _descriptionText.color = Color.gray;
        }
    }
}
