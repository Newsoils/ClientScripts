using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class FriendPanel : MonoBehaviour
{
    public GameObject obj;

    [Header("Friend List")]
    public Transform friendListRoot;
    public GameObject friendUnitPrefab;

    [Header("Search")]
    public GameObject searchBar;
    public TMP_InputField searchInput;
    public Button btnSearch;
    //public Button btnClearSearch;

    [Header("Search Result")]
    public FriendUnit searchedAcceptedFriend;
    public UnAcceptFriendUnit searchedUnacceptedFriend;

    private bool _isSearching;

    public GameObject friendsItemView;

    Player_Social_Manager SM => Player_Social_Manager._instance;

    private void Start()
    {
        if (SM != null)
        {
            SM._on_refresh_social_state.AddListener(ShowSearchResults);
        }

        searchInput.onSubmit.AddListener(DoSearch);

        btnSearch.onClick.AddListener(DoSearch);
        //btnClearSearch.onClick.AddListener(ClearSearch);
    }

    private void OnDestroy()
    {
        if (SM != null)
        {
            SM._on_refresh_social_state.RemoveListener(ShowSearchResults);
        }

        searchInput.onSubmit.RemoveListener(DoSearch);

        btnSearch.onClick.RemoveAllListeners();
        //btnClearSearch.onClick.RemoveAllListeners();
    }

    public void OpenPanel()
    {
        obj.SetActive(true);
        HideSearchResults();
        RefreshPanel();
    }

    public void ClosePanel()
    {
        obj.SetActive(false);
        ClearSearch();
    }


    public void RefreshPanel()
    {
        var friends = SM._current_social_info._friend_accepted_info_record;
        RebuildList(friendListRoot, friendUnitPrefab, friends.Count, (go, i) =>
        {
            var unit = go.GetComponent<FriendUnit>();
            unit.record = friends[i];
            unit.InitFriendUnit();
        });
    }


    #region Search

    private void DoSearch()
    {
        string keyword = searchInput.text;

        DoSearch(keyword);
    }

    private void DoSearch(string text)
    {
        if (string.IsNullOrEmpty(text) || text == Global_Game_Manager._instance._current_player_id)
        {
            ClearSearch();
            return;
        }

        friendsItemView.SetActive(false );      
        string keyword = text;
        //if (string.IsNullOrWhiteSpace(keyword)) return;

        _isSearching = true;
        SM.on_try_find_friend(keyword);
    }

    private void ClearSearch()
    {
        friendsItemView?.SetActive(true);

        _isSearching = false;
        searchInput.text = "";
        HideSearchResults();
    }

    private void HideSearchResults()
    {
        if (searchedAcceptedFriend != null) searchedAcceptedFriend.gameObject.SetActive(false);
        if (searchedUnacceptedFriend != null) searchedUnacceptedFriend.gameObject.SetActive(false);
    }

    /// <summary>
    /// 搜索结果返回后由 Player_Social_Manager._on_refresh_social_state 触发。
    /// 在 RefreshCurrentTab 内自动调用。
    /// </summary>
    public void ShowSearchResults()
    {
        if (!_isSearching || SM._temp_search_result == null) return;

        var results = SM._temp_search_result;
        if (results.Count == 0 || string.IsNullOrWhiteSpace(results[0].friend_id))
        {
            PromptMessage.Instance.ShowUpPrompt("未找到该玩家");
            return;
        }

        var record = results[0];
        bool isAccepted = SM._current_social_info._friend_accepted_info_record
            .Exists(f => f.friend_id == record.friend_id);

        if (isAccepted)
        {
            searchedAcceptedFriend.record = record;
            searchedAcceptedFriend.InitFriendUnit();
            searchedAcceptedFriend.gameObject.SetActive(true);
            searchedUnacceptedFriend.gameObject.SetActive(false);
        }
        else
        {
            searchedUnacceptedFriend.record = record;
            searchedUnacceptedFriend.InitFriendUnit("添加");
            searchedAcceptedFriend.gameObject.SetActive(false);
            searchedUnacceptedFriend.gameObject.SetActive(true);
        }
    }
    #endregion

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
