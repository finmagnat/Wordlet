using System;
using System.Collections.Generic;
using Core.Config;
using UnityEngine;

namespace Core.Services
{
    [Serializable]
    public sealed class AdsRewardItem
    {
        public BoosterType BoosterType;
        public RewardType RewardType;
        public int Count;
        [Min(0), Tooltip("More weight - more chance of showing")]
        public int Weight;
        public string[] LabelLocaleKeys;
    }

    [CreateAssetMenu(menuName = "Configs/AdsConfig")]
    public sealed class AdsConfig : ScriptableObject
    {   
        [Header("AdMob Rewarded Ad Unit")]
        public string RewardedAd;

        [Header("AdMob Interstitial Ad Unit")]
        public bool InterstitialIsActive;
        public string InterstitialAd;

#if UNITY_EDITOR
        [Header("Test Mode")]
        public bool UseTestIds;
#endif

        [Header("Rewarded Booster Offers (finish popups)")]
        public List<AdsRewardItem> AdsRewardItems = new();
        
        public string GetRewardedId()
        {
#if UNITY_EDITOR
            if (UseTestIds)
                return "ca-app-pub-3940256099942544/5224354917";
#endif
            return RewardedAd;
        }
    }
}
