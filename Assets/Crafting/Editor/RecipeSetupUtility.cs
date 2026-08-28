using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Worldforge.Crafting;
using Worldforge.Inventory;
using Worldforge.Item;

namespace Worldforge.Crafting.Editor
{
    /// <summary>
    /// Editor utility for creating, configuring, and standardizing all recipe definitions for Worldforge v0.1.
    /// </summary>
    public static class RecipeSetupUtility
    {
        private const string RecipesDirectory = "Assets/Resources/Definitions/Recipes";
        private const string ItemsDirectory = "Assets/Resources/Definitions/Items";

        [MenuItem("Worldforge/Setup/Configure All Recipe Definitions", priority = 102)]
        public static void ConfigureAllRecipeDefinitions()
        {
            EnsureDirectoryExists(RecipesDirectory);

            var wood = LoadItem("Item_Resource_Wood");
            var stone = LoadItem("Item_Resource_Stone");
            var fiber = LoadItem("Item_Resource_Fiber");
            var plank = LoadItem("Item_Material_Plank");
            var leather = LoadItem("Item_Material_Leather");
            var axe = LoadItem("Item_Tool_BasicAxe");
            var pickaxe = LoadItem("Item_Tool_BasicPickaxe");
            var sickle = LoadItem("Item_Tool_BasicSickle");
            var potion = LoadItem("Item_Consumable_HealthPotion");
            var bow = LoadItem("Item_Weapon_WoodenBow");
            var backpack = LoadItem("Item_Backpack_LeatherBackpack");
            var armor = LoadItem("Item_Armor_LeatherChest");

            if (wood == null || stone == null || fiber == null || plank == null ||
                leather == null || axe == null || pickaxe == null || sickle == null ||
                potion == null || bow == null || backpack == null || armor == null)
            {
                Debug.LogError("[RecipeSetupUtility] Failed to load one or more item definitions from 'Assets/Resources/Definitions/Items'. Aborting recipe configuration.");
                return;
            }

            var count = 0;

            // 1. Material: Plank (1 Wood -> 2 Planks)
            ConfigureRecipe(
                "Recipe_Material_Plank",
                "RECIPE_MAT_PLANK",
                "Refined Wood Plank",
                "Process raw wood logs into refined timber planks for construction and crafting.",
                RecipeType.Material,
                CraftingStationType.None,
                craftTime: 0f,
                requiredLevel: 1,
                successRate: 1.0f,
                isUnlockedByDefault: true,
                ingredients: new[] { new RecipeIngredientEntry(wood, 1, isConsumed: true) },
                outputs: new[] { new RecipeOutputEntry(plank, 2, probability: 1.0f) });
            count++;

            // 2. Tool: Crude Stone Axe (3 Wood + 2 Stone + 2 Fiber -> 1 Axe)
            ConfigureRecipe(
                "Recipe_Tool_BasicAxe",
                "RECIPE_TOOL_AXE_01",
                "Crude Stone Axe",
                "Assemble a primitive stone axe for cutting trees and logging timber.",
                RecipeType.Tool,
                CraftingStationType.None,
                craftTime: 0f,
                requiredLevel: 1,
                successRate: 1.0f,
                isUnlockedByDefault: true,
                ingredients: new[]
                {
                    new RecipeIngredientEntry(wood, 3, isConsumed: true),
                    new RecipeIngredientEntry(stone, 2, isConsumed: true),
                    new RecipeIngredientEntry(fiber, 2, isConsumed: true)
                },
                outputs: new[] { new RecipeOutputEntry(axe, 1, probability: 1.0f) });
            count++;

            // 3. Tool: Crude Stone Pickaxe (3 Wood + 3 Stone + 2 Fiber -> 1 Pickaxe)
            ConfigureRecipe(
                "Recipe_Tool_BasicPickaxe",
                "RECIPE_TOOL_PICKAXE_01",
                "Crude Stone Pickaxe",
                "Assemble a heavy stone pickaxe for breaking boulders and mining stone ore.",
                RecipeType.Tool,
                CraftingStationType.None,
                craftTime: 0f,
                requiredLevel: 1,
                successRate: 1.0f,
                isUnlockedByDefault: true,
                ingredients: new[]
                {
                    new RecipeIngredientEntry(wood, 3, isConsumed: true),
                    new RecipeIngredientEntry(stone, 3, isConsumed: true),
                    new RecipeIngredientEntry(fiber, 2, isConsumed: true)
                },
                outputs: new[] { new RecipeOutputEntry(pickaxe, 1, probability: 1.0f) });
            count++;

            // 4. Tool: Crude Stone Sickle (2 Wood + 1 Stone + 3 Fiber -> 1 Sickle)
            ConfigureRecipe(
                "Recipe_Tool_BasicSickle",
                "RECIPE_TOOL_SICKLE_01",
                "Crude Stone Sickle",
                "Fashion a curved sickle for efficiently reaping fiber and herbs from wild bushes.",
                RecipeType.Tool,
                CraftingStationType.None,
                craftTime: 0f,
                requiredLevel: 1,
                successRate: 1.0f,
                isUnlockedByDefault: true,
                ingredients: new[]
                {
                    new RecipeIngredientEntry(wood, 2, isConsumed: true),
                    new RecipeIngredientEntry(stone, 1, isConsumed: true),
                    new RecipeIngredientEntry(fiber, 3, isConsumed: true)
                },
                outputs: new[] { new RecipeOutputEntry(sickle, 1, probability: 1.0f) });
            count++;

            // 5. Consumable: Health Potion (1 Wood + 2 Fiber -> 1 Health Draught)
            ConfigureRecipe(
                "Recipe_Consumable_HealthPotion",
                "RECIPE_CONS_POTION_HP_01",
                "Minor Health Draught",
                "Brew plant fibers and wood extracts into a restorative herbal health elixir.",
                RecipeType.Consumable,
                CraftingStationType.None,
                craftTime: 0f,
                requiredLevel: 1,
                successRate: 1.0f,
                isUnlockedByDefault: true,
                ingredients: new[]
                {
                    new RecipeIngredientEntry(wood, 1, isConsumed: true),
                    new RecipeIngredientEntry(fiber, 2, isConsumed: true)
                },
                outputs: new[] { new RecipeOutputEntry(potion, 1, probability: 1.0f) });
            count++;

            // 6. Weapon: Hunter Shortbow (3 Wood + 4 Fiber -> 1 Bow)
            ConfigureRecipe(
                "Recipe_Weapon_WoodenBow",
                "RECIPE_WEAP_BOW_01",
                "Hunter Shortbow",
                "String flexible wood and resilient plant fibers into a short hunting bow.",
                RecipeType.Weapon,
                CraftingStationType.None,
                craftTime: 0f,
                requiredLevel: 1,
                successRate: 1.0f,
                isUnlockedByDefault: true,
                ingredients: new[]
                {
                    new RecipeIngredientEntry(wood, 3, isConsumed: true),
                    new RecipeIngredientEntry(fiber, 4, isConsumed: true)
                },
                outputs: new[] { new RecipeOutputEntry(bow, 1, probability: 1.0f) });
            count++;

            // 7. Backpack: Traveler Leather Backpack (4 Leather + 3 Fiber -> 1 Backpack)
            ConfigureRecipe(
                "Recipe_Backpack_LeatherBackpack",
                "RECIPE_PACK_LEATHER_01",
                "Traveler Leather Backpack",
                "Stitch cured leather and fiber binding into an expansive adventurer backpack.",
                RecipeType.Basic,
                CraftingStationType.None,
                craftTime: 0f,
                requiredLevel: 1,
                successRate: 1.0f,
                isUnlockedByDefault: true,
                ingredients: new[]
                {
                    new RecipeIngredientEntry(leather, 4, isConsumed: true),
                    new RecipeIngredientEntry(fiber, 3, isConsumed: true)
                },
                outputs: new[] { new RecipeOutputEntry(backpack, 1, probability: 1.0f) });
            count++;

            // 8. Armor: Hardened Leather Tunic (6 Leather + 4 Fiber -> 1 Leather Chest)
            ConfigureRecipe(
                "Recipe_Armor_LeatherChest",
                "RECIPE_ARM_CHEST_01",
                "Hardened Leather Tunic",
                "Tailor layers of cured leather with fiber stitching into a protective leather chestpiece.",
                RecipeType.Armor,
                CraftingStationType.None,
                craftTime: 0f,
                requiredLevel: 1,
                successRate: 1.0f,
                isUnlockedByDefault: true,
                ingredients: new[]
                {
                    new RecipeIngredientEntry(leather, 6, isConsumed: true),
                    new RecipeIngredientEntry(fiber, 4, isConsumed: true)
                },
                outputs: new[] { new RecipeOutputEntry(armor, 1, probability: 1.0f) });
            count++;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[RecipeSetupUtility] Successfully configured {count} recipe definitions in '{RecipesDirectory}'.");
        }

