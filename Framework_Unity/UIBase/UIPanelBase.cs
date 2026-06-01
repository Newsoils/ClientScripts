using System;
using UnityEngine;
using CLIP.Framework_Unity.UI;
using CLIP.Framework_Unity;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// UIPanel基类，提供自动绑定UI组件的功能
/// </summary>
public abstract class UIPanelBase : UIBase, IPanel
{
    public string PanelID { get { return GetType().Name; } }

    private bool _isRegistered = false;

    public virtual void Awake()
    {
        TryRegister();
    }

    protected virtual void OnEnable()
    {
        TryRegister();
    }

    public virtual void OnDestroy()
    {
        UIManager.Instance?.UnregisterPanel(PanelID);
    }

    private void TryRegister()
    {
        if (_isRegistered) return;
        _isRegistered = true;
        Initialize();
    }

    /// <summary>
    /// 初始化面板
    /// </summary>
    public virtual void Initialize()
    {
        UIManager.Instance?.RegisterPanel(this);
    }

    /// <summary>
    /// 打开面板
    /// </summary>
    /// <param name="data">传递的参数</param>
    public abstract void OpenPanel(params object[] data);

    /// <summary>
    /// 关闭面板
    /// </summary>
    public abstract void ClosePanel();

    /// <summary>
    /// 更新面板
    /// </summary>
    public virtual void UpdatePanel(params object[] data) { }
}
