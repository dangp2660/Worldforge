namespace Worldforge.Character.Traversal
{
    public readonly struct TraversalCheckResult
    {
        public TraversalCheckResult(
            bool isTraversable,
            float speedMultiplier,
            SurfaceType surfaceType,
            float slopeSpeedFactor,
            TraversalDenialReason denialReason)
        {
            IsTraversable = isTraversable;
            SpeedMultiplier = speedMultiplier;
            SurfaceType = surfaceType;
            SlopeSpeedFactor = slopeSpeedFactor;
            DenialReason = denialReason;
        }

        public bool IsTraversable { get; }

        public float SpeedMultiplier { get; }

        public SurfaceType SurfaceType { get; }

        public float SlopeSpeedFactor { get; }

        public TraversalDenialReason DenialReason { get; }

        /// <summary>
        /// Combined multiplier (surface speed * slope speed factor).
        /// </summary>
        public float EffectiveSpeedMultiplier
        {
            get { return SpeedMultiplier * SlopeSpeedFactor; }
        }

        public static TraversalCheckResult Allowed(SurfaceType surfaceType, float speedMultiplier, float slopeSpeedFactor)
        {
            return new TraversalCheckResult(
                true,
                speedMultiplier,
                surfaceType,
                slopeSpeedFactor,
                TraversalDenialReason.None);
        }

        public static TraversalCheckResult Denied(SurfaceType surfaceType, TraversalDenialReason reason)
        {
            return new TraversalCheckResult(
                false,
                0f,
                surfaceType,
                0f,
                reason);
        }

        public static TraversalCheckResult DefaultAllowed
        {
            get
            {
                return new TraversalCheckResult(
                    true,
                    1f,
                    SurfaceType.Default,
                    1f,
                    TraversalDenialReason.None);
            }
        }
    }
}
