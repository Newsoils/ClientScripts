using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    private static DontDestroy instance;

    void Awake()
    {
        // 如果已存在实例，销毁新创建的对象
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // 否则设置为实例，并标记为不销毁
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

