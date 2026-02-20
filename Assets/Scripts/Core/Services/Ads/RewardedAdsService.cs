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
        private readonly HashSet<RewardType> _showing = new();

        [Inject] private IConfigService _configs;
        private AdsConfig Ads => _configs.Ads;

        private RewardType _pendingReward = RewardType.None;
        private bool _rewardGrantedThisAd;

        /// <summary>Срабатывает только когда сеть реально дала "earned".</summary>
        public event Action<RewardType> OnRewardEarned;

        /// <summary>Для UI: готовность rewarded по конкретному типу.</summary>
        public event Action<RewardType, bool> OnAvailabilityChanged;

        /// <summary>Для UI: сейчас показывается ли реклама по конкретному типу.</summary>
        public event Action<RewardType, bool> OnShowingChanged;

        public async UniTask InitializeAsync()
        {
#if UNITY_ANDROID || UNITY_IOS
            await InitializeMobileAdsAsync();

            // Прелоадим оба типа
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
        {
            return _ads.TryGetValue(type, out var ad) && ad != null && ad.CanShowAd();
        }

        public bool IsShowing(RewardType type) => _showing.Contains(type);

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
            Debug.Log($"[Ads] ShowFor called with: {rewardType}");
            
            if (rewardType == RewardType.None) return;

            if (!IsReady(rewardType))
            {
                Debug.Log($"[Ads] Rewarded not ready for {rewardType}. Triggering load.");
                EnsureLoaded(rewardType);
                OnAvailabilityChanged?.Invoke(rewardType, false);
                return;
            }

            if (_showing.Contains(rewardType))
            {
                Debug.LogWarning($"[Ads] ShowFor ignored. Already showing: {rewardType}");
                return;
            }

            if (!_ads.TryGetValue(rewardType, out var ad) || ad == null)
                return;

            _pendingReward = rewardType;
            _rewardGrantedThisAd = false;

            _showing.Add(rewardType);
            OnShowingChanged?.Invoke(rewardType, true);
            OnAvailabilityChanged?.Invoke(rewardType, false);

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
            OnAvailabilityChanged?.Invoke(type, false);

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
                    OnAvailabilityChanged?.Invoke(type, false);
                    return;
                }

                _ads[type] = ad;
                Debug.Log($"[Ads] Rewarded loaded for {type}");

                HookFullScreenEvents(type, ad);

                // Если прямо сейчас не показываем — считаем готовой
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

                EnsureLoaded(type);
            };

            ad.OnAdFullScreenContentFailed += err =>
            {
                Debug.LogWarning($"[Ads] Fullscreen failed for {type}: {err}");

                if (_showing.Remove(type))
                    OnShowingChanged?.Invoke(type, false);

                EnsureLoaded(type);
            };

            ad.OnAdFullScreenContentOpened += () => Debug.Log($"[Ads] Rewarded opened for {type}.");
            ad.OnAdImpressionRecorded += () => Debug.Log($"[Ads] Impression recorded for {type}.");
        }
    }
}
