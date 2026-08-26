using System;
using UnityEngine;
using Worldforge.Item;

namespace Worldforge.Gathering
{
    [CreateAssetMenu(
        fileName = "ResourceNodeDefinition",
        menuName = "Worldforge/Gathering/Resource Node Definition")]
    public sealed class ResourceNodeDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _nodeCode = string.Empty;
        [SerializeField] private string _displayName = string.Empty;
        [SerializeField, TextArea(2, 4)] private string _description = string.Empty;
        [SerializeField] private string _biomeType = "Default";

        [Header("Yield & Drops")]
        [SerializeField] private ItemDefinition _primaryYield;
        [SerializeField] private int _primaryMinAmount = 1;
        [SerializeField] private int _primaryMaxAmount = 3;
        [SerializeField] private ResourceYieldEntry[] _bonusYields = Array.Empty<ResourceYieldEntry>();

        [Header("Gathering Requirements")]
        [SerializeField] private GatheringRequirements _requirements = new();

        [Header("Resource Node Properties")]
        [SerializeField] private float _hardness = 1f;
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _baseGatherDuration = 2f;
        [SerializeField] private float _respawnTime = 60f;
        [SerializeField] private bool _canRespawn = true;
        [SerializeField] private int _discoveryXP = 10;

        [Header("Presentation")]
        [SerializeField] private GameObject _worldPrefab;

        public string NodeCode
        {
            get { return _nodeCode; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public string Description
        {
            get { return _description; }
        }

        public string BiomeType
        {
            get { return _biomeType; }
        }

        public ItemDefinition PrimaryYield
        {
            get { return _primaryYield; }
        }

        public int PrimaryMinAmount
        {
            get { return _primaryMinAmount; }
        }

        public int PrimaryMaxAmount
        {
            get { return _primaryMaxAmount; }
        }

        public ResourceYieldEntry[] BonusYields
        {
            get { return _bonusYields; }
        }

        public GatheringRequirements Requirements
        {
            get { return _requirements; }
        }

        public float Hardness
        {
            get { return _hardness; }
        }

        public float MaxHealth
        {
            get { return _maxHealth; }
        }

        public float BaseGatherDuration
        {
            get { return _baseGatherDuration; }
        }

        public float RespawnTime
        {
            get { return _respawnTime; }
        }

        public bool CanRespawn
        {
            get { return _canRespawn; }
        }

        public int DiscoveryXP
        {
            get { return _discoveryXP; }
        }

        public GameObject WorldPrefab
        {
            get { return _worldPrefab; }
        }

        private void OnValidate()
        {
            _primaryMinAmount = Mathf.Max(1, _primaryMinAmount);
            _primaryMaxAmount = Mathf.Max(_primaryMinAmount, _primaryMaxAmount);
            _hardness = Mathf.Max(0f, _hardness);
            _maxHealth = Mathf.Max(1f, _maxHealth);
            _baseGatherDuration = Mathf.Max(0.1f, _baseGatherDuration);
            _respawnTime = Mathf.Max(0f, _respawnTime);
            _discoveryXP = Mathf.Max(0, _discoveryXP);

            if (_requirements != null)
            {
                _requirements.ValidateData();
            }

            if (_bonusYields != null)
            {
                for (var i = 0; i < _bonusYields.Length; i++)
                {
                    _bonusYields[i]?.Validate();
                }
            }
        }
    }
}
