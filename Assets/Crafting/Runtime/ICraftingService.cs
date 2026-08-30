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

        [TestMethod(DisplayName = "Register Recipe", IsPrimary = false, Description = "Registers a recipe definition at runtime")]
        void RegisterRecipe(RecipeDefinition recipe);

        [TestMethod(DisplayName = "Unregister Recipe", IsPrimary = false)]
        bool UnregisterRecipe(string recipeCode);

        [TestMethod(DisplayName = "Contains Recipe", IsPrimary = false)]
        bool ContainsRecipe(string recipeCode);

        [TestMethod(DisplayName = "Get Recipe By Code", IsPrimary = false)]
        RecipeDefinition GetRecipeByCode(string recipeCode);

        [TestMethod(DisplayName = "Get All Recipes", IsPrimary = false)]
        IReadOnlyList<RecipeDefinition> GetAllRecipes();

        [TestMethod(DisplayName = "Get Unlocked Recipes", IsPrimary = false)]
        IReadOnlyList<RecipeDefinition> GetUnlockedRecipes();

        [TestMethod(DisplayName = "Get Recipes By Category", IsPrimary = false)]
        IReadOnlyList<RecipeDefinition> GetRecipesByCategory(RecipeType category);

        [TestMethod(DisplayName = "Get Recipes By Station", IsPrimary = false)]
        IReadOnlyList<RecipeDefinition> GetRecipesByStation(CraftingStationType station);

        [TestMethod(DisplayName = "Get Recipes For Output", IsPrimary = false)]
        IReadOnlyList<RecipeDefinition> GetRecipesForOutput(ItemDefinition outputItem);

        [TestMethod(DisplayName = "Is Recipe Unlocked", IsPrimary = false)]
        bool IsRecipeUnlocked(string recipeCode);

        [TestMethod(DisplayName = "Unlock Recipe", IsPrimary = true, Order = 3, Description = "Unlocks a recipe code")]
        bool UnlockRecipe(string recipeCode);

        [TestMethod(DisplayName = "Validate Craft", IsPrimary = true, Order = 2, Description = "Validates ingredients, station, and level without consuming items")]
        CraftingValidationResult ValidateCraft(
            RecipeDefinition recipe,
            IInventoryContainer inventory,
            CraftingStationType currentStation = CraftingStationType.None,
            int characterLevel = 1);

        [TestMethod(DisplayName = "Execute Craft", IsPrimary = true, Order = 1, Description = "Executes recipe crafting transaction, consumes ingredients, and yields outputs")]
        CraftingResult ExecuteCraft(
            RecipeDefinition recipe,
            IInventoryContainer inventory,
            CraftingStationType currentStation = CraftingStationType.None,
            int characterLevel = 1);
    }
}
