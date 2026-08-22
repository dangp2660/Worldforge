namespace Worldforge.Character.Traversal
{
    /// <summary>
    /// Read-only public API for querying traversal state.
    /// Other systems (UI, AI, animation) can use this to react to surface conditions.
    /// Future versions will add methods like CanTraverse(Vector3 targetPosition) for AI pathfinding.
    /// </summary>
    public interface ITraversalService
    {
        bool IsTraversalActive { get; }

        TraversalCheckResult LastResult { get; }

        SurfaceType CurrentSurface { get; }

        float CurrentSpeedMultiplier { get; }
    }
}
