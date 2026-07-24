using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class HdrpToUrpMigrationUtility
{
    private const string UrpRendererDataPath = "Assets/Settings/URP/Worldforge_URP_Renderer.asset";
    private const string UrpPipelineAssetPath = "Assets/Settings/URP/Worldforge_URP.asset";
    private const string UrpVolumeProfilePath = "Assets/Settings/URP/Worldforge_URP_GlobalVolume.asset";
    private const string LegacySkyAndFogProfilePath = "Assets/Settings/SkyandFogSettingsProfile.asset";

    private static readonly string[] HdrpComponentTypeNames =
    {
        "UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData",
        "UnityEngine.Rendering.HighDefinition.HDAdditionalLightData",
        "UnityEngine.Rendering.HighDefinition.StaticLightingSky"
    };

    [MenuItem("Tools/Rendering/Migrate HDRP Project To URP")]
    public static void MigrateProject()
    {
        if (!EnsureUrpTypes(out var urpAssetType, out var urpRendererDataType))
        {
            Debug.LogError("URP package is not available yet. Let Unity finish resolving packages, then run the migration again.");
            return;
        }

        EnsureFolder("Assets/Settings");
        EnsureFolder("Assets/Settings/URP");

        var rendererData = LoadOrCreateAsset(UrpRendererDataPath, urpRendererDataType);
        var pipelineAsset = LoadOrCreateAsset(UrpPipelineAssetPath, urpAssetType);
        var volumeProfile = LoadOrCreateAsset(UrpVolumeProfilePath, typeof(VolumeProfile));

        ConfigurePipelineAsset(pipelineAsset, rendererData);
        ApplyPipelineSettings(pipelineAsset);
        MigrateScenes(volumeProfile);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("HDRP to URP migration prep finished. You can now remove the HDRP package.");
    }

    public static void MigrateProjectBatchMode()
    {
        try
        {
            MigrateProject();
            EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }

    private static bool EnsureUrpTypes(out Type urpAssetType, out Type urpRendererDataType)
    {
        urpAssetType = FindType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset");
        urpRendererDataType = FindType("UnityEngine.Rendering.Universal.UniversalRendererData");
        return urpAssetType != null && urpRendererDataType != null;
    }

    private static void EnsureFolder(string assetPath)
    {
        if (AssetDatabase.IsValidFolder(assetPath))
        {
            return;
        }

        var parent = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
        var folderName = Path.GetFileName(assetPath);
        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
        {
            throw new InvalidOperationException($"Invalid folder path: {assetPath}");
        }

        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folderName);
    }

    private static UnityEngine.Object LoadOrCreateAsset(string assetPath, Type assetType)
    {
        var existing = AssetDatabase.LoadAssetAtPath(assetPath, assetType);
        if (existing != null)
        {
            return existing;
        }

        var instance = ScriptableObject.CreateInstance(assetType);
        instance.name = Path.GetFileNameWithoutExtension(assetPath);
        AssetDatabase.CreateAsset(instance, assetPath);
        return instance;
    }

    private static void ConfigurePipelineAsset(UnityEngine.Object pipelineAsset, UnityEngine.Object rendererData)
    {
        var serializedObject = new SerializedObject(pipelineAsset);

        var rendererList = serializedObject.FindProperty("m_RendererDataList");
        if (rendererList == null)
        {
            throw new MissingFieldException("Could not find m_RendererDataList on the URP pipeline asset.");
        }

        rendererList.arraySize = 1;
        rendererList.GetArrayElementAtIndex(0).objectReferenceValue = rendererData;

        SetIntIfPresent(serializedObject, "m_DefaultRendererIndex", 0);
        SetIntIfPresent(serializedObject, "m_MSAA", 1);
        SetBoolIfPresent(serializedObject, "m_SupportsHDR", false);

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(pipelineAsset);
    }

    private static void ApplyPipelineSettings(UnityEngine.Object pipelineAsset)
    {
        var renderPipelineAsset = pipelineAsset as RenderPipelineAsset;
        if (renderPipelineAsset == null)
        {
            throw new InvalidCastException("URP pipeline asset is not a RenderPipelineAsset.");
        }

        GraphicsSettings.defaultRenderPipeline = renderPipelineAsset;

        var originalQualityLevel = QualitySettings.GetQualityLevel();
        for (var qualityIndex = 0; qualityIndex < QualitySettings.names.Length; qualityIndex++)
        {
            QualitySettings.SetQualityLevel(qualityIndex, false);
            QualitySettings.renderPipeline = renderPipelineAsset;
        }

        QualitySettings.SetQualityLevel(originalQualityLevel, false);
        QualitySettings.renderPipeline = renderPipelineAsset;
    }

    private static void MigrateScenes(UnityEngine.Object replacementVolumeProfile)
    {
        var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        var legacyProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(LegacySkyAndFogProfilePath);

        foreach (var sceneGuid in sceneGuids)
        {
            var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var modified = false;

            foreach (var root in scene.GetRootGameObjects())
            {
                modified |= MigrateGameObjectHierarchy(root, replacementVolumeProfile as VolumeProfile, legacyProfile);
            }

            if (modified)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }
    }

    private static bool MigrateGameObjectHierarchy(GameObject root, VolumeProfile replacementVolumeProfile, VolumeProfile legacyProfile)
    {
        var modified = false;
        foreach (var transform in root.GetComponentsInChildren<Transform>(true))
        {
            var gameObject = transform.gameObject;

            foreach (var hdrpComponentTypeName in HdrpComponentTypeNames)
            {
                var hdrpComponentType = FindType(hdrpComponentTypeName);
                if (hdrpComponentType == null)
                {
                    continue;
                }

                var component = gameObject.GetComponent(hdrpComponentType);
                if (component == null)
                {
                    continue;
                }

                UnityEngine.Object.DestroyImmediate(component, true);
                modified = true;
            }

            var volume = gameObject.GetComponent<Volume>();
            if (volume != null && replacementVolumeProfile != null && volume.sharedProfile == legacyProfile)
            {
                volume.sharedProfile = replacementVolumeProfile;
                EditorUtility.SetDirty(volume);
                modified = true;
            }
        }

        return modified;
    }

    private static void SetBoolIfPresent(SerializedObject serializedObject, string propertyName, bool value)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static void SetIntIfPresent(SerializedObject serializedObject, string propertyName, int value)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.intValue = value;
        }
    }

    private static Type FindType(string fullName)
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(assembly => assembly.GetType(fullName))
            .FirstOrDefault(type => type != null);
    }
}
