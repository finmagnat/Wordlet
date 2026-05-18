using System;
using System.Collections.Generic;
using Core.Events;
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
        private readonly Dictionary<RewardType, float> _loadStartedAt = new();
        private readonly HashSet<RewardType> _rewardEarnedInCurrentShow = new();

        [Inject] private IConfigService _configs;
        [Inject] private AnalyticsService _analytics;
        private AdsConfig Ads => _configs.Ads;

        public event Action<RewardType> OnRewardEarned;
        public event Action<RewardType, bool> OnAvailabilityChanged;
        public event Action<RewardType, bool> OnShowingChanged;
        public event Action<RewardType> OnClosed;

        public UniTask InitializeAsync()
        {
#if UNITY_ANDROID || UNITY_IOS
            foreach (var definition in RewardedBoosterCatalog.All)
                EnsureLoaded(definition.RewardType, AnalyticsEvents.Option.Initial);
#else
            Debug.Log("[Ads] Skipping rewarded init on this platform.");
#endif
            return UniTask.CompletedTask;
        }

        public bool IsLoading(RewardType type)
            => _loading.Contains(type);
        
        public bool IsReady(RewardType type)
            => _ads.TryGetValue(type, out var ad) && ad != null && ad.CanShowAd();

        public bool IsShowing(RewardType type)
            => _showing.Contains(type);

        public void EnsureLoaded(RewardType type, string loadReason = AnalyticsEvents.Option.Initial)
        {
            if (type == RewardType.None)
                return;

            if (_loading.Contains(type))
                return;

            if (IsReady(type))
            {
                OnAvailabilityChanged?.Invoke(type, true);
                return;
            }

            LoadRewarded(type, loadReason);
        }

        public void ShowFor(RewardType rewardType)
        {
            if (rewardType == RewardType.None)
                return;

            bool isReady = IsReady(rewardType);
            _analytics.TrackEvent(
                AnalyticsEvents.Ads.RewardedShowAttempt,
                AdsAnalyticsHelper.RewardedAttemptParams(rewardType, isReady));

            if (!isReady)
            {
                Debug.Log($"[Ads] Rewarded not ready for {rewardType}. Triggering load.");
                _analytics.TrackEvent(
                    AnalyticsEvents.Ads.RewardedShowFailed,
                    AdsAnalyticsHelper.RewardedErrorParams(rewardType, AnalyticsEvents.Option.NotReady));
                EnsureLoaded(rewardType, AnalyticsEvents.Option.ReloadOnDemand);
                OnAvailabilityChanged?.Invoke(rewardType, false);
                return;
            }

            if (_showing.Contains(rewardType))
                return;

            if (!_ads.TryGetValue(rewardType, out var ad) || ad == null)
                return;

            bool rewardGranted = false;
            var expectedRewardType = rewardType;
            _rewardEarnedInCurrentShow.Remove(rewardType);

            _showing.Add(rewardType);
            OnShowingChanged?.Invoke(rewardType, true);
            OnAvailabilityChanged?.Invoke(rewardType, false);

            Debug.Log($"[Ads] Showing rewarded for: {rewardType}");

            EventBus.Raise(new AdsOverlayAcquireEvent());

            try
            {
                ad.Show(reward =>
                {
                    if (rewardGranted)
                        return;

                    rewardGranted = true;
                    _rewardEarnedInCurrentShow.Add(expectedRewardType);

                    Debug.Log($"[Ads] Reward earned. expected={expectedRewardType}, adRewardType={reward?.Type}, adRewardAmount={reward?.Amount}");
                    _analytics.TrackEvent(
                        AnalyticsEvents.Ads.RewardedEarned,
                        AdsAnalyticsHelper.RewardedTypeParams(expectedRewardType));
                    OnRewardEarned?.Invoke(expectedRewardType);
                });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Ads] Failed to show rewarded for {rewardType}: {ex}");
                _analytics.TrackEvent(
                    AnalyticsEvents.Ads.RewardedShowFailed,
                    AdsAnalyticsHelper.RewardedErrorParams(rewardType, AdsAnalyticsHelper.NormalizeError(ex.ToString())));

                EventBus.Raise(new AdsOverlayReleaseEvent());

                if (_showing.Remove(rewardType))
                    OnShowingChanged?.Invoke(rewardType, false);

                _rewardEarnedInCurrentShow.Remove(rewardType);
                EnsureLoaded(rewardType, AnalyticsEvents.Option.ReloadAfterFail);
            }
        }

        private void LoadRewarded(RewardType type, string loadReason)
        {
            var adUnitId = Ads.GetRewardedId(type);
            if (string.IsNullOrEmpty(adUnitId))
            {
                Debug.LogError($"[Ads] Missing ad unit id for {type} in AdsConfig.");
                _analytics.TrackEvent(
                    AnalyticsEvents.Ads.RewardedLoadFailed,
                    AdsAnalyticsHelper.RewardedLoadFailedParams(type, AnalyticsEvents.Option.InvalidRequest, loadReason));
                return;
            }

            _loadStartedAt[type] = Time.realtimeSinceStartup;
            _loading.Add(type);
            _analytics.TrackEvent(
                AnalyticsEvents.Ads.RewardedLoadStart,
                AdsAnalyticsHelper.RewardedLoadParams(type, loadReason));
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
                    _analytics.TrackEvent(
                        AnalyticsEvents.Ads.RewardedLoadFailed,
                        AdsAnalyticsHelper.RewardedLoadFailedParams(type, AdsAnalyticsHelper.NormalizeError(error?.ToString()), loadReason));
                    OnAvailabilityChanged?.Invoke(type, false);
                    return;
                }

                _ads[type] = ad;
                Debug.Log($"[Ads] Rewarded loaded for {type}");
                int loadTimeMs = _loadStartedAt.TryGetValue(type, out var startedAt)
                    ? AdsAnalyticsHelper.ElapsedMs(startedAt)
                    : 0;
                _analytics.TrackEvent(
                    AnalyticsEvents.Ads.RewardedLoadSuccess,
                    AdsAnalyticsHelper.RewardedLoadSuccessParams(type, loadTimeMs, loadReason));

                HookFullScreenEvents(type, ad);

                if (!_showing.Contains(type))
                    OnAvailabilityChanged?.Invoke(type, true);
            });
        }

        private void HookFullScreenEvents(RewardType type, RewardedAd ad)
        {
            ad.OnAdFullScreenContentOpened += () =>
            {
                _analytics.TrackEvent(
                    AnalyticsEvents.Ads.RewardedShowStart,
                    AdsAnalyticsHelper.RewardedTypeParams(type));
            };

            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log($"[Ads] Rewarded closed for {type}. Reloading...");
                bool wasRewarded = _rewardEarnedInCurrentShow.Contains(type);
                _analytics.TrackEvent(
                    AnalyticsEvents.Ads.RewardedClosed,
                    AdsAnalyticsHelper.RewardedClosedParams(wasRewarded, type));

                EventBus.Raise(new AdsOverlayReleaseEvent());

                if (_showing.Remove(type))
                    OnShowingChanged?.Invoke(type, false);

                _rewardEarnedInCurrentShow.Remove(type);
                OnClosed?.Invoke(type);
                EnsureLoaded(type, AnalyticsEvents.Option.ReloadAfterClose);
            };

            ad.OnAdFullScreenContentFailed += error =>
            {
                Debug.LogWarning($"[Ads] Rewarded fullscreen failed for {type}: {error}");
                _analytics.TrackEvent(
                    AnalyticsEvents.Ads.RewardedShowFailed,
                    AdsAnalyticsHelper.RewardedErrorParams(type, AdsAnalyticsHelper.NormalizeError(error?.ToString())));

                EventBus.Raise(new AdsOverlayReleaseEvent());

                if (_showing.Remove(type))
                    OnShowingChanged?.Invoke(type, false);

                _rewardEarnedInCurrentShow.Remove(type);
                OnClosed?.Invoke(type);
                EnsureLoaded(type, AnalyticsEvents.Option.ReloadAfterFail);
            };
        }
    }
}
