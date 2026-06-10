using UnityEngine;
using UnityEngine.EventSystems;

public class DispatchDeskPageButton : MonoBehaviour, IPointerClickHandler
{
    public DispatchDeskPageController pageController;
    public int targetPage;

    public void OnPointerClick(PointerEventData eventData)
    {
        pageController.SwitchToPage(targetPage);
    }
}
