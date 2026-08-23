//////////////////////////////////////////////////////////
// WORLD FORGE DATABASE SCHEMA v0.1
// PART 1 - CORE
//////////////////////////////////////////////////////////

Table GameplayTag {
  TagId bigint [pk, increment]
  TagCode varchar(100) [not null, unique]
  DisplayName varchar(100) [not null]
  Description text
  ParentTagId bigint
  Category varchar(50)
  IsDeprecated bool [default: false]
  SortOrder int
}

Table DamageType {
  DamageTypeId bigint [pk, increment]
  DamageCode varchar(50) [not null, unique]
  DisplayName varchar(100)
  Description text
  IgnoreArmor bool [default: false]
  CanCritical bool [default: true]
  IsElement bool [default: false]
  SortOrder int
}

Table Rarity {
  RarityId bigint [pk, increment]
  RarityCode varchar(50) [not null, unique]
  DisplayName varchar(100)
  Description text
  ColorHex varchar(20)
  DropMultiplier float
  ValueMultiplier float
  SortOrder int
}

Table AttributeType {
  AttributeId bigint [pk, increment]
  AttributeCode varchar(50) [not null, unique]
  DisplayName varchar(100)
  Description text
  DefaultValue float
  MinValue float
  MaxValue float
  IsVisible bool [default: true]
  SortOrder int
}

Table FormulaDefinition {
  FormulaId bigint [pk, increment]
  FormulaCode varchar(100) [not null, unique]
  DisplayName varchar(100)
  FormulaType varchar(50)
  Expression text
  Description text
  Version int
  IsEnabled bool [default: true]
}

Table StatType {
  StatId bigint [pk, increment]
  StatCode varchar(50) [not null, unique]
  DisplayName varchar(100)
  Description text
  Unit varchar(20)
  FormulaId bigint
  IsCalculated bool [default: true]
  AllowNegative bool [default: false]
  SortOrder int
}

Table GameConfig {
  ConfigId bigint [pk, increment]
  GameName varchar(100)
  Version varchar(20)
  DefaultLanguage varchar(20)
  DefaultDifficulty varchar(20)
  Gravity float
  TimeScale float
  DayLength float
  NightLength float
  TargetFPS int
  FixedTickRate float
  InteractionDistance float
  PickupDistance float
  BuildDistance float
  MaxBuildHeight float
  AutoSaveInterval float
  MaxSaveSlot int
  StartingRaceId bigint
  StartingInventorySlot int
  StartingHP float
  StartingMana float
  StartingStamina float
  WorldSeed int
  IsDebugMode bool
}

//////////////////////////////////////////////////////////
// RELATIONSHIP
//////////////////////////////////////////////////////////

Ref: GameplayTag.ParentTagId > GameplayTag.TagId

Ref: StatType.FormulaId > FormulaDefinition.FormulaId
//////////////////////////////////////////////////////////
// PART 2 - CHARACTER MODULE
//////////////////////////////////////////////////////////

Table CreatureType {
  CreatureTypeId bigint [pk, increment]
  CreatureTypeCode varchar(100) [not null, unique]
  DisplayName varchar(100) [not null]
  Description text
  IsPlayable bool [default: false]
  SortOrder int
}

Table Race {
  RaceId bigint [pk, increment]
  CreatureTypeId bigint
  RaceCode varchar(100) [not null, unique]
  DisplayName varchar(100)
  Description text
  IconPath varchar(255)
  DefaultAnimationProfile varchar(100)
  IsPlayable bool [default: false]
  SortOrder int
}

Table SubRace {
  SubRaceId bigint [pk, increment]
  RaceId bigint
  SubRaceCode varchar(100) [not null, unique]
  DisplayName varchar(100)
  Description text
  SortOrder int
}

Table CharacterProgressionProfile {
  ProgressionProfileId bigint [pk, increment]
  ProfileCode varchar(100) [not null, unique]
  DisplayName varchar(100)
  Description text
  StartingLevel int
  MaxLevel int
  XPFormulaId bigint
}