        [MenuItem("Worldforge/Testing/Run Recipe Validation Checks", priority = 201)]
        public static void RunRecipeValidationChecks()
        {
            Debug.Log("<b><color=#00E5FF>[Worldforge Recipe Test]</color></b> Starting validation checks...");

            // Test 1: Load all recipe definitions from Resources
            var recipes = Resources.LoadAll<RecipeDefinition>("Definitions/Recipes");
            if (recipes == null || recipes.Length == 0)
            {
                Debug.LogError("[Worldforge Recipe Test] Failed: No recipe definitions loaded from 'Definitions/Recipes'.");
                return;
            }

            Debug.Log($"[Worldforge Recipe Test] <color=#00FF66>✓ Test 1/6 PASSED:</color> Loaded {recipes.Length} recipe definitions from Resources.");

            // Test 2: Verify inputs and quantities on all loaded recipes
            for (var i = 0; i < recipes.Length; i++)
            {
                var r = recipes[i];
                if (r == null)
                {
                    Debug.LogError($"[Worldforge Recipe Test] Failed: Recipe definition at index {i} is null.");
                    return;
                }

                if (!r.IsValid(out var error))
                {
                    Debug.LogError($"[Worldforge Recipe Test] Failed: Recipe '{r.name}' validation error: {error}");
                    return;
                }

                if (r.Ingredients == null || r.Ingredients.Count == 0)
                {
                    Debug.LogError($"[Worldforge Recipe Test] Failed: Recipe '{r.RecipeCode}' has no ingredients.");
                    return;
                }

                for (var j = 0; j < r.Ingredients.Count; j++)
                {
                    var ing = r.Ingredients[j];
                    if (ing == null || ing.Item == null || ing.Amount <= 0)
                    {
                        Debug.LogError($"[Worldforge Recipe Test] Failed: Recipe '{r.RecipeCode}' ingredient {j} is invalid.");
                        return;
                    }
                }
            }

            Debug.Log("[Worldforge Recipe Test] <color=#00FF66>✓ Test 2/6 PASSED:</color> All recipe input ingredients and quantities verified valid.");

            // Test 3: Verify outputs and quantities on all loaded recipes
            for (var i = 0; i < recipes.Length; i++)
            {
                var r = recipes[i];
                if (r.Outputs == null || r.Outputs.Count == 0)
                {
                    Debug.LogError($"[Worldforge Recipe Test] Failed: Recipe '{r.RecipeCode}' has no outputs.");
                    return;
                }

                for (var j = 0; j < r.Outputs.Count; j++)
                {
                    var output = r.Outputs[j];
                    if (output == null || output.Item == null || output.Amount <= 0)
                    {
                        Debug.LogError($"[Worldforge Recipe Test] Failed: Recipe '{r.RecipeCode}' output {j} is invalid.");
                        return;
                    }
                }
            }

            Debug.Log("[Worldforge Recipe Test] <color=#00FF66>✓ Test 3/6 PASSED:</color> All recipe output items and quantities verified valid.");

            // Test 4: Register with RuntimeCraftingService and test querying
            var service = new RuntimeCraftingService();
            for (var i = 0; i < recipes.Length; i++)
            {
                service.RegisterRecipe(recipes[i]);
            }

            if (service.RegisteredRecipeCount != recipes.Length)
            {
                Debug.LogError($"[Worldforge Recipe Test] Failed: Registered recipe count mismatch ({service.RegisteredRecipeCount} vs {recipes.Length}).");
                return;
            }

            var axeRecipe = service.GetRecipeByCode("RECIPE_TOOL_AXE_01");
            if (axeRecipe == null || axeRecipe.PrimaryOutputItem?.ItemCode != "TOOL_AXE_01")
            {
                Debug.LogError("[Worldforge Recipe Test] Failed: GetRecipeByCode for RECIPE_TOOL_AXE_01 failed.");
                return;
            }

            var plank = Resources.Load<ItemDefinition>("Definitions/Items/Item_Material_Plank");
            var plankRecipes = service.GetRecipesForOutput(plank);
            if (plankRecipes == null || plankRecipes.Count == 0 || plankRecipes[0].RecipeCode != "RECIPE_MAT_PLANK")
            {
                Debug.LogError("[Worldforge Recipe Test] Failed: GetRecipesForOutput for MAT_PLANK failed.");
                return;
            }

            Debug.Log("[Worldforge Recipe Test] <color=#00FF66>✓ Test 4/6 PASSED:</color> RuntimeCraftingService indexing, code lookup, and output queries verified.");

            // Test 5: Validation with empty inventory (should fail with MissingIngredients)
            var inventory = new InventoryContainer("CraftingTestInventory", 10, 50f);
            var validation = service.ValidateCraft(axeRecipe, inventory);
            if (validation.IsValid || validation.FailureReason != CraftingFailureReason.MissingIngredients)
            {
                Debug.LogError($"[Worldforge Recipe Test] Failed: Empty inventory validation should fail with MissingIngredients, got: {validation.FailureReason}");
                return;
            }

            Debug.Log("[Worldforge Recipe Test] <color=#00FF66>✓ Test 5/6 PASSED:</color> Crafting validation correctly identified missing ingredients.");

            // Test 6: Crafting execution with sufficient ingredients
            var wood = Resources.Load<ItemDefinition>("Definitions/Items/Item_Resource_Wood");
            var stone = Resources.Load<ItemDefinition>("Definitions/Items/Item_Resource_Stone");
            var fiber = Resources.Load<ItemDefinition>("Definitions/Items/Item_Resource_Fiber");

            inventory.AddItem(wood, 10);
            inventory.AddItem(stone, 10);
            inventory.AddItem(fiber, 10);

            var validResult = service.ValidateCraft(axeRecipe, inventory);
            if (!validResult.IsValid)
            {
                Debug.LogError($"[Worldforge Recipe Test] Failed: Validation failed with sufficient items: {validResult.Message}");
                return;
            }

            var craftResult = service.ExecuteCraft(axeRecipe, inventory);
            if (!craftResult.IsSuccess)
            {
                Debug.LogError($"[Worldforge Recipe Test] Failed: Crafting execution failed: {craftResult.Message}");
                return;
            }

            // Verify consumed quantities (3 wood, 2 stone, 2 fiber)
            if (inventory.GetItemCount(wood) != 7 || inventory.GetItemCount(stone) != 8 || inventory.GetItemCount(fiber) != 8)
            {
                Debug.LogError($"[Worldforge Recipe Test] Failed: Ingredient consumption mismatch. Wood: {inventory.GetItemCount(wood)}, Stone: {inventory.GetItemCount(stone)}, Fiber: {inventory.GetItemCount(fiber)}");
                return;
            }

            // Verify output added (1 axe)
            var axeItem = Resources.Load<ItemDefinition>("Definitions/Items/Item_Tool_BasicAxe");
            if (inventory.GetItemCount(axeItem) != 1)
            {
                Debug.LogError($"[Worldforge Recipe Test] Failed: Output item not found in inventory. Axe count: {inventory.GetItemCount(axeItem)}");
                return;
            }

            Debug.Log("[Worldforge Recipe Test] <color=#00FF66>✓ Test 6/6 PASSED:</color> Crafting transaction executed successfully: consumed resources and added output item to inventory.");
            Debug.Log("<b><color=#00FF66>[Worldforge Recipe Test] ALL 6 TESTS PASSED SUCCESSFULLY!</color></b>");
        }

