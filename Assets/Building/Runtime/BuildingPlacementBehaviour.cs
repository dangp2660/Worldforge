using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Worldforge.Core.Bootstrap;
using Worldforge.Inventory;

namespace Worldforge.Building
{
    // Presentation and input controller for structure placement.
    // Connects Unity input, camera raycasting, and preview rendering with IBuildingPlacementService.
    [DisallowMultipleComponent]
    [AddComponentMenu("Worldforge/Building/Building Placement Behaviour")]
    public sealed class BuildingPlacementBehaviour : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private BuildingPlacementConfiguration _configuration;
        [SerializeField] private Camera _targetCamera;
        [SerializeField] private PlayerInventoryBehaviour _playerInventory;

        [Header("Debug & Testing")]
        [SerializeField] private StructureDefinition _debugStructureToPlace;
        [SerializeField] private string _currentStateDebug = "None";
        [SerializeField] private string _activeStructureDebug = "None";
        [SerializeField] private bool _isValidPlacementDebug;
        [SerializeField] private string _validationMessageDebug = string.Empty;

        private IBuildingPlacementService _placementService;
        private GameObject _previewGhost;
        private Material _previewMaterial;
        private readonly List<Renderer> _previewRenderers = new();
        private MaterialPropertyBlock _propertyBlock;
        private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorProp = Shader.PropertyToID("_Color");

        public IBuildingPlacementService PlacementService
        {
            get { return _placementService; }
        }

        public BuildingPlacementConfiguration Configuration
        {
            get { return _configuration; }
        }

        private void Awake()
        {
            if (_targetCamera == null)
            {
                _targetCamera = Camera.main;
            }

            if (_playerInventory == null)
            {
                _playerInventory = GetComponentInParent<PlayerInventoryBehaviour>()
                    ?? GetComponentInChildren<PlayerInventoryBehaviour>();
            }

            if (_configuration == null)
            {
                _configuration = Resources.Load<BuildingPlacementConfiguration>("BuildingPlacementConfiguration")
                    ?? Resources.Load<BuildingPlacementConfiguration>("Building/BuildingPlacementConfiguration")
                    ?? ScriptableObject.CreateInstance<BuildingPlacementConfiguration>();
            }

            _propertyBlock = new MaterialPropertyBlock();
        }

        private void Start()
        {
            InitializeService();
        }

        private void OnEnable()
        {
            SubscribeServiceEvents();
        }

        private void OnDisable()
        {
            UnsubscribeServiceEvents();
            DestroyPreviewGhost();
        }

        private void Update()
        {
            if (_placementService == null)
            {
                return;
            }

            UpdateDebugFields();

            if (_placementService.CurrentState == PlacementState.Previewing)
            {
                HandlePlacementPreviewLoop();
                HandlePlacementInput();
            }
        }

        public void InitializeService()
        {
            if (_placementService != null)
            {
                return;
            }

            if (BootstrapManager.HasInstance && BootstrapManager.TryResolve<IBuildingPlacementService>(out var resolvedService))
            {
                _placementService = resolvedService;
            }
            else
            {
                _placementService = new RuntimeBuildingPlacementService(_configuration);
            }

            SubscribeServiceEvents();
        }

        private void SubscribeServiceEvents()
        {
            if (_placementService == null) return;

            _placementService.PlacementStarted += OnPlacementStarted;
            _placementService.PlacementConfirmed += OnPlacementConfirmed;
            _placementService.PlacementCancelled += OnPlacementCancelled;
            _placementService.PlacementValidityChanged += OnPlacementValidityChanged;
        }

        private void UnsubscribeServiceEvents()
        {
            if (_placementService == null) return;

            _placementService.PlacementStarted -= OnPlacementStarted;
            _placementService.PlacementConfirmed -= OnPlacementConfirmed;
            _placementService.PlacementCancelled -= OnPlacementCancelled;
            _placementService.PlacementValidityChanged -= OnPlacementValidityChanged;
        }

        [ContextMenu("Start Debug Placement")]
        public void StartDebugPlacement()
        {
            if (_debugStructureToPlace != null)
            {
                StartPlacement(_debugStructureToPlace);
            }
        }

