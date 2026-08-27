using System;
using UnityEngine;

namespace Worldforge.Item
{
    [Serializable]
    public sealed class WeaponProperties
    {
        [SerializeField] private string _weaponType = "Sword";
        [SerializeField] private string _damageType = "Physical";
        [SerializeField] private float _baseDamage = 10f;
        [SerializeField] private float _attackSpeed = 1f;
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _criticalChance = 0.05f;
        [SerializeField] private float _criticalMultiplier = 1.5f;

        public WeaponProperties()
        {
        }

        public WeaponProperties(
            string weaponType,
            string damageType,
            float baseDamage,
            float attackSpeed,
            float attackRange,
            float criticalChance = 0.05f,
            float criticalMultiplier = 1.5f)
        {
            _weaponType = weaponType;
            _damageType = damageType;
            _baseDamage = baseDamage;
            _attackSpeed = attackSpeed;
            _attackRange = attackRange;
            _criticalChance = criticalChance;
            _criticalMultiplier = criticalMultiplier;
        }

        public string WeaponType
        {
            get { return _weaponType; }
        }

        public string DamageType
        {
            get { return _damageType; }
        }

        public float BaseDamage
        {
            get { return _baseDamage; }
        }

        public float AttackSpeed
        {
            get { return _attackSpeed; }
        }

        public float AttackRange
        {
            get { return _attackRange; }
        }

        public float CriticalChance
        {
            get { return _criticalChance; }
        }

        public float CriticalMultiplier
        {
            get { return _criticalMultiplier; }
        }

        public void Validate()
        {
            _baseDamage = Mathf.Max(0f, _baseDamage);
            _attackSpeed = Mathf.Max(0.1f, _attackSpeed);
            _attackRange = Mathf.Max(0.1f, _attackRange);
            _criticalChance = Mathf.Clamp01(_criticalChance);
            _criticalMultiplier = Mathf.Max(1f, _criticalMultiplier);
        }
    }
}
