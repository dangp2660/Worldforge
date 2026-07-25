using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Worldforge.Infrastructure.Editor
{
    [InitializeOnLoad]
    public static class DevelopmentBuildConfigurator
    {
        private const string StartupScenePath = "Assets/Scenes/WorldforgeDevelopment.unity";
        private const string DevelopmentDefine = "WORLDFORGE_DEVELOPMENT_BUILD";
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
            var currentSymbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            var symbols = new List<string>(
                currentSymbols.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

            var changed = false;
            changed |= AppendDefineIfMissing(symbols, "SENTIS_ANALYTICS_ENABLED");
            changed |= AppendDefineIfMissing(symbols, "APP_UI_EDITOR_ONLY");
            changed |= AppendDefineIfMissing(symbols, DevelopmentDefine);

            if (!changed)
            {
                return false;
            }

            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, string.Join(";", symbols));
            return true;
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
    }
}