Table CharacterDefinition {
  CharacterDefinitionId bigint [pk, increment]
  CharacterCode varchar(100) [not null, unique]
  DisplayName varchar(100)
  Description text

  RaceId bigint
  SubRaceId bigint

  ProgressionProfileId bigint

  DefaultFactionId bigint

  StartingLevel int

  DefaultInventorySize int

  DefaultMoveSpeed float

  DefaultHP float
  DefaultMana float
  DefaultStamina float

  CanRespawn bool

  IsPlayer bool
}

Table RaceAttribute {
  RaceAttributeId bigint [pk, increment]

  RaceId bigint

  AttributeId bigint

  BaseValue float
}

Table RaceStat {
  RaceStatId bigint [pk, increment]

  RaceId bigint

  StatId bigint

  BaseValue float
}

Table CharacterTrait {
  CharacterTraitId bigint [pk, increment]

  CharacterDefinitionId bigint

  GameplayTagId bigint

  Description text
}

Table CharacterWeakness {
  CharacterWeaknessId bigint [pk, increment]

  CharacterDefinitionId bigint

  GameplayTagId bigint

  Description text
}

Table CharacterResistance {
  CharacterResistanceId bigint [pk, increment]

  CharacterDefinitionId bigint

  DamageTypeId bigint

  ResistancePercent float
}

Table CharacterAbility {
  CharacterAbilityId bigint [pk, increment]

  CharacterDefinitionId bigint

  AbilityId bigint

  UnlockLevel int

  IsDefault bool
}

//////////////////////////////////////////////////////////
// RELATIONSHIP
//////////////////////////////////////////////////////////

Ref: Race.CreatureTypeId > CreatureType.CreatureTypeId

Ref: SubRace.RaceId > Race.RaceId

Ref: CharacterDefinition.RaceId > Race.RaceId
Ref: CharacterDefinition.SubRaceId > SubRace.SubRaceId
Ref: CharacterDefinition.ProgressionProfileId > CharacterProgressionProfile.ProgressionProfileId

Ref: CharacterProgressionProfile.XPFormulaId > FormulaDefinition.FormulaId

Ref: RaceAttribute.RaceId > Race.RaceId
Ref: RaceAttribute.AttributeId > AttributeType.AttributeId

Ref: RaceStat.RaceId > Race.RaceId
Ref: RaceStat.StatId > StatType.StatId

Ref: CharacterTrait.CharacterDefinitionId > CharacterDefinition.CharacterDefinitionId
Ref: CharacterTrait.GameplayTagId > GameplayTag.TagId

Ref: CharacterWeakness.CharacterDefinitionId > CharacterDefinition.CharacterDefinitionId
Ref: CharacterWeakness.GameplayTagId > GameplayTag.TagId

Ref: CharacterResistance.CharacterDefinitionId > CharacterDefinition.CharacterDefinitionId
Ref: CharacterResistance.DamageTypeId > DamageType.DamageTypeId

Ref: CharacterAbility.CharacterDefinitionId > CharacterDefinition.CharacterDefinitionId

//////////////////////////////////////////////////////////
// PART 3 - ITEM MODULE
//////////////////////////////////////////////////////////

Table ItemCategory {
  ItemCategoryId bigint [pk, increment]
  ParentCategoryId bigint
  CategoryCode varchar(100) [not null, unique]
  DisplayName varchar(100)
  Description text
  IconPath varchar(255)
  SortOrder int
}

Table ItemDefinition {
  ItemId bigint [pk, increment]

  ItemCode varchar(100) [not null, unique]

  DisplayName varchar(100)

  Description text

  CategoryId bigint

  RarityId bigint

  Weight float

  MaxStack int

  BuyPrice int

  SellPrice int

  IconPath varchar(255)

  PrefabPath varchar(255)

  WorldPrefabPath varchar(255)

  IsUnique bool

  IsQuestItem bool

  IsTradable bool

  IsDroppable bool

  CanDestroy bool
}

Table ItemTag {
  ItemTagId bigint [pk, increment]

  ItemId bigint

  GameplayTagId bigint
}

Table ItemAttributeModifier {
  ModifierId bigint [pk, increment]

  ItemId bigint

  AttributeId bigint

  Value float
}

