using System;
using UnityEngine;
using Worldforge.Item;

namespace Worldforge.Gathering
{
    [Serializable]
    public sealed class ResourceYieldEntry
    {
        [ThreadStatic]
        private static System.Random s_sharedRandom;

        [SerializeField] private ItemDefinition _item;
        [SerializeField] private int _minAmount = 1;
        [SerializeField] private int _maxAmount = 1;
        [SerializeField, Range(0f, 1f)] private float _dropChance = 1f;

        public ResourceYieldEntry()
        {
        }

        public ResourceYieldEntry(ItemDefinition item, int minAmount, int maxAmount, float dropChance = 1f)
        {
            _item = item;
            _minAmount = minAmount;
            _maxAmount = maxAmount;
            _dropChance = dropChance;
        }

        public ItemDefinition Item
        {
            get { return _item; }
        }

        public int MinAmount
        {
            get { return _minAmount; }
        }

        public int MaxAmount
        {
            get { return _maxAmount; }
        }

        public float DropChance
        {
            get { return _dropChance; }
        }

        public int RollAmount(System.Random random = null)
        {
            var rng = random ?? (s_sharedRandom ??= new System.Random());

            if (rng.NextDouble() > _dropChance)
            {
                return 0;
            }

            return rng.Next(_minAmount, _maxAmount + 1);
        }

        public void Validate()
        {
            _minAmount = Mathf.Max(1, _minAmount);
            _maxAmount = Mathf.Max(_minAmount, _maxAmount);
            _dropChance = Mathf.Clamp01(_dropChance);
        }
    }
}
