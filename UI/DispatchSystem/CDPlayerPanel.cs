using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CDPlayerPanel : UIPanelBase
{
    public GameObject obj;

    public TMP_Text MoodTagText;
    public Button LastCDButton;
    public Button NextCDButton;
    public Button playOrStopButton;


    // Start is called before the first frame update
    void Start()
    {
        EvtDsp.AddEvt<Item_Type, string>(EvtNames.Dispatch_Change_Item, ChangeMoodTagText);

        //playOrStopButton.onClick.AddListener(()=>)
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        EvtDsp.RemoveEvt<Item_Type, string>(EvtNames.Dispatch_Change_Item, ChangeMoodTagText);
    }

    public void ChangeMoodTagText(Item_Type type, string name)
    {
        if (type != Item_Type.Tape) return;

        OpenPanel();
        var tape_Info = Dispatch_Manager._instance._dispatch_configuration_so._dispatch_config.tape_info_list.FirstOrDefault(t=>t.tape_name == name);

        if (tape_Info == null) return;
        var text = "";
        foreach (var mood in tape_Info.mood_tag_list)

        if (Dispatch_Manager.chineseMoodMap.TryGetValue(mood, out var cnMood))
        {
            text += cnMood + "\t";
        }

        MoodTagText.text = text;
    }

    public override void OpenPanel(object[] parameters)
    {
        obj.SetActive(true);
    }
    public override void ClosePanel()
    {
        obj.SetActive(false);
    }

    public override void UpdatePanel(params object[] data)
    {
    }


}
