//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Game_Play_System.Planting_System;
//using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
//using Lean.Touch;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;
//using CLIP.Project_Mouse.Game_Play_System;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class CustomRaycastTool : MonoBehaviour
//            {
//                [Header("检测参数")]
//                public LayerMask normalTargetLayers = ~0;
//                public LayerMask editTargetLayers = ~0; 
//                public LayerMask targetLayers = ~0;
//                public LayerMask UILayers = 0;
//                public List<string> targetTags = new List<string>(); // 可选，留空则不过滤
//                public List<string> requiredComponentNames = new List<string>(); // 只检测有这些脚本的物体

//                [Header("检测距离")]
//                public float maxDistance = 100f;

//                public Camera mainCamera;

//                private Vector2 touchStartPosition = Vector2.zero; // 记录触摸起始位置
//                private float touchStartTime; // 记录触摸起始时间
//                private Vector2 touchEndPosition = Vector2.zero;
//                private const float swipeThreshold = 50f; // 滑动距离阈值
//                private const float tapTimeThreshold = 0.5f; // 点击时间阈值

//                private bool touchProcessed = false;
//                private bool mouseProcessed = false;

//                private LeanFinger activeFinger = null;

//                public GameObject hitGo;

//                private void Awake()
//                {
//                    mainCamera = GetComponent<Camera>();
//                }

//                void Update()
//                {
//                    //mainCamera = Camera.main;
//#if UNITY_EDITOR || UNITY_STANDALONE
//                    if (LeanTouch.Fingers.Count == 2)
//                    {
//                        var finger = LeanTouch.Fingers[1];
//                        UpdateHitObject(finger.ScreenPosition, out RaycastHit hitInfo);
//                    }
//#endif

//#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
//                    if (LeanTouch.Fingers.Count == 1)
//                    {
//                        var finger = LeanTouch.Fingers[0];
//                        UpdateHitObject(finger.ScreenPosition, out RaycastHit hitInfo);
//                    }
//#endif


//#if UNITY_EDITOR || UNITY_STANDALONE
//                    //Debug.Log($"当前触摸点数量: {LeanTouch.Fingers.Count}");
//                    if (LeanTouch.Fingers.Count == 2)
//                    {
//                        var finger = LeanTouch.Fingers[1];
//                        //Debug.Log($"状态: {(finger.Down ? "Began" : finger.Up ? "Ended" : "Moved")}");

//                        if (finger.Down && activeFinger == null)
//                        {
//                            activeFinger = finger;
//                            touchStartPosition = finger.StartScreenPosition;
//                            touchStartTime = Time.time;
//                            //touchProcessed = true;
//                        }
//                        if (finger == activeFinger && finger.Up)
//                        {
//                            touchEndPosition = finger.LastScreenPosition;
//                            float touchDuration = Time.time - touchStartTime;
//                            float touchDistance = Vector2.Distance(touchStartPosition, touchEndPosition);
//                            //Debug.Log($"持续时间: {touchDuration}, 移动距离: {touchDistance}");

//                            if (touchDistance < swipeThreshold && touchDuration < tapTimeThreshold)
//                            {
//                                if (IsPointerOverUI(activeFinger))
//                                {
//                                    Debug.Log("触摸在UI上，忽略射线检测");
//                                    activeFinger = null;
//                                    //touchProcessed = true;
//                                    return;
//                                }
//                                PerformRaycast(touchEndPosition);

//                            }
//                            else
//                            {
//                                Debug.Log("滑动操作，忽略射线检测");
//                            }
//                            //touchProcessed = true;

//                            activeFinger = null;
//                        }
//                    }
//                    else
//                    {
//                        //Debug.Log("More than 2 fingers detected, resetting positions.");
//                        touchStartPosition = Vector2.zero;
//                        touchEndPosition = Vector2.zero;
//                        touchStartTime = 0f;
//                    }

//                    //Debug.Log($"开始时间:{touchStartTime}, 开始位置：{touchStartPosition}, 结束位置：{touchEndPosition}");
//#endif

