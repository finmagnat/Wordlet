using System.Collections.Generic;
using Core.Config;
using UnityEngine;

namespace Core.Services
{
    internal static class AdsAnalyticsHelper
    {
        public const string SdkNameAdMob = "admob";

        public static Dictionary<string, object> RewardedTypeParams(RewardType rewardType)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.RewardType] = rewardType.ToString()
            };
        }

        public static Dictionary<string, object> RewardedLoadParams(RewardType rewardType, string loadReason)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.RewardType] = rewardType.ToString(),
                [AnalyticsEvents.Parameter.LoadReason] = loadReason
            };
        }

        public static Dictionary<string, object> RewardedAttemptParams(RewardType rewardType, bool isReady)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.RewardType] = rewardType.ToString(),
                [AnalyticsEvents.Parameter.IsReady] = isReady
            };
        }

        public static Dictionary<string, object> RewardedErrorParams(RewardType rewardType, string error)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.RewardType] = rewardType.ToString(),
                [AnalyticsEvents.Parameter.Error] = error
            };
        }

        public static Dictionary<string, object> RewardedLoadFailedParams(RewardType rewardType, string error, string loadReason)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.RewardType] = rewardType.ToString(),
                [AnalyticsEvents.Parameter.Error] = error,
                [AnalyticsEvents.Parameter.LoadReason] = loadReason
            };
        }

        public static Dictionary<string, object> RewardedLoadSuccessParams(RewardType rewardType, int loadTimeMs, string loadReason)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.RewardType] = rewardType.ToString(),
                [AnalyticsEvents.Parameter.LoadTimeMs] = loadTimeMs,
                [AnalyticsEvents.Parameter.LoadReason] = loadReason
            };
        }

        public static Dictionary<string, object> RewardedClosedParams(bool wasRewarded, RewardType rewardType)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.RewardType] = rewardType.ToString(),
                [AnalyticsEvents.Parameter.WasRewarded] = wasRewarded
            };
        }

        public static Dictionary<string, object> PlacementParams(string placement)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Placement] = placement
            };
        }

        public static Dictionary<string, object> PlacementLoadParams(string placement, string loadReason)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Placement] = placement,
                [AnalyticsEvents.Parameter.LoadReason] = loadReason
            };
        }

        public static Dictionary<string, object> PlacementAttemptParams(string placement, bool isReady)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Placement] = placement,
                [AnalyticsEvents.Parameter.IsReady] = isReady
            };
        }

        public static Dictionary<string, object> PlacementLoadSuccessParams(string placement, int loadTimeMs, string loadReason)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Placement] = placement,
                [AnalyticsEvents.Parameter.LoadTimeMs] = loadTimeMs,
                [AnalyticsEvents.Parameter.LoadReason] = loadReason
            };
        }

        public static Dictionary<string, object> PlacementErrorParams(string placement, string error)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Placement] = placement,
                [AnalyticsEvents.Parameter.Error] = error
            };
        }

        public static Dictionary<string, object> PlacementLoadFailedParams(string placement, string error, string loadReason)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Placement] = placement,
                [AnalyticsEvents.Parameter.Error] = error,
                [AnalyticsEvents.Parameter.LoadReason] = loadReason
            };
        }

        public static string NormalizeError(string rawError)
        {
            if (string.IsNullOrWhiteSpace(rawError))
                return AnalyticsEvents.Option.Unknown;

            string text = rawError.ToLowerInvariant();

            if (text.Contains("no fill") || text.Contains("no_fill"))
                return AnalyticsEvents.Option.NoFill;

            if (text.Contains("timeout"))
                return AnalyticsEvents.Option.Timeout;

            if (text.Contains("network") || text.Contains("offline"))
                return AnalyticsEvents.Option.Network;

            if (text.Contains("not ready") || text.Contains("not_ready"))
                return AnalyticsEvents.Option.NotReady;

            if (text.Contains("invalid"))
                return AnalyticsEvents.Option.InvalidRequest;

            if (text.Contains("internal") || text.Contains("exception") || text.Contains("failed to show"))
                return AnalyticsEvents.Option.InternalError;

            return AnalyticsEvents.Option.Unknown;
        }

        public static int ElapsedMs(float startedAt)
        {
            return Mathf.Max(0, Mathf.RoundToInt((Time.realtimeSinceStartup - startedAt) * 1000f));
        }
        
        public static Dictionary<string, object> GetWaitTimeParams()
        {
            int waitTimeMs = Mathf.RoundToInt((Time.realtimeSinceStartup - AppLaunchTracker.LaunchRealtimeSinceStartup) * 1000f);
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.WaitTimeMs] = waitTimeMs,
                [AnalyticsEvents.Parameter.WaitTimeSeconds] = waitTimeMs / 1000f
            };
        }
    }
}
