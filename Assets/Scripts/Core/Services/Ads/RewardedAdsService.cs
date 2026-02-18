using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;
using Zenject;

namespace Core.Services
{
    /// <summary>
    /// Только реклама: init + load + show + сигнал "earned".
    /// Никакой логики бустеров внутри.
    /// </summary>
    public sealed class RewardedAdsService : IService
    {
        private readonly Dictionary<RewardType, RewardedAd> _ads = new();
        private readonly HashSet<RewardType> _loading = new();

        
        [Inject] private IConfigService _configs;
        private AdsConfig Ads => _configs.Ads;

        private RewardedAd _rewardedAd;
        private bool _isLoading;

        private RewardType _pendingReward = RewardType.None;
        private bool _rewardGrantedThisAd;

        public bool IsReady => _rewardedAd != null && _rewardedAd.CanShowAd();

        /// <summary>Срабатывает только когда сеть реально дала "earned".</summary>
        public event Action<RewardType> OnRewardEarned;

        /// <summary>Для UI: готова ли реклама.</summary>
        public event Action<bool> OnAvailabilityChanged;

        public async UniTask InitializeAsync()
        {
#if UNITY_ANDROID || UNITY_IOS
            await InitializeMobileAdsAsync();
            EnsureLoaded(RewardType.Letter);
            EnsureLoaded(RewardType.Slowdown);
#else
            // В редакторе можешь оставить так (или тоже инициализировать — не критично)
            Debug.Log("[Ads] Skipping MobileAds init on this platform.");
#endif
        }

        private static UniTask InitializeMobileAdsAsync()
        {
            var tcs = new UniTaskCompletionSource();

            MobileAds.Initialize(_ =>
            {
                Debug.Log("[Ads] MobileAds initialized.");
                tcs.TrySetResult();
            });

            return tcs.Task;
        }

        public void EnsureLoaded(RewardType type)
        {
            if (_loading.Contains(type)) return;

            if (_ads.TryGetValue(type, out var ad) && ad != null && ad.CanShowAd())
            {
                OnAvailabilityChanged?.Invoke(true);
                return;
            }

            LoadRewarded(type);
        }

        public void ShowFor(RewardType rewardType)
        {
            if (rewardType == RewardType.None) return;

            if (!_ads.TryGetValue(rewardType, out var ad) || ad == null || !ad.CanShowAd())
            {
                Debug.Log($"[Ads] Rewarded not ready for {rewardType}. Triggering load.");
                EnsureLoaded(rewardType);
                return;
            }

            _pendingReward = rewardType;
            _rewardGrantedThisAd = false;

            Debug.Log($"[Ads] Showing rewarded for: {_pendingReward}");

            ad.Show(reward =>
            {
                if (_rewardGrantedThisAd) return;
                _rewardGrantedThisAd = true;

                Debug.Log($"[Ads] Reward earned signal received. pending={_pendingReward}, adRewardType={reward?.Type}, adRewardAmount={reward?.Amount}");
                OnRewardEarned?.Invoke(_pendingReward);

                _pendingReward = RewardType.None;
            });
        }


        private void LoadRewarded(RewardType type)
        {
            var adUnitId = Ads.GetRewardedId(type);
            if (string.IsNullOrEmpty(adUnitId))
            {
                Debug.LogError($"[Ads] Missing ad unit id for {type} in AdsConfig");
                return;
            }

            _loading.Add(type);
            OnAvailabilityChanged?.Invoke(false);

            // уничтожаем старую по этому типу
            if (_ads.TryGetValue(type, out var oldAd) && oldAd != null)
                oldAd.Destroy();

            var request = new AdRequest();
            Debug.Log($"[Ads] Loading rewarded for {type}...");

            RewardedAd.Load(adUnitId, request, (ad, error) =>
            {
                _loading.Remove(type);

                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[Ads] Failed to load rewarded for {type}: {error}");
                    OnAvailabilityChanged?.Invoke(false);
                    return;
                }

                _ads[type] = ad;
                Debug.Log($"[Ads] Rewarded loaded for {type}");
                OnAvailabilityChanged?.Invoke(true);

                HookFullScreenEvents(type, ad);
            });
        }


        private void HookFullScreenEvents(RewardType type, RewardedAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log($"[Ads] Rewarded closed for {type}. Reloading...");
                EnsureLoaded(type);
            };

            ad.OnAdFullScreenContentFailed += err =>
            {
                Debug.LogWarning($"[Ads] Fullscreen failed for {type}: {err}");
                EnsureLoaded(type);
            };
        }

    }
}
