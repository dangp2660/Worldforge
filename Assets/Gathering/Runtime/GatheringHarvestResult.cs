using System;
using System.Collections.Generic;
using Worldforge.Item;

namespace Worldforge.Gathering
{
    [Serializable]
    public readonly struct BonusYieldResult : IEquatable<BonusYieldResult>
    {
        public ItemDefinition Item { get; }
        public int Amount { get; }

        public BonusYieldResult(ItemDefinition item, int amount)
        {
            Item = item;
            Amount = amount;
        }

        public bool Equals(BonusYieldResult other)
        {
            return Equals(Item, other.Item) && Amount == other.Amount;
        }

        public override bool Equals(object obj)
        {
            return obj is BonusYieldResult other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Item, Amount);
        }
    }

    [Serializable]
    public readonly struct GatheringHarvestResult : IEquatable<GatheringHarvestResult>
    {
        private static readonly BonusYieldResult[] EmptyBonusYields = Array.Empty<BonusYieldResult>();

        public bool IsSuccess { get; }
        public ItemDefinition PrimaryYieldItem { get; }
        public int PrimaryYieldAmount { get; }
        public IReadOnlyList<BonusYieldResult> BonusYields { get; }
        public int DiscoveryXP { get; }
        public float DamageDealt { get; }
        public float RemainingHealth { get; }
        public bool WasDepleted { get; }
        public string FailureMessage { get; }

        public GatheringHarvestResult(
            bool isSuccess,
            ItemDefinition primaryYieldItem,
            int primaryYieldAmount,
            IReadOnlyList<BonusYieldResult> bonusYields,
            int discoveryXP,
            float damageDealt,
            float remainingHealth,
            bool wasDepleted,
            string failureMessage = null)
        {
            IsSuccess = isSuccess;
            PrimaryYieldItem = primaryYieldItem;
            PrimaryYieldAmount = primaryYieldAmount;
            BonusYields = bonusYields ?? EmptyBonusYields;
            DiscoveryXP = discoveryXP;
            DamageDealt = damageDealt;
            RemainingHealth = remainingHealth;
            WasDepleted = wasDepleted;
            FailureMessage = failureMessage ?? string.Empty;
        }

        public static GatheringHarvestResult Success(
            ItemDefinition primaryItem,
            int primaryAmount,
            IReadOnlyList<BonusYieldResult> bonusYields,
            int discoveryXP,
            float damageDealt,
            float remainingHealth,
            bool wasDepleted)
        {
            return new GatheringHarvestResult(
                true,
                primaryItem,
                primaryAmount,
                bonusYields,
                discoveryXP,
                damageDealt,
                remainingHealth,
                wasDepleted,
                string.Empty);
        }

        public static GatheringHarvestResult Failed(string message)
        {
            return new GatheringHarvestResult(
                false,
                null,
                0,
                EmptyBonusYields,
                0,
                0f,
                0f,
                false,
                message ?? "Gathering failed.");
        }

        public bool Equals(GatheringHarvestResult other)
        {
            return IsSuccess == other.IsSuccess &&
                   Equals(PrimaryYieldItem, other.PrimaryYieldItem) &&
                   PrimaryYieldAmount == other.PrimaryYieldAmount &&
                   DiscoveryXP == other.DiscoveryXP &&
                   MathfApproximately(DamageDealt, other.DamageDealt) &&
                   MathfApproximately(RemainingHealth, other.RemainingHealth) &&
                   WasDepleted == other.WasDepleted &&
                   string.Equals(FailureMessage, other.FailureMessage, StringComparison.Ordinal);
        }

        private static bool MathfApproximately(float a, float b)
        {
            return Math.Abs(b - a) < Math.Max(0.000001f * Math.Max(Math.Abs(a), Math.Abs(b)), float.Epsilon * 8);
        }

        public override bool Equals(object obj)
        {
            return obj is GatheringHarvestResult other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IsSuccess, PrimaryYieldItem, PrimaryYieldAmount, DiscoveryXP, DamageDealt, RemainingHealth, WasDepleted);
        }
    }
}
