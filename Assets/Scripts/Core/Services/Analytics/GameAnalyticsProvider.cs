using System;
using System.Collections.Generic;
using GameAnalyticsSDK;

namespace Core.Services
{
    public sealed class GameAnalyticsProvider : IAnalyticsProvider
    {
        private readonly AnalyticsInstallerSettings _settings;

        public GameAnalyticsProvider(AnalyticsInstallerSettings settings)
        {
            _settings = settings;
        }

        public string ProviderName => nameof(GameAnalyticsProvider);
        public bool IsEnabled => _settings != null && _settings.EnableGameAnalytics;

        public Cysharp.Threading.Tasks.UniTask InitializeAsync()
        {
            if (!IsEnabled)
                return Cysharp.Threading.Tasks.UniTask.CompletedTask;

            EnsureInitialized();
            return Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }

        public void Track(AnalyticsEvent analyticsEvent)
        {
            if (!IsEnabled || analyticsEvent == null)
                return;

            EnsureInitialized();

            var customFields = SanitizeParameters(analyticsEvent.Parameters);

            switch (analyticsEvent.Type)
            {
                case AnalyticsEventType.Design:
                    TrackDesignEvent(analyticsEvent.Name, customFields);
                    break;

                case AnalyticsEventType.Progression:
                    TrackProgressionEvent(analyticsEvent.ProgressionData, customFields);
                    break;

                case AnalyticsEventType.Resource:
                    TrackResourceEvent(analyticsEvent.ResourceData, customFields);
                    break;

                case AnalyticsEventType.Ad:
                    TrackAdEvent(analyticsEvent.AdData, customFields);
                    break;

                default:
                    AnalyticsDebug.Warn($"Unsupported analytics event type: {analyticsEvent.Type}");
                    break;
            }
        }

        private static void TrackDesignEvent(string eventName, Dictionary<string, object> customFields)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                return;

            if (customFields != null && customFields.Count > 0)
            {
                GameAnalytics.NewDesignEvent(eventName, customFields);
                return;
            }

            GameAnalytics.NewDesignEvent(eventName);
        }

