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

        public async UniTask InitializeAsync()
        {
#if UNITY_ANDROID || UNITY_IOS
            await InitializeMobileAdsAsync();
            EnsureLoaded();
#else
            Debug.Log("[Ads] Skipping interstitial init on this platform.");
#endif
        }

        private static UniTask InitializeMobileAdsAsync()
        {
            var tcs = new UniTaskCompletionSource();
            MobileAds.Initialize(_ => tcs.TrySetResult());
            return tcs.Task;
        }

        public void EnsureLoaded()
        {
            if (_loading) return;
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

            EventBus.Raise(new ShowAdsEvent(true));
            _ad.Show();
        }

        public async UniTask<bool> ShowAndWaitAsync()
        {
            if (!IsReady || IsShowing)
                return false;

            var tcs = new UniTaskCompletionSource();

            void OnClosedHandler()
            {
                EventBus.Raise(new ShowAdsEvent(false));
                
                OnClosed -= OnClosedHandler;
                tcs.TrySetResult();
            }

            OnClosed += OnClosedHandler;

            Show();

            // Safety: чтобы не зависнуть навечно, если что-то пошло не так
            // (обычно не нужно, но лучше пусть будет)
            var completed = await UniTask.WhenAny(tcs.Task, UniTask.Delay(TimeSpan.FromSeconds(30), DelayType.UnscaledDeltaTime));

            // если таймаут — отписываемся
            if (completed != 0)
            {
                OnClosed -= OnClosedHandler;
                return true; // реклама была показана (мы ее начали), но не дождались close — считаем как "показали"
            }

            return true; // показали и дождались закрытия
        }

        private void Load()
        {
            var adUnitId = Ads.InterstitialAd; // 👈 добавим в AdsConfig
            if (string.IsNullOrWhiteSpace(adUnitId))
            {
                Debug.LogError("[Ads] InterstitialMain is empty in AdsConfig");
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

                Debug.Log("[Ads] Interstitial loaded");
                OnAvailabilityChanged?.Invoke(true);
            });
        }

        private void Hook(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log($"[Ads] OnAdFullScreenContentClosed");
                EventBus.Raise(new ShowAdsEvent(false));
                
                _showing = false;
                OnShowingChanged?.Invoke(false);
                OnClosed?.Invoke();

                EnsureLoaded();
            };

            ad.OnAdFullScreenContentFailed += err =>
            {
                Debug.Log($"[Ads] OnAdFullScreenContentFailed...");
                EventBus.Raise(new ShowAdsEvent(false));
                
                _showing = false;
                OnShowingChanged?.Invoke(false);

                EnsureLoaded();
            };
        }
    }
}