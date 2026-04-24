using System.Collections.Generic;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.UI;
using UnityEngine;

/// <summary>
/// UI管理器，统一管理所有UI面板
/// </summary>
public class UIManager : SingletonMono<UIManager>
{
    private Dictionary<string, IPanel> _registeredPanels = new Dictionary<string, IPanel>();

    /// <summary>
    /// 是否初始化完成
    /// </summary>
    public bool IsInitialized { get; private set; }


    /// <summary>
    /// 初始化UI管理器
    /// </summary>
    public void Initialize()
    {
        if (IsInitialized) return;

        //CreateLayerContainers();
        IsInitialized = true;
        Debug.Log("UIManager Initialized");
    }

    /// <summary>
    /// 注册面板
    /// </summary>
    /// <param name="panel">面板实例</param>
    public bool RegisterPanel(IPanel panel)
    {
        if (panel == null)
        {
            Debug.LogError("Cannot register null panel!");
            return false;
        }

        string panelID = panel.PanelID;

        if (_registeredPanels.ContainsKey(panelID))
        {
            Debug.LogWarning($"Panel {panelID} is already registered!");
            return false;
        }

        _registeredPanels.Add(panelID, panel);


        Debug.Log($"Panel {panelID} registered successfully");
        return true;
    }

    /// <summary>
    /// 取消注册面板
    /// </summary>
    /// <param name="panelID">面板ID</param>
    public bool UnregisterPanel(string panelID)
    {
        if (_registeredPanels.ContainsKey(panelID))
        {
            _registeredPanels.Remove(panelID);
            Debug.Log($"Panel {panelID} unregistered");
            return true;
        }

        Debug.LogWarning($"Panel {panelID} is not registered!");
        return false;
    }

    /// <summary>
    /// 打开面板
    /// </summary>
    /// <param name="panelID">面板ID</param>
    /// <param name="data">传递的参数</param>
    public T OpenPanel<T>(params object[] data) where T : class, IPanel
    {
        string panelID = typeof(T).Name;
        if (!_registeredPanels.TryGetValue(panelID, out IPanel panel))
        {
            // 尝试通过 FindObjectOfType 查找
            var panelMono = FindObjectOfType(typeof(T)) as IPanel;

            if (panelMono != null)
            {
                RegisterPanel(panelMono);
                panel = panelMono;
            }
            else
            {
                Debug.LogWarning($"Panel {panelID} is not registered!");
                return null;
            }
        }

        // 调用面板的打开方法
        panel.OpenPanel(data);

        Debug.Log($"Panel {panelID} opened");
        return panel as T;
    }

    public void UpdatePanel(string panelID,params object[] data)
    {
        if(!_registeredPanels.TryGetValue(panelID, out IPanel panel))
        {
            Debug.LogWarning($"Panel {panelID} is not registered!");
            return;
        }
        panel.UpdatePanel(data);
    }

    /// <summary>
    /// 关闭所有面板
    /// </summary>
    public void CloseAllPanels()
    {
        foreach(var panel in _registeredPanels.Values)
        {
            panel.ClosePanel();
        }
    }

    /// <summary>
    /// 检查面板是否已注册
    /// </summary>
    public bool IsPanelRegistered(string panelID)
    {
        return _registeredPanels.ContainsKey(panelID);
    }

    /// <summary>
    /// 获取面板实例
    /// </summary>
    /// <param name="panelID">面板ID</param>
    /// <returns>面板实例，如果不存在返回null</returns>
    public T GetPanel<T>() where T : class, IPanel
    {
        string panelID = typeof(T).Name;

        if (_registeredPanels.TryGetValue(panelID, out IPanel panel))
            return panel as T;

        // 自动重新查找
        var panelMono = GameObject.FindObjectOfType(typeof(T)) as IPanel;

        if (panelMono != null)
        {
            RegisterPanel(panelMono);
            return panelMono as T;
        }

        return null;
    }

    

}