        [ContextMenu("Confirm Current Placement")]
        public void ConfirmDebugPlacement()
        {
            ConfirmPlacement();
        }

        [ContextMenu("Cancel Current Placement")]
        public void CancelDebugPlacement()
        {
            CancelPlacement();
        }

        public bool StartPlacement(StructureDefinition definition)
        {
            InitializeService();
            if (_placementService == null || definition == null)
            {
                return false;
            }

            return _placementService.StartPlacement(definition);
        }

        public PlacementResult ConfirmPlacement()
        {
            if (_placementService == null)
            {
                return PlacementResult.Failure(PlacementFailureReason.PlacementNotActive, "Placement service not initialized.");
            }

            var inventory = _playerInventory != null ? _playerInventory.Container : null;
            return _placementService.ConfirmPlacement(inventory);
        }

        public void CancelPlacement()
        {
            _placementService?.CancelPlacement();
        }

        public void RotatePlacement(float angleDegrees)
        {
            _placementService?.RotatePreview(angleDegrees);
        }

        private void HandlePlacementPreviewLoop()
        {
            if (_targetCamera == null)
            {
                _targetCamera = Camera.main;
                if (_targetCamera == null) return;
            }

            var ray = GetPointerRay();
            var maxDistance = _configuration != null ? _configuration.MaxPlacementDistance : 25f;
            var groundMask = _configuration != null && _configuration.GroundLayerMask.value != 0
                ? _configuration.GroundLayerMask.value
                : ~0;

            if (Physics.Raycast(ray, out var hit, maxDistance, groundMask, QueryTriggerInteraction.Ignore))
            {
                var targetPosition = hit.point;
                var activeDef = _placementService.ActiveDefinition;

                if (activeDef != null && activeDef.PlacementRule.SnapToGrid)
                {
                    var gridSize = _configuration != null ? _configuration.DefaultGridSize : 1f;
                    targetPosition = SnapToGrid(targetPosition, gridSize);
                }

                if (_previewGhost != null)
                {
                    _previewGhost.transform.position = targetPosition;
                    _previewGhost.transform.rotation = _placementService.CurrentRotation;
                    if (!_previewGhost.activeSelf)
                    {
                        _previewGhost.SetActive(true);
                    }
                }

                var inventory = _playerInventory != null ? _playerInventory.Container : null;
                _placementService.UpdatePlacement(targetPosition, _placementService.CurrentRotation, inventory);
            }
            else
            {
                if (_previewGhost != null && _previewGhost.activeSelf)
                {
                    _previewGhost.SetActive(false);
                }
            }
        }

        private void HandlePlacementInput()
        {
            // Rotate Input (R key)
            if (IsRotateInputPressed())
            {
                var step = _configuration != null ? _configuration.DefaultRotationStep : 90f;
                _placementService.RotatePreview(step);
            }

            // Confirm Placement (Left Mouse Click)
            if (IsConfirmInputPressed())
            {
                ConfirmPlacement();
            }

            // Cancel Placement (Right Mouse Click or Escape)
            if (IsCancelInputPressed())
            {
                CancelPlacement();
            }
        }

