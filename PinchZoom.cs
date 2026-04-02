using UnityEngine;

public class PinchZoom : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float minZoom = 0.5f;
    public float maxZoom = 3.0f;
    public float zoomSpeed = 0.1f;

    private float initialDistance;
    private Vector3 initialScale;

    void Update()
    {
        // 检查是否有两个触点在屏幕上
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            // 在双指触摸开始时，记录初始距离和物体的初始缩放值
            if (touch2.phase == TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(touch1.position, touch2.position);
                initialScale = transform.localScale;
            }
            else
            {
                // 计算当前双指的距离
                float currentDistance = Vector2.Distance(touch1.position, touch2.position);
                // 计算双指距离的变化量，并据此计算缩放比例
                float scaleFactor = (currentDistance - initialDistance) * zoomSpeed * Time.deltaTime;

                // 应用新的缩放值
                Vector3 newScale = initialScale * (1 + scaleFactor);

                // 将缩放值限制在设定的最小和最大值之间[citation:9]
                newScale.x = Mathf.Clamp(newScale.x, minZoom, maxZoom);
                newScale.y = Mathf.Clamp(newScale.y, minZoom, maxZoom);
                newScale.z = Mathf.Clamp(newScale.z, minZoom, maxZoom);

                transform.localScale = newScale;
            }
        }
    }
}