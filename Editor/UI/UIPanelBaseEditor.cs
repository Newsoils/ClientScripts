using System;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIPanelBase的编辑器扩展
/// 注意：这个脚本必须放在Editor文件夹中！
/// </summary>
[CustomEditor(typeof(UIPanelBase), true)]
public class UIPanelBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 显示默认的Inspector（这样你就能看到原本的序列化字段）
        DrawDefaultInspector();

        GUILayout.Space(10);

        // 绘制Bind按钮
        if (GUILayout.Button("Bind UI Components", GUILayout.Height(30)))
        {
            UIPanelBase panel = target as UIPanelBase;
            if (panel != null)
            {
                Undo.RecordObject(panel, "Bind UI Components");
                panel.AutoBind();
                EditorUtility.SetDirty(panel);
                Debug.Log($"已完成 {panel.name} 的UI组件绑定");
            }
        }

        GUILayout.Space(5);

        // 清除绑定按钮
        if (GUILayout.Button("Clear Bindings"))
        {
            UIPanelBase panel = target as UIPanelBase;
            if (panel != null)
            {
                Undo.RecordObject(panel, "Clear UI Bindings");
                ClearBindings(panel);
                EditorUtility.SetDirty(panel);
                Debug.Log($"已清除 {panel.name} 的UI组件绑定");
            }
        }
    }

    /// <summary>
    /// 清除所有UI组件绑定
    /// </summary>
    private void ClearBindings(UIPanelBase panel)
    {
        var fields = panel.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            if (panel.IsUIComponentType(field.FieldType))
            {
                field.SetValue(panel, null);
            }
        }
    }

   
}