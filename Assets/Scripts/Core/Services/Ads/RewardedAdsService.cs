using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;
using Zenject;

namespace Core.Services
{
    public sealed class RewardedAdsService : IService
    {
        private readonly Dictionary<RewardType, RewardedAd> _ads = new();
        private readonly HashSet<RewardType> _loading = new();
        private readonly HashSet<RewardType> _showing = new();

        [Inject] private IConfigService _configs;
        private AdsConfig Ads => _configs.Ads;

        private bool _rewardGrantedThisAd;

        public event Action<RewardType> OnRewardEarned;
        public event Action<RewardType, bool> OnAvailabilityChanged;
        public event Action<RewardType, bool> OnShowingChanged;
        public event Action<RewardType> OnClosed;

        public async UniTask InitializeAsync()
        {
#if UNITY_ANDROID || UNITY_IOS
            await InitializeMobileAdsAsync();

            EnsureLoaded(RewardType.Letter);
            EnsureLoaded(RewardType.Slowdown);
#else
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

        public bool IsReady(RewardType type)
            => _ads.TryGetValue(type, out var ad) && ad != null && ad.CanShowAd();

        public bool IsShowing(RewardType type)
            => _showing.Contains(type);

        public void EnsureLoaded(RewardType type)
        {
            if (type == RewardType.None) return;
            if (_loading.Contains(type)) return;

            if (IsReady(type))
            {
                OnAvailabilityChanged?.Invoke(type, true);
                return;
            }

            LoadRewarded(type);
        }

        public void ShowFor(RewardType rewardType)
        {
            if (rewardType == RewardType.None) return;

            if (!IsReady(rewardType))
            {
                Debug.Log($"[Ads] Rewarded not ready for {rewardType}. Triggering load.");
                EnsureLoaded(rewardType);
                OnAvailabilityChanged?.Invoke(rewardType, false);
                return;
            }

            if (_showing.Contains(rewardType))
                return;

            if (!_ads.TryGetValue(rewardType, out var ad) || ad == null)
                return;

            _rewardGrantedThisAd = false;

            _showing.Add(rewardType);
            OnShowingChanged?.Invoke(rewardType, true);
            OnAvailabilityChanged?.Invoke(rewardType, false);

            Debug.Log($"[Ads] Showing rewarded for: {rewardType}");

            // Важно: используем локальную переменную expected, чтобы не словить “гонки”
            var expected = rewardType;

            ad.Show(reward =>
            {
                if (_rewardGrantedThisAd) return;
                _rewardGrantedThisAd = true;

                Debug.Log($"[Ads] Reward earned. expected={expected}, adRewardType={reward?.Type}, adRewardAmount={reward?.Amount}");
                OnRewardEarned?.Invoke(expected);
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
            OnAvailabilityChanged?.Invoke(type, false);

            if (_ads.TryGetValue(type, out var oldAd) && oldAd != null)
                oldAd.Destroy();

            Debug.Log($"[Ads] Loading rewarded for {type}...");
            var request = new AdRequest();

            RewardedAd.Load(adUnitId, request, (ad, error) =>
            {
                _loading.Remove(type);

                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[Ads] Failed to load rewarded for {type}: {error}");
                    OnAvailabilityChanged?.Invoke(type, false);
                    return;
                }

                _ads[type] = ad;
                Debug.Log($"[Ads] Rewarded loaded for {type}");

                HookFullScreenEvents(type, ad);

                if (!_showing.Contains(type))
                    OnAvailabilityChanged?.Invoke(type, true);
            });
        }

        private void HookFullScreenEvents(RewardType type, RewardedAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log($"[Ads] Rewarded closed for {type}. Reloading...");

                if (_showing.Remove(type))
                    OnShowingChanged?.Invoke(type, false);

                OnClosed?.Invoke(type);
                
                EnsureLoaded(type);
            };

            ad.OnAdFullScreenContentFailed += err =>
            {
                Debug.LogWarning($"[Ads] Fullscreen failed for {type}: {err}");

                if (_showing.Remove(type))
                    OnShowingChanged?.Invoke(type, false);

                EnsureLoaded(type);
            };
        }
    }
}