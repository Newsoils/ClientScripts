using System;
using System.Reflection;
using UnityEngine;
using CLIP.Framework_Unity.UI;
using CLIP.Framework_Unity;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// UIPanel基类，提供自动绑定UI组件的功能
/// </summary>
public abstract class UIPanelBase: MonoBehaviour, IPanel 
{
    public string PanelID  { get { return GetType().Name; } }


    public virtual void Awake()
    {
        Initialize();
    }
    public virtual void OnDestroy()
    {
        // 面板销毁时取消注册
        UIManager.Instance.UnregisterPanel(PanelID);
    }

    /// <summary>
    /// 初始化面板
    /// </summary>
    public virtual void Initialize()
    {
        // 注册到UIManager
        if (!UIManager.Instance.RegisterPanel(this))
        {
            Log.Warn($"Failed to register panel: {PanelID}");
        }
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
    

    #region 自动绑定UI组件
#if UNITY_EDITOR
    /// <summary>
    /// 自动绑定当前面板的所有UI组件
    /// </summary>
    public void AutoBind()
    {
        // 获取当前类的所有字段
        var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            // 跳过不是UI组件类型的字段
            if (!IsUIComponentType(field.FieldType))
                continue;


            // 在 AutoBind 循环里
            string searchName = field.Name.TrimStart('_'); // 去掉开头的下划线
            //if (searchName.StartsWith("m_")) searchName = searchName.Substring(2); // 去掉 m_ 前缀


            // 尝试在子物体中查找同名游戏对象
            Transform targetTransform = FindChildRecursive(transform, searchName);

            Component component = null;
            if (targetTransform != null)
            {
                if (field.FieldType == typeof(GameObject))
                {
                    // 如果字段类型是GameObject，直接赋值
                    field.SetValue(this, targetTransform.gameObject);
                    Debug.Log($"成功绑定: {field.Name} -> GameObject");
                    continue;
                }
                else if (field.FieldType == typeof(Transform))
                {
                    // 如果字段类型是Transform，直接赋值
                    field.SetValue(this, targetTransform);
                    Debug.Log($"成功绑定: {field.Name} -> Transform");
                    continue;
                }
                else
                {
                    // 获取对应类型的组件
                    component = targetTransform.GetComponent(field.FieldType);
                }


                if (component != null)
                {
                    // 将组件赋值给字段
                    field.SetValue(this, component);
                    Debug.Log($"成功绑定: {field.Name} -> {field.FieldType.Name}");
                }
                else
                {
                    Debug.LogWarning($"找到游戏对象 {field.Name}，但没有找到 {field.FieldType.Name} 组件");
                }
            }
            else
            {
                Debug.LogWarning($"未找到名为 {field.Name} 的游戏对象");
            }
        }
    }

    /// <summary>
    /// 递归查找子物体
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string name)
    {
        // 首先在当前层级查找
        foreach (Transform child in parent)
        {
            // 使用忽略大小写的比较
            if (string.Equals(child.name, name, System.StringComparison.OrdinalIgnoreCase))
                return child;
        }

        // 如果当前层级没找到，递归查找子物体
        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, name);
            if (result != null)
                return result;
        }

        return null;
    }


    public bool IsUIComponentType(Type type)
    {
        // 只要是 Component 的子类，或者是 GameObject，都允许尝试绑定
        return typeof(Component).IsAssignableFrom(type) || type == typeof(GameObject);
    }

#endif
#endregion
}
