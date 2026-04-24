using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class RewardPanel : UIPanelBase
            {
                public GameObject panelObj;
                public Button exitButton;
                //public Button showPhoto;
                public Transform itemFrame;
                public RectTransform backgroundImage;
                public GameObject itemCellPrefab;
                public GameObject photoCellPrefab;

                private string photoName;
                private bool NeedShowPhoto = false;

                private void Start()
                {
                    DontDestroyOnLoad(gameObject);
                    EvtDsp.AddReturnEvt<List<(string, int)>, bool>(EvtNames.Show_Reward, OpenPanel);
                    EvtDsp.AddEvt<List<(string, int)>>(EvtNames.ShowFirstLoginReward, OpenPanelFirstLogin);
                    EvtDsp.AddReturnEvt< List<(string, int)>, bool> (EvtNames.Dispatch_Show_Reward, DispatchOpenPanel);

                    exitButton.onClick.AddListener(ClosePanel);
                }

                public override void OnDestroy()
                {
                    base.OnDestroy();
                    EvtDsp.RemoveReturnEvt<List<(string, int)>, bool>(EvtNames.Show_Reward, OpenPanel);
                    EvtDsp.RemoveEvt<List<(string, int)>>(EvtNames.ShowFirstLoginReward, OpenPanelFirstLogin);
                    EvtDsp.RemoveReturnEvt<List<(string, int)>, bool>(EvtNames.Dispatch_Show_Reward, DispatchOpenPanel);
                    exitButton.onClick.RemoveAllListeners();
                }

                public bool DispatchOpenPanel(List<(string, int)> items)
                {
                    NeedShowPhoto = true;
                    return OpenPanel(items);
                }
                public bool OpenPanel(List<(string,int)> items)
                {
                    photoName = "";

                    panelObj.SetActive(true);
                    RefreshUI(items);
                    Log.Sucess("DispatchRewardPanel Show Items");
                    AudioManager.Instance.PlayAudioByRefKey("getReward");
                    if(panelObj != null) { return true; } else { return false; }

                }
                public void OpenPanelFirstLogin(List<(string, int)> items)
                {
                    photoName = "";

                    panelObj.SetActive(true);
                    RefreshUI(items);

                    exitButton.onClick.AddListener(OpenGuide);

                    Log.Sucess("DispatchRewardPanel Show Items");
                    AudioManager.Instance.PlayAudioByRefKey("getReward");
                }

                private void OpenGuide()
                {
                    GuideManager.Instance.StartGuide();
                    exitButton.onClick.RemoveAllListeners();
                    exitButton.onClick.AddListener(ClosePanel);
                }

                private void RefreshUI(List<(string, int)> items)
                {
                    foreach(Transform child in itemFrame)
                    {
                        Destroy(child.gameObject);
                    }
                    foreach(var item in items)
                    {
                        List<string> itemNames = item.Item1.Split('_').ToList();
                        if(itemNames.Count < 2)
                        {
                            CreateItemCell(item.Item1, item.Item2);
                        }
                        else
                        {
                            //CreatePhotoCell(item.Item1);
                        }
                    }
                    StartCoroutine(RebuildLayoutAfterFrame());
                    LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundImage);
                }
                IEnumerator RebuildLayoutAfterFrame()
                {
                    yield return null;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundImage);
                }
                private void CreateItemCell(string itemName, int count)
                {
                    GameObject obj = Instantiate(itemCellPrefab, itemFrame);
                    obj.GetComponent<RewardItemCell>().Init(itemName, count);
                }
                private void CreatePhotoCell(string photoName)
                {
                    GameObject obj = Instantiate(photoCellPrefab, itemFrame);
                    obj.GetComponent<PhotoRewardCell>().Init(photoName);
                }

                public override void OpenPanel(params object[] data)
                {
                    List<(string, int)> datas = data[0] as List<(string, int)>;
                    OpenPanel(datas);
                }

                public override void ClosePanel()
                {
                    panelObj.SetActive(false);


                    var showPhotoPanel = UIManager.Instance.GetPanel<ShowPhotoPanel>();
                    if (showPhotoPanel == null) return;
                    if(NeedShowPhoto)
                    {
                        if (!string.IsNullOrEmpty(photoName))
                        {
                            UIManager.Instance.GetPanel<ShowPhotoPanel>().OpenPanel(photoName);
                        }
                        else
                        {
                            UIManager.Instance.GetPanel<ShowPhotoPanel>().OpenPanel();
                        }
                    }
                    NeedShowPhoto = false;
                    
                }
            }
        }
    }
}

