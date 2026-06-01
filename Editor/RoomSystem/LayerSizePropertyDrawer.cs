//using UnityEditor;
//using UnityEngine;

//// 标记该Drawer用于GridLayer_Size_SO中的LayerSize类
//[CustomPropertyDrawer(typeof(Room_SO.LayerSize))]
//public class LayerSizePropertyDrawer : PropertyDrawer
//{
//    // 保持和默认绘制相同的高度，避免布局错乱
//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        // 获取所有子属性的总高度（默认一行字段高度是16，这里按实际字段数调整）
//        return EditorGUI.GetPropertyHeight(property, label, true);
//    }

//    // 自定义绘制逻辑（核心：修改列表项标题 + 保留原有字段绘制）
//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        // 1. 获取LayerSize中的roomName和layerType字段
//        SerializedProperty roomNameProp = property.FindPropertyRelative("room_Name");
//        SerializedProperty layerTypeProp = property.FindPropertyRelative("layerType");

//        // 2. 组合自定义标题（处理roomName为空的情况，避免标题空白）
//        string roomName = string.IsNullOrEmpty(roomNameProp.stringValue) ? "未命名房间" : roomNameProp.stringValue;
//        string layerType = layerTypeProp.enumDisplayNames[layerTypeProp.enumValueIndex]; // 获取枚举的显示名称
//        string customLabel = $"{roomName} - {layerType}";

//        // 3. 替换原有标签的文本，绘制完整属性（保留默认的折叠/编辑功能）
//        EditorGUI.PropertyField(position, property, new GUIContent(customLabel), true);
//    }
//}