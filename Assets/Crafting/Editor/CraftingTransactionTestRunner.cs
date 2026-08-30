using UnityEngine;
using UnityEditor;
using Worldforge.Crafting;
using Worldforge.Inventory;
using Worldforge.Item;

namespace Worldforge.Crafting.Editor
{
    /// <summary>
    /// Editor-only utility to verify crafting transaction logic without entering Play Mode.
    /// </summary>
    public static class CraftingTransactionTestRunner
    {
        [MenuItem("Worldforge/Tests/Crafting Transaction — Test Case 1 (Happy Path)")]
        private static void RunTestCase1_HappyPath()
        {
            Debug.Log("=== Crafting Transaction Test Case 1: Happy Path ===");

            // Load assets
            var wood = Resources.Load<ItemDefinition>("Definitions/Items/Item_Resource_Wood");
            var plank = Resources.Load<ItemDefinition>("Definitions/Items/Item_Material_Plank");
            var recipe = Resources.Load<RecipeDefinition>("Definitions/Recipes/Recipe_Material_Plank");

            if (wood == null || plank == null || recipe == null)
            {
                Debug.LogError("[TEST FAIL] Could not load required assets (Wood, Plank, or Recipe).");
                return;
            }

            Debug.Log($"  Loaded: Wood={wood.DisplayName}, Plank={plank.DisplayName}, Recipe={recipe.DisplayName}");
            Debug.Log($"  Recipe requires: {recipe.Ingredients[0].Item.DisplayName} x{recipe.Ingredients[0].Amount}");
            Debug.Log($"  Recipe produces: {recipe.Outputs[0].Item.DisplayName} x{recipe.Outputs[0].Amount}");

            // Setup inventory with 10 Wood
            var inventory = new InventoryContainer("TestInventory", slotCount: 20, maxWeight: 100f);
            inventory.AddItem(wood, 10);

            var woodBefore = inventory.GetItemCount(wood);
            var plankBefore = inventory.GetItemCount(plank);
            Debug.Log($"  Inventory BEFORE: Wood={woodBefore}, Plank={plankBefore}");

            // Setup crafting service
            var service = new RuntimeCraftingService();
            service.RegisterRecipe(recipe);

            // Track event
            var eventFired = false;
            CraftingResult eventResult = null;
            service.CraftingCompleted += result =>
            {
                eventFired = true;
                eventResult = result;
            };

            // Execute
            var craftResult = service.ExecuteCraft(recipe, inventory);

            var woodAfter = inventory.GetItemCount(wood);
            var plankAfter = inventory.GetItemCount(plank);
            Debug.Log($"  Inventory AFTER:  Wood={woodAfter}, Plank={plankAfter}");

            // Verify
            var allPassed = true;

            // Check 1: Result is success
            if (!craftResult.IsSuccess)
            {
                Debug.LogError($"  [FAIL] CraftResult.IsSuccess expected true, got false. Reason: {craftResult.FailureReason}, Message: {craftResult.Message}");
                allPassed = false;
            }
            else
            {
                Debug.Log("  [PASS] CraftResult.IsSuccess = true");
            }

            // Check 2: Wood consumed exactly 3
            if (woodAfter != woodBefore - 3)
            {
                Debug.LogError($"  [FAIL] Wood count expected {woodBefore - 3}, got {woodAfter}");
                allPassed = false;
            }
            else
            {
                Debug.Log($"  [PASS] Wood consumed: {woodBefore} -> {woodAfter} (consumed 3)");
            }

            // Check 3: Plank produced exactly 2
            if (plankAfter != plankBefore + 2)
            {
                Debug.LogError($"  [FAIL] Plank count expected {plankBefore + 2}, got {plankAfter}");
                allPassed = false;
            }
            else
            {
                Debug.Log($"  [PASS] Plank produced: {plankBefore} -> {plankAfter} (produced 2)");
            }

            // Check 4: ConsumedIngredients reported
            if (craftResult.ConsumedIngredients == null || craftResult.ConsumedIngredients.Count == 0)
            {
                Debug.LogError("  [FAIL] ConsumedIngredients is empty");
                allPassed = false;
            }
            else
            {
                Debug.Log($"  [PASS] ConsumedIngredients count: {craftResult.ConsumedIngredients.Count}");
            }

            // Check 5: ProducedItems reported
            if (craftResult.ProducedItems == null || craftResult.ProducedItems.Count == 0)
            {
                Debug.LogError("  [FAIL] ProducedItems is empty");
                allPassed = false;
            }
            else
            {
                Debug.Log($"  [PASS] ProducedItems count: {craftResult.ProducedItems.Count}, first: {craftResult.ProducedItems[0].Item.DisplayName} x{craftResult.ProducedItems[0].Quantity}");
            }

            // Check 6: Event fired
            if (!eventFired)
            {
                Debug.LogError("  [FAIL] CraftingCompleted event was NOT fired");
                allPassed = false;
            }
            else
            {
                Debug.Log($"  [PASS] CraftingCompleted event fired, IsSuccess={eventResult.IsSuccess}");
            }

            // Summary
            if (allPassed)
            {
                Debug.Log("=== TEST CASE 1: ALL PASSED ===");
            }
            else
            {
                Debug.LogError("=== TEST CASE 1: SOME CHECKS FAILED ===");
            }
        }
    }
}
