using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CLIP.Framework_Core.Event;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class PromptMessage : MonoBehaviour
            {
                public static PromptMessage Instance { get; private set; }

                [Header("提示框")]
                public GameObject promptPanel;
                public TMP_Text prompt;
                public Toggle dontRemindNextTime;
                private int promptId;
                private Action onConfirm;

                [Header("屏幕上方提示")]
                public GameObject popUpObj;
                public CanvasGroup popUpCanvas;
                public Sequence popTween;
                public TMP_Text popUpText;
                private Vector3 popOriginPosition;

                [Header("提示框配置")]
                public List<PromptConfig> promptConfigs = new List<PromptConfig>();
                public Dictionary<int, bool> dontRemindDict = new Dictionary<int, bool>();

                private void Awake()
                {
                    if (Instance != null && Instance != this)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    InitializeDict();
                    Instance = this;
                    DontDestroyOnLoad(gameObject);
                }

                private void Start()
                {
                    popOriginPosition = popUpObj.GetComponent<RectTransform>().anchoredPosition;
                    EvtDsp.AddEvt<string>(EvtNames.ShowUpPrompt, ShowUpPrompt);
                    EvtDsp.AddEvt<string, Action>(EvtNames.ShowPrompt, ShowPrompt);
                }

                public void OnDestroy()
                {
                    EvtDsp.RemoveEvt<string>(EvtNames.ShowUpPrompt, ShowUpPrompt);
                    EvtDsp.RemoveEvt<string, Action>(EvtNames.ShowPrompt, ShowPrompt);
                }

                public void ShowUpPrompt(string message)
                {
                    popTween?.Kill();
                    popUpObj.SetActive(true);
                    popUpText.text = message;
                    popUpObj.GetComponent<RectTransform>().anchoredPosition = popOriginPosition + new Vector3(0, 30, 0);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(popUpObj.GetComponent<RectTransform>());

                    popUpCanvas.alpha = 0;
                    Sequence sequence = DOTween.Sequence();
                    sequence.Append(popUpObj.GetComponent<RectTransform>().DOAnchorPos(popOriginPosition, 0.3f));
                    sequence.Join(popUpCanvas.DOFade(1, 0.3f));
                    sequence.AppendInterval(2f);
                    sequence.Append(popUpCanvas.DOFade(0, 0.3f).OnComplete(() =>
                    {
                        popUpObj.gameObject.SetActive(false);
                    }));
                    popTween = sequence;
                }

                public string GetMessageById(int id)
                {
                    foreach (var config in promptConfigs)
                    {
                        if (config.id == id)
                            return config.message;
                    }
                    return string.Empty;
                }

                public bool GetDontRemindNextTime(int id)
                {
                    if (dontRemindDict.TryGetValue(id, out bool value))
                        return value;
                    return false;
                }

                public void SetDontRemindNextTime(int id, bool value)
                {
                    dontRemindDict[id] = value;
                }

                public void InitializeDict()
                {
                    foreach (var config in promptConfigs)
                    {
                        if (!dontRemindDict.ContainsKey(config.id))
                        {
                            dontRemindDict[config.id] = false;
                        }
                    }
                }

                public void ShowPrompt(int id, Action onConfirmAction)
                {
                    string message = GetMessageById(id);
                    if (string.IsNullOrEmpty(message))
                    {
                        Debug.LogWarning($"未找到id={id}的提示内容");
                        return;
                    }
                    promptId = id;
                    //Debug.Log(GetDontRemindNextTime(id));
                    if (!GetDontRemindNextTime(id))
                    {
                        prompt.text = message;
                        dontRemindNextTime.isOn = false;
                        onConfirm = onConfirmAction;
                        promptPanel.SetActive(true);
                    }
                    else
                    {
                        onConfirmAction?.Invoke();
                    }
                }
                public void ShowPrompt(string text, Action onConfirmAction)
                {
                    promptPanel.SetActive(true);
                    prompt.text = text;
                    onConfirm = onConfirmAction;
                }
                public void OnCancel()
                {
                    promptPanel.SetActive(false);
                }

                public void OnConfirm()
                {
                    onConfirm?.Invoke();
                    if (dontRemindNextTime.isOn)
                    {
                        // Debug.Log("不再提醒");
                        SetDontRemindNextTime(promptId, true);
                    }
                    promptPanel.SetActive(false);
                }

                // 序列化保存
                public void ResetDontRemindAll()
                {
                    dontRemindDict.Clear();
                    Debug.Log("所有不再提醒状态已重置");
                }

                public IEnumerator UpPrompt(float time, string message)
                {
                    popUpText.text = message;
                    popUpObj.SetActive(true);
                    yield return new WaitForSeconds(time);
                    popUpObj.SetActive(false);
                }
            }

        }
    }
}