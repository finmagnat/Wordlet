using System.Collections.Generic;

namespace Core.Services
{
    public interface IAnalyticsService : IService
    {
        void TrackEvent(string eventName, IReadOnlyDictionary<string, object> parameters = null);
        void TrackEvent(AnalyticsEvent analyticsEvent);
        void TrackProgressionEvent(
            AnalyticsProgressionStatus status,
            string progression01,
            string progression02 = null,
            string progression03 = null,
            int? score = null,
            IReadOnlyDictionary<string, object> parameters = null);

        void TrackResourceEvent(
            AnalyticsResourceFlowType flowType,
            string currency,
            float amount,
            string itemType,
            string itemId,
            IReadOnlyDictionary<string, object> parameters = null);

        void TrackAdEvent(
            AnalyticsAdAction action,
            AnalyticsAdType adType,
            string sdkName,
            string placement,
            long? duration = null,
            AnalyticsAdError? error = null,
            IReadOnlyDictionary<string, object> parameters = null);
    }
}
