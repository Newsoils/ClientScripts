using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
using UnityEngine;
using CLIP.Framework_Unity;

#if UNITY_EDITOR
using UnityEditor;


namespace CLIP.Project_Mouse.Game_Play_System
{
    public class CharacterInteractPointTool : MonoBehaviour
    {
        [Header("配置")]
        public Animator animator;
        public GameObject character;
        public InteractPositionSO interactPositionSO;

        [Header("操作面板")]
        public string interactName;
        public string furnitureName;

        private string curAnimationBoolName;
        private List<InteractInfo> infos = new List<InteractInfo>();
        private List<Room_Placement_Info> placement_Infos = new List<Room_Placement_Info>();
        
        private GameObject furnitureInstance;
        private void Start()
        {
            string info = JsonData_Manager.Load_Single_JsonData("project_mouse_tb_interact_info");
            infos = JsonConvert.DeserializeObject<List<InteractInfo>>(info);
            string placement_Info = JsonData_Manager.Load_Single_JsonData("project_mouse_tb_room_placement_info");
            placement_Infos = JsonConvert.DeserializeObject<List<Room_Placement_Info>>(placement_Info);
        }
        public void ChangeInteract()
        {
            string animationName = infos.Find(x => x.interactName == interactName).animationBoolName;
            ChangeAnimation(animationName);
        }
        public void RotatePlayer()
        {
            character.transform.Rotate(0, 90, 0, Space.World);
        }
        private void ChangeAnimation(string newBoolName)
        {
            animator.SetBool(curAnimationBoolName, false);
            curAnimationBoolName = newBoolName;
            animator.SetBool(curAnimationBoolName, true);
        }
        public void GenerateFurniture()
        {
            if (!Application.isPlaying) return; 
            if (furnitureInstance != null)
            {
                Destroy(furnitureInstance.gameObject);
            }
            furnitureInstance = PrefabUtility.InstantiatePrefab(LoadFurniturePrefab(furnitureName), transform) as GameObject;
            DestroyImmediate(furnitureInstance.GetComponent<PlacementRuntime>());
        }
        public void SaveInteractPositionInfo()
        {
            if (furnitureInstance == null) return;

            var rootTf = furnitureInstance.transform.Find("Root");
            if (rootTf == null)
            {
                Debug.LogError("CharacterInteractPointTool: 家具预制体下未找到名为 Root 的子物体，无法保存相对 Root 的交互坐标。");
                return;
            }

            Vector3 position = rootTf.InverseTransformPoint(character.transform.position);
            Quaternion rotation = character.transform.rotation;
            interactPositionSO.AddPositionInfo(furnitureName, interactName, position, rotation);
        }
        public void LoadInteractPositionInfo()
        {
            if (furnitureInstance == null) return;

            var placementData = interactPositionSO.positionInfos.Find(x => x.placementName == furnitureName);
            if(placementData == null)
            {
                Log.Info("当前没有该家具数据");
                return;
            }
            var interactData = placementData.positions.Find(x => x.interactName == interactName);
            if(interactData == null)
            {
                Log.Info("该家具没有该交互的数据");
                return;
            }
            var rootTf = furnitureInstance.transform.Find("Root");
            if (rootTf == null)
            {
                Debug.LogError("CharacterInteractPointTool: 家具预制体下未找到名为 Root 的子物体，无法加载相对 Root 的交互坐标。");
                return;
            }

            Vector3 position = rootTf.TransformPoint(interactData.position);
            Quaternion rotation = interactData.rotation;

            ChangeInteract();
            character.transform.position = position;
            character.transform.rotation = rotation;
        }
        private GameObject LoadFurniturePrefab(string name)
        {
            var placement = placement_Infos.Find(x => x.room_placement_name == name);
            if (placement == null)
            {
                Debug.Log("该家具不存在！");
                return null;
            }
            var obj = Resources.Load<GameObject>(placement.res_url);
            return obj;
        }
    }
}

#endif