using System.Collections.Generic;
using UnityEngine;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEditor;

namespace CLIP.Project_Mouse.Custom_Tool
{
    [CustomEditor(typeof(Global_Inventory_Manager))]
    public class Global_Inventory_Manager_Editor : Editor
    {
        public Global_Inventory_Manager _instance;

        // 用于在编辑器中存储输入值的变量
        private string itemNameToAdd = "";
        private int itemCountToAdd = 1;

        private void OnEnable()
        {
            _instance = (Global_Inventory_Manager)target;

        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(40);
            GUILayout.Label("修改工具", EditorStyles.boldLabel);
            if (GUILayout.Button("生成所有物品并覆盖到当前物品栏"))
            {
                _instance.Generate_Full_Inventory();
            }
            if (GUILayout.Button("清空所有物品"))
            {
                _instance.ClearAllItems();
            }
            GUILayout.Label("修改物品数量", EditorStyles.label);

            itemNameToAdd = EditorGUILayout.TextField("Item Name", itemNameToAdd);
            itemCountToAdd = EditorGUILayout.IntField("Count", itemCountToAdd);

            if (GUILayout.Button("Add/Remove Item"))
            {
                if (Global_Inventory_Manager.Items != null && !string.IsNullOrEmpty(itemNameToAdd) && itemCountToAdd != 0)
                {
                    List<(string, int)> change_list = new List<(string, int)>();
                    change_list.Add((itemNameToAdd, itemCountToAdd));
                    Global_Inventory_Manager.Change_Items_Count(in change_list);
                    Debug.Log($"操作完成: 对物品 '{itemNameToAdd}' 的数量进行了 '{itemCountToAdd}' 的变更。");
                }
                else
                {
                    Debug.LogWarning("操作失败: 请确保库存不为空，且物品名称和数量已正确填写。");
                }
            }

            GUILayout.Space(20);
            GUILayout.Label("服务器相关", EditorStyles.boldLabel);

            if (GUILayout.Button("从服务器更新物品栏"))
            {
                if (_instance != null)
                {
                    _instance.update_inventory_from_server();
                    Debug.Log($"Global_Inventory_Manager '{_instance.name}' 调用 update_inventory_from_server()");
                }
                else
                {
                    Debug.LogWarning("Global_Inventory_Manager 实例为空，无法调用 update_inventory_from_server()");
                }
            }
            if (GUILayout.Button("发送物品栏到服务器"))
            {
                if (_instance != null)
                {
                    _instance.Send_inventory_to_server();
                    Debug.Log($"Global_Inventory_Manager '{_instance.name}' 调用 Send_inventory_to_server()");
                }
                else
                {
                    Debug.LogWarning("Global_Inventory_Manager 实例为空，无法调用 Send_inventory_to_server()");
                }
            }
            if (GUILayout.Button("从服务器更新商店状态"))
            {
                if (_instance != null)
                {
                    _instance.update_shop_state_from_server();
                    Debug.Log($"Global_Inventory_Manager '{_instance.name}' 调用 update_shop_state_from_server()");
                }
                else
                {
                    Debug.LogWarning("Global_Inventory_Manager 实例为空，无法调用 update_shop_state_from_server()");
                }
            }
            if (GUILayout.Button("发送商店状态到服务器"))
            {
                if (_instance != null)
                {
                    _instance.send_shop_state_to_server();
                    Debug.Log($"Global_Inventory_Manager '{_instance.name}' 调用 send_shop_state_to_server()");
                }
                else
                {
                    Debug.LogWarning("Global_Inventory_Manager 实例为空，无法调用 send_shop_state_to_server()");
                }
            }

        }


    }
}