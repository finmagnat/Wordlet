using System;
using System.Collections.Generic;
using Core.Config;

namespace Core.Services
{
    public sealed class DailyBonusState
    {
        public static DailyBonusState Unavailable { get; } = new(0, null, false, false);

        public DailyBonusState(int dailyRewardDay, DateTime? lastClaimUtc, bool claimAvailable, bool isUnlocked)
        {
            DailyRewardDay = dailyRewardDay;
            LastClaimUtc = lastClaimUtc;
            ClaimAvailable = claimAvailable;
            IsUnlocked = isUnlocked;
        }

        public int DailyRewardDay { get; }
        public DateTime? LastClaimUtc { get; }
        public bool ClaimAvailable { get; }
        public bool IsUnlocked { get; }
    }

    public sealed class DailyBonusRewardItem
    {
        public DailyBonusRewardItem(BoosterType boosterType, int amount)
        {
            BoosterType = boosterType;
            Amount = amount;
        }

        public BoosterType BoosterType { get; }
        public int Amount { get; }
    }

    public sealed class DailyBonusCycle
    {
        public static DailyBonusCycle Empty { get; } = new(0, Array.Empty<DailyBonusCycleDay>());

        public DailyBonusCycle(int cycleLength, IReadOnlyList<DailyBonusCycleDay> days)
        {
            CycleLength = cycleLength;
            Days = days ?? Array.Empty<DailyBonusCycleDay>();
        }

        public int CycleLength { get; }
        public IReadOnlyList<DailyBonusCycleDay> Days { get; }
    }

    public sealed class DailyBonusCycleDay
    {
        public DailyBonusCycleDay(
            int day,
            string rewardKind,
            IReadOnlyList<DailyBonusRewardItem> rewards,
            IReadOnlyList<DailyBonusChestDrop> chestDrops)
        {
            Day = day;
            RewardKind = rewardKind;
            Rewards = rewards ?? Array.Empty<DailyBonusRewardItem>();
            ChestDrops = chestDrops ?? Array.Empty<DailyBonusChestDrop>();
        }

        public int Day { get; }
        public string RewardKind { get; }
        public IReadOnlyList<DailyBonusRewardItem> Rewards { get; }
        public IReadOnlyList<DailyBonusChestDrop> ChestDrops { get; }
    }

    public sealed class DailyBonusChestDrop
    {
        public DailyBonusChestDrop(
            int weight,
            string mode,
            int multiplier,
            IReadOnlyList<BoosterType> pool,
            IReadOnlyList<DailyBonusRewardItem> rewards)
        {
            Weight = weight;
            Mode = mode;
            Multiplier = multiplier;
            Pool = pool ?? Array.Empty<BoosterType>();
            Rewards = rewards ?? Array.Empty<DailyBonusRewardItem>();
        }

        public int Weight { get; }
        public string Mode { get; }
        public int Multiplier { get; }
        public IReadOnlyList<BoosterType> Pool { get; }
        public IReadOnlyList<DailyBonusRewardItem> Rewards { get; }
    }

    public sealed class DailyBonusClaimResult
    {
        private DailyBonusClaimResult(
            bool success,
            DailyBonusState state,
            int claimedDay,
            IReadOnlyList<DailyBonusRewardItem> rewards,
            bool isJackpot,
            int multiplier,
            BoosterType selectedBooster,
            string error)
        {
            Success = success;
            State = state ?? DailyBonusState.Unavailable;
            ClaimedDay = claimedDay;
            Rewards = rewards ?? Array.Empty<DailyBonusRewardItem>();
            IsJackpot = isJackpot;
            Multiplier = multiplier;
            SelectedBooster = selectedBooster;
            Error = error;
        }

        public bool Success { get; }
        public DailyBonusState State { get; }
        public int ClaimedDay { get; }
        public IReadOnlyList<DailyBonusRewardItem> Rewards { get; }
        public bool IsJackpot { get; }
        public int Multiplier { get; }
        public BoosterType SelectedBooster { get; }
        public string Error { get; }

        public static DailyBonusClaimResult Granted(
            DailyBonusState state,
            int claimedDay,
            IReadOnlyList<DailyBonusRewardItem> rewards,
            bool isJackpot,
            int multiplier,
            BoosterType selectedBooster)
        {
            return new DailyBonusClaimResult(
                true,
                state,
                claimedDay,
                rewards,
                isJackpot,
                multiplier,
                selectedBooster,
                null);
        }

        public static DailyBonusClaimResult NotAvailable(DailyBonusState state, string error = null)
        {
            return new DailyBonusClaimResult(
                false,
                state,
                0,
                Array.Empty<DailyBonusRewardItem>(),
                false,
                0,
                BoosterType.None,
                error);
        }
    }
}
