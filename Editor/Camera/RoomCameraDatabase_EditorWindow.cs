#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RoomCameraDatabase_EditorWindow : EditorWindow
{
    private RoomCameraDatabase roomCameraDb;

    private const string ROOM_CAMERA_DB_KEY = "ROOM_CAMERA_DB_KEY";

    [MenuItem("Tools/Camera/Open Camera Database Editor")]
    public static void Open()
    {
        var window = GetWindow<RoomCameraDatabase_EditorWindow>();
        window.titleContent = new GUIContent("Room Camera DB");
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(5);
        EditorGUI.BeginChangeCheck();

        roomCameraDb = (RoomCameraDatabase)EditorGUILayout.ObjectField(
            "Database",
            roomCameraDb,
            typeof(RoomCameraDatabase),
            false);

        if (roomCameraDb == null)
        {
            EditorGUILayout.HelpBox("Please assign RoomCameraDatabase", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(10);

        DrawBakeSection();
        DrawResetSection();
        DrawClearSection();

        if (EditorGUI.EndChangeCheck())
        {
            SaveData(); // 一改就存
        }
    }

    private void OnEnable()
    {
        LoadData();
    }


    private void OnDisable()
    {
        SaveData();
    }


    // ================================
    // Bake
    // ================================
    private void DrawBakeSection()
    {
        EditorGUILayout.LabelField("Bake", EditorStyles.boldLabel);

        if (GUILayout.Button("Bake From Scene"))
        {
            Selection.activeObject = roomCameraDb;
            Bake();
        }
    }

    public  void Bake()
    {
        if (roomCameraDb == null)
        {
            Debug.LogError("Please select RoomCameraDatabase");
            return;
        }

        var tags = GameObject.FindObjectsOfType<RoomCameraTag>(true);

        foreach (var t in tags)
        {
            var roomData = roomCameraDb.rooms.FirstOrDefault(r => r.roomName == t.roomName);
            if (roomData == null)
            {
                roomData = new RoomCameraData(t.roomName);
                roomCameraDb.rooms.Add(roomData);
            }

            var view = roomData.GetView(t.cameraState);

            Undo.RecordObject(roomCameraDb, "Bake Camera");
            if (t.isPivot)
            {
                view.pivotEuler = t.transform.eulerAngles;
                view.pivotOffset = t.transform.position;
            }
            else
            {
                view.positionOffset = t.transform.position;
                view.euler = t.transform.eulerAngles;
            }
            EditorUtility.SetDirty(roomCameraDb);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("✅ Camera Bake Complete");
    }

    // ================================
    // Reset
    // ================================
    private void DrawResetSection()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Reset", EditorStyles.boldLabel);

        if (GUILayout.Button("Reset Offsets"))
        {
            if (!Confirm("Reset all offsets to zero?")) return;

            Undo.RecordObject(roomCameraDb, "Reset Camera Offsets");
            roomCameraDb.ResetOffsets();
            EditorUtility.SetDirty(roomCameraDb);
            AssetDatabase.SaveAssets();

            Debug.Log("✅ Offsets Reset");
        }
    }

    // ================================
    // Clear
    // ================================
    private void DrawClearSection()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Danger Zone", EditorStyles.boldLabel);

        GUI.backgroundColor = Color.red;

        if (GUILayout.Button("CLEAR ALL CAMERA DATA"))
        {
            if (!Confirm("⚠ This will CLEAR ALL camera data. Continue?")) return;

            Undo.RecordObject(roomCameraDb, "Clear Camera Data");
            roomCameraDb.ClearAllData();
            EditorUtility.SetDirty(roomCameraDb);
            AssetDatabase.SaveAssets();

            Debug.Log("🔥 Camera Data Cleared");
        }

        GUI.backgroundColor = Color.white;
    }

    private bool Confirm(string msg)
    {
        return EditorUtility.DisplayDialog(
            "Confirm",
            msg,
            "Yes",
            "Cancel");
    }



    void LoadData()
    {
        if (EditorPrefs.HasKey(ROOM_CAMERA_DB_KEY))
        {
            string dbPath = EditorPrefs.GetString(ROOM_CAMERA_DB_KEY, "");

            if (!string.IsNullOrEmpty(dbPath))
            {
                roomCameraDb = AssetDatabase.LoadAssetAtPath<RoomCameraDatabase>(dbPath);
            }
        }
    }

    void SaveData()
    {
        if (roomCameraDb == null)
        {
            EditorPrefs.DeleteKey(ROOM_CAMERA_DB_KEY);
            return;
        }

        string dbPath = AssetDatabase.GetAssetPath(roomCameraDb);
        EditorPrefs.SetString(ROOM_CAMERA_DB_KEY, dbPath);
    }
}
#endif