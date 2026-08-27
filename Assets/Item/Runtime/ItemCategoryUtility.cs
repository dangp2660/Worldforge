using UnityEngine;

namespace Worldforge.Item
{
    /// <summary>
    /// Utility methods and classification rules for item categories based on GDD specification.
    /// </summary>
    public static class ItemCategoryUtility
    {
        public static bool IsEquipment(ItemCategoryType category)
        {
            return category switch
            {
                ItemCategoryType.Equipment => true,
                ItemCategoryType.Weapon => true,
                ItemCategoryType.Armor => true,
                ItemCategoryType.Backpack => true,
                _ => false
            };
        }

        public static bool IsStackableByDefault(ItemCategoryType category)
        {
            return category switch
            {
                ItemCategoryType.Resource => true,
                ItemCategoryType.Material => true,
                ItemCategoryType.Consumable => true,
                _ => false
            };
        }

        public static int GetDefaultMaxStack(ItemCategoryType category)
        {
            return category switch
            {
                ItemCategoryType.Resource => 100,
                ItemCategoryType.Material => 50,
                ItemCategoryType.Consumable => 20,
                _ => 1
            };
        }

        public static Vector2Int GetDefaultGridSize(ItemCategoryType category)
        {
            return category switch
            {
                ItemCategoryType.Resource => new Vector2Int(1, 1),
                ItemCategoryType.Material => new Vector2Int(1, 1),
                ItemCategoryType.Tool => new Vector2Int(1, 2),
                ItemCategoryType.Weapon => new Vector2Int(1, 3),
                ItemCategoryType.Armor => new Vector2Int(2, 2),
                ItemCategoryType.Backpack => new Vector2Int(2, 2),
                ItemCategoryType.Container => new Vector2Int(2, 2),
                ItemCategoryType.Deployable => new Vector2Int(2, 2),
                ItemCategoryType.Consumable => new Vector2Int(1, 1),
                ItemCategoryType.Quest => new Vector2Int(1, 1),
                ItemCategoryType.Special => new Vector2Int(1, 1),
                _ => new Vector2Int(1, 1)
            };
        }
    }
}
