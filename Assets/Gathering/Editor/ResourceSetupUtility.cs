using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Worldforge.Character.Player;
using Worldforge.Gathering;
using Worldforge.Gathering.Services;
using Worldforge.Interaction;
using Worldforge.Item;

namespace Worldforge.Gathering.Editor
{
    public static class ResourceSetupUtility
    {
        private const string ItemsDirectory = "Assets/Resources/Definitions/Items";
        private const string NodesDirectory = "Assets/Resources/Definitions/Nodes";
        private const string PrefabsDirectory = "Assets/Prefabs/Nodes";

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
            var oakTreeNode = CreateOrUpdateNode(
                "Node_OakTree",
                "NODE_OAK_TREE",
                "Oak Tree",
                "A mature oak tree rich in sturdy wood.",
                "Forest",
                woodItem,
                2,
                4,
                new[] { new ResourceYieldEntry(fiberItem, 1, 2, 0.35f) },
                new GatheringRequirements(ToolType.Axe, 1.0f, 1, 5f, 3.0f),
                1.0f,
                100f,
                2.5f,
                60f,
                true,
                15,
                null);

            var graniteRockNode = CreateOrUpdateNode(
                "Node_GraniteRock",
                "NODE_GRANITE_ROCK",
                "Granite Rock",
                "A dense rock deposit containing stone and mineral ore.",
                "Highland",
                stoneItem,
                2,
                5,
                new[] { new ResourceYieldEntry(stoneItem, 1, 2, 0.25f) },
                new GatheringRequirements(ToolType.Pickaxe, 2.0f, 1, 8f, 2.5f),
                2.0f,
                150f,
                3.5f,
                90f,
                true,
                20,
                null);

            var wildBushNode = CreateOrUpdateNode(
                "Node_WildBush",
                "NODE_WILD_BUSH",
                "Wild Bush",
                "A lush wild bush with fibrous stems that can be gathered by hand or sickle.",
                "Plains",
                fiberItem,
                1,
                3,
                new[] { new ResourceYieldEntry(fiberItem, 1, 1, 0.5f) },
                new GatheringRequirements(ToolType.None, 0.0f, 0, 2f, 2.0f),
                0.5f,
                50f,
                1.5f,
                45f,
                true,
                10,
                null);

            // 4. Create and associate Prefabs
            var oakPrefab = CreateOakTreePrefab(oakTreeNode);
            var rockPrefab = CreateGraniteRockPrefab(graniteRockNode);
            var bushPrefab = CreateWildBushPrefab(wildBushNode);

            SetNodePrefab(oakTreeNode, oakPrefab);
            SetNodePrefab(graniteRockNode, rockPrefab);
            SetNodePrefab(wildBushNode, bushPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "[Worldforge] Resource definitions and node prefabs setup completed successfully.\n" +
                "1. Resource Items: Oak Wood Log, Granite Stone Ore, Plant Fiber\n" +
                "2. Tool Items: Crude Stone Axe, Crude Stone Pickaxe, Crude Sickle\n" +
                "3. Resource Nodes: Oak Tree, Granite Rock, Wild Bush\n" +
                "4. Prefabs: Node_OakTree_Prefab, Node_GraniteRock_Prefab, Node_WildBush_Prefab\n" +
                $"Location: {ItemsDirectory}, {NodesDirectory}, {PrefabsDirectory}");
        }

        [MenuItem("Worldforge/Setup/Create Resource Nodes in Active Scene")]
        public static void CreateResourceNodesInActiveScene()
        {
            const string scenePath = "Assets/Scenes/WorldforgeDevelopment.unity";
            if (File.Exists(scenePath) && EditorSceneManager.GetActiveScene().path != scenePath)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }

            SetupResourceDefinitions();

            var oakDef = AssetDatabase.LoadAssetAtPath<ResourceNodeDefinition>($"{NodesDirectory}/Node_OakTree.asset");
            var rockDef = AssetDatabase.LoadAssetAtPath<ResourceNodeDefinition>($"{NodesDirectory}/Node_GraniteRock.asset");
            var bushDef = AssetDatabase.LoadAssetAtPath<ResourceNodeDefinition>($"{NodesDirectory}/Node_WildBush.asset");

            var rootObj = GameObject.Find("Resource Nodes");
            if (rootObj == null)
            {
                rootObj = new GameObject("Resource Nodes");
                rootObj.transform.position = Vector3.zero;
            }

