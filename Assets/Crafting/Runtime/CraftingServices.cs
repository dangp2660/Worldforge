using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;
using Worldforge.Inventory;
using Worldforge.Inventory.Services;
using Worldforge.Item;

namespace Worldforge.Crafting
{
    /// <summary>
    /// Default runtime implementation of ICraftingService.
    /// Manages recipe registry, validation, and crafting transaction execution.
    /// </summary>
    public sealed class RuntimeCraftingService : ICraftingService
    {
        private readonly Dictionary<string, RecipeDefinition> _recipesByCode =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _unlockedRecipeCodes =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<RecipeType, List<RecipeDefinition>> _recipesByCategory = new();
        private readonly Dictionary<CraftingStationType, List<RecipeDefinition>> _recipesByStation = new();
        private readonly Dictionary<ItemDefinition, List<RecipeDefinition>> _recipesByOutput = new();
        private readonly List<RecipeDefinition> _allRecipesList = new();

        public event Action<RecipeDefinition> RecipeRegistered;
        public event Action<string> RecipeUnlocked;
        public event Action<CraftingResult> CraftingCompleted;

        public int RegisteredRecipeCount
        {
            get { return _recipesByCode.Count; }
        }

        public void RegisterRecipe(RecipeDefinition recipe)
        {
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.RecipeCode))
            {
                return;
            }

            if (_recipesByCode.ContainsKey(recipe.RecipeCode))
            {
                UnregisterRecipe(recipe.RecipeCode);
            }

            _recipesByCode[recipe.RecipeCode] = recipe;
            _allRecipesList.Add(recipe);

            // Register by category
            if (!_recipesByCategory.TryGetValue(recipe.RecipeType, out var categoryList))
            {
                categoryList = new List<RecipeDefinition>();
                _recipesByCategory[recipe.RecipeType] = categoryList;
            }
            categoryList.Add(recipe);

            // Register by station
            if (!_recipesByStation.TryGetValue(recipe.StationType, out var stationList))
            {
                stationList = new List<RecipeDefinition>();
                _recipesByStation[recipe.StationType] = stationList;
            }
            stationList.Add(recipe);

            // Register by outputs
            if (recipe.Outputs != null)
            {
                for (var i = 0; i < recipe.Outputs.Count; i++)
                {
                    var outItem = recipe.Outputs[i]?.Item;
                    if (outItem != null)
                    {
                        if (!_recipesByOutput.TryGetValue(outItem, out var outputList))
                        {
                            outputList = new List<RecipeDefinition>();
                            _recipesByOutput[outItem] = outputList;
                        }
                        if (!outputList.Contains(recipe))
                        {
                            outputList.Add(recipe);
                        }
                    }
                }
            }

            if (recipe.IsUnlockedByDefault)
            {
                _unlockedRecipeCodes.Add(recipe.RecipeCode);
            }

