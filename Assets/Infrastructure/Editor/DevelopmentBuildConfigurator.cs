using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Worldforge.Infrastructure.Editor
{
    [InitializeOnLoad]
    public static class DevelopmentBuildConfigurator
    {
        private const string StartupScenePath = "Assets/Scenes/WorldforgeDevelopment.unity";
        private const string DevelopmentDefine = "WORLDFORGE_DEVELOPMENT_BUILD";
        private const string DebugToolsDefine = "WORLDFORGE_DEBUG_TOOLS";
        private const string DebugSettingsDirectoryPath = "Assets/Resources";
        private const string DebugSettingsAssetPath = "Assets/Resources/WorldforgeDevelopmentDebugSettings.asset";
        private const string DebugSettingsTypeName = "Worldforge.Infrastructure.Development.DevelopmentDebugSettings";
        private const string SessionKey = "Worldforge.Infrastructure.Editor.DevelopmentBuildConfigurator.Initialized";

        static DevelopmentBuildConfigurator()
        {
            EditorApplication.delayCall += ApplyOnLoad;
        }

        [MenuItem("Tools/Build/Apply Worldforge Development Build Configuration")]
        public static void ApplyFromMenu()
        {
            ApplyConfiguration();
        }

        [MenuItem("Tools/Development/Open Worldforge Debug Settings")]
        public static void OpenDebugSettings()
        {
            var settings = GetOrCreateDebugSettingsAsset();
            if (settings == null)
            {
                Debug.LogError("[Worldforge] [Error] [Development.Build] Could not create the Worldforge debug settings asset.");
                return;
            }

            EditorGUIUtility.PingObject(settings);
            Selection.activeObject = settings;
        }

        private static void ApplyOnLoad()
        {
            if (SessionState.GetBool(SessionKey, false))
            {
                return;
            }

            SessionState.SetBool(SessionKey, true);
            ApplyConfiguration();
        }

        private static void ApplyConfiguration()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(StartupScenePath) == null)
            {
                Debug.LogError(
                    $"[Worldforge] [Error] [Development.Build] Startup scene '{StartupScenePath}' could not be found.");
                return;
            }

            var updatedParts = new List<string>();

            if (EnsureBuildSettings())
            {
                updatedParts.Add("Build Settings");
            }

            if (EnsureStandaloneDefineSymbols())
            {
                updatedParts.Add("scripting define symbols");
            }

            if (EnsureDevelopmentBuildOptions())
            {
                updatedParts.Add("development build options");
            }

            if (EnsureDebugSettingsAsset())
            {
                updatedParts.Add("debug settings asset");
            }

            if (EnsureStandaloneWindows64Target())
            {
                updatedParts.Add("Windows 64-bit build target");
            }

            if (updatedParts.Count == 0)
            {
                Debug.Log("[Worldforge] [Info] [Development.Build] Development build configuration is already up to date.");
                return;
            }

            Debug.Log(
                $"[Worldforge] [Info] [Development.Build] Applied development build configuration: {string.Join(", ", updatedParts)}.");
        }

        private static bool EnsureBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length == 1 && scenes[0].enabled && string.Equals(scenes[0].path, StartupScenePath, StringComparison.Ordinal))
            {
                return false;
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(StartupScenePath, true)
            };

            return true;
        }

        private static bool EnsureStandaloneDefineSymbols()
        {
            var currentSymbols = GetStandaloneDefineSymbols();
            var symbols = new List<string>(
                currentSymbols.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

            var changed = false;
            changed |= AppendDefineIfMissing(symbols, "SENTIS_ANALYTICS_ENABLED");
            changed |= AppendDefineIfMissing(symbols, "APP_UI_EDITOR_ONLY");
            changed |= AppendDefineIfMissing(symbols, DevelopmentDefine);
            changed |= AppendDefineIfMissing(symbols, DebugToolsDefine);

            if (!changed)
            {
                return false;
            }

            SetStandaloneDefineSymbols(string.Join(";", symbols));
            return true;
        }

        private static string GetStandaloneDefineSymbols()
        {
            if (TryGetNamedBuildTargetStandalone(out var namedBuildTarget))
            {
                var method = typeof(PlayerSettings).GetMethod(
                    "GetScriptingDefineSymbols",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { namedBuildTarget.GetType() },
                    null);

                if (method != null)
                {
                    return method.Invoke(null, new[] { namedBuildTarget }) as string ?? string.Empty;
                }
            }

#pragma warning disable CS0618
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
#pragma warning restore CS0618
        }

        private static void SetStandaloneDefineSymbols(string defineSymbols)
        {
            if (TryGetNamedBuildTargetStandalone(out var namedBuildTarget))
            {
                var method = typeof(PlayerSettings).GetMethod(
                    "SetScriptingDefineSymbols",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { namedBuildTarget.GetType(), typeof(string) },
                    null);

                if (method != null)
                {
                    method.Invoke(null, new object[] { namedBuildTarget, defineSymbols });
                    return;
                }
            }

#pragma warning disable CS0618
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defineSymbols);
#pragma warning restore CS0618
        }

        private static bool TryGetNamedBuildTargetStandalone(out object namedBuildTarget)
        {
            var namedBuildTargetType = Type.GetType("UnityEditor.Build.NamedBuildTarget, UnityEditor.CoreModule")
                ?? Type.GetType("UnityEditor.Build.NamedBuildTarget, UnityEditor");
            if (namedBuildTargetType == null)
            {
                namedBuildTarget = null;
                return false;
            }

            var standaloneProperty = namedBuildTargetType.GetProperty(
                "Standalone",
                BindingFlags.Public | BindingFlags.Static);
            if (standaloneProperty == null)
            {
                namedBuildTarget = null;
                return false;
            }

            namedBuildTarget = standaloneProperty.GetValue(null, null);
            return namedBuildTarget != null;
        }

        private static bool AppendDefineIfMissing(List<string> symbols, string define)
        {
            for (var index = 0; index < symbols.Count; index++)
            {
                if (string.Equals(symbols[index].Trim(), define, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            symbols.Add(define);
            return true;
        }

        private static bool EnsureDevelopmentBuildOptions()
        {
            var changed = false;

            if (!EditorUserBuildSettings.development)
            {
                EditorUserBuildSettings.development = true;
                changed = true;
            }

            if (!EditorUserBuildSettings.allowDebugging)
            {
                EditorUserBuildSettings.allowDebugging = true;
                changed = true;
            }

            if (EditorUserBuildSettings.waitForManagedDebugger)
            {
                EditorUserBuildSettings.waitForManagedDebugger = false;
                changed = true;
            }

            if (EditorUserBuildSettings.connectProfiler)
            {
                EditorUserBuildSettings.connectProfiler = false;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureStandaloneWindows64Target()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
            {
                return false;
            }

            return EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Standalone,
                BuildTarget.StandaloneWindows64);
        }

        private static bool EnsureDebugSettingsAsset()
        {
            var existingSettings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(DebugSettingsAssetPath);
            if (existingSettings != null)
            {
                return false;
            }

            var createdSettings = GetOrCreateDebugSettingsAsset();
            return createdSettings != null;
        }

        private static ScriptableObject GetOrCreateDebugSettingsAsset()
        {
            var existingSettings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(DebugSettingsAssetPath);
            if (existingSettings != null)
            {
                return existingSettings;
            }

            var debugSettingsType = FindDebugSettingsType();
            if (debugSettingsType == null || !typeof(ScriptableObject).IsAssignableFrom(debugSettingsType))
            {
                Debug.LogError(
                    $"[Worldforge] [Error] [Development.Build] Could not resolve debug settings type '{DebugSettingsTypeName}'.");
                return null;
            }

            EnsureFolderExists(DebugSettingsDirectoryPath);

            var createdSettings = CreateDebugSettingsInstance(debugSettingsType);
            if (createdSettings == null)
            {
                return null;
            }

            AssetDatabase.CreateAsset(createdSettings, DebugSettingsAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return AssetDatabase.LoadAssetAtPath<ScriptableObject>(DebugSettingsAssetPath);
        }

        private static Type FindDebugSettingsType()
        {
            var directType = Type.GetType(DebugSettingsTypeName);
            if (directType != null)
            {
                return directType;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                if (assembly == null || assembly.IsDynamic)
                {
                    continue;
                }

                var resolvedType = assembly.GetType(DebugSettingsTypeName, false);
                if (resolvedType != null)
                {
                    return resolvedType;
                }
            }

            return null;
        }

        private static ScriptableObject CreateDebugSettingsInstance(Type debugSettingsType)
        {
            var createDefaultMethod = debugSettingsType.GetMethod(
                "CreateDefaultInstance",
                BindingFlags.Public | BindingFlags.Static,
                null,
                Type.EmptyTypes,
                null);

            if (createDefaultMethod != null &&
                createDefaultMethod.Invoke(null, null) is ScriptableObject createdViaFactory)
            {
                return createdViaFactory;
            }

            return ScriptableObject.CreateInstance(debugSettingsType);
        }

        private static void EnsureFolderExists(string assetFolderPath)
        {
            if (AssetDatabase.IsValidFolder(assetFolderPath))
            {
                return;
            }

            var normalizedPath = assetFolderPath.Replace('\\', '/');
            var segments = normalizedPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
            {
                return;
            }

            var currentPath = segments[0];
            for (var i = 1; i < segments.Length; i++)
            {
                var nextPath = $"{currentPath}/{segments[i]}";
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, segments[i]);
                }

                currentPath = nextPath;
            }
        }
    }
}
