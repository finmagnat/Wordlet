using System;
using Core.Events;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;
using Zenject;

namespace Core.Services
{
    public sealed class InterstitialAdsService : IService
    {
        [Inject] private IConfigService _configs;
        [Inject] private AnalyticsService _analytics;
        private AdsConfig Ads => _configs.Ads;

        private InterstitialAd _ad;
        private bool _loading;
        private bool _showing;
        private float _loadStartedAt;
        private string _currentPlacement;

        public event Action<bool> OnAvailabilityChanged;
        public event Action<bool> OnShowingChanged;
        public event Action OnClosed;
        public event Action OnFailed;

        public bool IsReady => _ad != null && _ad.CanShowAd();
        public bool IsShowing => _showing;

        public UniTask InitializeAsync()
        {
#if UNITY_ANDROID || UNITY_IOS
            EnsureLoaded(AnalyticsEvents.Option.Initial);
#else
            Debug.Log("[Ads] Skipping interstitial init on this platform.");
#endif
            return UniTask.CompletedTask;
        }

        public void EnsureLoaded(string loadReason = AnalyticsEvents.Option.Initial)
        {
            if (!_configs.Ads.InterstitialIsActive || _loading)
                return;

            if (IsReady)
            {
                OnAvailabilityChanged?.Invoke(true);
                return;
            }

            Load(loadReason);
        }

        public void Show(string placement)
        {
            if (!_configs.Ads.InterstitialIsActive || !IsReady || _showing)
                return;

            _currentPlacement = placement;
            _showing = true;
            OnShowingChanged?.Invoke(true);
            OnAvailabilityChanged?.Invoke(false);

            EventBus.Raise(new AdsOverlayAcquireEvent());

            try
            {
                _ad.Show();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Ads] Failed to show interstitial: {ex}");
                _analytics.TrackEvent(
                    AnalyticsEvents.Ads.InterstitialShowFailed,
                    AdsAnalyticsHelper.PlacementErrorParams(placement, AdsAnalyticsHelper.NormalizeError(ex.ToString())));
                EventBus.Raise(new AdsOverlayReleaseEvent());

                _showing = false;
                OnShowingChanged?.Invoke(false);
                _currentPlacement = null;
                OnFailed?.Invoke();
                EnsureLoaded(AnalyticsEvents.Option.ReloadAfterFail);
            }
        }

        public async UniTask<AdShowResult> ShowAndWaitAsync(string placement, float timeoutSeconds = 30f)
        {
            if (!IsReady || _showing)
                return AdShowResult.NotReady;

            var tcs = new UniTaskCompletionSource<AdShowResult>();

            void OnClosedHandler()
            {
                OnClosed -= OnClosedHandler;
                OnFailed -= OnFailedHandler;
                tcs.TrySetResult(AdShowResult.Completed);
            }

            void OnFailedHandler()
            {
                OnClosed -= OnClosedHandler;
                OnFailed -= OnFailedHandler;
                tcs.TrySetResult(AdShowResult.Failed);
            }

            OnClosed += OnClosedHandler;
            OnFailed += OnFailedHandler;

            Show(placement);

            var whenAnyResult = await UniTask.WhenAny(
                tcs.Task,
                UniTask.Delay(TimeSpan.FromSeconds(timeoutSeconds), DelayType.UnscaledDeltaTime));

            if (whenAnyResult.hasResultLeft)
            {
                return whenAnyResult.result;
            }

            OnClosed -= OnClosedHandler;
            OnFailed -= OnFailedHandler;

            if (_showing)
            {
                EventBus.Raise(new AdsOverlayReleaseEvent());
                _showing = false;
                OnShowingChanged?.Invoke(false);
                _analytics.TrackEvent(
                    AnalyticsEvents.Ads.InterstitialShowFailed,
                    AdsAnalyticsHelper.PlacementErrorParams(placement, AnalyticsEvents.Option.Timeout));
                _currentPlacement = null;
            }

            return AdShowResult.Timeout;
        }

        private void Load(string loadReason)
        {
            var adUnitId = Ads.InterstitialAd;
            if (string.IsNullOrWhiteSpace(adUnitId))
            {
                Debug.LogError("[Ads] Interstitial ad unit id is empty in AdsConfig.");
                _analytics.TrackEvent(
                    AnalyticsEvents.Ads.InterstitialLoadFailed,
                    AdsAnalyticsHelper.PlacementLoadFailedParams(AnalyticsEvents.Placement.Global, AnalyticsEvents.Option.InvalidRequest, loadReason));
                return;
            }

            _loadStartedAt = Time.realtimeSinceStartup;
            _loading = true;
            _analytics.TrackEvent(
                AnalyticsEvents.Ads.InterstitialLoadStart,
                AdsAnalyticsHelper.PlacementLoadParams(AnalyticsEvents.Placement.Global, loadReason));
            OnAvailabilityChanged?.Invoke(false);

            _ad?.Destroy();
            _ad = null;

            var request = new AdRequest();
            Debug.Log("[Ads] Loading interstitial...");

            InterstitialAd.Load(adUnitId, request, (ad, error) =>
            {
                _loading = false;

                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[Ads] Failed to load interstitial: {error}");
                    _analytics.TrackEvent(
                        AnalyticsEvents.Ads.InterstitialLoadFailed,
                        AdsAnalyticsHelper.PlacementLoadFailedParams(
                            AnalyticsEvents.Placement.Global,
                            AdsAnalyticsHelper.NormalizeError(error?.ToString()),
                            loadReason));
                    OnAvailabilityChanged?.Invoke(false);
                    return;
                }

                _ad = ad;
                Hook(ad);

                Debug.Log("[Ads] Interstitial loaded.");
                _analytics.TrackEvent(
                    AnalyticsEvents.Ads.InterstitialLoadSuccess,
                    AdsAnalyticsHelper.PlacementLoadSuccessParams(
                        AnalyticsEvents.Placement.Global,
                        AdsAnalyticsHelper.ElapsedMs(_loadStartedAt),
                        loadReason));
                OnAvailabilityChanged?.Invoke(true);
            });
        }

        private void Hook(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentOpened += () =>
            {
                _analytics.TrackEvent(
                    AnalyticsEvents.Ads.InterstitialShowStart,
                    AdsAnalyticsHelper.PlacementParams(_currentPlacement));
            };

            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("[Ads] Interstitial closed.");
                _analytics.TrackEvent(
                    AnalyticsEvents.Ads.InterstitialClosed,
                    AdsAnalyticsHelper.PlacementParams(_currentPlacement));

                EventBus.Raise(new AdsOverlayReleaseEvent());

                _showing = false;
                OnShowingChanged?.Invoke(false);
                _currentPlacement = null;
                OnClosed?.Invoke();

                EnsureLoaded(AnalyticsEvents.Option.ReloadAfterClose);
            };

            ad.OnAdFullScreenContentFailed += error =>
            {
                Debug.LogWarning($"[Ads] Interstitial fullscreen failed: {error}");
                _analytics.TrackEvent(
                    AnalyticsEvents.Ads.InterstitialShowFailed,
                    AdsAnalyticsHelper.PlacementErrorParams(_currentPlacement, AdsAnalyticsHelper.NormalizeError(error?.ToString())));

                EventBus.Raise(new AdsOverlayReleaseEvent());

                _showing = false;
                OnShowingChanged?.Invoke(false);
                _currentPlacement = null;
                OnFailed?.Invoke();

                EnsureLoaded(AnalyticsEvents.Option.ReloadAfterFail);
            };
        }
    }
}
