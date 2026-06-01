using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel.Social;
using CLIP.Project_Mouse.UI;
using Common;
using UnityEngine;

public class FriendRequestPanel : MonoBehaviour
{
    public GameObject obj;

    [Header("Request List")]
    public Transform requestListRoot;
    public GameObject requestUnitPrefab;

    [Header("Empty State")]
    [Tooltip("无好友请求时显示，文案在预制体上自行配置")]
    public GameObject emptyRequestHint;

    Player_Social_Manager SM => Player_Social_Manager._instance;

    public void OpenPanel()
    {
        obj.SetActive(true);
        RefreshPanel();
    }

    public void ClosePanel()
    {
        obj.SetActive(false);
    }

    public void RefreshPanel()
    {
        var applyInfos = SM.ApplyInfos ?? new List<PlayerDetailedInfo>();
        emptyRequestHint.SetActive(applyInfos.Count == 0);
        RebuildList(requestListRoot, requestUnitPrefab, applyInfos.Count, (go, i) =>
        {
            var unit = go.GetComponent<FriendRequestUnit>();
            unit.record = Friend_Social_Record.Create(applyInfos[i]);
            unit.InitFriendUnit();
        });
    }


    private void RebuildList(Transform root, GameObject prefab, int count,
       System.Action<GameObject, int> bindAction)
    {
        var existing = new List<GameObject>();
        for (int i = 0; i < root.childCount; i++)
            existing.Add(root.GetChild(i).gameObject);

        for (int i = 0; i < count; i++)
        {
            GameObject go;
            if (i < existing.Count)
            {
                go = existing[i];
                go.SetActive(true);
            }
            else
            {
                go = Instantiate(prefab, root);
            }
            bindAction(go, i);
        }

        for (int i = count; i < existing.Count; i++)
            existing[i].SetActive(false);
    }
}
