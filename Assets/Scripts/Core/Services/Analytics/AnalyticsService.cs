using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Core.Services
{
    public sealed class AnalyticsService : IAnalyticsService
    {
        private readonly List<IAnalyticsProvider> _providers;

        public AnalyticsService([InjectOptional] List<IAnalyticsProvider> providers)
        {
            _providers = providers ?? new List<IAnalyticsProvider>();
        }

        public async UniTask InitializeAsync()
        {
            if (_providers.Count == 0)
            {
                AnalyticsDebug.Warn("No analytics providers are bound.");
                return;
            }

            foreach (var provider in _providers.Where(x => x != null && x.IsEnabled))
            {
                try
                {
                    await provider.InitializeAsync();
                    AnalyticsDebug.Log($"Provider initialized: {provider.ProviderName}");
                }
                catch (Exception exception)
                {
                    AnalyticsDebug.Warn($"Provider init failed: {provider.ProviderName}. {exception}");
                }
            }
        }

        public void TrackEvent(string eventName, IReadOnlyDictionary<string, object> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                AnalyticsDebug.Warn("Skipped design event with empty event name.");
                return;
            }

            TrackEvent(AnalyticsEvent.Design(eventName, parameters));
        }

        public void TrackEvent(AnalyticsEvent analyticsEvent)
        {
            if (analyticsEvent == null)
            {
                AnalyticsDebug.Warn("Skipped null analytics event.");
                return;
            }

            foreach (var provider in _providers)
            {
                if (provider == null || !provider.IsEnabled)
                    continue;

                try
                {
                    provider.Track(analyticsEvent);
                }
                catch (Exception exception)
                {
                    AnalyticsDebug.Warn($"Provider track failed: {provider.ProviderName}. Event={analyticsEvent.Name}. {exception}");
                }
            }
        }

        public void TrackProgressionEvent(
            AnalyticsProgressionStatus status,
            string progression01,
            string progression02 = null,
            string progression03 = null,
            int? score = null,
            IReadOnlyDictionary<string, object> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(progression01))
            {
                AnalyticsDebug.Warn("Skipped progression event with empty progression01.");
                return;
            }

            TrackEvent(AnalyticsEvent.Progression(status, progression01, progression02, progression03, score, parameters));
        }

        public void TrackResourceEvent(
            AnalyticsResourceFlowType flowType,
            string currency,
            float amount,
            string itemType,
            string itemId,
            IReadOnlyDictionary<string, object> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(currency) ||
                string.IsNullOrWhiteSpace(itemType) ||
                string.IsNullOrWhiteSpace(itemId))
            {
                AnalyticsDebug.Warn("Skipped resource event because required fields are missing.");
                return;
            }

            TrackEvent(AnalyticsEvent.Resource(flowType, currency, amount, itemType, itemId, parameters));
        }

        public void TrackAdEvent(
            AnalyticsAdAction action,
            AnalyticsAdType adType,
            string sdkName,
            string placement,
            long? duration = null,
            AnalyticsAdError? error = null,
            IReadOnlyDictionary<string, object> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(sdkName) || string.IsNullOrWhiteSpace(placement))
            {
                AnalyticsDebug.Warn("Skipped ad event because sdkName or placement is empty.");
                return;
            }

            TrackEvent(AnalyticsEvent.Ad(action, adType, sdkName, placement, duration, error, parameters));
        }
    }
}
