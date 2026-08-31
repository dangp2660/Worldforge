using System;
using UnityEngine;
using Worldforge.Core.Attributes;

namespace Worldforge.Item
{
    /// <summary>
    /// Component representing an equipped or held gathering tool on a GameObject.
    /// Implements <see cref="IGatheringTool"/> and <see cref="IGatheringToolProvider"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Worldforge/Item/Gathering Tool Behaviour")]
    [TestTarget(Category = "Item", DisplayName = "Gathering Tool", Order = 25)]
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

        public event Action<ItemDefinition> ToolChanged;

        public ItemDefinition ToolItem
        {
            get { return _toolItem; }
            set
            {
                _toolItem = value;
                SyncFromItemDefinition();
                ToolChanged?.Invoke(_toolItem);
            }
        }

        public bool HasEquippedTool
        {
            get { return _toolItem != null && _toolItem.IsTool; }
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

        [TestMethod(DisplayName = "Equip Tool", Order = 1, Description = "Equips a tool item definition")]
        public bool EquipTool(ItemDefinition toolItem)
        {
            if (toolItem == null || !toolItem.IsTool)
            {
                return false;
            }

            _toolItem = toolItem;
            SyncFromItemDefinition();
            ToolChanged?.Invoke(_toolItem);
            return true;
        }

        [TestMethod(DisplayName = "Unequip Tool", Order = 2, Description = "Unequips current tool and resets to None")]
        public void UnequipTool()
        {
            _toolItem = null;
            _toolType = ToolType.None;
            _harvestPower = 1f;
            _efficiency = 1f;
            _toolTier = 1;
            _durabilityCostPerUse = 1f;
            ToolChanged?.Invoke(null);
        }

        public void Configure(ToolType toolType, float harvestPower, float efficiency = 1f, int toolTier = 1, float durabilityCost = 1f)
        {
            _toolItem = null;
            _toolType = toolType;
            _harvestPower = Mathf.Max(0f, harvestPower);
            _efficiency = Mathf.Max(0.1f, efficiency);
            _toolTier = Mathf.Max(0, toolTier);
            _durabilityCostPerUse = Mathf.Max(0f, durabilityCost);
            ToolChanged?.Invoke(null);
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
