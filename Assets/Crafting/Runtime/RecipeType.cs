namespace Worldforge.Crafting
{
    /// <summary>
    /// Categorization for crafting recipes.
    /// Corresponds to Schema Part 5: RecipeDefinition.RecipeType.
    /// </summary>
    public enum RecipeType
    {
        Basic = 0,
        Material = 1,
        Tool = 2,
        Weapon = 3,
        Armor = 4,
        Consumable = 5,
        Building = 6,
        Other = 7
    }

    /// <summary>
    /// Requirement for crafting workstation or facility.
    /// </summary>
    public enum CraftingStationType
    {
        None = 0,
        Workbench = 1,
        Forge = 2,
        Campfire = 3,
        CookingPot = 4
    }
}
