using System;
using UnityEngine;

namespace Worldforge.Item
{
    [Serializable]
    public sealed class ToolProperties : IGatheringTool
    {
        [SerializeField] private ToolType _toolType = ToolType.None;
        [SerializeField] private float _harvestPower = 1f;
        [SerializeField] private float _efficiency = 1f;
        [SerializeField] private int _toolTier = 1;
        [SerializeField] private float _durabilityCostPerUse = 1f;

        public ToolProperties()
        {
        }

        public ToolProperties(ToolType toolType, float harvestPower, float efficiency, int toolTier = 1, float durabilityCostPerUse = 1f)
        {
            _toolType = toolType;
            _harvestPower = harvestPower;
            _efficiency = efficiency;
            _toolTier = toolTier;
            _durabilityCostPerUse = durabilityCostPerUse;
        }

        public ToolType ToolType
        {
            get { return _toolType; }
        }

        public float HarvestPower
        {
            get { return _harvestPower; }
        }

        public float Efficiency
        {
            get { return _efficiency; }
        }

        public int ToolTier
        {
            get { return _toolTier; }
        }

        public float DurabilityCostPerUse
        {
            get { return _durabilityCostPerUse; }
        }

        public void Validate()
        {
            _harvestPower = Mathf.Max(0f, _harvestPower);
            _efficiency = Mathf.Max(0.1f, _efficiency);
            _toolTier = Mathf.Max(0, _toolTier);
            _durabilityCostPerUse = Mathf.Max(0f, _durabilityCostPerUse);
        }
    }
}
