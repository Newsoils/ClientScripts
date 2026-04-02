#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// 这个类主要是用于当成场景占位符。
/// 当场景中需要动态加载某个预制体时，可以先放置一个 ScenePlaceholder 物体在场景中，
/// 编辑器模式下打开场景时会自动加载。
/// 由于微信小程序首包限制很小，有时候场景中不能放过大的资源，只能在运行时使用热更新下载并加载。
/// 如果不需要发布微信小程序，可以将物体直接放在场景中。
/// 因为采用热更新时，场景里没有物体，编辑起来非常不直观，所以才有这个功能，来方便编辑器下的编辑。
/// </summary>
public class Model_Placeholder : MonoBehaviour
{
    [Tooltip("Prefab 名称（不需要后缀，不需要路径）")]
    public string prefabName;

    [Tooltip("可直接拖拽一个 Prefab 替代 prefabName 加载方式。若填写则优先使用此对象。")]
    public GameObject prefabObject;

    // 为了运行时自动隐藏占位物体
    private void Awake()
    {
        // 运行时隐藏占位物体
        //gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(prefabName)) return;

        // 避免每次都改名字导致多次刷新
        string expectedName = $"ScenePlaceholder_{prefabName}";
        if (gameObject.name != expectedName)
        {
            gameObject.name = expectedName;
            EditorUtility.SetDirty(gameObject);
        }
    }
#endif


}
