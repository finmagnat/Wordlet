using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.Services
{
    public sealed class InterstitialPolicyService : IService
    {
        private const int MinSecondsSinceSessionStart = 45;
        private const int CooldownSeconds = 120;

        [Inject] private AdsEntitlementService _entitlement;
        [Inject] private InterstitialAdsService _ads;
        [Inject] private AnalyticsService _analytics;

        private float _sessionStart;
        private float _lastShownTime = -99999f;

        public UniTask InitializeAsync()
        {
            _sessionStart = Time.realtimeSinceStartup;
            return UniTask.CompletedTask;
        }

        public bool TryShow(string placement)
        {
            bool isReady = _ads.IsReady && !_ads.IsShowing;
            _analytics.TrackEvent(
                AnalyticsEvents.Ads.InterstitialShowAttempt,
                AdsAnalyticsHelper.PlacementAttemptParams(placement, isReady));

            if (_entitlement.NoInterstitialAds)
            {
                TrackFailedAttempt(placement, AnalyticsEvents.Option.Disabled);
                return false;
            }

            if (Time.realtimeSinceStartup - _sessionStart < MinSecondsSinceSessionStart)
            {
                TrackFailedAttempt(placement, AnalyticsEvents.Option.TooEarly);
                return false;
            }

            if (Time.realtimeSinceStartup - _lastShownTime < CooldownSeconds)
            {
                TrackFailedAttempt(placement, AnalyticsEvents.Option.Cooldown);
                return false;
            }

            if (!_ads.IsReady || _ads.IsShowing)
            {
                if (!_ads.IsShowing)
                    _ads.EnsureLoaded(AnalyticsEvents.Option.ReloadOnDemand);

                TrackFailedAttempt(placement, AnalyticsEvents.Option.NotReady);
                return false;
            }

            _lastShownTime = Time.realtimeSinceStartup;
            _ads.Show(placement);
            return true;
        }

        public async UniTask<bool> TryShowAndWaitAsync(string placement)
        {
            bool isReady = _ads.IsReady && !_ads.IsShowing;
            _analytics.TrackEvent(
                AnalyticsEvents.Ads.InterstitialShowAttempt,
                AdsAnalyticsHelper.PlacementAttemptParams(placement, isReady));

            if (_entitlement.NoInterstitialAds)
            {
                TrackFailedAttempt(placement, AnalyticsEvents.Option.Disabled);
                return false;
            }

            if (Time.realtimeSinceStartup - _sessionStart < MinSecondsSinceSessionStart)
            {
                TrackFailedAttempt(placement, AnalyticsEvents.Option.TooEarly);
                return false;
            }

            if (Time.realtimeSinceStartup - _lastShownTime < CooldownSeconds)
            {
                TrackFailedAttempt(placement, AnalyticsEvents.Option.Cooldown);
                return false;
            }

            if (!_ads.IsReady || _ads.IsShowing)
            {
                if (!_ads.IsShowing)
                    _ads.EnsureLoaded(AnalyticsEvents.Option.ReloadOnDemand);

                TrackFailedAttempt(placement, AnalyticsEvents.Option.NotReady);
                return false;
            }

            _lastShownTime = Time.realtimeSinceStartup;

            AdShowResult result = await _ads.ShowAndWaitAsync(placement);
            return result == AdShowResult.Completed;
        }

        private void TrackFailedAttempt(string placement, string error)
        {
            _analytics.TrackEvent(
                AnalyticsEvents.Ads.InterstitialShowFailed,
                AdsAnalyticsHelper.PlacementErrorParams(placement, error));
        }
    }
}
