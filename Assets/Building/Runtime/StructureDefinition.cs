using System;
using System.Collections.Generic;
using UnityEngine;

namespace Worldforge.Building
{
    // Definition data for a buildable structure.
    // Maps to Schema Part 6: BuildingDefinition.
    // This is immutable Definition Data — do not store Runtime State here.
    [CreateAssetMenu(
        fileName = "StructureDefinition",
        menuName = "Worldforge/Building/Structure Definition")]
    public sealed class StructureDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _structureCode = "STRUCT_DEFAULT";
        [SerializeField] private string _displayName = "New Structure";
        [SerializeField, TextArea(2, 4)] private string _description = string.Empty;

        [Header("Classification")]
        [SerializeField] private StructureCategoryType _category = StructureCategoryType.Foundation;
        [SerializeField] private StructureFunctionType _functionType = StructureFunctionType.None;

        [Header("Presentation")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private GameObject _prefab;

        [Header("Properties")]
        [SerializeField, Min(1f)] private float _maxHealth = 100f;
        [SerializeField, Min(0f)] private float _buildTime = 5f;
        [SerializeField, Min(1)] private int _maxWorker = 1;

        [Header("Flags")]
        [SerializeField] private bool _canUpgrade = false;
        [SerializeField] private bool _canRepair = true;
        [SerializeField] private bool _canDestroy = true;

        [Header("Resource Requirements")]
        [SerializeField] private List<StructureResourceRequirement> _requirements = new();

        [Header("Placement Rules")]
        [SerializeField] private StructurePlacementRule _placementRule = new();

        [Header("Upgrade Path")]
        [Tooltip("Next tier structure definition when upgraded")]
        [SerializeField] private StructureDefinition _upgradeTarget;

        public string StructureCode
        {
            get { return _structureCode; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public string Description
        {
            get { return _description; }
        }

        public StructureCategoryType Category
        {
            get { return _category; }
        }

        public StructureFunctionType FunctionType
        {
            get { return _functionType; }
        }

        public Sprite Icon
        {
            get { return _icon; }
        }

        public GameObject Prefab
        {
            get { return _prefab; }
        }

        public float MaxHealth
        {
            get { return _maxHealth; }
        }

        public float BuildTime
        {
            get { return _buildTime; }
        }

        public int MaxWorker
        {
            get { return _maxWorker; }
        }

        public bool CanUpgrade
        {
            get { return _canUpgrade; }
        }

        public bool CanRepair
        {
            get { return _canRepair; }
        }

        public bool CanDestroy
        {
            get { return _canDestroy; }
        }

        public IReadOnlyList<StructureResourceRequirement> Requirements
        {
            get { return _requirements; }
        }

        public StructurePlacementRule PlacementRule
        {
            get { return _placementRule; }
        }

        public StructureDefinition UpgradeTarget
        {
            get { return _upgradeTarget; }
        }

        public bool HasUpgradePath
        {
            get { return _canUpgrade && _upgradeTarget != null; }
        }

        public bool HasFunction
        {
            get { return _functionType != StructureFunctionType.None; }
        }

        public bool HasRequirements
        {
            get { return _requirements != null && _requirements.Count > 0; }
        }

        // Checks if this is a structural building piece (foundation, wall, door, roof).
        public bool IsStructuralPiece
        {
            get
            {
                return _category == StructureCategoryType.Foundation
                    || _category == StructureCategoryType.Wall
                    || _category == StructureCategoryType.Door
                    || _category == StructureCategoryType.Roof;
            }
        }

        public bool IsValid(out string reason)
        {
            if (string.IsNullOrWhiteSpace(_structureCode))
            {
                reason = "StructureCode is null or empty.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_displayName))
            {
                reason = $"Structure '{_structureCode}' has no display name.";
                return false;
            }

            if (_requirements != null)
            {
                for (var i = 0; i < _requirements.Count; i++)
                {
                    var req = _requirements[i];
                    if (req == null || req.Item == null)
                    {
                        reason = $"Structure '{_structureCode}' has null requirement at index {i}.";
                        return false;
                    }

                    if (req.Amount <= 0)
                    {
                        reason = $"Structure '{_structureCode}' requirement '{req.Item.DisplayName}' has non-positive amount {req.Amount}.";
                        return false;
                    }
                }
            }

            if (_canUpgrade && _upgradeTarget == this)
            {
                reason = $"Structure '{_structureCode}' upgrade target references itself.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private void OnValidate()
        {
            _maxHealth = Mathf.Max(1f, _maxHealth);
            _buildTime = Mathf.Max(0f, _buildTime);
            _maxWorker = Mathf.Max(1, _maxWorker);

            if (_placementRule != null)
            {
                _placementRule.Validate();
            }

            if (_requirements != null)
            {
                for (var i = 0; i < _requirements.Count; i++)
                {
                    _requirements[i]?.Validate();
                }
            }

            // Prevent self-referencing upgrade
            if (_upgradeTarget == this)
            {
                Debug.LogWarning($"[StructureDefinition] '{_structureCode}' cannot upgrade to itself. Clearing upgrade target.");
                _upgradeTarget = null;
                _canUpgrade = false;
            }
        }
    }
}
