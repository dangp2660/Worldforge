using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Worldforge.Interaction;

namespace Worldforge.Interaction.Editor
{
    public static class InteractionSetupUtility
    {
        private const string AssetPath = "Assets/Resources/InteractionConfiguration.asset";

        [MenuItem("Worldforge/Setup/Setup Interaction System")]
        public static void SetupInteractionSystem()
        {
            CreateOrUpdateInteractionConfiguration();
            Debug.Log(
                "[Worldforge] Interaction system setup completed successfully.\n" +
                "1. InteractionConfiguration asset initialized at Assets/Resources/InteractionConfiguration.asset\n" +
                "Ready to run in Play mode.");
        }

        [MenuItem("Worldforge/Setup/Create Sample Interactable Object")]
        public static void CreateSampleInteractable()
        {
            var interactableObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interactableObject.name = "Sample_Interactable_ResourceNode";
            interactableObject.transform.position = new Vector3(0f, 0.5f, 2f);

            var collider = interactableObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = false;
            }

            var interactable = interactableObject.AddComponent<InteractableBehaviour>();
            var serializedObject = new SerializedObject(interactable);
            serializedObject.FindProperty("_interactionType").enumValueIndex = (int)InteractionType.Gather;
            serializedObject.FindProperty("_interactionPrompt").stringValue = "Press E to Gather Resource";
            serializedObject.FindProperty("_interactionDuration").floatValue = 1.5f;
            serializedObject.FindProperty("_isInteractable").boolValue = true;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = interactableObject;
            EditorUtility.SetDirty(interactableObject);

            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(interactableObject.scene);
            }

            Debug.Log(
                "[Worldforge] Created Sample Interactable Object in Scene at (0, 0.5, 2).\n" +
                "Type: Gather, Prompt: 'Press E to Gather Resource', Duration: 1.5s.");
        }

        private static void CreateOrUpdateInteractionConfiguration()
        {
            var configuration = ScriptableObject.CreateInstance<InteractionConfiguration>();

            var serializedObject = new SerializedObject(configuration);
            serializedObject.FindProperty("_maxDetectionDistance").floatValue = 3.5f;

            var layerMaskProp = serializedObject.FindProperty("_detectionLayerMask");
            if (layerMaskProp != null)
            {
                var bitsProp = layerMaskProp.FindPropertyRelative("m_Bits");
                if (bitsProp != null)
                {
                    bitsProp.intValue = ~0;
                }
                else
                {
                    layerMaskProp.intValue = ~0;
                }
            }

            serializedObject.FindProperty("_maxDetectionResults").intValue = 10;
            serializedObject.FindProperty("_detectionInterval").floatValue = 0.05f;
            serializedObject.FindProperty("_interactionCooldown").floatValue = 0.3f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            var existing = AssetDatabase.LoadAssetAtPath<InteractionConfiguration>(AssetPath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(configuration, existing);
                EditorUtility.SetDirty(existing);
            }
            else
            {
                AssetDatabase.CreateAsset(configuration, AssetPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var loadedAsset = AssetDatabase.LoadAssetAtPath<InteractionConfiguration>(AssetPath);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = loadedAsset;
        }
    }
}
