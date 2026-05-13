using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class SmallIllustrateItem : MonoBehaviour
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

                public void ShowMediumItem()
                {
                    // 设置image
                    //IllustrateInteractManager.Instance.mediumItemName.text = item_name;
                    UIManager.Instance.GetPanel<IllustratePanel>().mediumItemName.text = item_name;
                    //IllustrateInteractManager.Instance.mediumItem.SetActive(true);
                    UIManager.Instance.GetPanel<IllustratePanel>().mediumItem.SetActive(true);
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

