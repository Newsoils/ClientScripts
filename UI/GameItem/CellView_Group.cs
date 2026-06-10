using System;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellView_Group : UIBase
{
    public GameObject obj;
    public TMP_Text groupName;
    public TMP_Text progressText;
    public ZButton button;
    public Image Icon;

    public void SetData(ScrollData_ItemGroup data, Action<ScrollData_ItemGroup> clickEvent = null)
    {
        if (data == null)
        {
            obj.SetActive(false);
            return;
        }

        obj.SetActive(true);

        gameObject.name = data.name;
        if (groupName != null)
            groupName.text = data.name;
        if (progressText != null)
            progressText.text = $"{data.obtainCount}/{data.totalCount}";
        if (Icon != null)
            _ = GameAssets.LoadAsync<Sprite>(data.res_url, sprite =>
            {
                if (sprite != null)
                    Icon.sprite = sprite;
            });
        if (button == null)
            return;
        button.onClick.RemoveAllListeners();
        if (clickEvent != null)
            button.onClick.AddListener(() => clickEvent.Invoke(data));

    }


    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveAllListeners();
    }
}
