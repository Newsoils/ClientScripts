using System;
using System.Reflection;
using UnityEngine;

namespace CLIP.Framework_Unity
{
    /// <summary>
    /// 自动绑定UI组件的基类
    /// 支持任何继承此类的MonoBehaviour，通过Editor或代码自动绑定子物体的UI组件
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        /// <summary>
        /// 自动绑定当前对象的所有UI组件
        /// 通过反射查找同名子物体并绑定对应组件
        /// </summary>
        public void AutoBind()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (!IsBindableType(field.FieldType))
                    continue;

                string searchName = field.Name.TrimStart('_');
                Transform targetTransform = FindChildRecursive(transform, searchName);

                if (targetTransform == null)
                {
                    Debug.LogWarning($"[AutoBind] 未找到名为 {field.Name} 的子物体");
                    continue;
                }

                if (field.FieldType == typeof(GameObject))
                {
                    field.SetValue(this, targetTransform.gameObject);
                    Debug.Log($"[AutoBind] 绑定: {field.Name} -> GameObject");
                    continue;
                }

                if (field.FieldType == typeof(Transform))
                {
                    field.SetValue(this, targetTransform);
                    Debug.Log($"[AutoBind] 绑定: {field.Name} -> Transform");
                    continue;
                }

                Component component = targetTransform.GetComponent(field.FieldType);
                if (component != null)
                {
                    field.SetValue(this, component);
                    Debug.Log($"[AutoBind] 绑定: {field.Name} -> {field.FieldType.Name}");
                }
                else
                {
                    Debug.LogWarning($"[AutoBind] 找到 {field.Name}，但没有 {field.FieldType.Name} 组件");
                }
            }
        }

        /// <summary>
        /// 清除所有UI组件绑定（将字段置空）
        /// </summary>
        public void ClearBindings()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (IsBindableType(field.FieldType))
                {
                    field.SetValue(this, null);
                }
            }

            Debug.Log($"[AutoBind] 已清除 {GetType().Name} 的所有绑定");
        }

        /// <summary>
        /// 递归查找子物体（忽略大小写）
        /// </summary>
        protected Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (string.Equals(child.name, name, StringComparison.OrdinalIgnoreCase))
                    return child;
            }

            foreach (Transform child in parent)
            {
                Transform result = FindChildRecursive(child, name);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// 判断字段类型是否可绑定
        /// </summary>
        public static bool IsBindableType(Type type)
        {
            return typeof(Component).IsAssignableFrom(type) || type == typeof(GameObject) || type == typeof(Transform);
        }
    }
}
