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

            AnalyticsDebug.Log($"Send event: {FormatEvent(analyticsEvent)}");

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

        private static string FormatEvent(AnalyticsEvent analyticsEvent)
        {
            string payload = analyticsEvent.Type switch
            {
                AnalyticsEventType.Design => $"type=design, name={analyticsEvent.Name}",
                AnalyticsEventType.Progression => FormatProgressionEvent(analyticsEvent),
                AnalyticsEventType.Resource => FormatResourceEvent(analyticsEvent),
                AnalyticsEventType.Ad => FormatAdEvent(analyticsEvent),
                _ => $"type={analyticsEvent.Type}, name={analyticsEvent.Name}"
            };

            return $"{payload}, params={FormatParameters(analyticsEvent.Parameters)}";
        }

        private static string FormatProgressionEvent(AnalyticsEvent analyticsEvent)
        {
            var data = analyticsEvent.ProgressionData;
            if (data == null)
                return $"type=progression, name={analyticsEvent.Name}";

            return $"type=progression, status={data.Status}, progression01={data.Progression01}, progression02={ValueOrDash(data.Progression02)}, progression03={ValueOrDash(data.Progression03)}, score={FormatNullable(data.Score)}";
        }

        private static string FormatResourceEvent(AnalyticsEvent analyticsEvent)
        {
            var data = analyticsEvent.ResourceData;
            if (data == null)
                return $"type=resource, name={analyticsEvent.Name}";

            return $"type=resource, flow={data.FlowType}, currency={data.Currency}, amount={data.Amount}, itemType={data.ItemType}, itemId={data.ItemId}";
        }

        private static string FormatAdEvent(AnalyticsEvent analyticsEvent)
        {
            var data = analyticsEvent.AdData;
            if (data == null)
                return $"type=ad, name={analyticsEvent.Name}";

            return $"type=ad, action={data.Action}, adType={data.AdType}, sdk={data.SdkName}, placement={data.Placement}, duration={FormatNullable(data.Duration)}, error={FormatNullable(data.Error)}";
        }

        private static string FormatParameters(IReadOnlyDictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return "{}";

            return "{ " + string.Join(", ", parameters.Select(x => $"{x.Key}={FormatValue(x.Value)}")) + " }";
        }

        private static string FormatValue(object value)
        {
            return value switch
            {
                null => "-",
                float floatValue => floatValue.ToString("0.###"),
                double doubleValue => doubleValue.ToString("0.###"),
                _ => value.ToString()
            };
        }

        private static string FormatNullable<T>(T? value) where T : struct
            => value.HasValue ? FormatValue(value.Value) : "-";

        private static string ValueOrDash(string value)
            => string.IsNullOrWhiteSpace(value) ? "-" : value;
    }
}