Table ItemStatModifier {
  ModifierId bigint [pk, increment]

  ItemId bigint

  StatId bigint

  Value float
}

Table ItemComponent {
  ComponentId bigint [pk, increment]

  ItemId bigint

  ComponentType varchar(100)

  DataKey varchar(100)

  DataValue varchar(255)
}

Table EquipmentComponent {
  EquipmentComponentId bigint [pk, increment]

  ItemId bigint

  EquipmentSlot varchar(100)

  RequiredLevel int

  MaxDurability float

  DurabilityMultiplier float
}

Table WeaponComponent {
  WeaponComponentId bigint [pk, increment]

  ItemId bigint

  WeaponType varchar(100)

  DamageTypeId bigint

  BaseDamage float

  AttackSpeed float

  AttackRange float

  CriticalChance float

  CriticalMultiplier float
}

Table ArmorComponent {
  ArmorComponentId bigint [pk, increment]

  ItemId bigint

  ArmorType varchar(100)

  Armor float

  MagicResistance float
}

Table ToolComponent {
  ToolComponentId bigint [pk, increment]

  ItemId bigint

  ToolType varchar(100)

  HarvestPower float

  Efficiency float
}

Table ConsumableComponent {
  ConsumableComponentId bigint [pk, increment]

  ItemId bigint

  Cooldown float

  ConsumeTime float

  IsReusable bool
}

Table ResourceComponent {
  ResourceComponentId bigint [pk, increment]

  ItemId bigint

  GatherTime float

  RespawnTime float

  Hardness float

  RequiredToolType varchar(100)
}

Table LootTable {
  LootTableId bigint [pk, increment]

  LootTableCode varchar(100) [unique]

  DisplayName varchar(100)
}

Table LootEntry {
  LootEntryId bigint [pk, increment]

  LootTableId bigint

  ItemId bigint

  DropChance float

  MinAmount int

  MaxAmount int
}

//////////////////////////////////////////////////////////
// RELATIONSHIP
//////////////////////////////////////////////////////////

Ref: ItemCategory.ParentCategoryId > ItemCategory.ItemCategoryId

Ref: ItemDefinition.CategoryId > ItemCategory.ItemCategoryId
Ref: ItemDefinition.RarityId > Rarity.RarityId

Ref: ItemTag.ItemId > ItemDefinition.ItemId
Ref: ItemTag.GameplayTagId > GameplayTag.TagId

Ref: ItemAttributeModifier.ItemId > ItemDefinition.ItemId
Ref: ItemAttributeModifier.AttributeId > AttributeType.AttributeId

Ref: ItemStatModifier.ItemId > ItemDefinition.ItemId
Ref: ItemStatModifier.StatId > StatType.StatId

Ref: ItemComponent.ItemId > ItemDefinition.ItemId

Ref: EquipmentComponent.ItemId > ItemDefinition.ItemId

Ref: WeaponComponent.ItemId > ItemDefinition.ItemId
Ref: WeaponComponent.DamageTypeId > DamageType.DamageTypeId

Ref: ArmorComponent.ItemId > ItemDefinition.ItemId

Ref: ToolComponent.ItemId > ItemDefinition.ItemId

Ref: ConsumableComponent.ItemId > ItemDefinition.ItemId

Ref: ResourceComponent.ItemId > ItemDefinition.ItemId

Ref: LootEntry.LootTableId > LootTable.LootTableId
Ref: LootEntry.ItemId > ItemDefinition.ItemId

//////////////////////////////////////////////////////////
// PART 4 - INVENTORY & EQUIPMENT MODULE
//////////////////////////////////////////////////////////

Table InventoryType {
  InventoryTypeId bigint [pk, increment]

  InventoryTypeCode varchar(100) [not null, unique]

  DisplayName varchar(100)

  Description text

  DefaultSlotCount int

  AllowExpansion bool
}

Table InventoryDefinition {
  InventoryDefinitionId bigint [pk, increment]

  InventoryCode varchar(100) [not null, unique]

  InventoryTypeId bigint

  DisplayName varchar(100)

  SlotCount int

  WeightLimit float

  AllowSort bool

  AllowStack bool

  AllowQuickMove bool
}