        private static void TrackProgressionEvent(AnalyticsProgressionEventData data, Dictionary<string, object> customFields)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.Progression01))
                return;

            var status = ToGameAnalyticsProgressionStatus(data.Status);
            bool hasFields = customFields != null && customFields.Count > 0;
            bool hasProgression02 = !string.IsNullOrWhiteSpace(data.Progression02);
            bool hasProgression03 = !string.IsNullOrWhiteSpace(data.Progression03);

            if (data.Score.HasValue)
            {
                if (hasProgression03)
                {
                    if (hasFields)
                        GameAnalytics.NewProgressionEvent(status, data.Progression01, data.Progression02, data.Progression03, data.Score.Value, customFields);
                    else
                        GameAnalytics.NewProgressionEvent(status, data.Progression01, data.Progression02, data.Progression03, data.Score.Value);

                    return;
                }

                if (hasProgression02)
                {
                    if (hasFields)
                        GameAnalytics.NewProgressionEvent(status, data.Progression01, data.Progression02, data.Score.Value, customFields);
                    else
                        GameAnalytics.NewProgressionEvent(status, data.Progression01, data.Progression02, data.Score.Value);

                    return;
                }

                if (hasFields)
                    GameAnalytics.NewProgressionEvent(status, data.Progression01, data.Score.Value, customFields);
                else
                    GameAnalytics.NewProgressionEvent(status, data.Progression01, data.Score.Value);

                return;
            }

            if (hasProgression03)
            {
                if (hasFields)
                    GameAnalytics.NewProgressionEvent(status, data.Progression01, data.Progression02, data.Progression03, customFields);
                else
                    GameAnalytics.NewProgressionEvent(status, data.Progression01, data.Progression02, data.Progression03);

                return;
            }

            if (hasProgression02)
            {
                if (hasFields)
                    GameAnalytics.NewProgressionEvent(status, data.Progression01, data.Progression02, customFields);
                else
                    GameAnalytics.NewProgressionEvent(status, data.Progression01, data.Progression02);

                return;
            }

            if (hasFields)
                GameAnalytics.NewProgressionEvent(status, data.Progression01, customFields);
            else
                GameAnalytics.NewProgressionEvent(status, data.Progression01);
        }

        private static void TrackResourceEvent(AnalyticsResourceEventData data, Dictionary<string, object> customFields)
        {
            if (data == null ||
                string.IsNullOrWhiteSpace(data.Currency) ||
                string.IsNullOrWhiteSpace(data.ItemType) ||
                string.IsNullOrWhiteSpace(data.ItemId))
            {
                return;
            }

            var flowType = ToGameAnalyticsResourceFlowType(data.FlowType);

            if (customFields != null && customFields.Count > 0)
            {
                GameAnalytics.NewResourceEvent(flowType, data.Currency, data.Amount, data.ItemType, data.ItemId, customFields);
                return;
            }

            GameAnalytics.NewResourceEvent(flowType, data.Currency, data.Amount, data.ItemType, data.ItemId);
        }

        private static void TrackAdEvent(AnalyticsAdEventData data, Dictionary<string, object> customFields)
        {
            if (data == null ||
                string.IsNullOrWhiteSpace(data.SdkName) ||
                string.IsNullOrWhiteSpace(data.Placement))
            {
                return;
            }

            var action = ToGameAnalyticsAdAction(data.Action);
            var adType = ToGameAnalyticsAdType(data.AdType);
            bool hasFields = customFields != null && customFields.Count > 0;

            if (data.Error.HasValue)
            {
                var error = ToGameAnalyticsAdError(data.Error.Value);

                if (hasFields)
                    GameAnalytics.NewAdEvent(action, adType, data.SdkName, data.Placement, error, customFields);
                else
                    GameAnalytics.NewAdEvent(action, adType, data.SdkName, data.Placement, error);

                return;
            }

            if (data.Duration.HasValue)
            {
                if (hasFields)
                    GameAnalytics.NewAdEvent(action, adType, data.SdkName, data.Placement, data.Duration.Value, customFields);
                else
                    GameAnalytics.NewAdEvent(action, adType, data.SdkName, data.Placement, data.Duration.Value);

                return;
            }

            if (hasFields)
                GameAnalytics.NewAdEvent(action, adType, data.SdkName, data.Placement, customFields);
            else
                GameAnalytics.NewAdEvent(action, adType, data.SdkName, data.Placement);
        }

        private static Dictionary<string, object> SanitizeParameters(IReadOnlyDictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return null;

            var result = new Dictionary<string, object>(parameters.Count);

            foreach (var pair in parameters)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value == null)
                    continue;

                switch (pair.Value)
                {
                    case string stringValue:
                        result[pair.Key] = stringValue;
                        break;
                    case bool boolValue:
                        result[pair.Key] = boolValue ? 1 : 0;
                        break;
                    case byte byteValue:
                        result[pair.Key] = (long)byteValue;
                        break;
                    case sbyte sbyteValue:
                        result[pair.Key] = (long)sbyteValue;
                        break;
                    case short shortValue:
                        result[pair.Key] = (long)shortValue;
                        break;
                    case ushort ushortValue:
                        result[pair.Key] = (long)ushortValue;
                        break;
                    case int intValue:
                        result[pair.Key] = intValue;
                        break;
                    case uint uintValue:
                        result[pair.Key] = (long)uintValue;
                        break;
                    case long longValue:
                        result[pair.Key] = longValue;
                        break;
                    case ulong ulongValue:
                        result[pair.Key] = Convert.ToDouble(ulongValue);
                        break;
                    case float floatValue:
                        result[pair.Key] = floatValue;
                        break;
                    case double doubleValue:
                        result[pair.Key] = doubleValue;
                        break;
                    case decimal decimalValue:
                        result[pair.Key] = Convert.ToDouble(decimalValue);
                        break;
                    case Enum enumValue:
                        result[pair.Key] = enumValue.ToString();
                        break;
                    default:
                        result[pair.Key] = pair.Value.ToString();
                        break;
                }
            }

            return result.Count > 0 ? result : null;
        }

        private static GAProgressionStatus ToGameAnalyticsProgressionStatus(AnalyticsProgressionStatus status)
            => status switch
            {
                AnalyticsProgressionStatus.Start => GAProgressionStatus.Start,
                AnalyticsProgressionStatus.Complete => GAProgressionStatus.Complete,
                AnalyticsProgressionStatus.Fail => GAProgressionStatus.Fail,
                _ => GAProgressionStatus.Undefined
            };

        private static GAResourceFlowType ToGameAnalyticsResourceFlowType(AnalyticsResourceFlowType flowType)
            => flowType switch
            {
                AnalyticsResourceFlowType.Source => GAResourceFlowType.Source,
                AnalyticsResourceFlowType.Sink => GAResourceFlowType.Sink,
                _ => GAResourceFlowType.Undefined
            };

        private static GAAdAction ToGameAnalyticsAdAction(AnalyticsAdAction action)
            => action switch
            {
                AnalyticsAdAction.Clicked => GAAdAction.Clicked,
                AnalyticsAdAction.Show => GAAdAction.Show,
                AnalyticsAdAction.FailedShow => GAAdAction.FailedShow,
                AnalyticsAdAction.RewardReceived => GAAdAction.RewardReceived,
                AnalyticsAdAction.Request => GAAdAction.Request,
                AnalyticsAdAction.Loaded => GAAdAction.Loaded,
                _ => GAAdAction.Undefined
            };

        private static GAAdType ToGameAnalyticsAdType(AnalyticsAdType adType)
            => adType switch
            {
                AnalyticsAdType.Video => GAAdType.Video,
                AnalyticsAdType.RewardedVideo => GAAdType.RewardedVideo,
                AnalyticsAdType.Playable => GAAdType.Playable,
                AnalyticsAdType.Interstitial => GAAdType.Interstitial,
                AnalyticsAdType.OfferWall => GAAdType.OfferWall,
                AnalyticsAdType.Banner => GAAdType.Banner,
                AnalyticsAdType.AppOpen => GAAdType.AppOpen,
                _ => GAAdType.Undefined
            };

        private static GAAdError ToGameAnalyticsAdError(AnalyticsAdError error)
            => error switch
            {
                AnalyticsAdError.Unknown => GAAdError.Unknown,
                AnalyticsAdError.Offline => GAAdError.Offline,
                AnalyticsAdError.NoFill => GAAdError.NoFill,
                AnalyticsAdError.InternalError => GAAdError.InternalError,
                AnalyticsAdError.InvalidRequest => GAAdError.InvalidRequest,
                AnalyticsAdError.UnableToPrecache => GAAdError.UnableToPrecache,
                _ => GAAdError.Undefined
            };

        private static void EnsureInitialized()
        {
            if (GameAnalytics.Initialized)
                return;

            GameAnalytics.Initialize();
            AnalyticsDebug.Log("GameAnalytics SDK initialized.");
        }
    }
}
