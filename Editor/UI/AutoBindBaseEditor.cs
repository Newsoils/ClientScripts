using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CLIP.Framework_Unity.Editor
{
    /// <summary>
    /// AutoBindBase的通用编辑器扩展
    /// 只要类继承AutoBindBase，这个编辑器就会自动为其提供Bind/Clear按钮
    /// </summary>
    [CustomEditor(typeof(UIBase), true)]
    public class AutoBindBaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10);

            if (GUILayout.Button("Bind UI Components", GUILayout.Height(30)))
            {
                UIBase target = base.target as UIBase;
                if (target != null)
                {
                    Undo.RecordObject(target, "Bind UI Components");
                    target.AutoBind();
                    EditorUtility.SetDirty(target);
                    Debug.Log($"已完成 {target.name} 的UI组件绑定");
                }
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Clear Bindings"))
            {
                UIBase target = base.target as UIBase;
                if (target != null)
                {
                    Undo.RecordObject(target, "Clear UI Bindings");
                    target.ClearBindings();
                    EditorUtility.SetDirty(target);
                }
            }
        }
    }
}
