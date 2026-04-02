using System;
using System.Collections.Generic;
using System.Linq;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using UnityEditor;
using UnityEngine;

namespace CLIP.Project_Mouse.Custom_Tool
{
    [CustomEditor(typeof(GridSystem))]
    public class GridSystem_Editor : Editor
    {
        public GridSystem _instance;
        private void OnEnable()
        {
            _instance = (GridSystem)target;
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Space(20);
            GUILayout.Label("工具", EditorStyles.boldLabel);
            if (GUILayout.Button("生成房间的网格"))
            {
                GenerateMGridView();
                Debug.Log("已生成所有房间的网格。");
            }

            GUILayout.Space(20);
            if (GUILayout.Button("清除网格"))
            {
                Unity_Tools.ClearAllChildrenImmediate(_instance.gridRoot);
            }

        }

        public void GenerateMGridView()
        {
            _instance.GenerateMGridView(GenerateRoomInfo());
        }

        /// <summary>
        /// 自己生成Roominfo，编辑器调试使用
        /// 正常情况下从RoomSystem获取RoomInfo，RoomSystem在Start的时候生成RoomInfo
        /// </summary>
        /// <returns></returns>
        List<RoomData> GenerateRoomInfo()
        {
            List<RoomData> rooms = new List<RoomData>();
            foreach (var roomType in Enum.GetValues(typeof(RoomType)))
            {
                var type = (RoomType)roomType;
                RoomData room = new RoomData(type);
                var layerDatas = RoomSystem.Instance.Room_SO.sizes.Where(d => d.room_Type == type);
                foreach (var layerData in layerDatas)
                {
                    GridLayer layer = new GridLayer(layerData.id, layerData.layer_Name,layerData.layer_UId,  layerData.layerType, layerData.width, layerData.height);
                    room.GridState.AddLayer(layer);
                }
                rooms.Add(room);
            }
            return rooms;
        }
    }

}