using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Worldforge.Inventory;
using Worldforge.Item;

namespace Worldforge.Building
{
    // Dedicated in-game testing UI overlay for the Building Placement System.
    // Provides instant structure selection buttons, live validation diagnostics,
    // placement action triggers, and quick inventory resource injection for testing.
    [DisallowMultipleComponent]
    [AddComponentMenu("Worldforge/Building/Building Placement Test UI")]
    public sealed class BuildingPlacementTestUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BuildingPlacementBehaviour _placementBehaviour;
        [SerializeField] private PlayerInventoryBehaviour _playerInventory;

        [Header("UI Display Settings")]
        [SerializeField] private bool _isVisible = true;
        [SerializeField] private KeyCode _toggleKey = KeyCode.B;
        [SerializeField] private Key _inputSystemToggleKey = Key.B;
        [SerializeField] private Vector2 _panelPosition = new Vector2(15f, 15f);
        [SerializeField] private Vector2 _panelSize = new Vector2(380f, 620f);

        private readonly List<StructureDefinition> _loadedStructures = new();
        private readonly List<ItemDefinition> _loadedItems = new();
        private Vector2 _structuresScrollPos;
        private GUIStyle _boxStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _subHeaderStyle;
        private GUIStyle _validStatusStyle;
        private GUIStyle _invalidStatusStyle;
        private GUIStyle _neutralStatusStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _smallButtonStyle;
        private GUIStyle _requirementStyle;
        private bool _stylesInitialized;

        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; }
        }

        private void Awake()
        {
            if (_placementBehaviour == null)
            {
                _placementBehaviour = GetComponent<BuildingPlacementBehaviour>()
                    ?? FindFirstObjectByType<BuildingPlacementBehaviour>();
            }

            if (_playerInventory == null)
            {
                _playerInventory = GetComponentInParent<PlayerInventoryBehaviour>()
                    ?? FindFirstObjectByType<PlayerInventoryBehaviour>();
            }

            LoadAvailableDefinitions();
        }

        private void Start()
        {
            if (_placementBehaviour == null)
            {
                _placementBehaviour = gameObject.AddComponent<BuildingPlacementBehaviour>();
            }
        }

        private void Update()
        {
            if (IsToggleHotkeyPressed())
            {
                _isVisible = !_isVisible;
            }
        }

        private bool IsToggleHotkeyPressed()
        {
            if (Keyboard.current != null && Keyboard.current[_inputSystemToggleKey].wasPressedThisFrame)
            {
                return true;
            }

            return Input.GetKeyDown(_toggleKey) || Input.GetKeyDown(KeyCode.F4);
        }

        private void LoadAvailableDefinitions()
        {
            _loadedStructures.Clear();

            // Load structures registry or definitions from Resources
            var registry = Resources.Load<StructureDefinitionRegistry>("Definitions/Structures/StructureDefinitionRegistry")
                ?? Resources.Load<StructureDefinitionRegistry>("StructureDefinitionRegistry");

            if (registry != null && registry.Definitions != null && registry.Definitions.Count > 0)
            {
                for (var i = 0; i < registry.Definitions.Count; i++)
                {
                    if (registry.Definitions[i] != null)
                    {
                        _loadedStructures.Add(registry.Definitions[i]);
                    }
                }
            }
            else
            {
                var structures = Resources.LoadAll<StructureDefinition>("Definitions/Structures");
                if (structures == null || structures.Length == 0)
                {
                    structures = Resources.LoadAll<StructureDefinition>("");
                }

                if (structures != null)
                {
                    _loadedStructures.AddRange(structures);
                }
            }

            // Load item definitions for test resource gifting
            var items = Resources.LoadAll<ItemDefinition>("Definitions/Items");
            if (items == null || items.Length == 0)
            {
                items = Resources.LoadAll<ItemDefinition>("");
            }

            if (items != null)
            {
                _loadedItems.AddRange(items);
            }
        }

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            var bgTex = MakeTex(2, 2, new Color(0.08f, 0.1f, 0.14f, 0.92f));

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = bgTex },
                padding = new RectOffset(12, 12, 10, 10),
                margin = new RectOffset(0, 0, 0, 0)
            };

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.35f, 0.8f, 1f) }
            };

            _subHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.85f, 0.35f) }
            };

            _validStatusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.2f, 0.95f, 0.3f) },
                wordWrap = true
            };

            _invalidStatusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.3f, 0.3f) },
                wordWrap = true
            };

            _neutralStatusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.85f, 0.85f, 0.85f) },
                wordWrap = true
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                fixedHeight = 26
            };

            _smallButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
                fixedHeight = 22
            };

            _requirementStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                normal = { textColor = new Color(0.8f, 0.9f, 1f) }
            };

            _stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            InitializeStyles();

            var rect = new Rect(_panelPosition.x, _panelPosition.y, _panelSize.x, _panelSize.y);
            GUILayout.BeginArea(rect, _boxStyle);

            // Header
            GUILayout.Label("🔨 BUILDING PLACEMENT TEST PANEL", _headerStyle);
            GUILayout.Label("Toggle overlay with [B] or [F4]", _neutralStatusStyle);
            GUILayout.Space(6);

            // 1. Structure Selection
            DrawStructureSelectionSection();

            GUILayout.Space(8);

            // 2. Status & Live Validation
            DrawLiveStatusSection();

            GUILayout.Space(8);

            // 3. Quick Action Controls
            DrawPlacementControlsSection();

            GUILayout.Space(8);

            // 4. Test Resources & Cheats
            DrawResourceCheatsSection();

            GUILayout.Space(6);

            // 5. Shortcut Legend
            DrawShortcutLegend();

            GUILayout.EndArea();
        }

        private void DrawStructureSelectionSection()
        {
            GUILayout.Label("1. Select Structure to Place:", _subHeaderStyle);

            if (_loadedStructures.Count == 0)
            {
                GUILayout.Label("No StructureDefinitions found in Resources.", _invalidStatusStyle);
                if (GUILayout.Button("Reload Definitions", _smallButtonStyle))
                {
                    LoadAvailableDefinitions();
                }
                return;
            }

            _structuresScrollPos = GUILayout.BeginScrollView(_structuresScrollPos, GUILayout.Height(130));
            for (var i = 0; i < _loadedStructures.Count; i++)
            {
                var def = _loadedStructures[i];
                if (def == null) continue;

                var isCurrent = _placementBehaviour != null
                    && _placementBehaviour.PlacementService != null
                    && _placementBehaviour.PlacementService.ActiveDefinition == def;

                var label = isCurrent ? $"▶ [{def.DisplayName}] (Active)" : def.DisplayName;
                var origColor = GUI.backgroundColor;
                if (isCurrent)
                {
                    GUI.backgroundColor = new Color(0.3f, 0.9f, 0.4f);
                }

                if (GUILayout.Button(label, _buttonStyle))
                {
                    _placementBehaviour?.StartPlacement(def);
                }
                GUI.backgroundColor = origColor;
            }
            GUILayout.EndScrollView();
        }

        private void DrawLiveStatusSection()
        {
            GUILayout.Label("2. Placement Diagnostics:", _subHeaderStyle);

            var service = _placementBehaviour != null ? _placementBehaviour.PlacementService : null;
            if (service == null)
            {
                GUILayout.Label("Placement Service: Not Initialized", _neutralStatusStyle);
                return;
            }

            var state = service.CurrentState;
            GUILayout.Label($"State: <b>{state}</b>", _neutralStatusStyle);

            if (state == PlacementState.Previewing && service.ActiveDefinition != null)
            {
                var def = service.ActiveDefinition;
                var pos = service.CurrentPosition;
                var rot = service.CurrentRotation.eulerAngles.y;
                var validation = service.LastValidationResult;

                GUILayout.Label($"Active: <b>{def.DisplayName}</b> (Size: {def.PlacementRule.Footprint.x}x{def.PlacementRule.Footprint.y})", _neutralStatusStyle);
                GUILayout.Label($"Position: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})  |  Rotation: {rot:F0}°", _neutralStatusStyle);

                // Validation Status Banner
                if (validation.IsValid)
                {
                    GUILayout.Label("✔ VALID — Surface and resources OK. Ready to confirm.", _validStatusStyle);
                }
                else
                {
                    GUILayout.Label($"✖ INVALID — {validation.FailureReason}: {validation.Message}", _invalidStatusStyle);
                }

                // Requirements Check
                if (def.HasRequirements)
                {
                    GUILayout.Label("Required Resources:", _neutralStatusStyle);
                    var container = _playerInventory != null ? _playerInventory.Container : null;

                    for (var r = 0; r < def.Requirements.Count; r++)
                    {
                        var req = def.Requirements[r];
                        if (req == null || req.Item == null) continue;

                        var have = container != null ? container.GetItemCount(req.Item) : 0;
                        var enough = have >= req.Amount;
                        var colorPrefix = enough ? "<color=#50FF60>" : "<color=#FF5050>";
                        GUILayout.Label($"  • {req.Item.DisplayName}: {colorPrefix}{have}/{req.Amount}</color>", _requirementStyle);
                    }
                }
            }
            else
            {
                GUILayout.Label("Click a structure above to start placement preview.", _neutralStatusStyle);
            }
        }

        private void DrawPlacementControlsSection()
        {
            GUILayout.Label("3. Action Triggers:", _subHeaderStyle);

            var service = _placementBehaviour != null ? _placementBehaviour.PlacementService : null;
            var isPreviewing = service != null && service.CurrentState == PlacementState.Previewing;

            GUI.enabled = isPreviewing;
            GUILayout.BeginHorizontal();

            var isValid = service != null && service.IsPlacementValid;
            var origColor = GUI.backgroundColor;
            if (isValid)
            {
                GUI.backgroundColor = new Color(0.2f, 0.9f, 0.3f);
            }

            if (GUILayout.Button("✔ Confirm [LMB]", _buttonStyle))
            {
                _placementBehaviour?.ConfirmPlacement();
            }
            GUI.backgroundColor = origColor;

            if (GUILayout.Button("✖ Cancel [RMB]", _buttonStyle))
            {
                _placementBehaviour?.CancelPlacement();
            }

            if (GUILayout.Button("↻ Rotate [R]", _buttonStyle))
            {
                _placementBehaviour?.RotatePlacement(90f);
            }

            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private void DrawResourceCheatsSection()
        {
            GUILayout.Label("4. Test Utilities & Cheats:", _subHeaderStyle);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Give All Materials x20", _smallButtonStyle))
            {
                GiveAllTestMaterials(20);
            }

            if (GUILayout.Button("Clear Bag", _smallButtonStyle))
            {
                if (_playerInventory != null && _playerInventory.Container != null)
                {
                    _playerInventory.Container.Clear();
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawShortcutLegend()
        {
            GUILayout.Label("<b>Controls:</b> LMB: Confirm | RMB/Esc: Cancel | R: Rotate | B: Toggle UI", _neutralStatusStyle);
        }

        private void GiveAllTestMaterials(int amount)
        {
            if (_playerInventory == null)
            {
                _playerInventory = GetComponentInParent<PlayerInventoryBehaviour>()
                    ?? FindFirstObjectByType<PlayerInventoryBehaviour>();
            }

            if (_playerInventory == null)
            {
                Debug.LogWarning("[BuildingPlacementTestUI] PlayerInventoryBehaviour not found in scene.");
                return;
            }

            if (_playerInventory.Container == null)
            {
                _playerInventory.InitializeContainer();
            }

            if (_loadedItems.Count == 0)
            {
                LoadAvailableDefinitions();
            }

            for (var i = 0; i < _loadedItems.Count; i++)
            {
                var item = _loadedItems[i];
                if (item != null)
                {
                    _playerInventory.AddItem(item, amount);
                }
            }

            Debug.Log($"[BuildingPlacementTestUI] Added {amount}x of all loaded items to Player Inventory for testing.");
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (var i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