        private static ItemDefinition LoadItem(string assetName)
        {
            var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>($"{ItemsDirectory}/{assetName}.asset");
            if (item == null)
            {
                item = Resources.Load<ItemDefinition>($"Definitions/Items/{assetName}");
            }
            return item;
        }

        private static void ConfigureRecipe(
            string assetName,
            string recipeCode,
            string displayName,
            string description,
            RecipeType recipeType,
            CraftingStationType stationType,
            float craftTime,
            int requiredLevel,
            float successRate,
            bool isUnlockedByDefault,
            IReadOnlyList<RecipeIngredientEntry> ingredients,
            IReadOnlyList<RecipeOutputEntry> outputs)
        {
            var path = $"{RecipesDirectory}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<RecipeDefinition>(path);
            var recipe = existing != null ? existing : ScriptableObject.CreateInstance<RecipeDefinition>();

            var so = new SerializedObject(recipe);

            so.FindProperty("_recipeCode").stringValue = recipeCode;
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_description").stringValue = description;
            so.FindProperty("_recipeType").enumValueIndex = (int)recipeType;
            so.FindProperty("_stationType").enumValueIndex = (int)stationType;
            so.FindProperty("_craftTime").floatValue = craftTime;
            so.FindProperty("_requiredLevel").intValue = requiredLevel;
            so.FindProperty("_successRate").floatValue = successRate;
            so.FindProperty("_isUnlockedByDefault").boolValue = isUnlockedByDefault;

            if (ingredients != null)
            {
                for (var i = 0; i < ingredients.Count; i++)
                {
                    if (ingredients[i]?.Item == null)
                    {
                        Debug.LogError($"[RecipeSetupUtility] Cannot configure recipe '{recipeCode}': ingredient at index {i} has null Item.");
                        return;
                    }
                }
            }

            if (outputs != null)
            {
                for (var i = 0; i < outputs.Count; i++)
                {
                    if (outputs[i]?.Item == null)
                    {
                        Debug.LogError($"[RecipeSetupUtility] Cannot configure recipe '{recipeCode}': output at index {i} has null Item.");
                        return;
                    }
                }
            }

            // Serialize ingredients
            var ingredientsProp = so.FindProperty("_ingredients");
            ingredientsProp.ClearArray();
            if (ingredients != null)
            {
                for (var i = 0; i < ingredients.Count; i++)
                {
                    ingredientsProp.InsertArrayElementAtIndex(i);
                    var entry = ingredientsProp.GetArrayElementAtIndex(i);
                    entry.FindPropertyRelative("_item").objectReferenceValue = ingredients[i].Item;
                    entry.FindPropertyRelative("_amount").intValue = ingredients[i].Amount;
                    entry.FindPropertyRelative("_isConsumed").boolValue = ingredients[i].IsConsumed;
                }
            }

            // Serialize outputs
            var outputsProp = so.FindProperty("_outputs");
            outputsProp.ClearArray();
            if (outputs != null)
            {
                for (var i = 0; i < outputs.Count; i++)
                {
                    outputsProp.InsertArrayElementAtIndex(i);
                    var entry = outputsProp.GetArrayElementAtIndex(i);
                    entry.FindPropertyRelative("_item").objectReferenceValue = outputs[i].Item;
                    entry.FindPropertyRelative("_amount").intValue = outputs[i].Amount;
                    entry.FindPropertyRelative("_probability").floatValue = outputs[i].Probability;
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            if (existing == null)
            {
                AssetDatabase.CreateAsset(recipe, path);
            }
            else
            {
                EditorUtility.SetDirty(recipe);
            }
        }

        private static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                AssetDatabase.Refresh();
            }
        }
    }
}
