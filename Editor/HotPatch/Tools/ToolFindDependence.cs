using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameCoreBuild
{
    public class ToolFindDependence : Editor
    {

        [MenuItem("Assets/查找依赖显示", false, 10)]
        private static void FindDependencies()
        {
            Debug.Log("查找依赖开始");
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            string[] files = AssetDatabase.GetDependencies(path);
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Equals(path)) { continue; }
                Debug.Log(files[i], AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(files[i]));
            }
            Debug.Log("查找依赖结束");
        }

        [MenuItem("Assets/清理未被依赖的..", false, 0)]
        public static void FindSelectsByDependence()
        {
            Dictionary<string, List<string> > selectDependence = new Dictionary<string, List<string>>();

            UnityEngine.Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
            foreach (UnityEngine.Object obj in objs)
            //foreach (var guid in Selection.assetGUIDs) 
            {
                DefaultAsset defaultAsset = obj as DefaultAsset;

                if (defaultAsset == null) 
                { 
                    long localId;
                    string guid;
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out guid, out localId);
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    selectDependence.Add(assetPath, new List<string>());
                }
            }

            //EditorUtility.ClearProgressBar();
            //return;
            
            string[] guidAllTarget = AssetDatabase.FindAssets("t:Prefab t:Material t:Scene");
            for(int i = 0; i < guidAllTarget.Length; i++)
            {
                string      guid = guidAllTarget[i];
                string      assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string[]    dependAssetPaths = AssetDatabase.GetDependencies(assetPath);

                if(EditorUtility.DisplayCancelableProgressBar("被依赖查找中", assetPath, (float)(i +1) / guidAllTarget.Length) )
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }

                foreach(string dependAssetPath in dependAssetPaths) 
                {
                    if(assetPath != dependAssetPath)
                    {
                        List<string> list;
                        if (selectDependence.TryGetValue(dependAssetPath, out list))
                        {
                            list.Add(assetPath);
                        }
                    }
                }
            }


            //删除没有依赖的-
            foreach(string key in selectDependence.Keys)
            {
                List<string>    dList = selectDependence[key];
                if(dList.Count < 1)
                {
                    FileUtil.DeleteFileOrDirectory(key);
                    FileUtil.DeleteFileOrDirectory(key + ".meta");
                }
            }


            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("清理未被依赖的", "检查完成", "确定");
        }
    }
}
