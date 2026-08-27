using System;
using UnityEngine;

namespace Worldforge.Item
{
    [Serializable]
    public sealed class BackpackProperties
    {
        [SerializeField] private int _bonusSlotCount = 8;
        [SerializeField] private int _bonusGridWidth = 4;
        [SerializeField] private int _bonusGridHeight = 2;
        [SerializeField] private float _carryCapacityBonus = 20f;

        public BackpackProperties()
        {
        }

        public BackpackProperties(int bonusSlotCount, int bonusGridWidth, int bonusGridHeight, float carryCapacityBonus)
        {
            _bonusSlotCount = bonusSlotCount;
            _bonusGridWidth = bonusGridWidth;
            _bonusGridHeight = bonusGridHeight;
            _carryCapacityBonus = carryCapacityBonus;
        }

        public int BonusSlotCount
        {
            get { return _bonusSlotCount; }
        }

        public int BonusGridWidth
        {
            get { return _bonusGridWidth; }
        }

        public int BonusGridHeight
        {
            get { return _bonusGridHeight; }
        }

        public float CarryCapacityBonus
        {
            get { return _carryCapacityBonus; }
        }

        public void Validate()
        {
            _bonusSlotCount = Mathf.Max(1, _bonusSlotCount);
            _bonusGridWidth = Mathf.Max(1, _bonusGridWidth);
            _bonusGridHeight = Mathf.Max(1, _bonusGridHeight);
            _carryCapacityBonus = Mathf.Max(0f, _carryCapacityBonus);
        }
    }
}
