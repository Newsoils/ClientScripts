using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.UI;
using UnityEngine;

public class NPCPanel : MonoBehaviour
{
    public GameObject obj;

    public Transform npcChatUnitRoot;

    public GameObject npcChatUnitPrefab;

    private void Start()
    {
        EvtDsp.AddEvt(EvtNames.On_NPC_Data_Update, RefreshNPCList);
    }

    private void OnDestroy()
    {
        EvtDsp.RemoveEvt(EvtNames.On_NPC_Data_Update, RefreshNPCList);
    }


    public void OpenPanel(params object[] data)
    {
        // 检查对象是否已被销毁
        if (this == null || gameObject == null || !gameObject.activeInHierarchy)
        {
            Debug.LogWarning("NPCPanel is destroyed or inactive, ignoring OpenPanel call");
            return;
        }
        obj.SetActive(true);
        RefreshNPCList();
    }

    public void ClosePanel()
    {
        obj.SetActive(false);
    }


    public void RefreshNPCList()
    {
        var npcDict = NPCManager.Instance.NPC_Info_Dic;
        var acquaintedNpcs = new List<NPC_Info>();
        foreach (var kv in npcDict)
        {
            if (kv.Value._npc_RuntimeData != null && kv.Value._npc_RuntimeData.is_acquainted)
                acquaintedNpcs.Add(kv.Value);
        }

        var npcUnitList = new List<GameObject>();
        for (int i = 1; i < npcChatUnitRoot.transform.childCount; i++)
        {
            npcUnitList.Add(npcChatUnitRoot.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < acquaintedNpcs.Count; i++)
        {
            GameObject unit;
            if (i < npcUnitList.Count)
            {
                unit = npcUnitList[i];
                unit.SetActive(true);
            }
            else
            {
                unit = Instantiate(npcChatUnitPrefab, npcChatUnitRoot.transform);
            }
            var npcUnitScript = unit.GetComponent<NpcChatUnit>();
            npcUnitScript.info = acquaintedNpcs[i];
            npcUnitScript.InitNpcChatUnit();
        }

        for (int i = acquaintedNpcs.Count; i < npcUnitList.Count; i++)
        {
            npcUnitList[i].SetActive(false);
        }
    }


}
