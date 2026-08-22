using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Worldforge.Character.Traversal;

namespace Worldforge.Character.Editor
{
    public static class TraversalSetupUtility
    {
        private const string GeneratorTypeName =
            "Worldforge.Infrastructure.Development.TraversalSurfaceGenerator, Infrastructure";

        [MenuItem("Worldforge/Setup/Setup Traversal System")]
        public static void SetupTraversalSystem()
        {
            CreateOrUpdateTraversalConfiguration();
            AttachSurfaceGeneratorToScene();

            Debug.Log(
                "[Worldforge] Traversal system setup completed successfully.\n" +
                "1. TraversalConfiguration asset initialized in Assets/Resources/TraversalConfiguration.asset\n" +
                "2. TraversalSurfaceGenerator configured on 'Environment Root'\n" +
                "Ready to run in Play mode.");
        }

        private static void CreateOrUpdateTraversalConfiguration()
        {
            var configuration = ScriptableObject.CreateInstance<TraversalConfiguration>();

            var defaultRules = new[]
            {
                new SurfaceTraversalRule(SurfaceType.Grass, true, 0.9f, -1f),
                new SurfaceTraversalRule(SurfaceType.Mud, true, 0.5f, -1f),
                new SurfaceTraversalRule(SurfaceType.Ice, true, 1.1f, -1f),
                new SurfaceTraversalRule(SurfaceType.Lava, false, 0f, -1f),
                new SurfaceTraversalRule(SurfaceType.Sand, true, 0.7f, 30f)
            };

            var serializedObject = new SerializedObject(configuration);

            serializedObject.FindProperty("_defaultMaxSlopeAngle").floatValue = 45f;
            serializedObject.FindProperty("_slopeSpeedReductionEnabled").boolValue = true;
            serializedObject.FindProperty("_defaultSurfaceSpeedMultiplier").floatValue = 1f;
            serializedObject.FindProperty("_defaultSurfaceTraversable").boolValue = true;

            var rulesProperty = serializedObject.FindProperty("_surfaceRules");
            rulesProperty.arraySize = defaultRules.Length;

            for (var i = 0; i < defaultRules.Length; i++)
            {
                var rule = defaultRules[i];
                var element = rulesProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("_surfaceType").enumValueIndex = (int)rule.SurfaceType;
                element.FindPropertyRelative("_isTraversable").boolValue = rule.IsTraversable;
                element.FindPropertyRelative("_speedMultiplier").floatValue = rule.SpeedMultiplier;
                element.FindPropertyRelative("_maxSlopeOverride").floatValue = rule.MaxSlopeOverride;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            var assetPath = "Assets/Resources/TraversalConfiguration.asset";

            var existing = AssetDatabase.LoadAssetAtPath<TraversalConfiguration>(assetPath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(configuration, existing);
                EditorUtility.SetDirty(existing);
            }
            else
            {
                AssetDatabase.CreateAsset(configuration, assetPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var loadedAsset = AssetDatabase.LoadAssetAtPath<TraversalConfiguration>(assetPath);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = loadedAsset;
        }

        private static void AttachSurfaceGeneratorToScene()
        {
            var environmentRoot = GameObject.Find("Environment Root");

            if (environmentRoot == null)
            {
                return;
            }

            var generatorType = Type.GetType(GeneratorTypeName);

            if (generatorType == null)
            {
                return;
            }

            var existing = environmentRoot.GetComponent(generatorType);

            if (existing == null)
            {
                var generator = environmentRoot.AddComponent(generatorType);

                var serializedObject = new SerializedObject(generator);
                var envRootProp = serializedObject.FindProperty("environmentRoot");

                if (envRootProp != null)
                {
                    envRootProp.objectReferenceValue = environmentRoot.transform;
                    serializedObject.ApplyModifiedProperties();
                }
            }

            EditorUtility.SetDirty(environmentRoot);
            EditorSceneManager.MarkSceneDirty(environmentRoot.scene);
        }
    }
}
