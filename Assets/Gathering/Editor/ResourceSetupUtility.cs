using System.IO;
using UnityEditor;
using UnityEngine;
using Worldforge.Gathering;
using Worldforge.Item;

namespace Worldforge.Gathering.Editor
{
    public static class ResourceSetupUtility
    {
        private const string ItemsDirectory = "Assets/Resources/Definitions/Items";
        private const string NodesDirectory = "Assets/Resources/Definitions/Nodes";

        [MenuItem("Worldforge/Setup/Setup Resource Definitions")]
        public static void SetupResourceDefinitions()
        {
            EnsureDirectoriesExist();

            // 1. Create Resource Items
            var woodItem = CreateOrUpdateItem(
                "Item_Resource_Wood",
                "RES_WOOD",
                "Oak Wood Log",
                "A sturdy oak wood log gathered from trees, essential for crafting and building.",
                ItemCategoryType.Resource,
                ItemRarity.Common,
                0.5f,
                100,
                new ResourceProperties(2.0f, 60f, 1.0f, ToolType.Axe),
                null);

            var stoneItem = CreateOrUpdateItem(
                "Item_Resource_Stone",
                "RES_STONE",
                "Granite Stone Ore",
                "Dense granite stone mined from rocks, used in robust constructions and tool forging.",
                ItemCategoryType.Resource,
                ItemRarity.Common,
                1.0f,
                100,
                new ResourceProperties(3.0f, 90f, 2.0f, ToolType.Pickaxe),
                null);

            var fiberItem = CreateOrUpdateItem(
                "Item_Resource_Fiber",
                "RES_FIBER",
                "Plant Fiber",
                "Flexible organic fibers collected from wild bushes, used for weaving, binding, and primitive crafting.",
                ItemCategoryType.Resource,
                ItemRarity.Common,
                0.2f,
                100,
                new ResourceProperties(1.0f, 45f, 0.5f, ToolType.None),
                null);

            // 2. Create Tool Items
            var axeTool = CreateOrUpdateItem(
                "Item_Tool_BasicAxe",
                "TOOL_AXE_01",
                "Crude Stone Axe",
                "A primitive axe suitable for chopping trees and gathering wood.",
                ItemCategoryType.Tool,
                ItemRarity.Common,
                1.5f,
                1,
                null,
                new ToolProperties(ToolType.Axe, 1.5f, 1.0f, 1, 1f));

            var pickaxeTool = CreateOrUpdateItem(
                "Item_Tool_BasicPickaxe",
                "TOOL_PICKAXE_01",
                "Crude Stone Pickaxe",
                "A heavy stone pickaxe for breaking rocks and harvesting stone ore.",
                ItemCategoryType.Tool,
                ItemRarity.Common,
                2.0f,
                1,
                null,
                new ToolProperties(ToolType.Pickaxe, 2.5f, 1.0f, 1, 1f));

            var sickleTool = CreateOrUpdateItem(
                "Item_Tool_BasicSickle",
                "TOOL_SICKLE_01",
                "Crude Sickle",
                "A curved blade used to efficiently harvest plant fibers and herbs.",
                ItemCategoryType.Tool,
                ItemRarity.Common,
                0.8f,
                1,
                null,
                new ToolProperties(ToolType.Sickle, 1.0f, 1.5f, 1, 1f));

            // 3. Create Resource Nodes
            CreateOrUpdateNode(
                "Node_OakTree",
                "NODE_OAK_TREE",
                "Oak Tree",
                "A mature oak tree rich in sturdy wood.",
                "Forest",
                woodItem,
                2,
                4,
                new GatheringRequirements(ToolType.Axe, 1.0f, 1, 5f, 3.0f),
                1.0f,
                100f,
                2.5f,
                60f,
                true,
                15);

            CreateOrUpdateNode(
                "Node_GraniteRock",
                "NODE_GRANITE_ROCK",
                "Granite Rock",
                "A dense rock deposit containing stone and mineral ore.",
                "Highland",
                stoneItem,
                2,
                5,
                new GatheringRequirements(ToolType.Pickaxe, 2.0f, 1, 8f, 2.5f),
                2.0f,
                150f,
                3.5f,
                90f,
                true,
                20);

            CreateOrUpdateNode(
                "Node_WildBush",
                "NODE_WILD_BUSH",
                "Wild Bush",
                "A lush wild bush with fibrous stems that can be gathered by hand or sickle.",
                "Plains",
                fiberItem,
                1,
                3,
                new GatheringRequirements(ToolType.None, 0.0f, 0, 2f, 2.0f),
                0.5f,
                50f,
                1.5f,
                45f,
                true,
                10);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "[Worldforge] Resource definitions setup completed successfully.\n" +
                "1. Resource Items: Oak Wood Log, Granite Stone Ore, Plant Fiber\n" +
                "2. Tool Items: Crude Stone Axe, Crude Stone Pickaxe, Crude Sickle\n" +
                "3. Resource Nodes: Oak Tree, Granite Rock, Wild Bush\n" +
                $"Location: {ItemsDirectory} & {NodesDirectory}");
        }

