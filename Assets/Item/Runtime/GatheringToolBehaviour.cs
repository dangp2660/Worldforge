using System;
using UnityEngine;

namespace Worldforge.Item
{
    /// <summary>
    /// Component representing an equipped or held gathering tool on a GameObject.
    /// Implements <see cref="IGatheringTool"/> and <see cref="IGatheringToolProvider"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Worldforge/Item/Gathering Tool Behaviour")]
    public sealed class GatheringToolBehaviour : MonoBehaviour, IGatheringTool, IGatheringToolProvider
    {
        [Header("Tool Item Definition")]
        [SerializeField] private ItemDefinition _toolItem;

        [Header("Tool Properties Override")]
        [SerializeField] private ToolType _toolType = ToolType.None;
        [SerializeField] private float _harvestPower = 1f;
        [SerializeField] private float _efficiency = 1f;
        [SerializeField] private int _toolTier = 1;
        [SerializeField] private float _durabilityCostPerUse = 1f;

        public ItemDefinition ToolItem
        {
            get { return _toolItem; }
            set
            {
                _toolItem = value;
                SyncFromItemDefinition();
            }
        }

        public ToolType ToolType
        {
            get { return _toolItem != null && _toolItem.IsTool ? _toolItem.ToolProperties.ToolType : _toolType; }
            set { _toolType = value; }
        }

        public float HarvestPower
        {
            get { return _toolItem != null && _toolItem.IsTool ? _toolItem.ToolProperties.HarvestPower : _harvestPower; }
            set { _harvestPower = Mathf.Max(0f, value); }
        }

        public float Efficiency
        {
            get { return _toolItem != null && _toolItem.IsTool ? _toolItem.ToolProperties.Efficiency : _efficiency; }
            set { _efficiency = Mathf.Max(0.1f, value); }
        }

        public int ToolTier
        {
            get { return _toolItem != null && _toolItem.IsTool ? _toolItem.ToolProperties.ToolTier : _toolTier; }
            set { _toolTier = Mathf.Max(0, value); }
        }

        public float DurabilityCostPerUse
        {
            get { return _toolItem != null && _toolItem.IsTool ? _toolItem.ToolProperties.DurabilityCostPerUse : _durabilityCostPerUse; }
            set { _durabilityCostPerUse = Mathf.Max(0f, value); }
        }

        public IGatheringTool ActiveTool
        {
            get { return this; }
        }

        private void Awake()
        {
            SyncFromItemDefinition();
        }

        private void OnValidate()
        {
            _harvestPower = Mathf.Max(0f, _harvestPower);
            _efficiency = Mathf.Max(0.1f, _efficiency);
            _toolTier = Mathf.Max(0, _toolTier);
            _durabilityCostPerUse = Mathf.Max(0f, _durabilityCostPerUse);

            SyncFromItemDefinition();
        }

        public void Configure(ToolType toolType, float harvestPower, float efficiency = 1f, int toolTier = 1, float durabilityCost = 1f)
        {
            _toolItem = null;
            _toolType = toolType;
            _harvestPower = Mathf.Max(0f, harvestPower);
            _efficiency = Mathf.Max(0.1f, efficiency);
            _toolTier = Mathf.Max(0, toolTier);
            _durabilityCostPerUse = Mathf.Max(0f, durabilityCost);
        }

        private void SyncFromItemDefinition()
        {
            if (_toolItem != null && _toolItem.IsTool && _toolItem.ToolProperties != null)
            {
                _toolType = _toolItem.ToolProperties.ToolType;
                _harvestPower = _toolItem.ToolProperties.HarvestPower;
                _efficiency = _toolItem.ToolProperties.Efficiency;
                _toolTier = _toolItem.ToolProperties.ToolTier;
                _durabilityCostPerUse = _toolItem.ToolProperties.DurabilityCostPerUse;
            }
        }
    }
}
