using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class MediumIllustrateItem : MonoBehaviour
            {
                public int item_id;
                public string item_name;
                public Image icon;
                public TMP_Text itemNameText;
                public GameObject redPoint;

                public void InitItem()
                {
                    // 设置image
                    itemNameText.text = "?";
                    redPoint.SetActive(false);
                }

                public void ShowBigItem()
                {
                    //var list = IllustrateInteractManager.Instance.currentGameItemList;
                    var list = UIManager.Instance.GetPanel<IllustratePanel>().currentGameItemList;
                    int index = list.FindIndex(item => item.item_id == item_id);
                    //IllustrateInteractManager.Instance.illustrateSlider.OnStepChanged(1);
                    UIManager.Instance.GetPanel<IllustratePanel>().illustrateSlider.OnStepChanged(1);
                    //IllustrateInteractManager.Instance.illustrateSlider.slider.value = 1f;
                    UIManager.Instance.GetPanel<IllustratePanel>().illustrateSlider.slider.value = 1f;
                    //IllustrateInteractManager.Instance.bigIllustrateItem.ShowItem(index);
                    UIManager.Instance.GetPanel<IllustratePanel>().bigIllustrateItem.ShowItem(index);
                    if (redPoint.activeSelf)
                    {
                        redPoint.SetActive(false);
                        //IllustrateInteractManager.Instance.RemoveNewObtainedItem(item_name);
                        UIManager.Instance.GetPanel<IllustratePanel>().RemoveNewObtainedItem(item_name);
                    }
                }
            }
        }
    }
}
