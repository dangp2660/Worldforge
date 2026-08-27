using System;
using UnityEngine;

namespace Worldforge.Item
{
    [CreateAssetMenu(
        fileName = "ItemDefinition",
        menuName = "Worldforge/Item/Item Definition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _itemCode = string.Empty;
        [SerializeField] private string _displayName = string.Empty;
        [SerializeField, TextArea(2, 5)] private string _description = string.Empty;

        [Header("Classification")]
        [SerializeField] private ItemCategoryType _category = ItemCategoryType.Resource;
        [SerializeField] private ItemRarity _rarity = ItemRarity.Common;

        [Header("Spatial Grid Footprint")]
        [SerializeField, Range(1, 5)] private int _gridWidth = 1;
        [SerializeField, Range(1, 5)] private int _gridHeight = 1;

        [Header("Economy & Stacking")]
        [SerializeField] private float _weight = 0.5f;
        [SerializeField] private bool _isStackable = true;
        [SerializeField] private int _maxStack = 100;
        [SerializeField] private int _buyPrice = 0;
        [SerializeField] private int _sellPrice = 0;

        [Header("Presentation")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private string _iconPath = string.Empty;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private GameObject _worldPrefab;

        [Header("Item Flags")]
        [SerializeField] private bool _isUnique = false;
        [SerializeField] private bool _isQuestItem = false;
        [SerializeField] private bool _isTradable = true;
        [SerializeField] private bool _isDroppable = true;
        [SerializeField] private bool _canDestroy = true;

        [Header("Component Data")]
        [SerializeField] private ResourceProperties _resourceProperties = new();
        [SerializeField] private ToolProperties _toolProperties = new();
        [SerializeField] private WeaponProperties _weaponProperties = new();
        [SerializeField] private ArmorProperties _armorProperties = new();
        [SerializeField] private ConsumableProperties _consumableProperties = new();
        [SerializeField] private BackpackProperties _backpackProperties = new();
        [SerializeField] private EquipmentProperties _equipmentProperties = new();

        public string ItemCode
        {
            get { return _itemCode; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public string Description
        {
            get { return _description; }
        }

        public ItemCategoryType Category
        {
            get { return _category; }
        }

        public ItemRarity Rarity
        {
            get { return _rarity; }
        }

        public int GridWidth
        {
            get { return _gridWidth; }
        }

        public int GridHeight
        {
            get { return _gridHeight; }
        }

        public Vector2Int GridSize
        {
            get { return new Vector2Int(_gridWidth, _gridHeight); }
        }

        public Vector2Int GetRotatedGridSize(bool isRotated)
        {
            return isRotated
                ? new Vector2Int(_gridHeight, _gridWidth)
                : new Vector2Int(_gridWidth, _gridHeight);
        }

        public float Weight
        {
            get { return _weight; }
        }

        public bool IsStackable
        {
            get { return _isStackable && _maxStack > 1; }
        }

        public int MaxStack
        {
            get { return _isStackable ? _maxStack : 1; }
        }

        public int BuyPrice
        {
            get { return _buyPrice; }
        }

        public int SellPrice
        {
            get { return _sellPrice; }
        }

        public Sprite Icon
        {
            get { return _icon; }
        }

        public string IconPath
        {
            get { return _iconPath; }
        }

        public GameObject Prefab
        {
            get { return _prefab; }
        }

        public GameObject WorldPrefab
        {
            get { return _worldPrefab; }
        }

        public bool IsUnique
        {
            get { return _isUnique; }
        }

        public bool IsQuestItem
        {
            get { return _isQuestItem; }
        }

        public bool IsTradable
        {
            get { return _isTradable; }
        }

        public bool IsDroppable
        {
            get { return _isDroppable; }
        }

        public bool CanDestroy
        {
            get { return _canDestroy; }
        }

        public ResourceProperties ResourceProperties
        {
            get { return _resourceProperties; }
        }

        public ToolProperties ToolProperties
        {
            get { return _toolProperties; }
        }

        public WeaponProperties WeaponProperties
        {
            get { return _weaponProperties; }
        }

        public ArmorProperties ArmorProperties
        {
            get { return _armorProperties; }
        }

        public ConsumableProperties ConsumableProperties
        {
            get { return _consumableProperties; }
        }

        public BackpackProperties BackpackProperties
        {
            get { return _backpackProperties; }
        }

        public EquipmentProperties EquipmentProperties
        {
            get { return _equipmentProperties; }
        }

        public bool IsResource
        {
            get { return _category == ItemCategoryType.Resource; }
        }

        public bool IsTool
        {
            get { return _category == ItemCategoryType.Tool; }
        }

        public bool IsWeapon
        {
            get { return _category == ItemCategoryType.Weapon; }
        }

        public bool IsArmor
        {
            get { return _category == ItemCategoryType.Armor; }
        }

        public bool IsBackpack
        {
            get { return _category == ItemCategoryType.Backpack; }
        }

        public bool IsConsumable
        {
            get { return _category == ItemCategoryType.Consumable; }
        }

        public bool IsMaterial
        {
            get { return _category == ItemCategoryType.Material; }
        }

        public bool IsEquipmentCategory
        {
            get { return ItemCategoryUtility.IsEquipment(_category); }
        }

        public bool CanStackWith(ItemDefinition other)
        {
            if (other == null)
            {
                return false;
            }

            return string.Equals(_itemCode, other._itemCode, StringComparison.Ordinal) && IsStackable;
        }

        public float TotalWeightFor(int quantity)
        {
            return Mathf.Max(0, quantity) * _weight;
        }

        private void OnValidate()
        {
            _gridWidth = Mathf.Clamp(_gridWidth, 1, 5);
            _gridHeight = Mathf.Clamp(_gridHeight, 1, 5);
            _weight = Mathf.Max(0f, _weight);
            _buyPrice = Mathf.Max(0, _buyPrice);
            _sellPrice = Mathf.Max(0, _sellPrice);

            // Apply stacking rules according to GDD specification
            if (_isUnique || ItemCategoryUtility.IsEquipment(_category) || _category == ItemCategoryType.Tool ||
                _category == ItemCategoryType.Deployable || _category == ItemCategoryType.Container ||
                _category == ItemCategoryType.Special)
            {
                _isStackable = false;
                _maxStack = 1;
            }
            else
            {
                _maxStack = Mathf.Max(1, _maxStack);
            }

            if (_resourceProperties != null)
            {
                _resourceProperties.Validate();
            }

            if (_toolProperties != null)
            {
                _toolProperties.Validate();
            }

            if (_weaponProperties != null)
            {
                _weaponProperties.Validate();
            }

            if (_armorProperties != null)
            {
                _armorProperties.Validate();
            }

            if (_consumableProperties != null)
            {
                _consumableProperties.Validate();
            }

            if (_backpackProperties != null)
            {
                _backpackProperties.Validate();
            }

            if (_equipmentProperties != null)
            {
                _equipmentProperties.Validate();
            }
        }
    }
}
