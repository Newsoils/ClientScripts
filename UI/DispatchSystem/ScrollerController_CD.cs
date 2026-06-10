using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using EnhancedUI.EnhancedScroller;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ScrollerController_CD : MonoBehaviour, IEnhancedScrollerDelegate
{
    public FirstCellView_CD firstCellView_CD_Prefab;
    public EnhancedScroller scroller;
    private List<ScrollData_GameItem> gameItemDataList = new List<ScrollData_GameItem>();
    public int numberOfCellsPerRow = 3;

    // Start is called before the first frame update
    void Start()
    {
        scroller.Delegate = this;
        ReloadData();

        EvtDsp.AddEvt(EvtNames.CD_Play_Next, PlayNext);
        EvtDsp.AddEvt(EvtNames.CD_Play_Before, PlayBefore);
    }

    void OnDestroy()
    {
        EvtDsp.RemoveEvt(EvtNames.CD_Play_Next, PlayNext);
        EvtDsp.RemoveEvt(EvtNames.CD_Play_Before, PlayBefore);
    }

    public void ReloadData()
    {
        ClearData();

        var inventory = Global_Inventory_Manager.Items.ToList();
        if(inventory == null)
        {
            Log.Error(" ScrollerController_CD:Inventory is null!");
            return;

        }

        var cd_Data = inventory.FindAll(item => item?.item_info?.type == Item_Type.Tape);
     
        foreach (var item in cd_Data)
        {
            gameItemDataList.Add(new ScrollData_GameItem(
                item.item_info.name,
                item.item_info.item_id,
                item._item_count,
                item.item_info.rarity,
                item.uid,
                Global_Inventory_Manager.IsNewObtainItem(item.uid)));
        }

        scroller.ReloadData();
    }

    
    public void ClearData()
    {
        gameItemDataList.Clear();
    }

    public static bool isPlaying = false;
    private int currentPlayingId = -1;
    private int lastPlayingId = -1;

    public bool IsPlaying => isPlaying;

    public Button GetFirstButton()
    {
        return GetFirstCell()?.zButton as Button;
    }

    public void PlayeOrPause(ScrollData_GameItem data)
    {
        var dispatch_config = Dispatch_Manager._instance._dispatch_configuration_so._dispatch_config;
        var wav_name = dispatch_config.tape_info_list.FirstOrDefault(t => t.item_id == data.id)?.wav_resource_name;

        UIManager.Instance.GetPanel<DispatchPanel>().SetDispatchItem(Item_Type.Tape, data.name);

        // 点击的是正在播放的 CD → 停止
        if (isPlaying && currentPlayingId == data.id)
        {
            AudioManager.Instance.StopMusic();
            isPlaying = false;
            currentPlayingId = -1;
            EvtDsp.TriggerEvt<bool>(EvtNames.CD_Player_Playing, false);
        }
        // 点击的是不同的 CD → 切换播放
        else
        if (isPlaying && currentPlayingId != data.id)
        {
            AudioManager.Instance.StopMusic();
            if (!string.IsNullOrEmpty(wav_name))
            {
                AudioManager.Instance.PlayAduioByResKey(wav_name, 0.5f, 0.5f);
            }
            currentPlayingId = data.id;
            EvtDsp.TriggerEvt<bool>(EvtNames.CD_Player_Playing,true);
        }
        // 当前没有播放 → 直接播放
        else
        {
            if (!string.IsNullOrEmpty(wav_name))
            {
                AudioManager.Instance.PlayAduioByResKey(wav_name, 0.5f, 0.5f);
            }
            isPlaying = true;
            currentPlayingId = data.id;
            EvtDsp.TriggerEvt<bool>(EvtNames.CD_Player_Playing,true);
        }

        EvtDsp.TriggerEvt(EvtNames.Dispatch_Change_Item, Item_Type.Tape, data.name);
    }

    public void StopMusic()
    {
        if (isPlaying)
        {
            AudioManager.Instance.StopMusic();
            isPlaying = false;
            lastPlayingId = currentPlayingId;
            currentPlayingId = -1;
            EvtDsp.TriggerEvt<bool>(EvtNames.CD_Player_Playing, false);
        }
    }

    public void TogglePlayPause()
    {
        if (isPlaying)
        {
            StopMusic();
        }
        //这时候这次现在播的肯定没了StopMusic会置为-1因此用lastPlayingId来记录一下
        else if (lastPlayingId >= 0)
        {
            var data = gameItemDataList.FirstOrDefault(d => d.id == lastPlayingId);
            if (data != null)
            {
                PlayeOrPause(data);
            }
        }
    }

    public void PlayNext()
    {
        if (gameItemDataList.Count == 0) return;

        int nextIndex;
        if (currentPlayingId < 0)
        {
            nextIndex = 0;
        }
        else
        {
            int currentIndex = gameItemDataList.FindIndex(d => d.id == currentPlayingId);
            if (currentIndex < 0)
            {
                nextIndex = 0;
            }
            else
            {
                nextIndex = currentIndex + 1;
                if (nextIndex >= gameItemDataList.Count)
                {
                    nextIndex = 0;
                }
            }
        }

        var nextData = gameItemDataList[nextIndex];
        PlayeOrPause(nextData);
    }

    public void PlayBefore()
    {
        if (gameItemDataList.Count == 0) return;

        int prevIndex;
        if (currentPlayingId < 0)
        {
            prevIndex = gameItemDataList.Count - 1;
        }
        else
        {
            int currentIndex = gameItemDataList.FindIndex(d => d.id == currentPlayingId);
            if (currentIndex < 0)
            {
                prevIndex = gameItemDataList.Count - 1;
            }
            else
            {
                prevIndex = currentIndex - 1;
                if (prevIndex < 0)
                {
                    prevIndex = gameItemDataList.Count - 1;
                }
            }
        }

        var prevData = gameItemDataList[prevIndex];
        PlayeOrPause(prevData);
    }



    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        FirstCellView_CD cellView = scroller.GetCellView(firstCellView_CD_Prefab) as FirstCellView_CD;

        // data index of the first sub cell
        var di = dataIndex * numberOfCellsPerRow;

        cellView.name = "CD " + (di).ToString() + " to " + ((di) + numberOfCellsPerRow - 1).ToString();

        // pass in a reference to our data set with the offset for this cell
        cellView.SetData( gameItemDataList, di, PlayeOrPause);

        return cellView;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return 350f;
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        if (gameItemDataList.Count == 0) return 0;
        return Mathf.CeilToInt((float)gameItemDataList.Count / numberOfCellsPerRow);
    }

    /// <summary>
    /// 尝试拿一下第一个Cell（这个应该是当前显示的第一个）
    /// </summary>
    /// <returns></returns>
    public SecondCellView_CD GetFirstCell()
    {
        var cell = scroller.GetCellViewAtDataIndex(scroller.StartCellViewIndex) as FirstCellView_CD;
        return cell?.GetFirstCell();
    }

}
