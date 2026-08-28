using System;
using UnityEngine;
using Worldforge.Item;

namespace Worldforge.Crafting
{
    /// <summary>
    /// Represents an input resource requirement for crafting a recipe.
    /// Corresponds to Schema Part 5: RecipeIngredient.
    /// </summary>
    [Serializable]
    public sealed class RecipeIngredientEntry
    {
        [SerializeField] private ItemDefinition _item;
        [SerializeField, Min(1)] private int _amount = 1;
        [SerializeField] private bool _isConsumed = true;

        public RecipeIngredientEntry()
        {
            _amount = 1;
            _isConsumed = true;
        }

        public RecipeIngredientEntry(ItemDefinition item, int amount, bool isConsumed = true)
        {
            _item = item;
            _amount = Mathf.Max(1, amount);
            _isConsumed = isConsumed;
        }

        public ItemDefinition Item
        {
            get { return _item; }
        }

        public int Amount
        {
            get { return _amount; }
        }

        public bool IsConsumed
        {
            get { return _isConsumed; }
        }

        public void Validate()
        {
            _amount = Mathf.Max(1, _amount);
        }
    }
}
