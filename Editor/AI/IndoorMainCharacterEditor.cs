using CLIP.Project_Mouse.Game_Play_System;
using UnityEditor;
using UnityEngine;

namespace CLIP.Project_Mouse.EditorInspectors
{
    [CustomEditor(typeof(IndoorMainCharacter))]
    public class IndoorMainCharacterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(8f);
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("与指定家具交互（使用 interactPlacement）", GUILayout.Height(28f)))
                {
                    ((IndoorMainCharacter)target).StartInteractWithSelectedPlacement();
                }
            }
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "运行模式下：在上方指定 interactPlacement，可选填写 interactName，再点此按钮使角色寻路并开始交互。",
                    MessageType.Info);
            }
        }
    }
}
