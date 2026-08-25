using UnityEngine;

namespace Core.Config
{
    public static class GameDurationSettings
    {
        public static int MinDurationGameSeconds;

        public static int GetDurationGameSeconds(GameConfig gameConfig)
        {
            int duration = GetDefaultDurationGameSeconds(gameConfig);
            return ClampDurationGameSeconds(duration);
        }

        public static int SetDurationGameSeconds(int duration)
        {
            int clampedDuration = ClampDurationGameSeconds(duration);
            return clampedDuration;
        }

        public static int GetDefaultDurationGameSeconds(GameConfig gameConfig)
        {
            MinDurationGameSeconds = GameConfig.MinDurationGameSeconds;
            
            int defaultDuration = gameConfig != null
                ? gameConfig.durationGameSeconds
                : MinDurationGameSeconds;

            return ClampDurationGameSeconds(defaultDuration);
        }

        public static int ClampDurationGameSeconds(int duration)
        {
            return Mathf.Max(MinDurationGameSeconds, duration);
        }
    }
}