Table InventorySlot {
  InventorySlotId bigint [pk, increment]

  InventoryDefinitionId bigint

  SlotIndex int

  SlotType varchar(100)

  AllowEquipment bool

  AllowItem bool

  AllowQuickAccess bool
}

Table ItemStack {
  ItemStackId bigint [pk, increment]

  ItemId bigint

  Quantity int

  CurrentDurability float

  RandomSeed bigint

  CustomName varchar(100)

  IsLocked bool
}

Table InventoryItem {
  InventoryItemId bigint [pk, increment]

  InventoryDefinitionId bigint

  InventorySlotId bigint

  ItemStackId bigint
}

//////////////////////////////////////////////////////////
// EQUIPMENT
//////////////////////////////////////////////////////////

Table EquipmentSlot {
  EquipmentSlotId bigint [pk, increment]

  SlotCode varchar(100) [unique]

  DisplayName varchar(100)

  SortOrder int
}

Table EquipmentLoadout {
  LoadoutId bigint [pk, increment]

  CharacterDefinitionId bigint

  LoadoutName varchar(100)

  IsDefault bool
}

Table EquippedItem {
  EquippedItemId bigint [pk, increment]

  LoadoutId bigint

  EquipmentSlotId bigint

  ItemStackId bigint
}

//////////////////////////////////////////////////////////
// STORAGE
//////////////////////////////////////////////////////////

Table StorageDefinition {
  StorageId bigint [pk, increment]

  StorageCode varchar(100) [unique]

  DisplayName varchar(100)

  SlotCount int

  WeightLimit float

  AllowShared bool

  AllowSort bool
}

Table StorageItem {
  StorageItemId bigint [pk, increment]

  StorageId bigint

  ItemStackId bigint

  SlotIndex int
}

//////////////////////////////////////////////////////////
// RELATIONSHIP
//////////////////////////////////////////////////////////

Ref: InventoryDefinition.InventoryTypeId > InventoryType.InventoryTypeId

Ref: InventorySlot.InventoryDefinitionId > InventoryDefinition.InventoryDefinitionId

Ref: ItemStack.ItemId > ItemDefinition.ItemId

Ref: InventoryItem.InventoryDefinitionId > InventoryDefinition.InventoryDefinitionId
Ref: InventoryItem.InventorySlotId > InventorySlot.InventorySlotId
Ref: InventoryItem.ItemStackId > ItemStack.ItemStackId

Ref: EquipmentLoadout.CharacterDefinitionId > CharacterDefinition.CharacterDefinitionId

Ref: EquippedItem.LoadoutId > EquipmentLoadout.LoadoutId
Ref: EquippedItem.EquipmentSlotId > EquipmentSlot.EquipmentSlotId
Ref: EquippedItem.ItemStackId > ItemStack.ItemStackId

Ref: StorageItem.StorageId > StorageDefinition.StorageId
Ref: StorageItem.ItemStackId > ItemStack.ItemStackId

//////////////////////////////////////////////////////////
// PART 5 - ABILITY & CRAFTING MODULE
//////////////////////////////////////////////////////////

Table AbilityCategory {
  AbilityCategoryId bigint [pk, increment]
  CategoryCode varchar(100) [not null, unique]
  DisplayName varchar(100)
  Description text
  ParentCategoryId bigint
}

Table AbilityDefinition {
  AbilityId bigint [pk, increment]

  AbilityCode varchar(100) [not null, unique]

  DisplayName varchar(100)

  Description text

  CategoryId bigint

  DamageTypeId bigint

  IconPath varchar(255)

  AnimationId bigint

  Cooldown float

  CastTime float

  ChannelTime float

  ManaCost float

  StaminaCost float

  HealthCost float

  Range float

  Radius float

  MaxTarget int

  IsPassive bool

  IsToggle bool

  IsChanneled bool
}

Table AbilityRequirement {
  RequirementId bigint [pk, increment]

  AbilityId bigint

  RequirementType varchar(100)

  RequirementValue varchar(255)
}

Table AbilityScaling {
  ScalingId bigint [pk, increment]

  AbilityId bigint

  StatId bigint

  ScaleValue float
}