        private static void EnsureDirectoriesExist()
        {
            EnsureDirectory("Assets/Resources");
            EnsureDirectory("Assets/Resources/Definitions");
            EnsureDirectory(ItemsDirectory);
            EnsureDirectory(NodesDirectory);
        }

        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent = Path.GetDirectoryName(path).Replace('\\', '/');
                var folderName = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static ItemDefinition CreateOrUpdateItem(
            string assetName,
            string itemCode,
            string displayName,
            string description,
            ItemCategoryType category,
            ItemRarity rarity,
            float weight,
            int maxStack,
            ResourceProperties resourceProperties,
            ToolProperties toolProperties)
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
            so.FindProperty("_weight").floatValue = weight;
            so.FindProperty("_maxStack").intValue = maxStack;
            so.FindProperty("_isTradable").boolValue = true;
            so.FindProperty("_isDroppable").boolValue = true;
            so.FindProperty("_canDestroy").boolValue = true;

            if (resourceProperties != null)
            {
                var resProp = so.FindProperty("_resourceProperties");
                resProp.FindPropertyRelative("_gatherTime").floatValue = resourceProperties.GatherTime;
                resProp.FindPropertyRelative("_respawnTime").floatValue = resourceProperties.RespawnTime;
                resProp.FindPropertyRelative("_hardness").floatValue = resourceProperties.Hardness;
                resProp.FindPropertyRelative("_requiredToolType").enumValueIndex = (int)resourceProperties.RequiredToolType;
            }

            if (toolProperties != null)
            {
                var toolProp = so.FindProperty("_toolProperties");
                toolProp.FindPropertyRelative("_toolType").enumValueIndex = (int)toolProperties.ToolType;
                toolProp.FindPropertyRelative("_harvestPower").floatValue = toolProperties.HarvestPower;
                toolProp.FindPropertyRelative("_efficiency").floatValue = toolProperties.Efficiency;
                toolProp.FindPropertyRelative("_toolTier").intValue = toolProperties.ToolTier;
                toolProp.FindPropertyRelative("_durabilityCostPerUse").floatValue = toolProperties.DurabilityCostPerUse;
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            if (existing == null)
            {
                AssetDatabase.CreateAsset(item, path);
            }
            else
            {
                EditorUtility.SetDirty(existing);
            }

            return item;
        }

        private static void CreateOrUpdateNode(
            string assetName,
            string nodeCode,
            string displayName,
            string description,
            string biomeType,
            ItemDefinition primaryYield,
            int minAmount,
            int maxAmount,
            GatheringRequirements requirements,
            float hardness,
            float maxHealth,
            float baseDuration,
            float respawnTime,
            bool canRespawn,
            int discoveryXP)
        {
            var path = $"{NodesDirectory}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ResourceNodeDefinition>(path);

            var node = existing != null ? existing : ScriptableObject.CreateInstance<ResourceNodeDefinition>();
            var so = new SerializedObject(node);

            so.FindProperty("_nodeCode").stringValue = nodeCode;
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_description").stringValue = description;
            so.FindProperty("_biomeType").stringValue = biomeType;
            so.FindProperty("_primaryYield").objectReferenceValue = primaryYield;
            so.FindProperty("_primaryMinAmount").intValue = minAmount;
            so.FindProperty("_primaryMaxAmount").intValue = maxAmount;

            if (requirements != null)
            {
                var reqProp = so.FindProperty("_requirements");
                reqProp.FindPropertyRelative("_requiredToolType").enumValueIndex = (int)requirements.RequiredToolType;
                reqProp.FindPropertyRelative("_minimumHarvestPower").floatValue = requirements.MinimumHarvestPower;
                reqProp.FindPropertyRelative("_requiredToolTier").intValue = requirements.RequiredToolTier;
                reqProp.FindPropertyRelative("_staminaCostPerAction").floatValue = requirements.StaminaCostPerAction;
                reqProp.FindPropertyRelative("_maxInteractionDistance").floatValue = requirements.MaxInteractionDistance;
            }

            so.FindProperty("_hardness").floatValue = hardness;
            so.FindProperty("_maxHealth").floatValue = maxHealth;
            so.FindProperty("_baseGatherDuration").floatValue = baseDuration;
            so.FindProperty("_respawnTime").floatValue = respawnTime;
            so.FindProperty("_canRespawn").boolValue = canRespawn;
            so.FindProperty("_discoveryXP").intValue = discoveryXP;

            so.ApplyModifiedPropertiesWithoutUndo();

            if (existing == null)
            {
                AssetDatabase.CreateAsset(node, path);
            }
            else
            {
                EditorUtility.SetDirty(existing);
            }
        }
    }
}
