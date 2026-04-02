using CLIP.Project_Mouse.Kernel;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(cloth_info))]

public class ClothInfoDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 获取 cloth_name 属性
        SerializedProperty nameProp = property.FindPropertyRelative("cloth_name");

        // 如果 cloth_name 不为空，则用它作为 Label 的名称
        string displayName = string.IsNullOrEmpty(nameProp.stringValue)
            ? label.text // 如果没写名字，显示默认的 Element X
            : $"[{property.FindPropertyRelative("cloth_id").intValue}] {nameProp.stringValue}";

        // 绘制属性面板
        EditorGUI.PropertyField(position, property, new GUIContent(displayName), true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 保持原本的高度，如果是展开状态则返回完整高度
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}

   