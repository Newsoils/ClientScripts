//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.VisualScripting;
//using CLIP.Project_Mouse.Game_Play_System;


//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace Custom_Tool
//        {
//            public class ReplaceGameGridWithPrefab : MonoBehaviour
//            {
//                public GameObject prefab;
//                public Vector3 orient;

//                public void ReplaceChildren()
//                {
//                    if (prefab == null)
//                    {
//                        Debug.LogError("prefab为空");
//                        return;
//                    }

//                    Transform[] children = new Transform[transform.childCount];
//                    for (int i = 0; i < transform.childCount; i++)
//                    {
//                        children[i] = transform.GetChild(i);
//                    }

//                    int index = 0;
//                    foreach (Transform child in children)
//                    {
//                        Vector3 originalPosition = child.position;
//                        Quaternion originalRotation = child.rotation;
//                        Vector3 originalScale = child.localScale;

//                        DestroyImmediate(child.gameObject);

//#if UNITY_EDITOR
//                        // 使用 PrefabUtility 实例化预制体并链接到原始预制体
//                        GameObject newChild = (GameObject)PrefabUtility.InstantiatePrefab(prefab, transform);
//#else
//                        GameObject newChild = Instantiate(prefab, transform);
//#endif
//                        newChild.transform.SetPositionAndRotation(originalPosition, originalRotation);
//                        newChild.transform.localScale = originalScale;

//                        newChild.name = prefab.name + $" ({index})";
//                        Game_Grid_Cell game_Grid_Cell = newChild.GetComponent<Game_Grid_Cell>();
//                        //game_Grid_Cell.cell_orient = orient;
//                        index++;
//                    }

//                    Debug.Log("所有子物体已成功替换为预制体！");
//                }
//            }
//        }
//    }
//}
         
