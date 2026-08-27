using System;
using UnityEngine;

namespace Worldforge.Item
{
    [Serializable]
    public sealed class ArmorProperties
    {
        [SerializeField] private string _armorType = "Chest";
        [SerializeField] private float _armor = 5f;
        [SerializeField] private float _magicResistance = 0f;

        public ArmorProperties()
        {
        }

        public ArmorProperties(string armorType, float armor, float magicResistance = 0f)
        {
            _armorType = armorType;
            _armor = armor;
            _magicResistance = magicResistance;
        }

        public string ArmorType
        {
            get { return _armorType; }
        }

        public float Armor
        {
            get { return _armor; }
        }

        public float MagicResistance
        {
            get { return _magicResistance; }
        }

        public void Validate()
        {
            _armor = Mathf.Max(0f, _armor);
            _magicResistance = Mathf.Max(0f, _magicResistance);
        }
    }
}
