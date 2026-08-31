using System;
using System.Collections.Generic;
using Worldforge.Core.Attributes;
using Worldforge.Inventory;
using Worldforge.Item;

namespace Worldforge.Crafting
{
    /// <summary>
    /// Service interface managing crafting recipe definitions, validation, and crafting execution.
    /// Corresponds to Schema Part 5: Crafting & PRD Section 9: FR-CRAFT-001/002.
    /// </summary>
    [TestTarget(Category = "Crafting", DisplayName = "Crafting Service", Order = 10)]
    public interface ICraftingService
    {
        int RegisteredRecipeCount { get; }

        event Action<RecipeDefinition> RecipeRegistered;
        event Action<string> RecipeUnlocked;
        event Action<CraftingResult> CraftingCompleted;

        void RegisterRecipe(RecipeDefinition recipe);

        bool UnregisterRecipe(string recipeCode);

        bool ContainsRecipe(string recipeCode);

        RecipeDefinition GetRecipeByCode(string recipeCode);

        IReadOnlyList<RecipeDefinition> GetAllRecipes();

        IReadOnlyList<RecipeDefinition> GetUnlockedRecipes();

        IReadOnlyList<RecipeDefinition> GetRecipesByCategory(RecipeType category);

        IReadOnlyList<RecipeDefinition> GetRecipesByStation(CraftingStationType station);

        IReadOnlyList<RecipeDefinition> GetRecipesForOutput(ItemDefinition outputItem);

        bool IsRecipeUnlocked(string recipeCode);

        [TestMethod(DisplayName = "Unlock Recipe", Order = 3, Description = "Unlocks a recipe code")]
        bool UnlockRecipe(string recipeCode);

        [TestMethod(DisplayName = "Validate Craft", Order = 2, Description = "Validates ingredients, station, and level without consuming items")]
        CraftingValidationResult ValidateCraft(
            RecipeDefinition recipe,
            IInventoryContainer inventory,
            CraftingStationType currentStation = CraftingStationType.None,
            int characterLevel = 1);

        [TestMethod(DisplayName = "Execute Craft", Order = 1, Description = "Executes recipe crafting transaction, consumes ingredients, and yields outputs")]
        CraftingResult ExecuteCraft(
            RecipeDefinition recipe,
            IInventoryContainer inventory,
            CraftingStationType currentStation = CraftingStationType.None,
            int characterLevel = 1);
    }
}
