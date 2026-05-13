using UnityEngine;

public class Unity_Tools
{
    /// <summary>
    /// 初始化GameObject状态：
    /// - 将其localPosition、rotation、scale重置；
    /// - 保证RectTransform对齐；
    /// - 统一Layer；
    /// </summary>
    public static void IdentityGameObject(GameObject go)
    {
        if (go == null) return;

        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        // 如果是UI对象
        if (go.TryGetComponent<RectTransform>(out var rect))
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        // 同步layer（如果父物体有layer）
        if (go.transform.parent != null)
        {
            go.layer = go.transform.parent.gameObject.layer;
        }
    }

    public static void ClearAllChildren(Transform root)
    {
        if (root == null)
            return;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);
            GameObject.Destroy(child.gameObject);
        }
    }



    public static void ClearAllChildrenImmediate(Transform root)
    {
        if (root == null)
            return;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);
            GameObject.DestroyImmediate(child.gameObject);
        }
    }

    public static string InternetSourceMode()
    {
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable:
                return "无网络";
            case NetworkReachability.ReachableViaCarrierDataNetwork:
                return "移动网络";
            case NetworkReachability.ReachableViaLocalAreaNetwork:
                return "Wi-Fi";
            default:
                return "未知网络";
        }
    }


    /// <summary>
    /// 清理unity控制台（利用反射）
    /// </summary>
    //public static void ClearConsole()
    //{
    //    Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
    //    Type logEntries = assembly.GetType("UnityEditor.LogEntries");
    //    MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
    //    clearConsoleMethod.Invoke(new object(), null);
    //}
}
