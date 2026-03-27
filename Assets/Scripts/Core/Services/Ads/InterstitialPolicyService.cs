using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.Services
{
    public sealed class InterstitialPolicyService : IService
    {
        // Настройки (можно потом вынести в конфиг)
        private const int MinSecondsSinceSessionStart = 45;
        private const int CooldownSeconds = 120;

        [Inject] private AdsEntitlementService _entitlement;
        [Inject] private InterstitialAdsService _ads;

        private float _sessionStart;
        private float _lastShownTime = -99999f;

        public UniTask InitializeAsync()
        {
            _sessionStart = Time.realtimeSinceStartup;
            return UniTask.CompletedTask;
        }

        public bool TryShow(string placement)
        {
            if (_entitlement.NoInterstitialAds)
                return false;

            if (Time.realtimeSinceStartup - _sessionStart < MinSecondsSinceSessionStart)
                return false;

            if (Time.realtimeSinceStartup - _lastShownTime < CooldownSeconds)
                return false;

            if (!_ads.IsReady || _ads.IsShowing)
                return false;

            _lastShownTime = Time.realtimeSinceStartup;
            _ads.Show();
            return true;
        }

        public async UniTask<bool> TryShowAndWaitAsync(string placement)
        {
            // те же условия, что и в TryShow:
            if (_entitlement.NoInterstitialAds)
                return false;

            if (Time.realtimeSinceStartup - _sessionStart < MinSecondsSinceSessionStart)
                return false;

            if (Time.realtimeSinceStartup - _lastShownTime < CooldownSeconds)
                return false;

            if (!_ads.IsReady || _ads.IsShowing)
                return false;

            _lastShownTime = Time.realtimeSinceStartup;

            // показать и дождаться закрытия
            AdShowResult result = await _ads.ShowAndWaitAsync();

            return result == AdShowResult.Completed;
        }
    }
}