//#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
//        //Debug.Log($"当前触摸点数量: {LeanTouch.Fingers.Count}");
//        if (LeanTouch.Fingers.Count == 1)
//        {
//            var finger = LeanTouch.Fingers[0];
//            //Debug.Log($"状态: {(finger.Down ? "Began" : finger.Up ? "Ended" : "Moved")}");

//            if (finger.Down && activeFinger == null)
//            {
//                activeFinger = finger;
//                touchStartPosition = finger.StartScreenPosition;
//                touchStartTime = Time.time;
//                //touchProcessed = true;
//            }
//            if (finger == activeFinger && finger.Up)
//            {
//                touchEndPosition = finger.LastScreenPosition;
//                float touchDuration = Time.time - touchStartTime;
//                float touchDistance = Vector2.Distance(touchStartPosition, touchEndPosition);
//                //Debug.Log($"持续时间: {touchDuration}, 移动距离: {touchDistance}");

//                if (touchDistance < swipeThreshold && touchDuration < tapTimeThreshold)
//                {
//                    if (IsPointerOverUI(activeFinger))
//                    {
//                        Debug.Log("触摸在UI上，忽略射线检测");
//                        activeFinger = null;
//                        //touchProcessed = true;
//                        return;
//                    }
//                    PerformRaycast(touchEndPosition);
//                }
//                else
//                {
//                    Debug.Log("滑动操作，忽略射线检测");
//                }
//                //touchProcessed = true;

//                activeFinger = null;
//            }
//        }
//        else
//        {
//            //Debug.Log("More than 2 fingers detected, resetting positions！！！");
//            touchStartPosition = Vector2.zero;
//            touchEndPosition = Vector2.zero;
//            touchStartTime = 0f;
//        }
//#endif
//                }

//                public bool IsPointerOverUI(LeanFinger finger)
//                {
//                    if (LeanTouch.PointOverGui(finger.ScreenPosition))
//                        return true;

//                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
//                        return true;

//                    return false;
//                }
                

//                private void FixedUpdate()
//                {
//                    update_layer_mask();

//                }
//                public void update_layer_mask()
//                {
//                    if (Global_Home_Room_Manager._instance != null && Global_Home_Room_Manager._instance._can_switch_scene)
//                    {
//                        targetLayers = normalTargetLayers;
//                    }
//                    else
//                    {
//                        targetLayers = editTargetLayers;
//                    }

//                    if (Indoor_Room_Game_Manager._activc_instance != null && Indoor_Room_Game_Manager._activc_instance._current_state=="Moving_Placement")
//                    {
//                        targetLayers = editTargetLayers;
//                    }
                  
//                }

//                private void PerformRaycast(Vector2 screenPos)
//                {
//                    Ray ray = mainCamera.ScreenPointToRay(screenPos);

//                    RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, targetLayers);

//                    System.Array.Sort(hits, (hit1, hit2) => hit1.distance.CompareTo(hit2.distance));

//                    if (hits.Length == 0)
//                    {
//                        Debug.Log("射线未击中任何对象");
//                        return;
//                    }

//                    foreach (var hit in hits)
//                    {
//                        GameObject obj = hit.collider.gameObject;

//                        if ((UILayers.value & (1 << obj.layer)) != 0)
//                        {
//                            if (obj.GetComponent<Button>() != null)
//                            {
//                                obj.GetComponent<Button>().onClick.Invoke();
//                                Debug.Log($"触发 {obj.name} 的 Button 点击事件");
//                                return;
//                            }
//                        }
//                        //Debug.Log($"射线击中: {hit.collider.gameObject.name} at distance {hit.distance}");
//                    }


//                    foreach (var hit in hits)
//                    {
//                        GameObject obj = hit.collider.gameObject;

//                        // 检查标签筛选
//                        if (targetTags.Count > 0 && !targetTags.Contains(obj.tag))
//                        {
//                            Debug.Log($" {obj.name} 不包含所需标签，已忽略");
//                            return;
//                        }

//                        // 检查组件筛选
//                        if (requiredComponentNames.Count > 0 && !HasRequiredComponents(obj))
//                        {
//                            Debug.Log($" {obj.name} 不包含所需组件，已忽略");
//                            return;
//                        }