Table AbilityEffect {
  EffectId bigint [pk, increment]

  AbilityId bigint

  EffectType varchar(100)

  EffectValue float

  Duration float

  Interval float
}

//////////////////////////////////////////////////////////
// CRAFTING
//////////////////////////////////////////////////////////

Table RecipeDefinition {

  RecipeId bigint [pk, increment]

  RecipeCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  RecipeType varchar(50)

  CraftFunction varchar(100)

  CraftTime float

  RequiredLevel int

  SuccessRate float

  IsUnlockedByDefault bool
}

Table RecipeIngredient {

  RecipeIngredientId bigint [pk, increment]

  RecipeId bigint

  ItemId bigint

  Amount int

  IsConsumed bool
}

Table RecipeOutput {

  RecipeOutputId bigint [pk, increment]

  RecipeId bigint

  ItemId bigint

  Amount int

  Probability float
}

Table RecipeRequirement {

  RecipeRequirementId bigint [pk, increment]

  RecipeId bigint

  RequirementType varchar(100)

  RequirementValue varchar(255)
}

//////////////////////////////////////////////////////////
// RELATIONSHIP
//////////////////////////////////////////////////////////

Ref: AbilityCategory.ParentCategoryId > AbilityCategory.AbilityCategoryId

Ref: AbilityDefinition.CategoryId > AbilityCategory.AbilityCategoryId
Ref: AbilityDefinition.DamageTypeId > DamageType.DamageTypeId

Ref: AbilityRequirement.AbilityId > AbilityDefinition.AbilityId

Ref: AbilityScaling.AbilityId > AbilityDefinition.AbilityId
Ref: AbilityScaling.StatId > StatType.StatId

Ref: AbilityEffect.AbilityId > AbilityDefinition.AbilityId

Ref: RecipeIngredient.RecipeId > RecipeDefinition.RecipeId
Ref: RecipeIngredient.ItemId > ItemDefinition.ItemId

Ref: RecipeOutput.RecipeId > RecipeDefinition.RecipeId
Ref: RecipeOutput.ItemId > ItemDefinition.ItemId

Ref: RecipeRequirement.RecipeId > RecipeDefinition.RecipeId
//////////////////////////////////////////////////////////
// PART 6 - BUILDING & SETTLEMENT MODULE
//////////////////////////////////////////////////////////

Table BuildingCategory {

  BuildingCategoryId bigint [pk, increment]

  CategoryCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  ParentCategoryId bigint
}

Table BuildingDefinition {

  BuildingId bigint [pk, increment]

  BuildingCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  CategoryId bigint

  IconPath varchar(255)

  PrefabPath varchar(255)

  MaxHealth float

  BuildTime float

  BuildRadius float

  MaxWorker int

  CanUpgrade bool

  CanRepair bool

  CanDestroy bool
}

Table BuildingRequirement {

  RequirementId bigint [pk, increment]

  BuildingId bigint

  ItemId bigint

  Amount int
}

Table BuildingUpgrade {

  UpgradeId bigint [pk, increment]

  BuildingId bigint

  NextBuildingId bigint

  UpgradeTime float

  RequiredLevel int
}

Table BuildingFunction {

  FunctionId bigint [pk, increment]

  BuildingId bigint

  FunctionType varchar(100)

  FunctionValue varchar(255)
}

Table SettlementDefinition {

  SettlementId bigint [pk, increment]

  SettlementCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  MaxPopulation int

  MaxBuilding int

  TerritoryRadius float
}

Table SettlementBuilding {

  SettlementBuildingId bigint [pk, increment]

  SettlementId bigint

  BuildingId bigint

  MaxCount int
}

//////////////////////////////////////////////////////////
// RELATIONSHIP
//////////////////////////////////////////////////////////

Ref: BuildingCategory.ParentCategoryId > BuildingCategory.BuildingCategoryId

Ref: BuildingDefinition.CategoryId > BuildingCategory.BuildingCategoryId

Ref: BuildingRequirement.BuildingId > BuildingDefinition.BuildingId
Ref: BuildingRequirement.ItemId > ItemDefinition.ItemId

