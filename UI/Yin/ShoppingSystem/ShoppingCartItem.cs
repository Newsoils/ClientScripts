using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CLIP.Project_Mouse.NewFrame.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class ShoppingCartItem : MonoBehaviour
            {
                public Game_Item_Info shoppingCartItem;
                public int itemCount = 1;
                public bool isSelected = false;

                public Toggle buyItem;
                public TMP_Text itemName;
                public Button deleteItem;

                public void InitShoppingCartItem()
                {
                    buyItem.isOn = isSelected;
                    itemName.text = shoppingCartItem.name;
                    buyItem.onValueChanged.AddListener(OnToggleValueChange);
                }

                public void OnToggleValueChange(bool isOn)
                {
                    isSelected = isOn;
                    //ShoppingCanvasInteractManager.Instance.UpdateCartUI();
                    UIManager.Instance.GetPanel<ShoppingPanel>().UpdateCartUI();
                }

                public void DeleteItem()
                {
                    //ShoppingCanvasInteractManager.Instance.shoppingCartItems.Remove(gameObject.GetComponent<ShoppingCartItem>());
                    UIManager.Instance.GetPanel<ShoppingPanel>().shoppingCartItems.Remove(gameObject.GetComponent<ShoppingCartItem>());
                    Destroy(gameObject);
                    //ShoppingCanvasInteractManager.Instance.UpdateCartUI();
                    UIManager.Instance.GetPanel<ShoppingPanel>().UpdateCartUI();
                }
            }
        }
    }
}

