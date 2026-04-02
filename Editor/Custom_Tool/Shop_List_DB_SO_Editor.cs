using CLIP.Project_Mouse.Game_Play_System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Shop_List_DB_SO))]
class Shop_List_DB_SO_Editor : Editor
{
    public Shop_List_DB_SO _obj;

    void OnEnable()
    {
        _obj = (Shop_List_DB_SO)target;
        // script_object = _obj.gameObject;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(32);
        GUILayout.Label("Editor_Function");

        if (GUILayout.Button("Load_data_from_json"))
        {
            _obj.load_from_json();
        }
        if (GUILayout.Button("Get_daily_shop_list"))
        {
            _obj.get_daily_shop_list();
        }
        if (GUILayout.Button("Get_limited_shop_list"))
        {
            _obj.get_limited_shop_list();
        }

       

    }
}