Ref: BuildingUpgrade.BuildingId > BuildingDefinition.BuildingId
Ref: BuildingUpgrade.NextBuildingId > BuildingDefinition.BuildingId

Ref: BuildingFunction.BuildingId > BuildingDefinition.BuildingId

Ref: SettlementBuilding.SettlementId > SettlementDefinition.SettlementId
Ref: SettlementBuilding.BuildingId > BuildingDefinition.BuildingId
//////////////////////////////////////////////////////////
// PART 7 - WORLD & EXPLORATION MODULE
//////////////////////////////////////////////////////////

Table WorldDefinition {

  WorldId bigint [pk, increment]

  WorldCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  Seed bigint

  MaxPlayer int

  DefaultBiomeId bigint
}

Table RegionDefinition {

  RegionId bigint [pk, increment]

  WorldId bigint

  RegionCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  RecommendedLevel int

  MinTemperature float

  MaxTemperature float
}

Table BiomeDefinition {

  BiomeId bigint [pk, increment]

  BiomeCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  Temperature float

  Humidity float

  DangerLevel int
}

Table RegionBiome {

  RegionBiomeId bigint [pk, increment]

  RegionId bigint

  BiomeId bigint

  Weight float
}

Table LandmarkDefinition {

  LandmarkId bigint [pk, increment]

  LandmarkCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  RegionId bigint

  DiscoveryXP int
}

Table ResourceNodeDefinition {

  ResourceNodeId bigint [pk, increment]

  NodeCode varchar(100) [unique]

  DisplayName varchar(100)

  BiomeId bigint

  ItemId bigint

  RespawnTime float

  GatherTime float

  MaxHealth float
}

Table SpawnGroup {

  SpawnGroupId bigint [pk, increment]

  SpawnCode varchar(100) [unique]

  DisplayName varchar(100)

  RegionId bigint

  RespawnTime float

  MaxSpawn int
}

Table SpawnEntry {

  SpawnEntryId bigint [pk, increment]

  SpawnGroupId bigint

  CreatureId bigint

  SpawnWeight float

  MinCount int

  MaxCount int
}

Table ExplorationReward {

  ExplorationRewardId bigint [pk, increment]

  LandmarkId bigint

  RewardType varchar(100)

  RewardValue varchar(255)
}

//////////////////////////////////////////////////////////
// RELATIONSHIP
//////////////////////////////////////////////////////////

Ref: WorldDefinition.DefaultBiomeId > BiomeDefinition.BiomeId

Ref: RegionDefinition.WorldId > WorldDefinition.WorldId

Ref: RegionBiome.RegionId > RegionDefinition.RegionId
Ref: RegionBiome.BiomeId > BiomeDefinition.BiomeId

Ref: LandmarkDefinition.RegionId > RegionDefinition.RegionId

Ref: ResourceNodeDefinition.BiomeId > BiomeDefinition.BiomeId
Ref: ResourceNodeDefinition.ItemId > ItemDefinition.ItemId

Ref: SpawnGroup.RegionId > RegionDefinition.RegionId

Ref: SpawnEntry.SpawnGroupId > SpawnGroup.SpawnGroupId
Ref: SpawnEntry.CreatureId > CharacterDefinition.CharacterDefinitionId

Ref: ExplorationReward.LandmarkId > LandmarkDefinition.LandmarkId

//////////////////////////////////////////////////////////
// PART 8 - AI, NPC & FACTION MODULE
//////////////////////////////////////////////////////////

Table FactionDefinition {

  FactionId bigint [pk, increment]

  FactionCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  IsPlayerFaction bool

  DefaultReputation int
}

Table FactionRelation {

  RelationId bigint [pk, increment]

  SourceFactionId bigint

  TargetFactionId bigint

  Reputation int

  IsHostile bool

  IsFriendly bool
}

Table AIProfile {

  AIProfileId bigint [pk, increment]

  AIProfileCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  DetectionRange float

  ChaseRange float

  AttackRange float

  PatrolRadius float

  LeashDistance float

  UpdateInterval float
}

Table BehaviorTree {

  BehaviorTreeId bigint [pk, increment]

  TreeCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  AssetPath varchar(255)
}

