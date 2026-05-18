using System;
using System.Collections.Generic;
using Core.Config;

namespace Core.Services
{
    public readonly struct RewardedBoosterDefinition
    {
        public RewardedBoosterDefinition(RewardType rewardType, BoosterType boosterType, string playFabKey)
        {
            RewardType = rewardType;
            BoosterType = boosterType;
            PlayFabKey = playFabKey;
        }

        public RewardType RewardType { get; }
        public BoosterType BoosterType { get; }
        public string PlayFabKey { get; }
    }

    public static class RewardedBoosterCatalog
    {
        // Add new rewarded booster mappings here; services derive ad loading, rewards, and PlayFab keys from this list.
        private static readonly RewardedBoosterDefinition[] Definitions =
        {
            new(RewardType.Letter, BoosterType.Letter, "boost_letter"),
            new(RewardType.Slowdown, BoosterType.Slowdown, "boost_slow"),
            new(RewardType.Eraser, BoosterType.Eraser, "boost_eraser"),
            new(RewardType.Swap, BoosterType.Swap, "boost_swap"),
        };

        public static IReadOnlyList<RewardedBoosterDefinition> All => Definitions;

        public static bool TryGetBoosterType(RewardType rewardType, out BoosterType boosterType)
        {
            foreach (var definition in Definitions)
            {
                if (definition.RewardType != rewardType)
                    continue;

                boosterType = definition.BoosterType;
                return true;
            }

            boosterType = BoosterType.None;
            return false;
        }

        public static bool TryGetPlayFabKey(BoosterType boosterType, out string key)
        {
            foreach (var definition in Definitions)
            {
                if (definition.BoosterType != boosterType)
                    continue;

                key = definition.PlayFabKey;
                return true;
            }

            key = null;
            return false;
        }

        public static string GetPlayFabKey(BoosterType boosterType)
        {
            if (TryGetPlayFabKey(boosterType, out var key))
                return key;

            throw new ArgumentOutOfRangeException(nameof(boosterType), boosterType, null);
        }

        public static List<string> GetPlayFabKeys()
        {
            var keys = new List<string>(Definitions.Length);
            foreach (var definition in Definitions)
                keys.Add(definition.PlayFabKey);

            return keys;
        }

        public static void EnsureRewardedAdUnits(List<RewardedAdUnitConfig> adUnits)
        {
            if (adUnits == null)
                return;

            foreach (var definition in Definitions)
            {
                if (ContainsAdUnit(adUnits, definition.RewardType))
                    continue;

                adUnits.Add(new RewardedAdUnitConfig
                {
                    RewardType = definition.RewardType
                });
            }
        }

        private static bool ContainsAdUnit(List<RewardedAdUnitConfig> adUnits, RewardType rewardType)
        {
            foreach (var adUnit in adUnits)
            {
                if (adUnit != null && adUnit.RewardType == rewardType)
                    return true;
            }

            return false;
        }
    }
}
