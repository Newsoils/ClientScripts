using CLIP.Framework_Core.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class WarningPanel :UIPanelBase
{
    public GameObject obj;
    public Button closeButton;
    public TMP_Text warningText;


    void Start()
    {
        EvtDsp.AddEvt<string>(EvtNames.Show_Warning_Panel, OpenPanel);

        closeButton.onClick.AddListener(ClosePanel);
    }

    public override void OnDestroy()
    {
        EvtDsp.RemoveEvt<string>(EvtNames.Show_Warning_Panel, OpenPanel);

        closeButton.onClick.RemoveAllListeners();
    }

    public override void ClosePanel()
    {
        obj.SetActive(false);
        warningText.text = "";
    }


    private void OpenPanel(string text)
    {
        if(!string.IsNullOrEmpty(text) )
        {
            warningText.text = text;
            obj.SetActive(true);
        }
    }
    public override void OpenPanel(params object[] data)
    { 
        if(data!=null&&data.Length>0)
        {
            var text = (string)data[1];
            if(!string.IsNullOrEmpty(text) )
            {
                warningText.text = text;
                obj.SetActive(true);
            }
        }
    }


 


}
