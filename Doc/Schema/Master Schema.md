// =====================================================
// WORLDFORGE MASTER SCHEMA V1
// Domain Level
// =====================================================

Table CreatureDefinition {
  CreatureID varchar [pk]
  RaceID varchar
  StatProfileID varchar
  LootTableID varchar
}

Table CreatureRace {
  RaceID varchar [pk]
}

Table CreatureTrait {
  TraitID varchar [pk]
  CreatureID varchar
}

Table StatProfile {
  StatProfileID varchar [pk]
  AttributeID varchar
  ResistanceProfileID varchar
}

Table AttributeDefinition {
  AttributeID varchar [pk]
}

Table ResistanceProfile {
  ResistanceProfileID varchar [pk]
}

Table AbilityDefinition {
  AbilityID varchar [pk]
}

Table AbilityEffect {
  EffectID varchar [pk]
  AbilityID varchar
}

Table StatusEffectDefinition {
  StatusEffectID varchar [pk]
}

Table SkillDefinition {
  SkillID varchar [pk]
  SkillTreeID varchar
}

Table SkillTree {
  SkillTreeID varchar [pk]
}

Table ItemDefinition {
  ItemID varchar [pk]
}

Table EquipmentDefinition {
  EquipmentID varchar [pk]
  ItemID varchar
}

Table Inventory {
  InventoryID varchar [pk]
  PlayerID varchar
  ItemID varchar
}

Table LootTable {
  LootTableID varchar [pk]
}

Table Recipe {
  RecipeID varchar [pk]
  ResultItemID varchar
  RequiredItemID varchar
}

Table WorldRegion {
  RegionID varchar [pk]
}

Table Biome {
  BiomeID varchar [pk]
  RegionID varchar
}

Table Dungeon {
  DungeonID varchar [pk]
  RegionID varchar
}

Table POI {
  POIID varchar [pk]
  RegionID varchar
}

Table ResourceNode {
  ResourceNodeID varchar [pk]
  BiomeID varchar
}

Table Settlement {
  SettlementID varchar [pk]
  RegionID varchar
}

Table Building {
  BuildingID varchar [pk]
  SettlementID varchar
}

Table NPCDefinition {
  NPCID varchar [pk]
  CreatureID varchar
  AIProfileID varchar
  FactionID varchar
  NPCCategory varchar
}

Table Faction {
  FactionID varchar [pk]
}

Table AIProfile {
  AIProfileID varchar [pk]
}

Table BehaviorProfile {
  BehaviorProfileID varchar [pk]
  AIProfileID varchar
}

Table QuestDefinition {
  QuestID varchar [pk]
  RegionID varchar
  NPCID varchar
}

Table QuestStage {
  StageID varchar [pk]
  QuestID varchar
}

Table QuestObjective {
  ObjectiveID varchar [pk]
  StageID varchar
}

Table Expedition {
  ExpeditionID varchar [pk]
  RegionID varchar
}

Table Technology {
  TechnologyID varchar [pk]
}

Table TeleportationWaypoint {
  WaypointID varchar [pk]
  RegionID varchar
  RequiredProgressionID varchar
}

Table Merchant {
  MerchantID varchar [pk]
  NPCID varchar
  CurrencyID varchar
}

Table Currency {
  CurrencyID varchar [pk]
}

Table PlayerProfile {
  PlayerID varchar [pk]
}

Table SaveProfile {
  SaveID varchar [pk]
  PlayerID varchar
}

// =====================
// RELATIONSHIPS
// =====================

Ref: CreatureDefinition.RaceID > CreatureRace.RaceID
Ref: CreatureDefinition.StatProfileID > StatProfile.StatProfileID
Ref: CreatureDefinition.LootTableID > LootTable.LootTableID

Ref: CreatureTrait.CreatureID > CreatureDefinition.CreatureID

Ref: StatProfile.AttributeID > AttributeDefinition.AttributeID
Ref: StatProfile.ResistanceProfileID > ResistanceProfile.ResistanceProfileID

Ref: AbilityEffect.AbilityID > AbilityDefinition.AbilityID

Ref: SkillDefinition.SkillTreeID > SkillTree.SkillTreeID

Ref: EquipmentDefinition.ItemID > ItemDefinition.ItemID

Ref: Inventory.PlayerID > PlayerProfile.PlayerID
Ref: Inventory.ItemID > ItemDefinition.ItemID

Ref: Recipe.ResultItemID > ItemDefinition.ItemID
Ref: Recipe.RequiredItemID > ItemDefinition.ItemID

Ref: Biome.RegionID > WorldRegion.RegionID
Ref: Dungeon.RegionID > WorldRegion.RegionID
Ref: POI.RegionID > WorldRegion.RegionID
Ref: ResourceNode.BiomeID > Biome.BiomeID

Ref: Settlement.RegionID > WorldRegion.RegionID
Ref: Building.SettlementID > Settlement.SettlementID

Ref: NPCDefinition.CreatureID > CreatureDefinition.CreatureID
Ref: NPCDefinition.AIProfileID > AIProfile.AIProfileID
Ref: NPCDefinition.FactionID > Faction.FactionID

Ref: BehaviorProfile.AIProfileID > AIProfile.AIProfileID

Ref: QuestStage.QuestID > QuestDefinition.QuestID
Ref: QuestObjective.StageID > QuestStage.StageID

Ref: QuestDefinition.RegionID > WorldRegion.RegionID
Ref: QuestDefinition.NPCID > NPCDefinition.NPCID

Ref: Merchant.NPCID > NPCDefinition.NPCID
Ref: Merchant.CurrencyID > Currency.CurrencyID

Ref: Expedition.RegionID > WorldRegion.RegionID
Ref: TeleportationWaypoint.RegionID > WorldRegion.RegionID

Ref: SaveProfile.PlayerID > PlayerProfile.PlayerID