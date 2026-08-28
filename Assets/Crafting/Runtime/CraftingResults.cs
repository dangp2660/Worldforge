using System;
using System.Collections.Generic;
using Worldforge.Item;

namespace Worldforge.Crafting
{
    public enum CraftingFailureReason
    {
        None = 0,
        NullRecipe = 1,
        InvalidRecipeDefinition = 2,
        RecipeLocked = 3,
        InsufficientLevel = 4,
        StationRequired = 5,
        MissingIngredients = 6,
        InsufficientInventorySpace = 7,
        InventoryNull = 8,
        CraftFailed = 9
    }

    public readonly struct CraftingValidationResult
    {
        public bool IsValid { get; }
        public CraftingFailureReason FailureReason { get; }
        public string Message { get; }
        public ItemDefinition MissingItem { get; }
        public int RequiredAmount { get; }
        public int AvailableAmount { get; }

        private CraftingValidationResult(
            bool isValid,
            CraftingFailureReason failureReason,
            string message,
            ItemDefinition missingItem = null,
            int requiredAmount = 0,
            int availableAmount = 0)
        {
            IsValid = isValid;
            FailureReason = failureReason;
            Message = message ?? string.Empty;
            MissingItem = missingItem;
            RequiredAmount = requiredAmount;
            AvailableAmount = availableAmount;
        }

        public static CraftingValidationResult Success()
        {
            return new CraftingValidationResult(true, CraftingFailureReason.None, "Validation succeeded.");
        }

        public static CraftingValidationResult Failure(
            CraftingFailureReason reason,
            string message,
            ItemDefinition missingItem = null,
            int requiredAmount = 0,
            int availableAmount = 0)
        {
            return new CraftingValidationResult(false, reason, message, missingItem, requiredAmount, availableAmount);
        }
    }

    public sealed class CraftingResult
    {
        public bool IsSuccess { get; }
        public CraftingFailureReason FailureReason { get; }
        public string Message { get; }
        public RecipeDefinition Recipe { get; }
        public IReadOnlyList<ItemStack> ProducedItems { get; }
        public IReadOnlyList<ItemStack> ConsumedIngredients { get; }

        private CraftingResult(
            bool isSuccess,
            CraftingFailureReason failureReason,
            string message,
            RecipeDefinition recipe,
            IReadOnlyList<ItemStack> producedItems,
            IReadOnlyList<ItemStack> consumedIngredients)
        {
            IsSuccess = isSuccess;
            FailureReason = failureReason;
            Message = message ?? string.Empty;
            Recipe = recipe;
            ProducedItems = producedItems ?? Array.Empty<ItemStack>();
            ConsumedIngredients = consumedIngredients ?? Array.Empty<ItemStack>();
        }

        public static CraftingResult Success(
            RecipeDefinition recipe,
            IReadOnlyList<ItemStack> producedItems,
            IReadOnlyList<ItemStack> consumedIngredients)
        {
            return new CraftingResult(
                true,
                CraftingFailureReason.None,
                "Crafting succeeded.",
                recipe,
                producedItems,
                consumedIngredients);
        }

        public static CraftingResult Failure(
            CraftingFailureReason reason,
            string message,
            RecipeDefinition recipe = null)
        {
            return new CraftingResult(
                false,
                reason,
                message,
                recipe,
                Array.Empty<ItemStack>(),
                Array.Empty<ItemStack>());
        }
    }
}
