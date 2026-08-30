using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Worldforge.Inventory;

namespace Worldforge.Item.Editor
{
    /// <summary>
    /// Editor utility for creating, configuring, and standardizing all item definitions and inventory configurations.
    /// </summary>
    public static class ItemSetupUtility
    {
        private const string ItemsDirectory = "Assets/Resources/Definitions/Items";
        private const string InventoryDirectory = "Assets/Resources/Definitions/Inventory";

        [InitializeOnLoadMethod]
        [MenuItem("Worldforge/Setup/Configure All Item Definitions", priority = 100)]
        public static void ConfigureAllItemDefinitions()
        {
            EnsureDirectoryExists(ItemsDirectory);
            EnsureDirectoryExists(InventoryDirectory);

            var count = 0;

            // 1. Resources
            ConfigureItem(
                "Item_Resource_Wood", "RES_WOOD", "Oak Wood Log",
                "A sturdy oak wood log gathered from trees, essential for crafting and building.",
                ItemCategoryType.Resource, ItemRarity.Common,
                gridWidth: 1, gridHeight: 2, weight: 0.5f, isStackable: true, maxStack: 100,
                buyPrice: 2, sellPrice: 1,
                resourceProps: new ResourceProperties(2f, 60f, 1f, ToolType.Axe));
            count++;

            ConfigureItem(
                "Item_Resource_Stone", "RES_STONE", "Stone Chunk",
                "A dense piece of rock mined from stone deposits, used in masonry and forging.",
                ItemCategoryType.Resource, ItemRarity.Common,
                gridWidth: 1, gridHeight: 1, weight: 1.0f, isStackable: true, maxStack: 100,
                buyPrice: 2, sellPrice: 1,
                resourceProps: new ResourceProperties(3f, 90f, 2f, ToolType.Pickaxe));
            count++;

            ConfigureItem(
                "Item_Resource_Fiber", "RES_FIBER", "Plant Fiber",
                "Flexible plant fibers harvested from bushes, used to weave ropes, fabrics, and bandages.",
                ItemCategoryType.Resource, ItemRarity.Common,
                gridWidth: 1, gridHeight: 1, weight: 0.1f, isStackable: true, maxStack: 100,
                buyPrice: 1, sellPrice: 1,
                resourceProps: new ResourceProperties(1f, 30f, 0.5f, ToolType.Sickle));
            count++;

            // 2. Tools
            ConfigureItem(
                "Item_Tool_BasicAxe", "TOOL_AXE_01", "Crude Stone Axe",
                "A primitive axe suitable for chopping trees and gathering wood.",
                ItemCategoryType.Tool, ItemRarity.Common,
                gridWidth: 1, gridHeight: 2, weight: 1.5f, isStackable: false, maxStack: 1,
                buyPrice: 10, sellPrice: 3,
                toolProps: new ToolProperties(ToolType.Axe, 1.5f, 1f, 1, 1f));
            count++;

            ConfigureItem(
                "Item_Tool_BasicPickaxe", "TOOL_PICKAXE_01", "Crude Stone Pickaxe",
                "A primitive pickaxe capable of fracturing stone boulders and extracting minerals.",
                ItemCategoryType.Tool, ItemRarity.Common,
                gridWidth: 1, gridHeight: 3, weight: 2.0f, isStackable: false, maxStack: 1,
                buyPrice: 12, sellPrice: 4,
                toolProps: new ToolProperties(ToolType.Pickaxe, 1.5f, 1f, 1, 1f));
            count++;

            ConfigureItem(
                "Item_Tool_BasicSickle", "TOOL_SICKLE_01", "Crude Stone Sickle",
                "A primitive curved sickle designed for swiftly reaping fibers and crops.",
                ItemCategoryType.Tool, ItemRarity.Common,
                gridWidth: 1, gridHeight: 2, weight: 1.0f, isStackable: false, maxStack: 1,
                buyPrice: 8, sellPrice: 2,
                toolProps: new ToolProperties(ToolType.Sickle, 1.5f, 1f, 1, 1f));
            count++;

            // 3. Materials
            ConfigureItem(
                "Item_Material_IronIngot", "MAT_IRON_INGOT", "Iron Ingot",
                "Smelted metallic iron ingot used in forging weapons, armor, and advanced components.",
                ItemCategoryType.Material, ItemRarity.Uncommon,
                gridWidth: 1, gridHeight: 1, weight: 1.5f, isStackable: true, maxStack: 50,
                buyPrice: 10, sellPrice: 5);
            count++;

            ConfigureItem(
                "Item_Material_Plank", "MAT_PLANK", "Refined Wood Plank",
                "Smoothly sawed timber plank used for construction, furniture, and structures.",
                ItemCategoryType.Material, ItemRarity.Common,
                gridWidth: 1, gridHeight: 2, weight: 0.3f, isStackable: true, maxStack: 100,
                buyPrice: 4, sellPrice: 2);
            count++;

            ConfigureItem(
                "Item_Material_Leather", "MAT_LEATHER", "Cured Leather",
                "Supple tanned leather suitable for crafting backpacks, light armor, and straps.",
                ItemCategoryType.Material, ItemRarity.Common,
                gridWidth: 1, gridHeight: 1, weight: 0.2f, isStackable: true, maxStack: 50,
                buyPrice: 8, sellPrice: 4);
            count++;

            // 4. Weapons
            ConfigureItem(
                "Item_Weapon_IronSword", "WEAP_SWORD_01", "Iron Broadsword",
                "A finely forged iron blade offering dependable balance between swing speed and cutting power.",
                ItemCategoryType.Weapon, ItemRarity.Uncommon,
                gridWidth: 1, gridHeight: 3, weight: 2.5f, isStackable: false, maxStack: 1,
                buyPrice: 50, sellPrice: 20,
                weaponProps: new WeaponProperties("Sword", "Physical", 15f, 1.2f, 1.8f, 0.08f, 1.6f),
                equipProps: new EquipmentProperties("MainHand", 1, 120f, 1f));
            count++;

            ConfigureItem(
                "Item_Weapon_WoodenBow", "WEAP_BOW_01", "Hunter Shortbow",
                "A flexible curved bow capable of launching arrows accurately over medium range.",
                ItemCategoryType.Weapon, ItemRarity.Common,
                gridWidth: 1, gridHeight: 3, weight: 1.2f, isStackable: false, maxStack: 1,
                buyPrice: 40, sellPrice: 16,
                weaponProps: new WeaponProperties("Bow", "Physical", 12f, 0.9f, 20.0f, 0.12f, 1.8f),
                equipProps: new EquipmentProperties("TwoHand", 1, 100f, 1f));
            count++;

            // 5. Armor
            ConfigureItem(
                "Item_Armor_LeatherChest", "ARM_CHEST_01", "Hardened Leather Tunic",
                "Sturdy chest protection tailored from layered animal hide.",
                ItemCategoryType.Armor, ItemRarity.Common,
                gridWidth: 2, gridHeight: 2, weight: 3.0f, isStackable: false, maxStack: 1,
                buyPrice: 35, sellPrice: 14,
                armorProps: new ArmorProperties("Chest", 10f, 2f),
                equipProps: new EquipmentProperties("Chest", 1, 100f, 1f));
            count++;

            ConfigureItem(
                "Item_Armor_IronHelmet", "ARM_HELMET_01", "Iron Reinforced Helmet",
                "A protective metal coif offering critical head protection against heavy blows.",
                ItemCategoryType.Armor, ItemRarity.Uncommon,
                gridWidth: 2, gridHeight: 2, weight: 2.0f, isStackable: false, maxStack: 1,
                buyPrice: 30, sellPrice: 12,
                armorProps: new ArmorProperties("Head", 8f, 0f),
                equipProps: new EquipmentProperties("Head", 1, 120f, 1f));
            count++;

            // 6. Backpack
            ConfigureItem(
                "Item_Backpack_LeatherBackpack", "PACK_LEATHER_01", "Traveler Leather Backpack",
                "An expansive adventuring backpack providing additional storage slots and carry capacity.",
                ItemCategoryType.Backpack, ItemRarity.Uncommon,
                gridWidth: 2, gridHeight: 2, weight: 1.0f, isStackable: false, maxStack: 1,
                buyPrice: 60, sellPrice: 25,
                backpackProps: new BackpackProperties(8, 4, 2, 20.0f),
                equipProps: new EquipmentProperties("Backpack", 1, 150f, 1f));
            count++;

            // 7. Consumables
            ConfigureItem(
                "Item_Consumable_HealthPotion", "CONS_POTION_HP_01", "Minor Health Draught",
                "An invigorating herbal elixir that swiftly restores health when ingested.",
                ItemCategoryType.Consumable, ItemRarity.Common,
                gridWidth: 1, gridHeight: 1, weight: 0.2f, isStackable: true, maxStack: 20,
                buyPrice: 15, sellPrice: 5,
                consumableProps: new ConsumableProperties(5f, 1.5f, false, 50f, 0f));
            count++;

            ConfigureItem(
                "Item_Consumable_CookedMeat", "CONS_MEAT_COOKED", "Roast Boar Meat",
                "Savory fire-roasted meat that replenishes vital stamina and soothes hunger.",
                ItemCategoryType.Consumable, ItemRarity.Common,
                gridWidth: 1, gridHeight: 1, weight: 0.5f, isStackable: true, maxStack: 10,
                buyPrice: 8, sellPrice: 3,
                consumableProps: new ConsumableProperties(2f, 2.0f, false, 20f, 30f));
            count++;

            // 8. Quest Items
            ConfigureItem(
                "Item_Quest_AncientRelic", "QST_ANCIENT_RELIC", "Fragment of the Ancients",
                "A mysterious glyph-carved fragment vibrating with latent ancestral energy.",
                ItemCategoryType.Quest, ItemRarity.Rare,
                gridWidth: 1, gridHeight: 1, weight: 0f, isStackable: false, maxStack: 1,
                buyPrice: 0, sellPrice: 0,
                isQuestItem: true, isTradable: false, isDroppable: false, canDestroy: false);
            count++;

            // 9. Default Inventory Definition
            ConfigureDefaultInventoryDefinition();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ItemSetupUtility] Successfully configured {count} item definitions in '{ItemsDirectory}' and inventory templates in '{InventoryDirectory}'.");
        }

