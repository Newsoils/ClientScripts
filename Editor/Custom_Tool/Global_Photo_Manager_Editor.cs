//using CLIP.Project_Mouse.Game_Play_System;
//using UnityEditor;
//using UnityEngine;

//#if UNITY_EDITOR
//namespace CLIP.Project_Mouse.Custom_Tool
//{

//    [CustomEditor(typeof(Global_Photo_Manager))]
//    public class Global_Photo_Manager_Editor : Editor
//    {
//        public Global_Photo_Manager manager;
//        private string photoName = "default_photo.png";

//        public void OnEnable()
//        {
//            manager = (Global_Photo_Manager)target;
//        }
//        public override void OnInspectorGUI()
//        {
//            // 绘制默认的 Inspector 界面
//            //DrawDefaultInspector();

//            //GUILayout.Space(20);
//            //GUILayout.Label("Editor Tools", EditorStyles.boldLabel);

//            //// 创建一个文本框用于输入照片名称
//            //photoName = EditorGUILayout.TextField("Photo Name", photoName);

//            //// 创建上传照片按钮
//            //if (GUILayout.Button("Upload Photo"))
//            //{
//            //    if (manager._photo_to_upload != null && !string.IsNullOrEmpty(photoName))
//            //    {
//            //        // 调用上传方法
//            //        photoName = manager.upload_photo_to_server_and_return_photo_name(manager._photo_to_upload);
//            //        Debug.Log($"发起上传照片请求: {photoName}");
//            //    }
//            //    else
//            //    {
//            //        Debug.LogWarning("请在 Inspector 中指定要上传的 Texture2D (_photo_to_upload) 并提供照片名称。");
//            //    }
//            //}

//            //// 创建下载照片按钮
//            //if (GUILayout.Button("Download Photo"))
//            //{
//            //    if (!string.IsNullOrEmpty(photoName))
//            //    {
//            //        // 调用下载方法
//            //        manager.get_photo_from_server(photoName);
//            //        Debug.Log($"发起下载照片请求: {photoName}");
//            //    }
//            //    else
//            //    {
//            //        Debug.LogWarning("请输入要下载的照片名称。");
//            //    }
//            //}
//        }
//    }

//}
//#endif