//                        if (obj.GetComponent<Button>() != null)
//                        {
//                            obj.GetComponent<Button>().onClick.Invoke();
//                            Debug.Log($"触发 {obj.name} 的 Button 点击事件");
//                            return;
//                        }

//                        if(obj.TryGetComponent<IndoorMainCharacter>(out var character))
//                        {
//                            PopManager.Instance.Pop();
//                        }
//                        // 触发事件
//                        TriggerEventTrigger(obj, EventTriggerType.PointerClick);
//                        Debug.Log($"触发 {obj.name}");
//                        return;
//                    }

//                    Debug.Log("射线未击中任何符合条件的对象");
//                }

//                private bool HasRequiredComponents(GameObject obj)
//                {
//                    foreach (var compName in requiredComponentNames)
//                    {
//                        if (obj.GetComponent(compName) == null)
//                        {
//                            return false;
//                        }
//                    }
//                    return true;
//                }

//                void TriggerEventTrigger(GameObject obj, EventTriggerType type)
//                {
//                    var eventTrigger = obj.GetComponent<EventTrigger>();
//                    if (eventTrigger != null)
//                    {
//                        foreach (var entry in eventTrigger.triggers)
//                        {
//                            if (entry.eventID == type)
//                            {
//                                entry.callback.Invoke(new BaseEventData(EventSystem.current));
//                            }
//                        }
//                    }
//                }

//                public void DebugRaycast()
//                {
//                    Debug.Log("Debug Raycast Triggered");
//                }

//                public GameObject UpdateHitObject(Vector2 screenPos, out RaycastHit hitInfo)
//                {
//                    //Debug.Log("更新hitGo");  
//                    Ray ray = mainCamera.ScreenPointToRay(screenPos);

//                    RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, targetLayers);

//                    System.Array.Sort(hits, (hit1, hit2) => hit1.distance.CompareTo(hit2.distance));

//                    hitInfo = new RaycastHit();

//                    if (hits.Length == 0)
//                    {
//                        //Debug.Log("射线未击中任何对象");
//                        hitGo = null;
//                        return null;
//                    }


//                    foreach (var hit in hits)
//                    {
//                        GameObject obj = hit.collider.gameObject;

//                        // 检查是否在目标层级
//                        if ((targetLayers.value & (1 << obj.layer)) != 0)
//                        {
//                            // 记录第一个符合条件的物体
//                            hitGo = obj;
//                            hitInfo = hit;
//                            // 只记录第一个物体，立即退出循环
//                            return hitGo;
//                        }
//                    }

//                    // 如果没有检测到符合条件的物体，清空 hitGo
//                    hitGo = null;
//                    return null;
//                }

//                public GameObject PerformRaycast(Vector3 screenOrWorldPos, string tag, LayerMask layerMask, Vector3 orient, out RaycastHit hitInfo)
//                {
//                    Ray ray;
//                    if (orient == Vector3.zero)
//                    {
//                        ray = mainCamera.ScreenPointToRay(new Vector2(screenOrWorldPos.x, screenOrWorldPos.y));
//                    }
//                    else
//                    {
//                        ray = new Ray(screenOrWorldPos, orient);
//                    }

//                    RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);

//                    System.Array.Sort(hits, (hit1, hit2) => hit1.distance.CompareTo(hit2.distance));

//                    hitInfo = new RaycastHit();

//                    if (hits.Length == 0)
//                    {
//                        Debug.Log("射线未击中任何对象");
//                        return null;
//                    }

//                    foreach (var hit in hits)
//                    {
//                        GameObject obj = hit.collider.gameObject;

//                        // 检查标签筛选
//                        if (obj.tag != tag)
//                        {
//                            Debug.Log($" {obj.name} 不包含所需标签，已忽略");
//                            continue;
//                        }
//                        hitInfo = hit;
//                        return obj;
//                    }

//                    Debug.Log("射线未击中任何符合条件的对象");
//                    return null;
//                }
//            }

//        }
//    }
//}