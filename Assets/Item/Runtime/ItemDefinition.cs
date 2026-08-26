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

        [Header("Economy & Weight")]
        [SerializeField] private float _weight = 0.5f;
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

        public float Weight
        {
            get { return _weight; }
        }

        public int MaxStack
        {
            get { return _maxStack; }
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

        public bool IsResource
        {
            get { return _category == ItemCategoryType.Resource; }
        }

        public bool IsTool
        {
            get { return _category == ItemCategoryType.Tool; }
        }

        private void OnValidate()
        {
            _weight = Mathf.Max(0f, _weight);
            _maxStack = Mathf.Max(1, _maxStack);
            _buyPrice = Mathf.Max(0, _buyPrice);
            _sellPrice = Mathf.Max(0, _sellPrice);

            if (_resourceProperties != null)
            {
                _resourceProperties.Validate();
            }

            if (_toolProperties != null)
            {
                _toolProperties.Validate();
            }
        }
    }
}
