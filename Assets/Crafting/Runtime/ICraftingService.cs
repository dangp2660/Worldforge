using System;
using System.Collections.Generic;
using Worldforge.Inventory;
using Worldforge.Item;

namespace Worldforge.Crafting
{
    /// <summary>
    /// Service interface managing crafting recipe definitions, validation, and crafting execution.
    /// Corresponds to Schema Part 5: Crafting & PRD Section 9: FR-CRAFT-001/002.
    /// </summary>
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
        bool UnlockRecipe(string recipeCode);

        CraftingValidationResult ValidateCraft(
            RecipeDefinition recipe,
            IInventoryContainer inventory,
            CraftingStationType currentStation = CraftingStationType.None,
            int characterLevel = 1);

        CraftingResult ExecuteCraft(
            RecipeDefinition recipe,
            IInventoryContainer inventory,
            CraftingStationType currentStation = CraftingStationType.None,
            int characterLevel = 1);
    }
}
