using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Worldforge.Core.Attributes;
using Worldforge.Core.Bootstrap;
using Worldforge.Crafting;
using Worldforge.Infrastructure.Development.MethodTester;
using Worldforge.Inventory;
using Worldforge.Item;
using Debug = UnityEngine.Debug;

namespace Worldforge.Infrastructure.Editor.MethodTester
{
    /// <summary>
    /// Postman-style Editor Window for testing game methods without running Play Mode.
    /// Supports Edit Mode simulation and Play Mode live binding.
    /// </summary>
    public sealed class WorldforgeMethodTesterWindow : EditorWindow
    {
        private Vector2 sidebarScroll;
        private Vector2 paramsScroll;
        private Vector2 outputScroll;
        private Vector2 inventoryScroll;

        private string searchText = string.Empty;
        private int selectedOutputTab = 0; // 0 = Debug Logs, 1 = Return Value, 2 = State Changes

        // Service & Method state
        private List<TestServiceDescriptor> services = new List<TestServiceDescriptor>();
        private TestServiceDescriptor selectedService;
        private TestMethodDescriptor selectedMethod;
        private MethodExecutionReport lastReport;

        // Edit Mode Simulation State
        private ICraftingService editModeCraftingService;
        private IInventoryContainer editModeInventory;
        private ItemDefinition testItemToAdd;
        private int testItemAmount = 10;
        private ItemDefinition[] availableItems;
        private readonly Dictionary<string, int> inventorySnapshotBefore = new Dictionary<string, int>();
        private readonly List<string> stateChangeDiffs = new List<string>();

        // GUI Styles
        private GUIStyle headerStyle;
        private GUIStyle primaryMethodStyle;
        private GUIStyle selectedPrimaryMethodStyle;
        private GUIStyle logEntryStyle;
        private GUIStyle logWarnStyle;
        private GUIStyle logErrorStyle;
        private GUIStyle sectionTitleStyle;
        private GUIStyle resultJsonStyle;

        [MenuItem("Worldforge/Method Tester (Postman for Unity) %&#t", priority = 100)]
        [MenuItem("Window/Worldforge/Method Tester", priority = 100)]
        public static void OpenWindow()
        {
            var window = GetWindow<WorldforgeMethodTesterWindow>("Method Tester", true);
            window.minSize = new Vector2(860f, 580f);
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            InitializeEditModeServices();
            RefreshServices();
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode || change == PlayModeStateChange.EnteredEditMode)
            {
                if (change == PlayModeStateChange.EnteredEditMode)
                {
                    InitializeEditModeServices();
                }
                RefreshServices();
                Repaint();
            }
        }

        private void InitializeEditModeServices()
        {
            // Setup Edit Mode Crafting Service with all loaded recipes
            var craftingService = new RuntimeCraftingService();
            var allRecipes = Resources.LoadAll<RecipeDefinition>("");
            if (allRecipes == null || allRecipes.Length == 0)
            {
                // Fallback via AssetDatabase if not in Resources
                var guids = AssetDatabase.FindAssets("t:RecipeDefinition");
                var recipeList = new List<RecipeDefinition>();
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var recipe = AssetDatabase.LoadAssetAtPath<RecipeDefinition>(path);
                    if (recipe != null)
                    {
                        recipeList.Add(recipe);
                    }
                }
                allRecipes = recipeList.ToArray();
            }

            foreach (var r in allRecipes)
            {
                craftingService.RegisterRecipe(r);
            }
            editModeCraftingService = craftingService;

            // Setup Edit Mode Test Inventory Container
            editModeInventory = new InventoryContainer("EditModeTestInventory", slotCount: 30, maxWeight: 150f);