        [MenuItem("Worldforge/Setup/Attach Player Inventory to Avatar", priority = 101)]
        public static void AttachPlayerInventoryToPlayer()
        {
            var player = GameObject.Find("Worldforge.Player") ?? GameObject.Find("Player") ?? GameObject.FindWithTag("Player");
            if (player == null)
            {
                var allObjs = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude);
                for (var i = 0; i < allObjs.Length; i++)
                {
                    if (allObjs[i].name.IndexOf("Player", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        player = allObjs[i];
                        break;
                    }
                }
            }

            if (player == null)
            {
                Debug.LogWarning("[Worldforge] Player GameObject not found in active scene. Start game or select player first.");
                return;
            }

            var inventoryBehaviour = player.GetComponent<PlayerInventoryBehaviour>();
            if (inventoryBehaviour == null)
            {
                inventoryBehaviour = player.AddComponent<PlayerInventoryBehaviour>();
            }

            var defaultDef = Resources.Load<InventoryDefinition>("Definitions/Inventory/Inventory_PlayerDefault");
            if (defaultDef != null)
            {
                var so = new SerializedObject(inventoryBehaviour);
                so.FindProperty("_definition").objectReferenceValue = defaultDef;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            inventoryBehaviour.InitializeContainer();

            Debug.Log($"[Worldforge] PlayerInventoryBehaviour attached/configured on '{player.name}'.");
        }

        [MenuItem("Worldforge/Testing/Run Item and Inventory Validation Checks", priority = 200)]
        public static void RunItemAndInventoryValidationChecks()
        {
            Debug.Log("<b><color=#00E5FF>[Worldforge Item & Inventory Test]</color></b> Starting validation checks...");

            // Test 1: Load all definitions
            var wood = Resources.Load<ItemDefinition>("Definitions/Items/Item_Resource_Wood");
            var stone = Resources.Load<ItemDefinition>("Definitions/Items/Item_Resource_Stone");
            var fiber = Resources.Load<ItemDefinition>("Definitions/Items/Item_Resource_Fiber");
            var axe = Resources.Load<ItemDefinition>("Definitions/Items/Item_Tool_BasicAxe");
            var sword = Resources.Load<ItemDefinition>("Definitions/Items/Item_Weapon_IronSword");
            var armor = Resources.Load<ItemDefinition>("Definitions/Items/Item_Armor_LeatherChest");
            var backpack = Resources.Load<ItemDefinition>("Definitions/Items/Item_Backpack_LeatherBackpack");
            var potion = Resources.Load<ItemDefinition>("Definitions/Items/Item_Consumable_HealthPotion");
            var meat = Resources.Load<ItemDefinition>("Definitions/Items/Item_Consumable_CookedMeat");
            var relic = Resources.Load<ItemDefinition>("Definitions/Items/Item_Quest_AncientRelic");
            var invDef = Resources.Load<InventoryDefinition>("Definitions/Inventory/Inventory_PlayerDefault");

            if (wood == null || stone == null || fiber == null || axe == null || sword == null ||
                armor == null || backpack == null || potion == null || meat == null || relic == null || invDef == null)
            {
                Debug.LogError("[Worldforge Item Test] Failed: Unable to load one or more item or inventory definitions from Resources.");
                return;
            }
            Debug.Log("[Worldforge Item Test] <color=#00FF66>✓ Test 1/6 PASSED:</color> All item and inventory definitions loaded successfully from Resources.");

            // Test 2: Category & Stacking constraints verification
            if (!wood.IsStackable || wood.MaxStack != 100 || !potion.IsStackable || potion.MaxStack != 20 ||
                sword.IsStackable || sword.MaxStack != 1 || armor.IsStackable || armor.MaxStack != 1 ||
                axe.IsStackable || axe.MaxStack != 1 || backpack.IsStackable || backpack.MaxStack != 1 ||
                relic.IsStackable || relic.MaxStack != 1)
            {
                Debug.LogError("[Worldforge Item Test] Failed: Stacking constraint mismatch on definitions.");
                return;
            }
            Debug.Log("[Worldforge Item Test] <color=#00FF66>✓ Test 2/6 PASSED:</color> Stacking constraints and category rules verified.");

            // Test 3: Grid Dimensions & Rotation
            if (sword.GridSize != new Vector2Int(1, 3) || sword.GetRotatedGridSize(true) != new Vector2Int(3, 1) ||
                armor.GridSize != new Vector2Int(2, 2) || wood.GridSize != new Vector2Int(1, 2) ||
                potion.GridSize != new Vector2Int(1, 1))
            {
                Debug.LogError("[Worldforge Item Test] Failed: Grid sizing / rotation mismatch.");
                return;
            }
            Debug.Log("[Worldforge Item Test] <color=#00FF66>✓ Test 3/6 PASSED:</color> Spatial Grid footprints and rotation verified.");

            // Test 4: ItemStack Lifecycle & Properties
            var stack = new ItemStack(wood, 50);
            if (stack.TotalWeight != 25f || stack.AvailableSpace != 50 || stack.IsFull)
            {
                Debug.LogError("[Worldforge Item Test] Failed: ItemStack weight or space calculation mismatch.");
                return;
            }
            var added = stack.Add(60, out var overflow);
            if (added != 50 || overflow != 10 || stack.Quantity != 100 || !stack.IsFull)
            {
                Debug.LogError("[Worldforge Item Test] Failed: ItemStack addition/overflow mismatch.");
                return;
            }
            var removed = stack.Remove(30, out var remainder);
            if (removed != 30 || remainder != 70 || stack.Quantity != 70)
            {
                Debug.LogError("[Worldforge Item Test] Failed: ItemStack removal mismatch.");
                return;
            }
            Debug.Log("[Worldforge Item Test] <color=#00FF66>✓ Test 4/6 PASSED:</color> ItemStack stacking, space, addition, and removal verified.");

            // Test 5: InventoryContainer Full Operations & Events
            var container = new InventoryContainer("TestPlayerBag", 6, 60f);
            var itemAddedEvents = 0;
            var itemRemovedEvents = 0;
            var slotChangedEvents = 0;
            var encumbranceFlipped = false;

            container.ItemAdded += _ => itemAddedEvents++;
            container.ItemRemoved += _ => itemRemovedEvents++;
            container.SlotChanged += _ => slotChangedEvents++;
            container.EncumbranceChanged += evt => encumbranceFlipped = evt.IsOverencumbered;

            // Add stackable exceeding 1 slot (150 wood -> slot0: 100, slot1: 50)
            var woodPlaced = container.AddItem(wood, 150);
            if (woodPlaced != 150 || container.GetItemCount(wood) != 150 || container.EmptySlotCount != 4)
            {
                Debug.LogError($"[Worldforge Item Test] Failed: AddItem multi-slot mismatch. Placed: {woodPlaced}, Count: {container.GetItemCount(wood)}");
                return;
            }

            // Add non-stackable (2 swords -> slot2: 1, slot3: 1)
            var sword1Placed = container.AddItem(sword, 1);
            var sword2Placed = container.AddItem(sword, 1);
            if (sword1Placed != 1 || sword2Placed != 1 || container.GetItemCount(sword) != 2 || container.EmptySlotCount != 2)
            {
                Debug.LogError("[Worldforge Item Test] Failed: Non-stackable addition mismatch.");
                return;
            }

            // Test Slot Swap
            var slot0Before = container.GetSlot(0).Item;
            var slot2Before = container.GetSlot(2).Item;
            var swapOk = container.SwapSlots(0, 2);
            if (!swapOk || container.GetSlot(0).Item != slot2Before || container.GetSlot(2).Item != slot0Before)
            {
                Debug.LogError("[Worldforge Item Test] Failed: Slot swap failed.");
                return;
            }

            // Test Split Stack (Split 20 wood from slot 2 into empty slot 4)
            var splitOk = container.SplitStack(2, 4, 20);
            if (!splitOk || container.GetSlot(2).Quantity != 80 || container.GetSlot(4).Quantity != 20)
            {
                Debug.LogError("[Worldforge Item Test] Failed: SplitStack mismatch.");
                return;
            }

            // Test Merge Stacks (Merge slot 4 back into slot 2)
            var mergeOk = container.MergeStacks(4, 2);
            if (!mergeOk || container.GetSlot(2).Quantity != 100 || !container.GetSlot(4).IsEmpty)
            {
                Debug.LogError("[Worldforge Item Test] Failed: MergeStacks mismatch.");
                return;
            }

            // Test AutoSort
            container.AutoSort();
            // After AutoSort: Resource (Wood: slot0=100, slot1=50) -> Weapon (Sword: slot2=1, slot3=1) -> Empty (slot4, slot5)
            if (container.GetSlot(0).Item != wood || container.GetSlot(1).Item != wood ||
                container.GetSlot(2).Item != sword || container.GetSlot(3).Item != sword ||
                !container.GetSlot(4).IsEmpty || !container.GetSlot(5).IsEmpty)
            {
                Debug.LogError("[Worldforge Item Test] Failed: AutoSort ordering mismatch.");
                return;
            }

            // Test Overencumbrance trigger
            container.MaxWeight = 50f;
            // Current weight: 150 wood * 0.5kg + 2 swords * 2.5kg = 75 + 5 = 80kg > 50kg -> overencumbered
            if (!container.IsOverencumbered || !encumbranceFlipped)
            {
                Debug.LogError($"[Worldforge Item Test] Failed: Encumbrance state not triggered. Weight: {container.CurrentWeight}/{container.MaxWeight}");
                return;
            }

            // Remove items across stacks (remove 70 wood -> slot1 emptied (50), slot0 reduced to 80)
            var removeOk = container.RemoveItem(wood, 70);
            if (!removeOk || container.GetItemCount(wood) != 80)
            {
                Debug.LogError($"[Worldforge Item Test] Failed: RemoveItem multi-stack mismatch. Count: {container.GetItemCount(wood)}");
                return;
            }

            if (itemAddedEvents == 0 || itemRemovedEvents == 0 || slotChangedEvents == 0)
            {
                Debug.LogError("[Worldforge Item Test] Failed: Event publishing counters were not fired.");
                return;
            }

            Debug.Log("[Worldforge Item Test] <color=#00FF66>✓ Test 5/6 PASSED:</color> InventoryContainer multi-slot add, remove, swap, split, merge, auto-sort, encumbrance, and event publishing verified.");

            // Test 6: IGatheredItemReceiver and PlayerInventoryBehaviour integration with Gathering Delivery
            var testPlayerGo = new GameObject("TestPlayer_InventoryGatheringIntegration");
            try
            {
                var playerInv = testPlayerGo.AddComponent<PlayerInventoryBehaviour>();
                playerInv.InitializeContainer();

                var receiver = (IGatheredItemReceiver)playerInv;
                var stoneReceived = receiver.ReceiveItem(stone, 40);
                var fiberReceived = receiver.ReceiveItem(fiber, 60);

                if (!stoneReceived || !fiberReceived ||
                    playerInv.GetItemCount(stone) != 40 || playerInv.GetItemCount(fiber) != 60)
                {
                    Debug.LogError("[Worldforge Item Test] Failed: Gathering receiver delivery into PlayerInventoryBehaviour failed.");
                    return;
                }

                // Verify weight tracking on player inventory: 40 stone (40kg) + 60 fiber (6kg) = 46kg
                var expectedWeight = 40f * 1.0f + 60f * 0.1f;
                if (Mathf.Abs(playerInv.CurrentWeight - expectedWeight) > 0.01f)
                {
                    Debug.LogError($"[Worldforge Item Test] Failed: Player inventory weight mismatch. Expected {expectedWeight}, got {playerInv.CurrentWeight}");
                    return;
                }

                Debug.Log("[Worldforge Item Test] <color=#00FF66>✓ Test 6/6 PASSED:</color> IGatheredItemReceiver and PlayerInventoryBehaviour gathering delivery fully integrated.");
                Debug.Log("<b><color=#00FF66>[Worldforge Item & Inventory Test] ALL 6 TESTS PASSED SUCCESSFULLY!</color></b>");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(testPlayerGo);
            }
        }

        private static void ConfigureDefaultInventoryDefinition()
        {
            var path = $"{InventoryDirectory}/Inventory_PlayerDefault.asset";
            var existing = AssetDatabase.LoadAssetAtPath<InventoryDefinition>(path);
            var def = existing != null ? existing : ScriptableObject.CreateInstance<InventoryDefinition>();

            var so = new SerializedObject(def);
            so.FindProperty("_inventoryCode").stringValue = "INV_PLAYER_BASIC";
            so.FindProperty("_displayName").stringValue = "Player Basic Inventory";
            so.FindProperty("_description").stringValue = "Standard exploratory inventory carrying basic equipment and gathered resources.";
            so.FindProperty("_slotCount").intValue = 20;
            so.FindProperty("_weightLimit").floatValue = 50f;
            so.FindProperty("_allowSort").boolValue = true;
            so.FindProperty("_allowStack").boolValue = true;
            so.FindProperty("_allowQuickMove").boolValue = true;

            var startingItemsProp = so.FindProperty("_startingItems");
            startingItemsProp.ClearArray();

            var axe = AssetDatabase.LoadAssetAtPath<ItemDefinition>($"{ItemsDirectory}/Item_Tool_BasicAxe.asset") 
                ?? Resources.Load<ItemDefinition>("Definitions/Items/Item_Tool_BasicAxe");
            var potion = AssetDatabase.LoadAssetAtPath<ItemDefinition>($"{ItemsDirectory}/Item_Consumable_HealthPotion.asset")
                ?? Resources.Load<ItemDefinition>("Definitions/Items/Item_Consumable_HealthPotion");

            if (axe != null)
            {
                var index = startingItemsProp.arraySize;
                startingItemsProp.InsertArrayElementAtIndex(index);
                var entry = startingItemsProp.GetArrayElementAtIndex(index);
                if (entry != null)
                {
                    entry.FindPropertyRelative("_item").objectReferenceValue = axe;
                    entry.FindPropertyRelative("_amount").intValue = 1;
                }
            }

            if (potion != null)
            {
                var index = startingItemsProp.arraySize;
                startingItemsProp.InsertArrayElementAtIndex(index);
                var entry = startingItemsProp.GetArrayElementAtIndex(index);
                if (entry != null)
                {
                    entry.FindPropertyRelative("_item").objectReferenceValue = potion;
                    entry.FindPropertyRelative("_amount").intValue = 3;
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            if (existing == null)
            {
                AssetDatabase.CreateAsset(def, path);
            }
            else
            {
                EditorUtility.SetDirty(def);
            }
        }

        private static void ConfigureItem(
            string assetName,
            string itemCode,
            string displayName,
            string description,
            ItemCategoryType category,
            ItemRarity rarity,
            int gridWidth,
            int gridHeight,
            float weight,
            bool isStackable,
            int maxStack,
            int buyPrice = 0,
            int sellPrice = 0,
            bool isTradable = true,
            bool isDroppable = true,
            bool canDestroy = true,
            bool isQuestItem = false,
            bool isUnique = false,
            ResourceProperties resourceProps = null,
            ToolProperties toolProps = null,
            WeaponProperties weaponProps = null,
            ArmorProperties armorProps = null,
            ConsumableProperties consumableProps = null,
            BackpackProperties backpackProps = null,
            EquipmentProperties equipProps = null)
        {
            var path = $"{ItemsDirectory}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            var item = existing != null ? existing : ScriptableObject.CreateInstance<ItemDefinition>();

            var so = new SerializedObject(item);

            so.FindProperty("_itemCode").stringValue = itemCode;
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_description").stringValue = description;
            so.FindProperty("_category").enumValueIndex = (int)category;
            so.FindProperty("_rarity").enumValueIndex = (int)rarity;
            so.FindProperty("_gridWidth").intValue = gridWidth;
            so.FindProperty("_gridHeight").intValue = gridHeight;
            so.FindProperty("_weight").floatValue = weight;
            so.FindProperty("_isStackable").boolValue = isStackable;
            so.FindProperty("_maxStack").intValue = maxStack;
            so.FindProperty("_buyPrice").intValue = buyPrice;
            so.FindProperty("_sellPrice").intValue = sellPrice;
            so.FindProperty("_isTradable").boolValue = isTradable;
            so.FindProperty("_isDroppable").boolValue = isDroppable;
            so.FindProperty("_canDestroy").boolValue = canDestroy;
            so.FindProperty("_isQuestItem").boolValue = isQuestItem;
            so.FindProperty("_isUnique").boolValue = isUnique;

            if (resourceProps != null)
            {
                var p = so.FindProperty("_resourceProperties");
                p.FindPropertyRelative("_gatherTime").floatValue = resourceProps.GatherTime;
                p.FindPropertyRelative("_respawnTime").floatValue = resourceProps.RespawnTime;
                p.FindPropertyRelative("_hardness").floatValue = resourceProps.Hardness;
                p.FindPropertyRelative("_requiredToolType").enumValueIndex = (int)resourceProps.RequiredToolType;
            }

            if (toolProps != null)
            {
                var p = so.FindProperty("_toolProperties");
                p.FindPropertyRelative("_toolType").enumValueIndex = (int)toolProps.ToolType;
                p.FindPropertyRelative("_harvestPower").floatValue = toolProps.HarvestPower;
                p.FindPropertyRelative("_efficiency").floatValue = toolProps.Efficiency;
                p.FindPropertyRelative("_toolTier").intValue = toolProps.ToolTier;
                p.FindPropertyRelative("_durabilityCostPerUse").floatValue = toolProps.DurabilityCostPerUse;
            }

            if (weaponProps != null)
            {
                var p = so.FindProperty("_weaponProperties");
                p.FindPropertyRelative("_weaponType").stringValue = weaponProps.WeaponType;
                p.FindPropertyRelative("_damageType").stringValue = weaponProps.DamageType;
                p.FindPropertyRelative("_baseDamage").floatValue = weaponProps.BaseDamage;
                p.FindPropertyRelative("_attackSpeed").floatValue = weaponProps.AttackSpeed;
                p.FindPropertyRelative("_attackRange").floatValue = weaponProps.AttackRange;
                p.FindPropertyRelative("_criticalChance").floatValue = weaponProps.CriticalChance;
                p.FindPropertyRelative("_criticalMultiplier").floatValue = weaponProps.CriticalMultiplier;
            }

            if (armorProps != null)
            {
                var p = so.FindProperty("_armorProperties");
                p.FindPropertyRelative("_armorType").stringValue = armorProps.ArmorType;
                p.FindPropertyRelative("_armor").floatValue = armorProps.Armor;
                p.FindPropertyRelative("_magicResistance").floatValue = armorProps.MagicResistance;
            }

            if (consumableProps != null)
            {
                var p = so.FindProperty("_consumableProperties");
                p.FindPropertyRelative("_cooldown").floatValue = consumableProps.Cooldown;
                p.FindPropertyRelative("_consumeTime").floatValue = consumableProps.ConsumeTime;
                p.FindPropertyRelative("_isReusable").boolValue = consumableProps.IsReusable;
                p.FindPropertyRelative("_healthRestored").floatValue = consumableProps.HealthRestored;
                p.FindPropertyRelative("_staminaRestored").floatValue = consumableProps.StaminaRestored;
            }

            if (backpackProps != null)
            {
                var p = so.FindProperty("_backpackProperties");
                p.FindPropertyRelative("_bonusSlotCount").intValue = backpackProps.BonusSlotCount;
                p.FindPropertyRelative("_bonusGridWidth").intValue = backpackProps.BonusGridWidth;
                p.FindPropertyRelative("_bonusGridHeight").intValue = backpackProps.BonusGridHeight;
                p.FindPropertyRelative("_carryCapacityBonus").floatValue = backpackProps.CarryCapacityBonus;
            }

            if (equipProps != null)
            {
                var p = so.FindProperty("_equipmentProperties");
                p.FindPropertyRelative("_equipmentSlot").stringValue = equipProps.EquipmentSlot;
                p.FindPropertyRelative("_requiredLevel").intValue = equipProps.RequiredLevel;
                p.FindPropertyRelative("_maxDurability").floatValue = equipProps.MaxDurability;
                p.FindPropertyRelative("_durabilityMultiplier").floatValue = equipProps.DurabilityMultiplier;
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            if (existing == null)
            {
                AssetDatabase.CreateAsset(item, path);
            }
            else
            {
                EditorUtility.SetDirty(item);
            }
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent = Path.GetDirectoryName(path).Replace('\\', '/');
                var folderName = Path.GetFileName(path);
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    EnsureDirectoryExists(parent);
                }
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
