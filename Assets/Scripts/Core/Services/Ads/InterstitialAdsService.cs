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
        private AdsConfig Ads => _configs.Ads;

        private InterstitialAd _ad;
        private bool _loading;
        private bool _showing;

        public event Action<bool> OnAvailabilityChanged;
        public event Action<bool> OnShowingChanged;
        public event Action OnClosed;

        public bool IsReady => _ad != null && _ad.CanShowAd();
        public bool IsShowing => _showing;

        public UniTask InitializeAsync()
        {
#if UNITY_ANDROID || UNITY_IOS
            EnsureLoaded();
#else
            Debug.Log("[Ads] Skipping interstitial init on this platform.");
#endif
            return UniTask.CompletedTask;
        }

        public void EnsureLoaded()
        {
            if (_loading)
                return;

            if (IsReady)
            {
                OnAvailabilityChanged?.Invoke(true);
                return;
            }

            Load();
        }

        public void Show()
        {
            if (!IsReady || _showing)
                return;

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
                EventBus.Raise(new AdsOverlayReleaseEvent());

                _showing = false;
                OnShowingChanged?.Invoke(false);
                EnsureLoaded();
            }
        }

        public async UniTask<AdShowResult> ShowAndWaitAsync(float timeoutSeconds = 30f)
        {
            if (!IsReady || _showing)
                return AdShowResult.NotReady;

            var tcs = new UniTaskCompletionSource<AdShowResult>();

            void OnClosedHandler()
            {
                OnClosed -= OnClosedHandler;
                tcs.TrySetResult(AdShowResult.Completed);
            }

            OnClosed += OnClosedHandler;

            Show();

            var whenAnyResult = await UniTask.WhenAny(
                tcs.Task,
                UniTask.Delay(TimeSpan.FromSeconds(timeoutSeconds), DelayType.UnscaledDeltaTime));

            if (whenAnyResult.hasResultLeft)
            {
                return whenAnyResult.result;
            }

            OnClosed -= OnClosedHandler;

            if (_showing)
            {
                EventBus.Raise(new AdsOverlayReleaseEvent());
                _showing = false;
                OnShowingChanged?.Invoke(false);
            }

            return AdShowResult.Timeout;
        }

        private void Load()
        {
            var adUnitId = Ads.InterstitialAd;
            if (string.IsNullOrWhiteSpace(adUnitId))
            {
                Debug.LogError("[Ads] Interstitial ad unit id is empty in AdsConfig.");
                return;
            }

            _loading = true;
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
                    OnAvailabilityChanged?.Invoke(false);
                    return;
                }

                _ad = ad;
                Hook(ad);

                Debug.Log("[Ads] Interstitial loaded.");
                OnAvailabilityChanged?.Invoke(true);
            });
        }

        private void Hook(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("[Ads] Interstitial closed.");

                EventBus.Raise(new AdsOverlayReleaseEvent());

                _showing = false;
                OnShowingChanged?.Invoke(false);
                OnClosed?.Invoke();

                EnsureLoaded();
            };

            ad.OnAdFullScreenContentFailed += error =>
            {
                Debug.LogWarning($"[Ads] Interstitial fullscreen failed: {error}");

                EventBus.Raise(new AdsOverlayReleaseEvent());

                _showing = false;
                OnShowingChanged?.Invoke(false);
                OnClosed?.Invoke();

                EnsureLoaded();
            };
        }
    }
}