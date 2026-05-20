using System;
using System.Collections.Generic;
using Core.Config;
using UnityEngine;

namespace Core.Services
{
    [Serializable]
    public sealed class RewardedAdUnitConfig
    {
        public RewardType RewardType;
        public string AdUnitId;
    }

    [CreateAssetMenu(menuName = "Configs/AdsConfig")]
    public sealed class AdsConfig : ScriptableObject
    {   
        [Header("Android Rewarded Ad Units")]
        public List<RewardedAdUnitConfig> RewardedAdUnits = new();

        [Header("Android Interstitial Ad Units")]
        public bool InterstitialIsActive;
        public string InterstitialAd;

#if UNITY_EDITOR
        [Header("Test Mode")]
        public bool UseTestIds;
#endif

        public string GetRewardedId(RewardType type)
        {
            if (type == RewardType.None)
                return null;

#if UNITY_EDITOR
            if (UseTestIds)
                return "ca-app-pub-3940256099942544/5224354917";
#endif

            if (RewardedAdUnits == null)
                return null;

            foreach (var adUnit in RewardedAdUnits)
            {
                if (adUnit != null && adUnit.RewardType == type)
                    return adUnit.AdUnitId;
            }

            return null;
        }

        private void OnValidate()
        {
            RewardedAdUnits ??= new List<RewardedAdUnitConfig>();
            RewardedBoosterCatalog.EnsureRewardedAdUnits(RewardedAdUnits);
        }
    }
}