            // Load available ItemDefinitions
            var allItems = Resources.LoadAll<ItemDefinition>("");
            if (allItems == null || allItems.Length == 0)
            {
                var guids = AssetDatabase.FindAssets("t:ItemDefinition");
                var itemList = new List<ItemDefinition>();
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
                    if (item != null)
                    {
                        itemList.Add(item);
                    }
                }
                allItems = itemList.ToArray();
            }

            availableItems = allItems;
            if (testItemToAdd == null && availableItems.Length > 0)
            {
                testItemToAdd = availableItems[0];
            }
        }

        private void RefreshServices()
        {
            services.Clear();

            if (Application.isPlaying)
            {
                // In Play Mode: Scan runtime bootstrap services & scene
                services = DynamicMethodScanner.ScanAllServices();
            }
            else
            {
                // In Edit Mode: Create mock/service descriptors for domain contracts
                if (editModeCraftingService != null)
                {
                    var craftingDesc = DynamicMethodScanner.CreateServiceDescriptor(
                        typeof(ICraftingService),
                        editModeCraftingService,
                        typeof(ICraftingService).GetCustomAttribute<TestTargetAttribute>(true));
                    if (craftingDesc != null)
                    {
                        services.Add(craftingDesc);
                    }
                }

                if (editModeInventory != null)
                {
                    var invDesc = DynamicMethodScanner.CreateServiceDescriptor(
                        typeof(IInventoryContainer),
                        editModeInventory,
                        typeof(IInventoryContainer).GetCustomAttribute<TestTargetAttribute>(true));
                    if (invDesc != null)
                    {
                        services.Add(invDesc);
                    }
                }
            }

            // Select initial method if null
            if (selectedMethod == null && services.Count > 0)
            {
                selectedService = services[0];
                selectedMethod = selectedService.PrimaryMethods.FirstOrDefault() ?? selectedService.AllMethods.FirstOrDefault();
            }
        }

        private void InitStyles()
        {
            headerStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                normal = { textColor = new Color(0.3f, 0.75f, 1f) }
            };

            sectionTitleStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.9f, 0.9f, 0.9f) }
            };

            primaryMethodStyle ??= new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.85f, 0.35f) }
            };

            selectedPrimaryMethodStyle ??= new GUIStyle(EditorStyles.miniButtonMid)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            logEntryStyle ??= new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                richText = true,
                wordWrap = true
            };

            logWarnStyle ??= new GUIStyle(logEntryStyle)
            {
                normal = { textColor = new Color(1f, 0.8f, 0.2f) }
            };

            logErrorStyle ??= new GUIStyle(logEntryStyle)
            {
                normal = { textColor = new Color(1f, 0.4f, 0.4f) },
                fontStyle = FontStyle.Bold
            };

            resultJsonStyle ??= new GUIStyle(EditorStyles.textArea)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                wordWrap = true
            };
        }

        private void OnGUI()
        {
            InitStyles();

            // Top Toolbar
            DrawTopToolbar();

            EditorGUILayout.Space(4);

            // Main Split
            EditorGUILayout.BeginHorizontal();

            // Left Sidebar (~280px)
            DrawSidebar();

            EditorGUILayout.Space(6);

            // Right Main Panel
            DrawMainPanel();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTopToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var isPlayMode = Application.isPlaying;
            var modeBadge = isPlayMode ? "🟢 PLAY MODE (Live Session)" : "🔵 EDIT MODE (No Run Required)";
            var modeColor = isPlayMode ? Color.cyan : new Color(0.4f, 0.85f, 0.4f);

            var prevColor = GUI.color;
            GUI.color = modeColor;
            GUILayout.Label(modeBadge, EditorStyles.boldLabel, GUILayout.Width(240));
            GUI.color = prevColor;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("🔄 Rescan Services & Recipes", EditorStyles.toolbarButton, GUILayout.Width(180)))
            {
                InitializeEditModeServices();
                RefreshServices();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(280), GUILayout.ExpandHeight(true));

            // Search Bar & Filter
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("🔍", GUILayout.Width(20));
            searchText = EditorGUILayout.TextField(searchText, EditorStyles.toolbarSearchField);
            if (!string.IsNullOrEmpty(searchText) && GUILayout.Button("x", EditorStyles.toolbarButton, GUILayout.Width(18)))
            {
                searchText = string.Empty;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);

            // Tree View
            sidebarScroll = EditorGUILayout.BeginScrollView(sidebarScroll);

            var filter = searchText?.Trim().ToLowerInvariant() ?? string.Empty;

            foreach (var service in services)
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

                var foldoutIcon = service.IsExpandedInUI ? "▼" : "▶";
                var label = $"{foldoutIcon} {service.DisplayName} ({methodsToDisplay.Count})";

                if (GUILayout.Button(label, EditorStyles.boldLabel))
                {
                    service.IsExpandedInUI = !service.IsExpandedInUI;
                }

                if (service.IsExpandedInUI)
                {
                    foreach (var method in methodsToDisplay)
                    {
                        var isSelected = selectedMethod == method;
                        var prefix = method.IsPrimary ? "⭐ " : "   ";
                        var btnText = $"{prefix}{method.DisplayName}";

                        var style = method.IsPrimary
                            ? (isSelected ? selectedPrimaryMethodStyle : primaryMethodStyle)
                            : (isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton);

                        if (GUILayout.Button(btnText, style))
                        {
                            selectedService = service;
                            selectedMethod = method;
                        }
                    }
                }

                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawMainPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (selectedMethod == null)
            {
                EditorGUILayout.HelpBox("Select a method from the left sidebar to start testing.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            // 1. Method Header
            DrawMethodHeader();

            EditorGUILayout.Space(4);

            // 2. Test Inventory Setup Bar (If applicable)
            DrawTestInventorySetup();

            EditorGUILayout.Space(4);

            // 3. Parameters Form
            DrawParametersForm();

            EditorGUILayout.Space(4);

            // 4. Action Bar: Execute Button
            DrawExecuteBar();

            EditorGUILayout.Space(6);

            // 5. Output Console & Logs
            DrawOutputConsole();

            EditorGUILayout.EndVertical();
        }

        private void DrawMethodHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var tag = selectedMethod.IsPrimary ? "⭐ [PRIMARY TEST METHOD]" : "[METHOD]";
            EditorGUILayout.LabelField($"{tag} {selectedMethod.TargetType?.Name}.{selectedMethod.DisplayName}", headerStyle);
            EditorGUILayout.SelectableLabel(selectedMethod.SignatureText, EditorStyles.boldLabel, GUILayout.Height(20));

            if (!string.IsNullOrEmpty(selectedMethod.Description))
            {
                EditorGUILayout.LabelField($"<i>{selectedMethod.Description}</i>", logEntryStyle);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawTestInventorySetup()
        {
            // Only draw if method has IInventoryContainer or we are testing Inventory/Crafting
            var hasInventoryParam = selectedMethod.Parameters.Any(p => typeof(IInventoryContainer).IsAssignableFrom(p.ParameterType)) ||
                                    selectedMethod.TargetType == typeof(IInventoryContainer);

            if (!hasInventoryParam)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            var title = Application.isPlaying
                ? "🎒 Active Player Inventory (Live Game Session):"
                : "🎒 Test Inventory Manager (Edit Mode Virtual Container):";
            EditorGUILayout.LabelField(title, sectionTitleStyle);

            var currentContainer = GetActiveInventoryContainer();
            if (currentContainer != null)
            {
                EditorGUILayout.LabelField($"Total Items: {currentContainer.TotalItemCount} | Weight: {currentContainer.CurrentWeight:F1}/{currentContainer.MaxWeight:F0}kg", EditorStyles.miniLabel, GUILayout.Width(220));
            }
            EditorGUILayout.EndHorizontal();

            // Item SO Selector + Amount + Add Button + Clear Button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Item (SO):", EditorStyles.boldLabel, GUILayout.Width(65));
            testItemToAdd = (ItemDefinition)EditorGUILayout.ObjectField(testItemToAdd, typeof(ItemDefinition), false, GUILayout.Width(220));

            EditorGUILayout.LabelField("Amount:", EditorStyles.boldLabel, GUILayout.Width(55));
            testItemAmount = EditorGUILayout.IntField(testItemAmount, GUILayout.Width(60));
            testItemAmount = Mathf.Max(1, testItemAmount);

            var prevColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.25f, 0.7f, 0.4f, 1f);
            if (GUILayout.Button("➕ Add Item", EditorStyles.miniButton, GUILayout.Width(85)))
            {
                if (testItemToAdd != null && currentContainer != null)
                {
                    currentContainer.AddItem(testItemToAdd, testItemAmount);
                }
            }
            GUI.backgroundColor = prevColor;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("🗑️ Clear Inventory", EditorStyles.miniButtonRight, GUILayout.Width(115)))
            {
                currentContainer?.Clear();
            }
            EditorGUILayout.EndHorizontal();

            // Inventory slots summary table
            if (currentContainer != null)
            {
                var slots = currentContainer.GetSlots();
                var nonEmptySlots = slots.Where(s => !s.IsEmpty).ToList();

                inventoryScroll = EditorGUILayout.BeginScrollView(inventoryScroll, GUILayout.Height(45));
                EditorGUILayout.BeginHorizontal();

                if (nonEmptySlots.Count == 0)
                {
                    EditorGUILayout.LabelField("  <i>Inventory is currently empty. Click buttons above to add test ingredients.</i>", logEntryStyle);
                }
                else
                {
                    foreach (var s in nonEmptySlots)
                    {
                        var name = s.Item != null ? s.Item.DisplayName : "Unknown";
                        GUILayout.Button($"{name} x{s.Quantity}", EditorStyles.helpBox, GUILayout.Width(140), GUILayout.Height(28));
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawParametersForm()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📥 Parameters / Input Payload:", sectionTitleStyle);

            paramsScroll = EditorGUILayout.BeginScrollView(paramsScroll, GUILayout.Height(110));

            if (selectedMethod.Parameters.Count == 0)
            {
                EditorGUILayout.LabelField("  (No parameters required for this method)", EditorStyles.miniLabel);
            }
            else
            {
                foreach (var p in selectedMethod.Parameters)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"<b>{p.Name}</b> ({p.ParameterType.Name}):", logEntryStyle, GUILayout.Width(180));

                    if (typeof(ScriptableObject).IsAssignableFrom(p.ParameterType) || p.AssociatedScriptableObjectType != null)
                    {
                        var soType = p.AssociatedScriptableObjectType ?? p.ParameterType;

                        // Auto-assign first available asset if unassigned
                        if (p.ResolvedValue == null && p.DropdownValues != null && p.DropdownValues.Length > 0 && p.DropdownValues[0] != null)
                        {
                            p.ResolvedValue = p.DropdownValues[0];
                            var initialSo = p.ResolvedValue as ScriptableObject;
                            if (initialSo is RecipeDefinition r) p.CurrentStringValue = r.RecipeCode ?? r.name;
                            else if (initialSo is ItemDefinition i) p.CurrentStringValue = i.ItemCode ?? i.name;
                            else if (initialSo != null) p.CurrentStringValue = initialSo.name;
                        }

                        // Object Field (Drag & Drop from Project or Click Circle Picker)
                        var currentObj = p.ResolvedValue as ScriptableObject;
                        var newObj = EditorGUILayout.ObjectField(currentObj, soType, false, GUILayout.ExpandWidth(true));
                        if (newObj != currentObj)
                        {
                            p.ResolvedValue = newObj;
                            if (newObj is RecipeDefinition r) p.CurrentStringValue = r.RecipeCode ?? r.name;
                            else if (newObj is ItemDefinition i) p.CurrentStringValue = i.ItemCode ?? i.name;
                            else if (newObj != null) p.CurrentStringValue = newObj.name;
                            else p.CurrentStringValue = string.Empty;
                        }
                    }
                    else if (p.ParameterType.IsEnum)
                    {
                        if (Enum.TryParse(p.ParameterType, p.CurrentStringValue, true, out var enumVal))
                        {
                            var newEnum = EditorGUILayout.EnumPopup((Enum)enumVal, GUILayout.ExpandWidth(true));
                            p.CurrentStringValue = newEnum.ToString();
                        }
                        else
                        {
                            p.CurrentStringValue = EditorGUILayout.TextField(p.CurrentStringValue, GUILayout.ExpandWidth(true));
                        }
                    }
                    else if (p.Kind == ParameterKind.Bool)
                    {
                        var boolVal = p.CurrentStringValue == "true" || p.CurrentStringValue == "1";
                        var newBool = EditorGUILayout.Toggle(boolVal, GUILayout.Width(40));
                        p.CurrentStringValue = newBool ? "true" : "false";
                        EditorGUILayout.LabelField(newBool ? "true" : "false", EditorStyles.miniLabel);
                    }
                    else if (p.Kind == ParameterKind.Int)
                    {
                        if (int.TryParse(p.CurrentStringValue, out var intVal))
                        {
                            var newInt = EditorGUILayout.IntField(intVal, GUILayout.ExpandWidth(true));
                            p.CurrentStringValue = newInt.ToString();
                        }
                        else
                        {
                            p.CurrentStringValue = EditorGUILayout.TextField(p.CurrentStringValue ?? "0", GUILayout.ExpandWidth(true));
                        }
                    }
                    else if (p.Kind == ParameterKind.Float)
                    {
                        if (float.TryParse(p.CurrentStringValue, out var floatVal))
                        {
                            var newFloat = EditorGUILayout.FloatField(floatVal, GUILayout.ExpandWidth(true));
                            p.CurrentStringValue = newFloat.ToString();
                        }
                        else
                        {
                            p.CurrentStringValue = EditorGUILayout.TextField(p.CurrentStringValue ?? "0", GUILayout.ExpandWidth(true));
                        }
                    }
                    else if (typeof(IInventoryContainer).IsAssignableFrom(p.ParameterType))
                    {
                        EditorGUILayout.LabelField("[Auto-Injected Test Inventory Container]", EditorStyles.boldLabel);
                    }
                    else
                    {
                        p.CurrentStringValue = EditorGUILayout.TextField(p.CurrentStringValue ?? string.Empty, GUILayout.ExpandWidth(true));
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawExecuteBar()
        {
            EditorGUILayout.BeginHorizontal();

            var isPlayMode = Application.isPlaying;
            var btnText = isPlayMode
                ? "▶ EXECUTE METHOD (Live Game Session)"
                : "▶ EXECUTE METHOD (No Play Mode Needed)";
            var btnColor = isPlayMode
                ? new Color(0.2f, 0.7f, 0.95f, 1f)
                : new Color(0.2f, 0.75f, 0.35f, 1f);

            var prevColor = GUI.backgroundColor;
            GUI.backgroundColor = btnColor;

            if (GUILayout.Button(btnText, GUILayout.Height(30)))
            {
                ExecuteMethodWithTracking();
            }

            GUI.backgroundColor = prevColor;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawOutputConsole()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true));

            // Summary Header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("📤 Response & Execution Output:", sectionTitleStyle);
            GUILayout.FlexibleSpace();

            if (lastReport != null)
            {
                var statusText = lastReport.IsSuccess ? "SUCCESS (200)" : "FAILED (Error/Exception)";
                var statusColor = lastReport.IsSuccess ? "#4ade80" : "#f87171";
                EditorGUILayout.LabelField($"<color={statusColor}><b>[{statusText}]</b></color> Time: {lastReport.ExecutionTimeMs:F2} ms | Logs: {lastReport.Logs.Count}", logEntryStyle, GUILayout.Width(260));
            }
            EditorGUILayout.EndHorizontal();

            // Tabs
            EditorGUILayout.BeginHorizontal();
            var logsCount = lastReport != null ? lastReport.Logs.Count : 0;
            if (GUILayout.Toggle(selectedOutputTab == 0, $"📜 Debug Logs ({logsCount})", EditorStyles.toolbarButton, GUILayout.Width(150)))
            {
                selectedOutputTab = 0;
            }
            if (GUILayout.Toggle(selectedOutputTab == 1, "📦 Return Value", EditorStyles.toolbarButton, GUILayout.Width(130)))
            {
                selectedOutputTab = 1;
            }
            if (GUILayout.Toggle(selectedOutputTab == 2, $"🔄 State Diffs ({stateChangeDiffs.Count})", EditorStyles.toolbarButton, GUILayout.Width(140)))
            {
                selectedOutputTab = 2;
            }

            GUILayout.FlexibleSpace();

            if (lastReport != null && GUILayout.Button("📋 Copy Result", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                var copyContent = selectedOutputTab == 0
                    ? string.Join("\n", lastReport.Logs.Select(l => $"[{l.Type}] {l.Message}"))
                    : (selectedOutputTab == 1 ? lastReport.ReturnFormatted : string.Join("\n", stateChangeDiffs));
                EditorGUIUtility.systemCopyBuffer = copyContent;
            }
            EditorGUILayout.EndHorizontal();

            // Content Scroll
            outputScroll = EditorGUILayout.BeginScrollView(outputScroll);

            if (lastReport == null)
            {
                EditorGUILayout.LabelField("<color=#888888>// Press [▶ EXECUTE METHOD] above to run and inspect live output & Debug.Log.</color>", logEntryStyle);
            }
            else if (selectedOutputTab == 0)
            {
                // Tab: Debug Logs
                if (lastReport.Logs.Count == 0)
                {
                    EditorGUILayout.LabelField("<color=#888888>// No Debug.Log was emitted during execution.</color>", logEntryStyle);
                }
                else
                {
                    foreach (var log in lastReport.Logs)
                    {
                        var timeStr = log.TimestampUtc.ToLocalTime().ToString("HH:mm:ss.fff");
                        switch (log.Type)
                        {
                            case LogType.Warning:
                                EditorGUILayout.LabelField($"<color=#fbbf24>[{timeStr}] ⚠️ WARN:</color> {log.Message}", logWarnStyle);
                                break;
                            case LogType.Error:
                            case LogType.Exception:
                                EditorGUILayout.LabelField($"<color=#f87171>[{timeStr}] ❌ {log.Type.ToString().ToUpper()}:</color> {log.Message}", logErrorStyle);
                                if (!string.IsNullOrEmpty(log.StackTrace))
                                {
                                    EditorGUILayout.LabelField($"<color=#fca5a5>{log.StackTrace}</color>", logEntryStyle);
                                }
                                break;
                            default:
                                EditorGUILayout.LabelField($"<color=#4ade80>[{timeStr}] 🟢 LOG:</color> {log.Message}", logEntryStyle);
                                break;
                        }
                    }
                }
            }
            else if (selectedOutputTab == 1)
            {
                // Tab: Return Value
                EditorGUILayout.TextArea(lastReport.ReturnFormatted ?? "null", resultJsonStyle, GUILayout.ExpandHeight(true));
            }
            else
            {
                // Tab: State Diffs
                if (stateChangeDiffs.Count == 0)
                {
                    EditorGUILayout.LabelField("<color=#888888>// No inventory item delta detected during this call.</color>", logEntryStyle);
                }
                else
                {
                    foreach (var diff in stateChangeDiffs)
                    {
                        EditorGUILayout.LabelField($"<b>{diff}</b>", logEntryStyle);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void ExecuteMethodWithTracking()
        {
            if (selectedMethod == null)
            {
                return;
            }

            // Snapshot inventory before execution
            var container = GetActiveInventoryContainer();
            TakeInventorySnapshot(container);

            // Bind container to any parameter expecting IInventoryContainer
            if (selectedMethod.Parameters != null)
            {
                foreach (var p in selectedMethod.Parameters)
                {
                    if (typeof(IInventoryContainer).IsAssignableFrom(p.ParameterType))
                    {
                        p.ResolvedValue = container;
                    }
                }
            }

            // Ensure target instance is bound in Edit Mode
            if (!Application.isPlaying)
            {
                if (selectedMethod.TargetType == typeof(ICraftingService))
                {
                    selectedMethod.TargetInstance = editModeCraftingService;
                }
                else if (selectedMethod.TargetType == typeof(IInventoryContainer))
                {
                    selectedMethod.TargetInstance = editModeInventory;
                }
            }

            // Execute
            lastReport = DynamicMethodInvoker.Execute(selectedMethod);

            // Compute diffs
            ComputeInventoryDiffs(container);
        }

        private IInventoryContainer GetActiveInventoryContainer()
        {
            if (Application.isPlaying)
            {
                if (BootstrapManager.HasInstance && BootstrapManager.Instance.Services != null)
                {
                    if (BootstrapManager.Instance.Services.TryResolve<IInventoryContainer>(out var liveContainer) && liveContainer != null)
                    {
                        return liveContainer;
                    }
                }

                // Fallback: search for active IInventoryContainer in scene
                var sceneObjects = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude);
                foreach (var obj in sceneObjects)
                {
                    if (obj is IInventoryContainer c)
                    {
                        return c;
                    }
                }
            }

            return editModeInventory;
        }

        private void TakeInventorySnapshot(IInventoryContainer container)
        {
            inventorySnapshotBefore.Clear();
            if (container == null) return;

            foreach (var slot in container.GetSlots())
            {
                if (slot.IsEmpty || slot.Item == null) continue;
                var code = slot.Item.ItemCode;
                if (!inventorySnapshotBefore.ContainsKey(code))
                {
                    inventorySnapshotBefore[code] = 0;
                }
                inventorySnapshotBefore[code] += slot.Quantity;
            }
        }

        private void ComputeInventoryDiffs(IInventoryContainer container)
        {
            stateChangeDiffs.Clear();
            if (container == null) return;

            var snapshotAfter = new Dictionary<string, int>();
            foreach (var slot in container.GetSlots())
            {
                if (slot.IsEmpty || slot.Item == null) continue;
                var code = slot.Item.ItemCode;
                if (!snapshotAfter.ContainsKey(code))
                {
                    snapshotAfter[code] = 0;
                }
                snapshotAfter[code] += slot.Quantity;
            }

            var allKeys = new HashSet<string>(inventorySnapshotBefore.Keys.Concat(snapshotAfter.Keys));
            foreach (var key in allKeys)
            {
                var before = inventorySnapshotBefore.ContainsKey(key) ? inventorySnapshotBefore[key] : 0;
                var after = snapshotAfter.ContainsKey(key) ? snapshotAfter[key] : 0;
                var diff = after - before;

                if (diff > 0)
                {
                    stateChangeDiffs.Add($"🟢 [{key}]: {before} -> {after} (+{diff})");
                }
                else if (diff < 0)
                {
                    stateChangeDiffs.Add($"🔴 [{key}]: {before} -> {after} ({diff})");
                }
            }
        }

        private void QuickAddItem(string resourcePath, int amount)
        {
            var item = Resources.Load<ItemDefinition>(resourcePath);
            if (item == null)
            {
                Debug.LogWarning($"[MethodTester] Could not load item at '{resourcePath}'.");
                return;
            }

            var container = GetActiveInventoryContainer();
            container?.AddItem(item, amount);
        }
    }
}