            // 1. Wild Bush (at center front)
            CreateOrUpdateSceneNode("Node_WildBush_Scene", bushDef, new Vector3(0f, 0f, 4f), rootObj.transform);

            // 2. Oak Tree (to right)
            CreateOrUpdateSceneNode("Node_OakTree_Scene", oakDef, new Vector3(4f, 0f, 5f), rootObj.transform);

            // 3. Granite Rock (to left)
            CreateOrUpdateSceneNode("Node_GraniteRock_Scene", rockDef, new Vector3(-4f, 0f, 4f), rootObj.transform);

            if (!Application.isPlaying)
            {
                var activeScene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
            }

            Debug.Log("[Worldforge] Created Resource Nodes in active scene under 'Resource Nodes' GameObject.");
        }

        [MenuItem("Worldforge/Setup/Attach Basic Tools to Player Avatar")]
        public static void AttachBasicToolsToPlayerAvatar()
        {
            var player = GameObject.Find("Player") ?? GameObject.FindWithTag("Player");
            if (player == null)
            {
                var avatar = Object.FindAnyObjectByType<PlayerAvatar>();
                if (avatar != null)
                {
                    player = avatar.gameObject;
                }
            }

            if (player == null)
            {
                Debug.LogWarning("[Worldforge] Player GameObject not found in active scene. Start game or select player first.");
                return;
            }

            var axeDef = AssetDatabase.LoadAssetAtPath<ItemDefinition>($"{ItemsDirectory}/Item_Tool_BasicAxe.asset");
            var toolBehaviour = player.GetComponent<GatheringToolBehaviour>();
            if (toolBehaviour == null)
            {
                toolBehaviour = player.AddComponent<GatheringToolBehaviour>();
            }

            if (axeDef != null)
            {
                toolBehaviour.ToolItem = axeDef;
            }
            else
            {
                toolBehaviour.Configure(ToolType.Axe, 1.5f, 1.0f, 1, 1f);
            }

            Debug.Log($"[Worldforge] GatheringToolBehaviour attached/configured on '{player.name}' with Axe tool.");
        }

        [MenuItem("Worldforge/Testing/Run Gathering Validation Checks")]
        public static void RunGatheringValidationChecks()
        {
            Debug.Log("<b><color=#00E5FF>[Worldforge Gathering Test]</color></b> Starting gathering workflow validation checks...");

            var woodDef = Resources.Load<ItemDefinition>("Definitions/Items/Item_Resource_Wood");
            var stoneDef = Resources.Load<ItemDefinition>("Definitions/Items/Item_Resource_Stone");
            var fiberDef = Resources.Load<ItemDefinition>("Definitions/Items/Item_Resource_Fiber");
            var axeDef = Resources.Load<ItemDefinition>("Definitions/Items/Item_Tool_BasicAxe");
            var pickaxeDef = Resources.Load<ItemDefinition>("Definitions/Items/Item_Tool_BasicPickaxe");

            var oakDef = Resources.Load<ResourceNodeDefinition>("Definitions/Nodes/Node_OakTree");
            var rockDef = Resources.Load<ResourceNodeDefinition>("Definitions/Nodes/Node_GraniteRock");
            var bushDef = Resources.Load<ResourceNodeDefinition>("Definitions/Nodes/Node_WildBush");

            var allDefinitionsValid = woodDef != null && stoneDef != null && fiberDef != null &&
                                      axeDef != null && pickaxeDef != null &&
                                      oakDef != null && rockDef != null && bushDef != null;

            if (!allDefinitionsValid)
            {
                Debug.LogError("[Worldforge Gathering Test] Failed to load one or more ScriptableObject definitions from Resources/Definitions/. Run 'Worldforge > Setup > Setup Resource Definitions' first.");
                return;
            }

            Debug.Log("[Worldforge Gathering Test] <color=#00FF66>✓ Test 1/5 PASSED:</color> All Item and ResourceNode Definition assets loaded successfully.");

            // 2. Tool requirement validation
            var axeTool = new ToolProperties(ToolType.Axe, 1.5f, 1.0f, 1, 1f);
            var pickaxeTool = new ToolProperties(ToolType.Pickaxe, 2.5f, 1.0f, 1, 1f);

            var bushHandValidation = bushDef.Requirements.Validate(null, 10f, 1.5f);
            var oakHandValidation = oakDef.Requirements.Validate(null, 10f, 1.5f);
            var oakAxeValidation = oakDef.Requirements.Validate(axeTool, 10f, 1.5f);
            var rockPickaxeValidation = rockDef.Requirements.Validate(pickaxeTool, 10f, 1.5f);

            if (!bushHandValidation.IsSuccess || oakHandValidation.IsSuccess || !oakAxeValidation.IsSuccess || !rockPickaxeValidation.IsSuccess)
            {
                Debug.LogError("[Worldforge Gathering Test] Failed: Tool requirement validation check mismatch.");
                return;
            }

            Debug.Log("[Worldforge Gathering Test] <color=#00FF66>✓ Test 2/5 PASSED:</color> Tool type, tier, and power validations verified (Hand vs Axe vs Pickaxe).");

            // 3. Distance & Stamina constraints
            var outOfRangeValidation = oakDef.Requirements.Validate(axeTool, 10f, 10f);
            var lowStaminaValidation = oakDef.Requirements.Validate(axeTool, 1f, 1.5f);

            if (outOfRangeValidation.FailureReason != GatheringFailureReason.OutOfRange ||
                lowStaminaValidation.FailureReason != GatheringFailureReason.InsufficientStamina)
            {
                Debug.LogError("[Worldforge Gathering Test] Failed: Distance or Stamina failure reason mismatch.");
                return;
            }

            Debug.Log("[Worldforge Gathering Test] <color=#00FF66>✓ Test 3/5 PASSED:</color> Distance and Stamina failure constraints correctly enforced.");

            // 4. Dynamic Duration & Harvest Yield calculations
            var service = new RuntimeGatheringService();
            var normalDuration = service.CalculateGatherDuration(oakDef, null);
            var fastTool = new ToolProperties(ToolType.Axe, 2.0f, 2.0f, 1, 1f);
            var fastDuration = service.CalculateGatherDuration(oakDef, fastTool);

            if (fastDuration >= normalDuration)
            {
                Debug.LogError($"[Worldforge Gathering Test] Failed: Efficiency 2.0 did not reduce gather duration (Normal: {normalDuration}s, Fast: {fastDuration}s).");
                service.Dispose();
                return;
            }

            var primaryYield = service.CalculatePrimaryYield(oakDef, axeTool);
            if (primaryYield < oakDef.PrimaryMinAmount)
            {
                Debug.LogError("[Worldforge Gathering Test] Failed: Harvest yield calculation returned amount less than minimum.");
                service.Dispose();
                return;
            }

            Debug.Log($"[Worldforge Gathering Test] <color=#00FF66>✓ Test 4/5 PASSED:</color> Dynamic duration & yield calculations verified (Yielded: {primaryYield}x {oakDef.PrimaryYield.DisplayName}, Duration: {normalDuration:F1}s -> {fastDuration:F1}s).");

            // 5. Service & Domain Event dispatching
            var eventFired = false;
            service.NodeGathered += (evt) => { eventFired = true; };

            var dummyObj = new GameObject("TestNode");
            var nodeBehaviour = dummyObj.AddComponent<ResourceNodeBehaviour>();
            nodeBehaviour.Initialize(bushDef);
            nodeBehaviour.BindGatheringService(service);

            var actionResult = service.ProcessGatheringAction(nodeBehaviour, null, null);
            Object.DestroyImmediate(dummyObj);
            service.Dispose();

            if (!actionResult.IsSuccess || !eventFired)
            {
                Debug.LogError("[Worldforge Gathering Test] Failed: Gathering service process action or domain event dispatch failed.");
                return;
            }

            Debug.Log("[Worldforge Gathering Test] <color=#00FF66>✓ Test 5/5 PASSED:</color> RuntimeGatheringService harvest action & domain event dispatching verified.");
            Debug.Log("<b><color=#00FF66>[Worldforge Gathering Test] ALL GATHERING TESTS PASSED (5/5)!</color></b>");
        }

        private static void EnsureDirectoriesExist()
        {
            EnsureDirectory("Assets/Resources");
            EnsureDirectory("Assets/Resources/Definitions");
            EnsureDirectory(ItemsDirectory);
            EnsureDirectory(NodesDirectory);
            EnsureDirectory("Assets/Prefabs");
            EnsureDirectory(PrefabsDirectory);
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

        private static ResourceNodeDefinition CreateOrUpdateNode(
            string assetName,
            string nodeCode,
            string displayName,
            string description,
            string biomeType,
            ItemDefinition primaryYield,
            int minAmount,
            int maxAmount,
            ResourceYieldEntry[] bonusYields,
            GatheringRequirements requirements,
            float hardness,
            float maxHealth,
            float baseDuration,
            float respawnTime,
            bool canRespawn,
            int discoveryXP,
            GameObject worldPrefab)
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

            if (bonusYields != null && bonusYields.Length > 0)
            {
                var bonusProp = so.FindProperty("_bonusYields");
                bonusProp.arraySize = bonusYields.Length;
                for (var i = 0; i < bonusYields.Length; i++)
                {
                    var elem = bonusProp.GetArrayElementAtIndex(i);
                    elem.FindPropertyRelative("_item").objectReferenceValue = bonusYields[i].Item;
                    elem.FindPropertyRelative("_minAmount").intValue = bonusYields[i].MinAmount;
                    elem.FindPropertyRelative("_maxAmount").intValue = bonusYields[i].MaxAmount;
                    elem.FindPropertyRelative("_dropChance").floatValue = bonusYields[i].DropChance;
                }
            }

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

            if (worldPrefab != null)
            {
                so.FindProperty("_worldPrefab").objectReferenceValue = worldPrefab;
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            if (existing == null)
            {
                AssetDatabase.CreateAsset(node, path);
            }
            else
            {
                EditorUtility.SetDirty(existing);
            }

            return node;
        }

        private static void SetNodePrefab(ResourceNodeDefinition definition, GameObject prefab)
        {
            if (definition == null || prefab == null) return;

            var so = new SerializedObject(definition);
            so.FindProperty("_worldPrefab").objectReferenceValue = prefab;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static GameObject CreateOakTreePrefab(ResourceNodeDefinition definition)
        {
            var prefabPath = $"{PrefabsDirectory}/Node_OakTree_Prefab.prefab";
            var root = new GameObject("Node_OakTree");

            var collider = root.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 1.5f, 0f);
            collider.radius = 0.5f;
            collider.height = 3.0f;

            var intactVisual = new GameObject("IntactVisual");
            intactVisual.transform.SetParent(root.transform, false);

            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(intactVisual.transform, false);
            trunk.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            trunk.transform.localScale = new Vector3(0.5f, 1.25f, 0.5f);
            RemoveCollider(trunk);
            SetColor(trunk, new Color(0.45f, 0.28f, 0.15f));

            var foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage.name = "Foliage";
            foliage.transform.SetParent(intactVisual.transform, false);
            foliage.transform.localPosition = new Vector3(0f, 3.2f, 0f);
            foliage.transform.localScale = new Vector3(2.5f, 2.2f, 2.5f);
            RemoveCollider(foliage);
            SetColor(foliage, new Color(0.18f, 0.55f, 0.22f));

            var depletedVisual = new GameObject("DepletedVisual");
            depletedVisual.transform.SetParent(root.transform, false);

            var stump = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stump.name = "Stump";
            stump.transform.SetParent(depletedVisual.transform, false);
            stump.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            stump.transform.localScale = new Vector3(0.6f, 0.2f, 0.6f);
            RemoveCollider(stump);
            SetColor(stump, new Color(0.38f, 0.24f, 0.12f));

            depletedVisual.SetActive(false);

            var nodeBehaviour = root.AddComponent<ResourceNodeBehaviour>();
            var so = new SerializedObject(nodeBehaviour);
            so.FindProperty("_definition").objectReferenceValue = definition;
            so.FindProperty("_intactVisual").objectReferenceValue = intactVisual;
            so.FindProperty("_depletedVisual").objectReferenceValue = depletedVisual;
            so.FindProperty("_currentHealth").floatValue = definition != null ? definition.MaxHealth : 100f;
            so.ApplyModifiedPropertiesWithoutUndo();

            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            return savedPrefab;
        }

        private static GameObject CreateGraniteRockPrefab(ResourceNodeDefinition definition)
        {
            var prefabPath = $"{PrefabsDirectory}/Node_GraniteRock_Prefab.prefab";
            var root = new GameObject("Node_GraniteRock");

            var collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.75f, 0f);
            collider.size = new Vector3(2.0f, 1.5f, 1.8f);

            var intactVisual = new GameObject("IntactVisual");
            intactVisual.transform.SetParent(root.transform, false);

            var rockMain = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rockMain.name = "RockMain";
            rockMain.transform.SetParent(intactVisual.transform, false);
            rockMain.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            rockMain.transform.localScale = new Vector3(1.8f, 1.4f, 1.6f);
            rockMain.transform.localRotation = Quaternion.Euler(10f, 25f, 5f);
            RemoveCollider(rockMain);
            SetColor(rockMain, new Color(0.48f, 0.5f, 0.55f));

            var depletedVisual = new GameObject("DepletedVisual");
            depletedVisual.transform.SetParent(root.transform, false);

            var rubble = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rubble.name = "Rubble";
            rubble.transform.SetParent(depletedVisual.transform, false);
            rubble.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            rubble.transform.localScale = new Vector3(1.4f, 0.25f, 1.2f);
            RemoveCollider(rubble);
            SetColor(rubble, new Color(0.38f, 0.39f, 0.42f));

            depletedVisual.SetActive(false);

            var nodeBehaviour = root.AddComponent<ResourceNodeBehaviour>();
            var so = new SerializedObject(nodeBehaviour);
            so.FindProperty("_definition").objectReferenceValue = definition;
            so.FindProperty("_intactVisual").objectReferenceValue = intactVisual;
            so.FindProperty("_depletedVisual").objectReferenceValue = depletedVisual;
            so.FindProperty("_currentHealth").floatValue = definition != null ? definition.MaxHealth : 150f;
            so.ApplyModifiedPropertiesWithoutUndo();

            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            return savedPrefab;
        }

        private static GameObject CreateWildBushPrefab(ResourceNodeDefinition definition)
        {
            var prefabPath = $"{PrefabsDirectory}/Node_WildBush_Prefab.prefab";
            var root = new GameObject("Node_WildBush");

            var collider = root.AddComponent<SphereCollider>();
            collider.center = new Vector3(0f, 0.5f, 0f);
            collider.radius = 0.8f;

            var intactVisual = new GameObject("IntactVisual");
            intactVisual.transform.SetParent(root.transform, false);

            var bushCluster1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bushCluster1.name = "Bush1";
            bushCluster1.transform.SetParent(intactVisual.transform, false);
            bushCluster1.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            bushCluster1.transform.localScale = new Vector3(1.4f, 0.9f, 1.3f);
            RemoveCollider(bushCluster1);
            SetColor(bushCluster1, new Color(0.25f, 0.65f, 0.28f));

            var depletedVisual = new GameObject("DepletedVisual");
            depletedVisual.transform.SetParent(root.transform, false);

            var twigs = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            twigs.name = "Twigs";
            twigs.transform.SetParent(depletedVisual.transform, false);
            twigs.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            twigs.transform.localScale = new Vector3(0.5f, 0.15f, 0.5f);
            RemoveCollider(twigs);
            SetColor(twigs, new Color(0.35f, 0.25f, 0.18f));

            depletedVisual.SetActive(false);

            var nodeBehaviour = root.AddComponent<ResourceNodeBehaviour>();
            var so = new SerializedObject(nodeBehaviour);
            so.FindProperty("_definition").objectReferenceValue = definition;
            so.FindProperty("_intactVisual").objectReferenceValue = intactVisual;
            so.FindProperty("_depletedVisual").objectReferenceValue = depletedVisual;
            so.FindProperty("_currentHealth").floatValue = definition != null ? definition.MaxHealth : 50f;
            so.ApplyModifiedPropertiesWithoutUndo();

            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            return savedPrefab;
        }

        private static void CreateOrUpdateSceneNode(
            string name,
            ResourceNodeDefinition definition,
            Vector3 position,
            Transform parent)
        {
            var existing = GameObject.Find(name);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            GameObject nodeObj;
            if (definition != null && definition.WorldPrefab != null)
            {
                nodeObj = (GameObject)PrefabUtility.InstantiatePrefab(definition.WorldPrefab);
            }
            else
            {
                nodeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                nodeObj.AddComponent<ResourceNodeBehaviour>();
            }

            nodeObj.name = name;
            nodeObj.transform.position = position;
            nodeObj.transform.SetParent(parent, true);

            var behaviour = nodeObj.GetComponent<ResourceNodeBehaviour>();
            if (behaviour != null && definition != null)
            {
                behaviour.Initialize(definition);
            }
        }

        private static void RemoveCollider(GameObject obj)
        {
            var col = obj.GetComponent<Collider>();
            if (col != null)
            {
                Object.DestroyImmediate(col);
            }
        }

        private static void SetColor(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer == null) return;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                shader = Shader.Find("Diffuse");
            }

            if (shader != null)
            {
                var mat = new Material(shader) { color = color };
                renderer.sharedMaterial = mat;
            }
        }
    }
}