        private bool IsRotateInputPressed()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                return true;
            }

            return Input.GetKeyDown(KeyCode.R);
        }

        private bool IsConfirmInputPressed()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                return true;
            }

            return Input.GetMouseButtonDown(0);
        }

        private bool IsCancelInputPressed()
        {
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                return true;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return true;
            }

            return Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape);
        }

        private Ray GetPointerRay()
        {
            var pointerPos = Vector2.zero;
            if (Mouse.current != null)
            {
                pointerPos = Mouse.current.position.ReadValue();
            }
            else
            {
                pointerPos = Input.mousePosition;
            }

            return _targetCamera.ScreenPointToRay(pointerPos);
        }

        private static Vector3 SnapToGrid(Vector3 position, float gridSize)
        {
            if (gridSize <= 0.001f) return position;
            return new Vector3(
                Mathf.Round(position.x / gridSize) * gridSize,
                position.y,
                Mathf.Round(position.z / gridSize) * gridSize);
        }

        private void OnPlacementStarted(StructureDefinition definition)
        {
            CreatePreviewGhost(definition);
            UpdatePreviewTint(_placementService != null && _placementService.IsPlacementValid);
        }

        private void OnPlacementValidityChanged(PlacementValidationResult result)
        {
            UpdatePreviewTint(result.IsValid);
        }

        private void OnPlacementConfirmed(PlacementResult result)
        {
            DestroyPreviewGhost();
        }

        private void OnPlacementCancelled(StructureDefinition definition)
        {
            DestroyPreviewGhost();
        }

        private void CreatePreviewGhost(StructureDefinition definition)
        {
            DestroyPreviewGhost();

            if (definition == null) return;

            if (definition.Prefab != null)
            {
                _previewGhost = Instantiate(definition.Prefab);
            }
            else
            {
                // Fallback ghost cube
                _previewGhost = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var footprint = definition.PlacementRule.Footprint;
                _previewGhost.transform.localScale = new Vector3(footprint.x, 1f, footprint.y);
            }

            _previewGhost.name = $"PreviewGhost_{definition.DisplayName}";

            // Disable all colliders on preview ghost to avoid self-collision with raycasts
            var colliders = _previewGhost.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            _previewRenderers.Clear();
            _previewGhost.GetComponentsInChildren(true, _previewRenderers);

            EnsurePreviewMaterial();
            ApplyPreviewMaterialToGhost();
        }

        private void EnsurePreviewMaterial()
        {
            if (_previewMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Sprites/Default")
                    ?? Shader.Find("Standard");

                _previewMaterial = new Material(shader)
                {
                    name = "M_BuildingPreviewGhost"
                };
            }
        }

        private void ApplyPreviewMaterialToGhost()
        {
            if (_previewMaterial == null) return;

            for (var i = 0; i < _previewRenderers.Count; i++)
            {
                var rend = _previewRenderers[i];
                if (rend != null)
                {
                    var mats = new Material[rend.sharedMaterials.Length];
                    for (var m = 0; m < mats.Length; m++)
                    {
                        mats[m] = _previewMaterial;
                    }
                    rend.materials = mats;
                }
            }
        }

        private void UpdatePreviewTint(bool isValid)
        {
            if (_previewGhost == null || _configuration == null) return;

            var targetColor = isValid ? _configuration.ValidPreviewColor : _configuration.InvalidPreviewColor;

            if (_propertyBlock == null)
            {
                _propertyBlock = new MaterialPropertyBlock();
            }

            for (var i = 0; i < _previewRenderers.Count; i++)
            {
                var rend = _previewRenderers[i];
                if (rend != null)
                {
                    rend.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor(BaseColorProp, targetColor);
                    _propertyBlock.SetColor(ColorProp, targetColor);
                    rend.SetPropertyBlock(_propertyBlock);
                }
            }

            if (_previewMaterial != null)
            {
                if (_previewMaterial.HasProperty(BaseColorProp))
                {
                    _previewMaterial.SetColor(BaseColorProp, targetColor);
                }
                if (_previewMaterial.HasProperty(ColorProp))
                {
                    _previewMaterial.SetColor(ColorProp, targetColor);
                }
            }
        }

        private void DestroyPreviewGhost()
        {
            if (_previewGhost != null)
            {
                Destroy(_previewGhost);
                _previewGhost = null;
            }
            _previewRenderers.Clear();
        }

        private void UpdateDebugFields()
        {
            if (_placementService != null)
            {
                _currentStateDebug = _placementService.CurrentState.ToString();
                _activeStructureDebug = _placementService.ActiveDefinition != null
                    ? _placementService.ActiveDefinition.DisplayName
                    : "None";
                _isValidPlacementDebug = _placementService.IsPlacementValid;
                _validationMessageDebug = _placementService.LastValidationResult.Message;
            }
            else
            {
                _currentStateDebug = "None";
                _activeStructureDebug = "None";
                _isValidPlacementDebug = false;
                _validationMessageDebug = "No Service";
            }
        }
    }
}