Table NPCRole {

  RoleId bigint [pk, increment]

  RoleCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text
}

Table NPCDefinition {

  NPCId bigint [pk, increment]

  NPCCode varchar(100) [unique]

  DisplayName varchar(100)

  CharacterDefinitionId bigint

  FactionId bigint

  RoleId bigint

  AIProfileId bigint

  BehaviorTreeId bigint

  DialogueId bigint

  ShopId bigint
}

Table DialogueDefinition {

  DialogueId bigint [pk, increment]

  DialogueCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text
}

Table DialogueNode {

  NodeId bigint [pk, increment]

  DialogueId bigint

  NodeCode varchar(100)

  Speaker varchar(100)

  DialogueText text

  NextNodeCode varchar(100)
}

Table MerchantDefinition {

  MerchantId bigint [pk, increment]

  MerchantCode varchar(100) [unique]

  NPCId bigint

  ShopName varchar(100)

  RefreshTime float
}

Table MerchantItem {

  MerchantItemId bigint [pk, increment]

  MerchantId bigint

  ItemId bigint

  Stock int

  BuyPrice int

  SellPrice int
}

//////////////////////////////////////////////////////////
// RELATIONSHIP
//////////////////////////////////////////////////////////

Ref: FactionRelation.SourceFactionId > FactionDefinition.FactionId
Ref: FactionRelation.TargetFactionId > FactionDefinition.FactionId

Ref: NPCDefinition.CharacterDefinitionId > CharacterDefinition.CharacterDefinitionId
Ref: NPCDefinition.FactionId > FactionDefinition.FactionId
Ref: NPCDefinition.RoleId > NPCRole.RoleId
Ref: NPCDefinition.AIProfileId > AIProfile.AIProfileId
Ref: NPCDefinition.BehaviorTreeId > BehaviorTree.BehaviorTreeId
Ref: NPCDefinition.DialogueId > DialogueDefinition.DialogueId

Ref: DialogueNode.DialogueId > DialogueDefinition.DialogueId

Ref: MerchantDefinition.NPCId > NPCDefinition.NPCId

Ref: MerchantItem.MerchantId > MerchantDefinition.MerchantId
Ref: MerchantItem.ItemId > ItemDefinition.ItemId

//////////////////////////////////////////////////////////
// PART 9 - QUEST & WORLD PROGRESSION MODULE
//////////////////////////////////////////////////////////

Table QuestCategory {

  QuestCategoryId bigint [pk, increment]

  CategoryCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text
}

Table QuestDefinition {

  QuestId bigint [pk, increment]

  QuestCode varchar(100) [not null, unique]

  DisplayName varchar(100)

  Description text

  CategoryId bigint

  QuestType varchar(50)

  RecommendedLevel int

  IsRepeatable bool

  AutoAccept bool

  AutoComplete bool
}

Table QuestStage {

  QuestStageId bigint [pk, increment]

  QuestId bigint

  StageIndex int

  StageName varchar(100)

  Description text
}

Table QuestObjective {

  ObjectiveId bigint [pk, increment]

  QuestStageId bigint

  ObjectiveType varchar(100)

  TargetType varchar(100)

  TargetId bigint

  TargetCode varchar(100)

  RequiredAmount int
}

Table QuestRequirement {

  RequirementId bigint [pk, increment]

  QuestId bigint

  RequirementType varchar(100)

  RequirementValue varchar(255)
}

Table QuestReward {

  RewardId bigint [pk, increment]

  QuestId bigint

  RewardType varchar(100)

  TargetId bigint

  Amount int
}

//////////////////////////////////////////////////////////
// EXPEDITION
//////////////////////////////////////////////////////////

Table ExpeditionDefinition {

  ExpeditionId bigint [pk, increment]

  ExpeditionCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  RegionId bigint

  RecommendedLevel int

  Duration float

  Difficulty varchar(50)
}

Table ExpeditionObjective {

  ExpeditionObjectiveId bigint [pk, increment]

  ExpeditionId bigint

  ObjectiveType varchar(100)

  TargetId bigint

  Amount int
}

Table ExpeditionReward {

  ExpeditionRewardId bigint [pk, increment]

  ExpeditionId bigint

  RewardType varchar(100)

  TargetId bigint

  Amount int
}

