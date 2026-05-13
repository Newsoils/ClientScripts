//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using CLIP.Project_Mouse.UI;
//using CLIP.Project_Mouse.NewFrame.UI;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class PhotoCameraSetting : MonoBehaviour
//            {
//                public PhotoCameraType cameraType;
//                public GameObject cameraRoot;
//                public string roomName;

//                private void Start()
//                {
//                    gameObject.GetComponent<Button>().onClick.AddListener(OnChoosePhotoCameraType);
//                }

//                public void OnChoosePhotoCameraType()
//                {
//                    //PhotoInteractManager.Instance.choosedPhotoCameraType = gameObject.GetComponent<PhotoCameraSetting>().cameraType;
//                    UIManager.Instance.GetPanel<PhotoCapturePanel>().choosedPhotoCameraType = gameObject.GetComponent<PhotoCameraSetting>().cameraType;
//                    //PhotoInteractManager.Instance.choosedPhotoCameraRoot = cameraRoot;
//                    UIManager.Instance.GetPanel<PhotoCapturePanel>().choosedPhotoCameraRoot = cameraRoot;
//                }
//            }
//        }
//    }
//}
