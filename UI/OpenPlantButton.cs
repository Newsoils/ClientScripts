using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenPlantButton : MonoBehaviour
{
    public Button button;
    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }
    private void OnClick()
    {
        UIManager.Instance.OpenPanel<PlantPanel>();
    }
}
