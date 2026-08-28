using System;
using System.Collections.Generic;
using UnityEngine;
using Worldforge.Item;

namespace Worldforge.Crafting
{
    /// <summary>
    /// ScriptableObject defining the specifications and requirements for a crafting recipe.
    /// Corresponds to Schema Part 5: RecipeDefinition.
    /// </summary>
    [CreateAssetMenu(
        fileName = "RecipeDefinition",
        menuName = "Worldforge/Crafting/Recipe Definition")]
    public sealed class RecipeDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _recipeCode = "RECIPE_DEFAULT";
        [SerializeField] private string _displayName = "New Recipe";
        [SerializeField, TextArea(2, 4)] private string _description = string.Empty;

        [Header("Classification & Station")]
        [SerializeField] private RecipeType _recipeType = RecipeType.Basic;
        [SerializeField] private CraftingStationType _stationType = CraftingStationType.None;
        [SerializeField] private string _craftFunction = string.Empty;

        [Header("Crafting Process & Requirements")]
        [SerializeField, Min(0f)] private float _craftTime = 0f;
        [SerializeField, Min(1)] private int _requiredLevel = 1;
        [SerializeField, Range(0f, 1f)] private float _successRate = 1.0f;
        [SerializeField] private bool _isUnlockedByDefault = true;

        [Header("Input Ingredients")]
        [SerializeField] private List<RecipeIngredientEntry> _ingredients = new();

        [Header("Output Yields")]
        [SerializeField] private List<RecipeOutputEntry> _outputs = new();

        public string RecipeCode
        {
            get { return _recipeCode; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public string Description
        {
            get { return _description; }
        }

        public RecipeType RecipeType
        {
            get { return _recipeType; }
        }

        public CraftingStationType StationType
        {
            get { return _stationType; }
        }

        public string CraftFunction
        {
            get { return _craftFunction; }
        }

        public float CraftTime
        {
            get { return _craftTime; }
        }

        public int RequiredLevel
        {
            get { return _requiredLevel; }
        }

        public float SuccessRate
        {
            get { return _successRate; }
        }

        public bool IsUnlockedByDefault
        {
            get { return _isUnlockedByDefault; }
        }

        public IReadOnlyList<RecipeIngredientEntry> Ingredients
        {
            get { return _ingredients; }
        }

        public IReadOnlyList<RecipeOutputEntry> Outputs
        {
            get { return _outputs; }
        }

        public RecipeOutputEntry PrimaryOutput
        {
            get { return _outputs != null && _outputs.Count > 0 ? _outputs[0] : null; }
        }

        public ItemDefinition PrimaryOutputItem
        {
            get { return PrimaryOutput?.Item; }
        }

        public int PrimaryOutputAmount
        {
            get { return PrimaryOutput?.Amount ?? 0; }
        }

        public bool HasIngredient(ItemDefinition item)
        {
            if (item == null || _ingredients == null)
            {
                return false;
            }

            for (var i = 0; i < _ingredients.Count; i++)
            {
                if (_ingredients[i]?.Item == item)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetRequiredAmount(ItemDefinition item)
        {
            if (item == null || _ingredients == null)
            {
                return 0;
            }

            var total = 0;
            for (var i = 0; i < _ingredients.Count; i++)
            {
                if (_ingredients[i]?.Item == item)
                {
                    total += _ingredients[i].Amount;
                }
            }

            return total;
        }

        public bool IsValid(out string reason)
        {
            if (string.IsNullOrWhiteSpace(_recipeCode))
            {
                reason = "RecipeCode is null or empty.";
                return false;
            }

            if (_ingredients == null || _ingredients.Count == 0)
            {
                reason = $"Recipe '{_recipeCode}' has no ingredients.";
                return false;
            }

            for (var i = 0; i < _ingredients.Count; i++)
            {
                var ingredient = _ingredients[i];
                if (ingredient == null || ingredient.Item == null)
                {
                    reason = $"Recipe '{_recipeCode}' has null ingredient at index {i}.";
                    return false;
                }

                if (ingredient.Amount <= 0)
                {
                    reason = $"Recipe '{_recipeCode}' ingredient '{ingredient.Item.DisplayName}' has non-positive amount {ingredient.Amount}.";
                    return false;
                }
            }

            if (_outputs == null || _outputs.Count == 0)
            {
                reason = $"Recipe '{_recipeCode}' has no output items.";
                return false;
            }

            for (var i = 0; i < _outputs.Count; i++)
            {
                var output = _outputs[i];
                if (output == null || output.Item == null)
                {
                    reason = $"Recipe '{_recipeCode}' has null output item at index {i}.";
                    return false;
                }

                if (output.Amount <= 0)
                {
                    reason = $"Recipe '{_recipeCode}' output '{output.Item.DisplayName}' has non-positive amount {output.Amount}.";
                    return false;
                }
            }

            reason = string.Empty;
            return true;
        }

        private void OnValidate()
        {
            _craftTime = Mathf.Max(0f, _craftTime);
            _requiredLevel = Mathf.Max(1, _requiredLevel);
            _successRate = Mathf.Clamp01(_successRate);

            if (_ingredients != null)
            {
                for (var i = 0; i < _ingredients.Count; i++)
                {
                    _ingredients[i]?.Validate();
                }
            }

            if (_outputs != null)
            {
                for (var i = 0; i < _outputs.Count; i++)
                {
                    _outputs[i]?.Validate();
                }
            }
        }
    }
}
