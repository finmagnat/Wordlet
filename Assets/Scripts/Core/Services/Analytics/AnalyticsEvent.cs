using System.Collections.Generic;

namespace Core.Services
{
    public enum AnalyticsEventType
    {
        Design = 0,
        Progression = 1,
        Resource = 2,
        Ad = 3
    }

    public enum AnalyticsProgressionStatus
    {
        Start = 1,
        Complete = 2,
        Fail = 3
    }

    public enum AnalyticsResourceFlowType
    {
        Source = 1,
        Sink = 2
    }

    public enum AnalyticsAdAction
    {
        Clicked = 1,
        Show = 2,
        FailedShow = 3,
        RewardReceived = 4,
        Request = 5,
        Loaded = 6
    }

    public enum AnalyticsAdType
    {
        Video = 1,
        RewardedVideo = 2,
        Playable = 3,
        Interstitial = 4,
        OfferWall = 5,
        Banner = 6,
        AppOpen = 7
    }

    public enum AnalyticsAdError
    {
        Unknown = 1,
        Offline = 2,
        NoFill = 3,
        InternalError = 4,
        InvalidRequest = 5,
        UnableToPrecache = 6
    }

    public sealed class AnalyticsEvent
    {
        private AnalyticsEvent(
            AnalyticsEventType type,
            string name,
            IReadOnlyDictionary<string, object> parameters,
            AnalyticsProgressionEventData progressionData,
            AnalyticsResourceEventData resourceData,
            AnalyticsAdEventData adData)
        {
            Type = type;
            Name = name;
            Parameters = parameters;
            ProgressionData = progressionData;
            ResourceData = resourceData;
            AdData = adData;
        }

        public AnalyticsEventType Type { get; }
        public string Name { get; }
        public IReadOnlyDictionary<string, object> Parameters { get; }
        public AnalyticsProgressionEventData ProgressionData { get; }
        public AnalyticsResourceEventData ResourceData { get; }
        public AnalyticsAdEventData AdData { get; }

        public static AnalyticsEvent Design(string eventName, IReadOnlyDictionary<string, object> parameters = null)
            => new(AnalyticsEventType.Design, eventName, parameters, null, null, null);

        public static AnalyticsEvent Progression(
            AnalyticsProgressionStatus status,
            string progression01,
            string progression02 = null,
            string progression03 = null,
            int? score = null,
            IReadOnlyDictionary<string, object> parameters = null)
            => new(
                AnalyticsEventType.Progression,
                progression01,
                parameters,
                new AnalyticsProgressionEventData(status, progression01, progression02, progression03, score),
                null,
                null);

        public static AnalyticsEvent Resource(
            AnalyticsResourceFlowType flowType,
            string currency,
            float amount,
            string itemType,
            string itemId,
            IReadOnlyDictionary<string, object> parameters = null)
            => new(
                AnalyticsEventType.Resource,
                itemId,
                parameters,
                null,
                new AnalyticsResourceEventData(flowType, currency, amount, itemType, itemId),
                null);

        public static AnalyticsEvent Ad(
            AnalyticsAdAction action,
            AnalyticsAdType adType,
            string sdkName,
            string placement,
            long? duration = null,
            AnalyticsAdError? error = null,
            IReadOnlyDictionary<string, object> parameters = null)
            => new(
                AnalyticsEventType.Ad,
                placement,
                parameters,
                null,
                null,
                new AnalyticsAdEventData(action, adType, sdkName, placement, duration, error));
    }

    public sealed class AnalyticsProgressionEventData
    {
        public AnalyticsProgressionEventData(
            AnalyticsProgressionStatus status,
            string progression01,
            string progression02,
            string progression03,
            int? score)
        {
            Status = status;
            Progression01 = progression01;
            Progression02 = progression02;
            Progression03 = progression03;
            Score = score;
        }

        public AnalyticsProgressionStatus Status { get; }
        public string Progression01 { get; }
        public string Progression02 { get; }
        public string Progression03 { get; }
        public int? Score { get; }
    }

    public sealed class AnalyticsResourceEventData
    {
        public AnalyticsResourceEventData(
            AnalyticsResourceFlowType flowType,
            string currency,
            float amount,
            string itemType,
            string itemId)
        {
            FlowType = flowType;
            Currency = currency;
            Amount = amount;
            ItemType = itemType;
            ItemId = itemId;
        }

        public AnalyticsResourceFlowType FlowType { get; }
        public string Currency { get; }
        public float Amount { get; }
        public string ItemType { get; }
        public string ItemId { get; }
    }

    public sealed class AnalyticsAdEventData
    {
        public AnalyticsAdEventData(
            AnalyticsAdAction action,
            AnalyticsAdType adType,
            string sdkName,
            string placement,
            long? duration,
            AnalyticsAdError? error)
        {
            Action = action;
            AdType = adType;
            SdkName = sdkName;
            Placement = placement;
            Duration = duration;
            Error = error;
        }

        public AnalyticsAdAction Action { get; }
        public AnalyticsAdType AdType { get; }
        public string SdkName { get; }
        public string Placement { get; }
        public long? Duration { get; }
        public AnalyticsAdError? Error { get; }
    }
}
