namespace Worldforge.Item
{
    public interface IGatheringTool
    {
        ToolType ToolType { get; }

        float HarvestPower { get; }

        float Efficiency { get; }

        int ToolTier { get; }
    }
}
