#if UNITY_EDITOR || WORLDFORGE_DEVELOPMENT_BUILD || WORLDFORGE_DEBUG_TOOLS
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Worldforge.Infrastructure.Development.MethodTester
{
    public sealed class MethodTesterGUI
    {
        private static MethodTesterGUI instance;

        public static MethodTesterGUI Instance => instance ??= new MethodTesterGUI();

        private Rect windowRect = new Rect(60f, 40f, 960f, 620f);
        private Vector2 sidebarScroll;
        private Vector2 paramsScroll;
        private Vector2 outputScroll;
        private int selectedOutputTab = 0; // 0 = Debug Logs, 1 = Return Value

        // GUI Styles
        private GUIStyle windowStyle;
        private GUIStyle headerStyle;
        private GUIStyle sidebarStyle;
        private GUIStyle methodItemStyle;
        private GUIStyle selectedMethodItemStyle;
        private GUIStyle primaryMethodItemStyle;
        private GUIStyle selectedPrimaryMethodItemStyle;
        private GUIStyle sectionHeaderStyle;
        private GUIStyle consoleBoxStyle;
        private GUIStyle executeButtonStyle;
        private GUIStyle signatureStyle;
        private GUIStyle logEntryStyle;
        private GUIStyle logWarnStyle;
        private GUIStyle logErrorStyle;

        private Texture2D darkBgTex;
        private Texture2D sidebarBgTex;
        private Texture2D consoleBgTex;
        private Texture2D selectedBgTex;
        private Texture2D primaryBgTex;
        private Texture2D executeBtnTex;

        public void DrawGUI()
        {
            var manager = MethodTesterManager.Instance;
            if (!manager.IsWindowOpen)
            {
                return;
            }

            InitStyles();

            // Clamp window within screen
            var minWidth = Mathf.Min(960f, Screen.width - 40f);
            var minHeight = Mathf.Min(640f, Screen.height - 40f);
            windowRect.width = Mathf.Max(windowRect.width, minWidth);
            windowRect.height = Mathf.Max(windowRect.height, minHeight);
            windowRect.x = Mathf.Clamp(windowRect.x, 10f, Screen.width - windowRect.width - 10f);
            windowRect.y = Mathf.Clamp(windowRect.y, 10f, Screen.height - windowRect.height - 10f);

            windowRect = GUI.Window(984321, windowRect, DrawWindowContent, string.Empty, windowStyle);
        }

        private void InitStyles()
        {
            if (windowStyle != null)
            {
                return;
            }

            darkBgTex = MakeColorTex(new Color(0.12f, 0.13f, 0.15f, 0.98f));
            sidebarBgTex = MakeColorTex(new Color(0.16f, 0.17f, 0.20f, 0.98f));
            consoleBgTex = MakeColorTex(new Color(0.08f, 0.08f, 0.10f, 1f));
            selectedBgTex = MakeColorTex(new Color(0.24f, 0.38f, 0.60f, 0.9f));
            primaryBgTex = MakeColorTex(new Color(0.45f, 0.35f, 0.12f, 0.7f));
            executeBtnTex = MakeColorTex(new Color(0.18f, 0.58f, 0.34f, 1f));

            windowStyle = new GUIStyle(GUI.skin.window)
            {
                normal = { background = darkBgTex },
                padding = new RectOffset(8, 8, 8, 8)
            };

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.95f, 0.95f) }
            };

            sidebarStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = sidebarBgTex },
                padding = new RectOffset(6, 6, 6, 6)
            };

            methodItemStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 11,
                normal = { textColor = new Color(0.85f, 0.85f, 0.85f) },
                padding = new RectOffset(8, 6, 4, 4)
            };

            selectedMethodItemStyle = new GUIStyle(methodItemStyle)
            {
                normal = { background = selectedBgTex, textColor = Color.white },
                fontStyle = FontStyle.Bold
            };

            primaryMethodItemStyle = new GUIStyle(methodItemStyle)
            {
                normal = { background = primaryBgTex, textColor = new Color(1f, 0.88f, 0.45f) },
                fontStyle = FontStyle.Bold
            };

            selectedPrimaryMethodItemStyle = new GUIStyle(primaryMethodItemStyle)
            {
                normal = { background = selectedBgTex, textColor = Color.white }
            };

            sectionHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.7f, 0.85f, 1f) }
            };

            consoleBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = consoleBgTex },
                padding = new RectOffset(8, 8, 8, 8)
            };

            executeButtonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { background = executeBtnTex, textColor = Color.white },
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                fixedHeight = 32
            };

            signatureStyle = new GUIStyle(GUI.skin.textArea)
            {
                normal = { textColor = new Color(0.75f, 0.92f, 0.75f) },
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };

            logEntryStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                wordWrap = true,
                richText = true,
                normal = { textColor = new Color(0.85f, 0.85f, 0.85f) }
            };

            logWarnStyle = new GUIStyle(logEntryStyle)
            {
                normal = { textColor = new Color(1f, 0.85f, 0.35f) }
            };

            logErrorStyle = new GUIStyle(logEntryStyle)
            {
                normal = { textColor = new Color(1f, 0.45f, 0.45f) },
                fontStyle = FontStyle.Bold
            };
        }

        private void DrawWindowContent(int windowId)
        {
            var manager = MethodTesterManager.Instance;

            // Top Header Bar
            GUILayout.BeginHorizontal();
            GUILayout.Label("⚡ WORLDFORGE METHOD TESTER (In-Game API Runner)", headerStyle);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("🔄 Refresh", GUILayout.Width(80), GUILayout.Height(22)))
            {
                manager.RefreshServices();
            }

            if (GUILayout.Button("✕ Close", GUILayout.Width(70), GUILayout.Height(22)))
            {
                manager.IsWindowOpen = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // Two Column Split
            GUILayout.BeginHorizontal();

            // LEFT SIDEBAR: Services & Methods List (~300px)
            DrawSidebar(manager);

            GUILayout.Space(8);

            // RIGHT PANEL: Request Parameters & Response Console
            DrawMainPanel(manager);

            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0, 0, windowRect.width - 160, 30));
        }

        private void DrawSidebar(MethodTesterManager manager)
        {
            GUILayout.BeginVertical(sidebarStyle, GUILayout.Width(300), GUILayout.ExpandHeight(true));

            // Search Bar & Filter
            GUILayout.BeginHorizontal();
            GUILayout.Label("🔍", GUILayout.Width(20));
            manager.SearchText = GUILayout.TextField(manager.SearchText, GUILayout.ExpandWidth(true));
            if (!string.IsNullOrEmpty(manager.SearchText) && GUILayout.Button("x", GUILayout.Width(20)))
            {
                manager.SearchText = string.Empty;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(2);

            // Services Tree View
            sidebarScroll = GUILayout.BeginScrollView(sidebarScroll);

            var filter = manager.SearchText?.Trim().ToLowerInvariant() ?? string.Empty;

            foreach (var service in manager.Services)
            {
                var methodsToDisplay = service.PrimaryMethods;
                if (!string.IsNullOrEmpty(filter))
                {
                    methodsToDisplay = methodsToDisplay
                        .Where(m => m.DisplayName.ToLowerInvariant().Contains(filter) ||
                                    m.Method.Name.ToLowerInvariant().Contains(filter) ||
                                    service.DisplayName.ToLowerInvariant().Contains(filter))
                        .ToList();
                }

                if (methodsToDisplay.Count == 0)
                {
                    continue;
                }

                // Service Foldout Header
                var foldoutIcon = service.IsExpandedInUI ? "▼" : "▶";
                var serviceLabel = $"{foldoutIcon} {service.DisplayName} ({methodsToDisplay.Count})";

                if (GUILayout.Button(serviceLabel, sectionHeaderStyle))
                {
                    service.IsExpandedInUI = !service.IsExpandedInUI;
                }

                if (service.IsExpandedInUI)
                {
                    foreach (var method in methodsToDisplay)
                    {
                        var isSelected = manager.SelectedMethod == method;
                        var prefix = method.IsPrimary ? "⭐ " : "   ";
                        var btnLabel = $"{prefix}{method.DisplayName}";

                        GUIStyle styleToUse;
                        if (method.IsPrimary)
                        {
                            styleToUse = isSelected ? selectedPrimaryMethodItemStyle : primaryMethodItemStyle;
                        }
                        else
                        {
                            styleToUse = isSelected ? selectedMethodItemStyle : methodItemStyle;
                        }

                        if (GUILayout.Button(btnLabel, styleToUse))
                        {
                            manager.SelectedService = service;
                            manager.SelectedMethod = method;
                        }
                    }
                }

                GUILayout.Space(3);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawMainPanel(MethodTesterManager manager)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            var method = manager.SelectedMethod;
            if (method == null)
            {
                GUILayout.Label("Select a method from the left sidebar to begin testing.", sectionHeaderStyle);
                GUILayout.EndVertical();
                return;
            }

            // Top Area: Method Header & Parameter Inputs
            DrawRequestHeader(method);

            GUILayout.Space(6);

            // Parameters Form
            DrawParameterForm(method);

            GUILayout.Space(6);

            // Presets & Execute Button
            DrawPresetAndExecuteBar(manager, method);

            GUILayout.Space(8);

            // Bottom Area: Output & Debug Logs Console
            DrawResponseConsole(manager);

            GUILayout.EndVertical();
        }

        private void DrawRequestHeader(TestMethodDescriptor method)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();

            var tag = method.IsPrimary ? "⭐ [PRIMARY TEST METHOD]" : "[METHOD]";
            GUILayout.Label($"{tag} {method.TargetType?.Name}.{method.DisplayName}", sectionHeaderStyle);
            GUILayout.EndHorizontal();

            // Signature
            GUILayout.TextField(method.SignatureText, signatureStyle);

            if (!string.IsNullOrEmpty(method.Description))
            {
                GUILayout.Label($"<i>Description: {method.Description}</i>", logEntryStyle);
            }
            GUILayout.EndVertical();
        }

        private void DrawParameterForm(TestMethodDescriptor method)
        {
            GUILayout.Label("📥 Request Parameters:", sectionHeaderStyle);

            paramsScroll = GUILayout.BeginScrollView(paramsScroll, GUILayout.Height(130));

            if (method.Parameters.Count == 0)
            {
                GUILayout.Label("  (No parameters required for this method)");
            }
            else
            {
                foreach (var p in method.Parameters)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"<b>{p.Name}</b> ({p.ParameterType.Name}):", GUILayout.Width(200));

                    if (p.Kind == ParameterKind.Bool)
                    {
                        var boolVal = p.CurrentStringValue == "true" || p.CurrentStringValue == "1";
                        var newBool = GUILayout.Toggle(boolVal, boolVal ? "true" : "false");
                        p.CurrentStringValue = newBool ? "true" : "false";
                    }
                    else if (p.DropdownOptions != null && p.DropdownOptions.Length > 0)
                    {
                        // Dropdown selection (Enum or ScriptableObject assets)
                        var currentIndex = Mathf.Clamp(p.SelectedDropdownIndex, 0, p.DropdownOptions.Length - 1);
                        var currentOption = p.DropdownOptions[currentIndex];

                        if (GUILayout.Button($"▼ {currentOption}", GUILayout.Width(260)))
                        {
                            p.SelectedDropdownIndex = (currentIndex + 1) % p.DropdownOptions.Length;
                        }

                        // Allow manual typing fallback if needed
                        p.CurrentStringValue = GUILayout.TextField(p.CurrentStringValue ?? currentOption, GUILayout.ExpandWidth(true));
                    }
                    else
                    {
                        p.CurrentStringValue = GUILayout.TextField(p.CurrentStringValue ?? string.Empty, GUILayout.ExpandWidth(true));
                    }

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();
        }

        private void DrawPresetAndExecuteBar(MethodTesterManager manager, TestMethodDescriptor method)
        {
            GUILayout.BeginHorizontal();

            // Preset actions
            GUILayout.Label("Preset:", GUILayout.Width(50));
            manager.NewPresetName = GUILayout.TextField(manager.NewPresetName, GUILayout.Width(130));

            if (GUILayout.Button("💾 Save Preset", GUILayout.Width(95)))
            {
                manager.SaveCurrentAsPreset(manager.NewPresetName);
            }

            if (manager.Presets.Count > 0)
            {
                var relevantPresets = manager.Presets.Where(p => p.MethodName == method.Method.Name).ToList();
                if (relevantPresets.Count > 0)
                {
                    if (GUILayout.Button($"Load '{relevantPresets[0].PresetName}'", GUILayout.Width(130)))
                    {
                        manager.ApplyPreset(relevantPresets[0]);
                    }
                }
            }

            GUILayout.FlexibleSpace();

            // Big Send / Execute Button
            if (GUILayout.Button("▶ EXECUTE METHOD", executeButtonStyle, GUILayout.Width(180)))
            {
                manager.ExecuteSelectedMethod();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawResponseConsole(MethodTesterManager manager)
        {
            var report = manager.LastReport;

            GUILayout.BeginHorizontal();
            GUILayout.Label("📤 Response & Execution Output:", sectionHeaderStyle);
            GUILayout.FlexibleSpace();

            if (report != null)
            {
                var timeStr = $"{report.ExecutionTimeMs:F2} ms";
                var statusColor = report.IsSuccess ? "#4ade80" : "#f87171";
                var statusText = report.IsSuccess ? "SUCCESS" : "FAILED";
                GUILayout.Label($"<color={statusColor}><b>[{statusText}]</b></color> Time: {timeStr}  |  Logs: {report.Logs.Count}", logEntryStyle);
            }

            GUILayout.EndHorizontal();

            // Sub-tabs: [Debug Logs] & [Return Value]
            GUILayout.BeginHorizontal();
            var logTabLabel = report != null ? $"📜 Captured Logs ({report.Logs.Count})" : "📜 Captured Logs";
            if (GUILayout.Toggle(selectedOutputTab == 0, logTabLabel, GUI.skin.button, GUILayout.Width(160)))
            {
                selectedOutputTab = 0;
            }

            if (GUILayout.Toggle(selectedOutputTab == 1, "📦 Return Value", GUI.skin.button, GUILayout.Width(140)))
            {
                selectedOutputTab = 1;
            }

            GUILayout.FlexibleSpace();

            if (report != null && GUILayout.Button("📋 Copy to Clipboard", GUILayout.Width(140)))
            {
                var copyContent = selectedOutputTab == 0
                    ? string.Join("\n", report.Logs.Select(l => $"[{l.Type}] {l.Message}"))
                    : report.ReturnFormatted;
                GUIUtility.systemCopyBuffer = copyContent;
            }
            GUILayout.EndHorizontal();

            // Console Box
            GUILayout.BeginVertical(consoleBoxStyle, GUILayout.ExpandHeight(true));
            outputScroll = GUILayout.BeginScrollView(outputScroll);

            if (report == null)
            {
                GUILayout.Label("<color=#888888>// Press [▶ EXECUTE METHOD] to invoke and capture real-time logs & return values.</color>", logEntryStyle);
            }
            else if (selectedOutputTab == 0)
            {
                // Tab: Debug Logs
                if (report.Logs.Count == 0)
                {
                    GUILayout.Label("<color=#888888>// No Debug.Log was emitted during execution.</color>", logEntryStyle);
                }
                else
                {
                    foreach (var log in report.Logs)
                    {
                        var timeStr = log.TimestampUtc.ToLocalTime().ToString("HH:mm:ss.fff");
                        switch (log.Type)
                        {
                            case LogType.Warning:
                                GUILayout.Label($"<color=#fbbf24>[{timeStr}] ⚠️ WARN:</color> {log.Message}", logWarnStyle);
                                break;
                            case LogType.Error:
                            case LogType.Exception:
                                GUILayout.Label($"<color=#f87171>[{timeStr}] ❌ {log.Type.ToString().ToUpper()}:</color> {log.Message}", logErrorStyle);
                                if (!string.IsNullOrEmpty(log.StackTrace))
                                {
                                    GUILayout.Label($"<color=#fca5a5>{log.StackTrace}</color>", logEntryStyle);
                                }
                                break;
                            default:
                                GUILayout.Label($"<color=#4ade80>[{timeStr}] 🟢 LOG:</color> {log.Message}", logEntryStyle);
                                break;
                        }
                    }
                }
            }
            else
            {
                // Tab: Return Value
                GUILayout.TextArea(report.ReturnFormatted ?? "null", logEntryStyle, GUILayout.ExpandHeight(true));
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private static Texture2D MakeColorTex(Color color)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }
    }
}
#endif
