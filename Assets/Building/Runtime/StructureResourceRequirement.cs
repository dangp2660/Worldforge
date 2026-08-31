using System;
using UnityEngine;
using Worldforge.Item;

namespace Worldforge.Building
{
    // Resource cost entry for constructing a structure.
    // Maps to Schema Part 6: BuildingRequirement.
    [Serializable]
    public sealed class StructureResourceRequirement
    {
        [SerializeField] private ItemDefinition _item;
        [SerializeField, Min(1)] private int _amount = 1;

        public StructureResourceRequirement()
        {
            _amount = 1;
        }

        public StructureResourceRequirement(ItemDefinition item, int amount)
        {
            _item = item;
            _amount = Mathf.Max(1, amount);
        }

        public ItemDefinition Item
        {
            get { return _item; }
        }

        public int Amount
        {
            get { return _amount; }
        }

        public void Validate()
        {
            _amount = Mathf.Max(1, _amount);
        }
    }
}
