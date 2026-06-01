using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CLIP.Project_Mouse.Custom_Tool
{
    [CustomEditor(typeof(GuidePanel), true)]
    public class GuidePanel_Editor : UnityEditor.Editor
    {
        private const string DefaultAssetPath = "Assets/Resources/Guide/GuideDefinition_SO.asset";

        private GuidePanel guidePanel;

        private void OnEnable()
        {
            guidePanel = (GuidePanel)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10);

            if (GUILayout.Button("Bind All Guide Steps", GUILayout.Height(30)))
            {
                Undo.RecordObject(guidePanel, "Bind All Guide Steps");
                BindSteps();
                EditorUtility.SetDirty(guidePanel);
            }

            if (GUILayout.Button("Bake Guide Definition SO", GUILayout.Height(30)))
            {
                Undo.RecordObject(guidePanel, "Bake Guide Definition SO");
                BindSteps();
                BakeGuideDefinitionSO(DefaultAssetPath);
                EditorUtility.SetDirty(guidePanel);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Hide All Steps", GUILayout.Height(30)))
            {
                Undo.RecordObject(guidePanel, "Hide All Guide Steps");
                guidePanel.HideAllSteps();
                EditorUtility.SetDirty(guidePanel);
            }

            GUILayout.Space(50);

            if (GUILayout.Button("Clear Guide Step Bindings", GUILayout.Height(30)))
            {
                Undo.RecordObject(guidePanel, "Clear Guide Step Bindings");
                guidePanel.dispatchSteps.Clear();
                guidePanel.dispatchReturnSteps.Clear();
                guidePanel.placementSteps.Clear();
                guidePanel.shopSteps.Clear();
                EditorUtility.SetDirty(guidePanel);
            }
        }

        private void BindSteps()
        {
            guidePanel.dispatchSteps.Clear();
            guidePanel.dispatchReturnSteps.Clear();
            guidePanel.placementSteps.Clear();
            guidePanel.shopSteps.Clear();

            AddStepsFromRoot(guidePanel.dispatchGuide, guidePanel.dispatchSteps);
            AddStepsFromRoot(guidePanel.dispatchReturnGuide, guidePanel.dispatchReturnSteps);
            AddStepsFromRoot(guidePanel.placementGuide, guidePanel.placementSteps);
            AddStepsFromRoot(guidePanel.shopGuide, guidePanel.shopSteps);
        }

        private void BakeGuideDefinitionSO(string assetPath)
        {
            EnsureFolder(Path.GetDirectoryName(assetPath)?.Replace("\\", "/"));

            var asset = AssetDatabase.LoadAssetAtPath<GuideDefinition_SO>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<GuideDefinition_SO>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            Undo.RecordObject(asset, "Bake Guide Definition SO");
            asset.guideDefinitions.Clear();
            asset.guideDefinitions.Add(BuildDefinition(GuideId.Dispatch, guidePanel.dispatchSteps));
            asset.guideDefinitions.Add(BuildDefinition(GuideId.DispatchReturn, guidePanel.dispatchReturnSteps));
            asset.guideDefinitions.Add(BuildDefinition(GuideId.MoveFurniture, guidePanel.placementSteps));
            asset.guideDefinitions.Add(BuildDefinition(GuideId.Shop, guidePanel.shopSteps));

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[GuidePanel] Baked guide definitions to {assetPath}");
        }

        private static GuideDefinition BuildDefinition(GuideId guideId, List<GuideStep> steps)
        {
            var definition = new GuideDefinition { Id = guideId };
            if (steps == null)
                return definition;

            foreach (var step in steps)
            {
                if (step != null && !definition.StepIds.Contains(step.StepId))
                    definition.StepIds.Add(step.StepId);
            }

            return definition;
        }

        private static void AddStepsFromRoot(Transform root, List<GuideStep> steps)
        {
            if (root == null)
                return;

            foreach (var step in root.GetComponentsInChildren<GuideStep>(true))
            {
                if (!steps.Contains(step))
                    steps.Add(step);
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || AssetDatabase.IsValidFolder(folderPath))
                return;

            var parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
            var folderName = Path.GetFileName(folderPath);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
