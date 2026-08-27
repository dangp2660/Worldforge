using System;
using UnityEngine;

namespace Worldforge.Item
{
    [Serializable]
    public sealed class EquipmentProperties
    {
        [SerializeField] private string _equipmentSlot = "MainHand";
        [SerializeField] private int _requiredLevel = 1;
        [SerializeField] private float _maxDurability = 100f;
        [SerializeField] private float _durabilityMultiplier = 1f;

        public EquipmentProperties()
        {
        }

        public EquipmentProperties(string equipmentSlot, int requiredLevel = 1, float maxDurability = 100f, float durabilityMultiplier = 1f)
        {
            _equipmentSlot = equipmentSlot;
            _requiredLevel = requiredLevel;
            _maxDurability = maxDurability;
            _durabilityMultiplier = durabilityMultiplier;
        }

        public string EquipmentSlot
        {
            get { return _equipmentSlot; }
        }

        public int RequiredLevel
        {
            get { return _requiredLevel; }
        }

        public float MaxDurability
        {
            get { return _maxDurability; }
        }

        public float DurabilityMultiplier
        {
            get { return _durabilityMultiplier; }
        }

        public void Validate()
        {
            _requiredLevel = Mathf.Max(1, _requiredLevel);
            _maxDurability = Mathf.Max(0f, _maxDurability);
            _durabilityMultiplier = Mathf.Max(0.1f, _durabilityMultiplier);
        }
    }
}