//////////////////////////////////////////////////////////
// WORLD PROGRESSION
//////////////////////////////////////////////////////////

Table ProgressionChapter {

  ChapterId bigint [pk, increment]

  ChapterCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  SortOrder int
}

Table ProgressionMilestone {

  MilestoneId bigint [pk, increment]

  ChapterId bigint

  MilestoneCode varchar(100)

  DisplayName varchar(100)

  Description text

  UnlockType varchar(100)

  UnlockTargetId bigint
}

//////////////////////////////////////////////////////////
// RELATIONSHIP
//////////////////////////////////////////////////////////

Ref: QuestDefinition.CategoryId > QuestCategory.QuestCategoryId

Ref: QuestStage.QuestId > QuestDefinition.QuestId

Ref: QuestObjective.QuestStageId > QuestStage.QuestStageId

Ref: QuestRequirement.QuestId > QuestDefinition.QuestId

Ref: QuestReward.QuestId > QuestDefinition.QuestId

Ref: ExpeditionDefinition.RegionId > RegionDefinition.RegionId

Ref: ExpeditionObjective.ExpeditionId > ExpeditionDefinition.ExpeditionId

Ref: ExpeditionReward.ExpeditionId > ExpeditionDefinition.ExpeditionId

Ref: ProgressionMilestone.ChapterId > ProgressionChapter.ChapterId

//////////////////////////////////////////////////////////
// PART 10 - WORLD SIMULATION & SAVE MODULE
//////////////////////////////////////////////////////////

Table SeasonDefinition {

  SeasonId bigint [pk, increment]

  SeasonCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  OrderIndex int

  DurationInDay float
}

Table WeatherDefinition {

  WeatherId bigint [pk, increment]

  WeatherCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  TemperatureModifier float

  HumidityModifier float

  WindModifier float

  VisibilityModifier float
}

Table WorldEventCategory {

  CategoryId bigint [pk, increment]

  CategoryCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text
}

Table WorldEventDefinition {

  EventId bigint [pk, increment]

  EventCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  CategoryId bigint

  RegionId bigint

  MinLevel int

  Duration float

  Cooldown float

  IsRepeatable bool
}

Table WorldEventRequirement {

  RequirementId bigint [pk, increment]

  EventId bigint

  RequirementType varchar(100)

  RequirementValue varchar(255)
}

Table WorldEventReward {

  RewardId bigint [pk, increment]

  EventId bigint

  RewardType varchar(100)

  TargetId bigint

  Amount int
}

//////////////////////////////////////////////////////////
// SAVE
//////////////////////////////////////////////////////////

Table SaveProfile {

  SaveProfileId bigint [pk, increment]

  SaveCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text

  Version varchar(20)

  WorldId bigint

  LastPlayed datetime
}

Table SaveSlot {

  SaveSlotId bigint [pk, increment]

  SaveProfileId bigint

  SlotIndex int

  DisplayName varchar(100)

  IsAutoSave bool

  PlayTime float

  LastSave datetime
}

Table PersistenceGroup {

  PersistenceGroupId bigint [pk, increment]

  GroupCode varchar(100) [unique]

  DisplayName varchar(100)

  Description text
}

Table PersistenceEntry {

  PersistenceEntryId bigint [pk, increment]

  PersistenceGroupId bigint

  EntityType varchar(100)

  EntityId bigint

  PersistenceType varchar(100)
}

//////////////////////////////////////////////////////////
// RELATIONSHIP
//////////////////////////////////////////////////////////

Ref: WorldEventDefinition.CategoryId > WorldEventCategory.CategoryId

Ref: WorldEventDefinition.RegionId > RegionDefinition.RegionId

Ref: WorldEventRequirement.EventId > WorldEventDefinition.EventId

Ref: WorldEventReward.EventId > WorldEventDefinition.EventId

Ref: SaveProfile.WorldId > WorldDefinition.WorldId

Ref: SaveSlot.SaveProfileId > SaveProfile.SaveProfileId

Ref: PersistenceEntry.PersistenceGroupId > PersistenceGroup.PersistenceGroupId