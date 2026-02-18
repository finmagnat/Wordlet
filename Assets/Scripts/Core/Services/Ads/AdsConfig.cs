using UnityEngine;

namespace Core.Services
{
    [CreateAssetMenu(menuName = "Configs/AdsConfig")]
    public sealed class AdsConfig : ScriptableObject
    {
        [Header("Android Rewarded Ad Units")]
        public string RewardedLetter;
        public string RewardedSlowdown;

#if UNITY_EDITOR
        [Header("Test Mode")]
        public bool UseTestIds;
#endif

        public string GetRewardedId(RewardType type)
        {
#if UNITY_EDITOR
            if (UseTestIds)
                return "ca-app-pub-3940256099942544/5224354917";
#endif

            return type switch
            {
                RewardType.Letter   => RewardedLetter,
                RewardType.Slowdown => RewardedSlowdown,
                _ => null
            };
        }
    }

}