            RecipeRegistered?.Invoke(recipe);
        }

        public bool UnregisterRecipe(string recipeCode)
        {
            if (string.IsNullOrWhiteSpace(recipeCode) || !_recipesByCode.TryGetValue(recipeCode, out var recipe))
            {
                return false;
            }

            _recipesByCode.Remove(recipeCode);
            _unlockedRecipeCodes.Remove(recipeCode);
            _allRecipesList.Remove(recipe);

            if (_recipesByCategory.TryGetValue(recipe.RecipeType, out var categoryList))
            {
                categoryList.Remove(recipe);
            }

            if (_recipesByStation.TryGetValue(recipe.StationType, out var stationList))
            {
                stationList.Remove(recipe);
            }

            if (recipe.Outputs != null)
            {
                for (var i = 0; i < recipe.Outputs.Count; i++)
                {
                    var outItem = recipe.Outputs[i]?.Item;
                    if (outItem != null && _recipesByOutput.TryGetValue(outItem, out var outputList))
                    {
                        outputList.Remove(recipe);
                    }
                }
            }

            return true;
        }

        public bool ContainsRecipe(string recipeCode)
        {
            return !string.IsNullOrWhiteSpace(recipeCode) && _recipesByCode.ContainsKey(recipeCode);
        }

        public RecipeDefinition GetRecipeByCode(string recipeCode)
        {
            if (string.IsNullOrWhiteSpace(recipeCode))
            {
                return null;
            }

            _recipesByCode.TryGetValue(recipeCode, out var recipe);
            return recipe;
        }

        public IReadOnlyList<RecipeDefinition> GetAllRecipes()
        {
            return _allRecipesList;
        }

        public IReadOnlyList<RecipeDefinition> GetUnlockedRecipes()
        {
            var unlocked = new List<RecipeDefinition>();
            for (var i = 0; i < _allRecipesList.Count; i++)
            {
                var recipe = _allRecipesList[i];
                if (recipe != null && _unlockedRecipeCodes.Contains(recipe.RecipeCode))
                {
                    unlocked.Add(recipe);
                }
            }

            return unlocked;
        }

        public IReadOnlyList<RecipeDefinition> GetRecipesByCategory(RecipeType category)
        {
            return _recipesByCategory.TryGetValue(category, out var list)
                ? list
                : Array.Empty<RecipeDefinition>();
        }

        public IReadOnlyList<RecipeDefinition> GetRecipesByStation(CraftingStationType station)
        {
            return _recipesByStation.TryGetValue(station, out var list)
                ? list
                : Array.Empty<RecipeDefinition>();
        }

        public IReadOnlyList<RecipeDefinition> GetRecipesForOutput(ItemDefinition outputItem)
        {
            if (outputItem == null)
            {
                return Array.Empty<RecipeDefinition>();
            }

            return _recipesByOutput.TryGetValue(outputItem, out var list)
                ? list
                : Array.Empty<RecipeDefinition>();
        }

        public bool IsRecipeUnlocked(string recipeCode)
        {
            return !string.IsNullOrWhiteSpace(recipeCode) && _unlockedRecipeCodes.Contains(recipeCode);
        }

        public bool UnlockRecipe(string recipeCode)
        {
            if (string.IsNullOrWhiteSpace(recipeCode) || !_recipesByCode.ContainsKey(recipeCode))
            {
                return false;
            }

            if (_unlockedRecipeCodes.Add(recipeCode))
            {
                RecipeUnlocked?.Invoke(recipeCode);
                return true;
            }

            return false;
        }

        public CraftingValidationResult ValidateCraft(
            RecipeDefinition recipe,
            IInventoryContainer inventory,
            CraftingStationType currentStation = CraftingStationType.None,
            int characterLevel = 1)
        {
            if (recipe == null)
            {
                return CraftingValidationResult.Failure(
                    CraftingFailureReason.NullRecipe,
                    "Recipe definition is null.");
            }

            if (!recipe.IsValid(out var validationError))
            {
                return CraftingValidationResult.Failure(
                    CraftingFailureReason.InvalidRecipeDefinition,
                    $"Recipe definition '{recipe.name}' is invalid: {validationError}");
            }

            if (characterLevel < recipe.RequiredLevel)
            {
                return CraftingValidationResult.Failure(
                    CraftingFailureReason.InsufficientLevel,
                    $"Character level {characterLevel} is lower than required level {recipe.RequiredLevel} for recipe '{recipe.DisplayName}'.");
            }

            if (recipe.StationType != CraftingStationType.None && recipe.StationType != currentStation)
            {
                return CraftingValidationResult.Failure(
                    CraftingFailureReason.StationRequired,
                    $"Recipe '{recipe.DisplayName}' requires station '{recipe.StationType}' but current station is '{currentStation}'.");
            }

            if (!IsRecipeUnlocked(recipe.RecipeCode))
            {
                return CraftingValidationResult.Failure(
                    CraftingFailureReason.RecipeLocked,
                    $"Recipe '{recipe.DisplayName}' ({recipe.RecipeCode}) is currently locked.");
            }

            if (inventory == null)
            {
                return CraftingValidationResult.Failure(
                    CraftingFailureReason.InventoryNull,
                    "Target inventory container is null.");
            }

            // Aggregate ingredient requirements across list in case same item appears multiple times
            var requiredTotals = new Dictionary<ItemDefinition, int>();
            for (var i = 0; i < recipe.Ingredients.Count; i++)
            {
                var ingredient = recipe.Ingredients[i];
                if (ingredient?.Item == null)
                {
                    continue;
                }

                if (requiredTotals.TryGetValue(ingredient.Item, out var currentAmount))
                {
                    requiredTotals[ingredient.Item] = currentAmount + ingredient.Amount;
                }
                else
                {
                    requiredTotals[ingredient.Item] = ingredient.Amount;
                }
            }

            foreach (var kvp in requiredTotals)
            {
                var item = kvp.Key;
                var requiredAmount = kvp.Value;
                var availableAmount = inventory.GetItemCount(item);

                if (availableAmount < requiredAmount)
                {
                    return CraftingValidationResult.Failure(
                        CraftingFailureReason.MissingIngredients,
                        $"Insufficient ingredient '{item.DisplayName}': requires {requiredAmount}, but inventory only contains {availableAmount}.",
                        item,
                        requiredAmount,
                        availableAmount);
                }
            }

            // Verify inventory capacity for output items
            if (recipe.Outputs != null)
            {
                for (var i = 0; i < recipe.Outputs.Count; i++)
                {
                    var output = recipe.Outputs[i];
                    if (output?.Item != null && !inventory.CanAcceptItem(output.Item, output.Amount))
                    {
                        return CraftingValidationResult.Failure(
                            CraftingFailureReason.InsufficientInventorySpace,
                            $"Inventory does not have enough capacity to receive {output.Amount}x '{output.Item.DisplayName}'.",
                            output.Item,
                            output.Amount,
                            0);
                    }
                }
            }

            return CraftingValidationResult.Success();
        }

        public CraftingResult ExecuteCraft(
            RecipeDefinition recipe,
            IInventoryContainer inventory,
            CraftingStationType currentStation = CraftingStationType.None,
            int characterLevel = 1)
        {
            var validation = ValidateCraft(recipe, inventory, currentStation, characterLevel);
            if (!validation.IsValid)
            {
                return CraftingResult.Failure(validation.FailureReason, validation.Message, recipe);
            }

            var consumedItems = new List<ItemStack>();
            var producedItems = new List<ItemStack>();

            // Consume ingredients
            for (var i = 0; i < recipe.Ingredients.Count; i++)
            {
                var ingredient = recipe.Ingredients[i];
                if (ingredient == null || ingredient.Item == null)
                {
                    continue;
                }

                if (ingredient.IsConsumed)
                {
                    var removed = inventory.RemoveItem(ingredient.Item, ingredient.Amount);
                    if (!removed)
                    {
                        // Rollback previously consumed ingredients
                        for (var c = 0; c < consumedItems.Count; c++)
                        {
                            inventory.AddItem(consumedItems[c]);
                        }

                        return CraftingResult.Failure(
                            CraftingFailureReason.MissingIngredients,
                            $"Failed to remove ingredient '{ingredient.Item.DisplayName}' during transaction.",
                            recipe);
                    }

                    consumedItems.Add(new ItemStack(ingredient.Item, ingredient.Amount));
                }
            }

            // Produce outputs
            if (recipe.Outputs != null)
            {
                for (var i = 0; i < recipe.Outputs.Count; i++)
                {
                    var output = recipe.Outputs[i];
                    if (output?.Item == null)
                    {
                        continue;
                    }

                    var shouldYield = output.Probability >= 1.0f || UnityEngine.Random.value <= output.Probability;
                    if (shouldYield)
                    {
                        var remaining = inventory.AddItem(output.Item, output.Amount);
                        var actualProduced = output.Amount - remaining;
                        if (actualProduced > 0)
                        {
                            producedItems.Add(new ItemStack(output.Item, actualProduced));
                        }
                    }
                }
            }

            var result = CraftingResult.Success(recipe, producedItems, consumedItems);
            CraftingCompleted?.Invoke(result);
            return result;
        }
    }

    /// <summary>
    /// Service registration provider injecting ICraftingService into the Core DI container.
    /// </summary>
    public sealed class CraftingServiceRegistrationProvider : IServiceRegistrationProvider
    {
        public int Order
        {
            get { return 115; }
        }

        public void RegisterServices(ApplicationBootstrapContext context, IServiceRegistry services)
        {
            services.AddScoped<ICraftingService>(_ => new RuntimeCraftingService());
        }
    }

    /// <summary>
    /// Game session system provider instantiating CraftingInitializationSystem during session startup.
    /// </summary>
    public sealed class CraftingInitializationSystemProvider : IGameSessionSystemProvider
    {
        public int Order
        {
            get { return 115; }
        }

        public IEnumerable<IGameSessionSystem> CreateSystems()
        {
            return new IGameSessionSystem[]
            {
                new CraftingInitializationSystem()
            };
        }
    }

    /// <summary>
    /// GameSession system initializing crafting service, preloading recipe definitions, and registering runtime state.
    /// </summary>
    internal sealed class CraftingInitializationSystem : IGameSessionSystem
    {
        private static readonly IReadOnlyList<string> DependenciesList =
            new[] { "Gameplay.Inventory" };

        private ICraftingService _craftingService;

        public string Name
        {
            get { return "Gameplay.Crafting"; }
        }

        public int Order
        {
            get { return 115; }
        }

        public IReadOnlyList<string> Dependencies
        {
            get { return DependenciesList; }
        }

        public void Initialize(GameSessionContext context)
        {
            context.Services.Resolve<IInventoryService>();
            _craftingService = context.Services.Resolve<ICraftingService>();
            var logger = context.Services.TryResolve<ILogService>(out var resolvedLogger) ? resolvedLogger : null;

            // Preload all RecipeDefinition assets in Resources
            var preloadedRecipes = UnityEngine.Resources.LoadAll<RecipeDefinition>("Definitions/Recipes");
            if (preloadedRecipes != null)
            {
                for (var i = 0; i < preloadedRecipes.Length; i++)
                {
                    if (preloadedRecipes[i] != null && !string.IsNullOrWhiteSpace(preloadedRecipes[i].RecipeCode))
                    {
                        _craftingService.RegisterRecipe(preloadedRecipes[i]);
                    }
                }
            }

            context.RecordRuntimeState("crafting.serviceLifetime", ServiceLifetime.Scoped.ToString());
            context.RecordRuntimeState(
                "crafting.registeredRecipeCount",
                _craftingService.RegisteredRecipeCount.ToString(CultureInfo.InvariantCulture));
            context.RecordRuntimeState(
                "crafting.unlockedRecipeCount",
                _craftingService.GetUnlockedRecipes().Count.ToString(CultureInfo.InvariantCulture));

            logger?.Info(
                "Gameplay.Crafting",
                $"Crafting gameplay module initialized with {_craftingService.RegisteredRecipeCount} recipe definitions.");
        }

        public void Shutdown(GameSessionContext context)
        {
            if (_craftingService != null)
            {
                context.RecordRuntimeState(
                    "crafting.registeredRecipeCount",
                    _craftingService.RegisteredRecipeCount.ToString(CultureInfo.InvariantCulture));
                context.RecordRuntimeState(
                    "crafting.unlockedRecipeCount",
                    _craftingService.GetUnlockedRecipes().Count.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
