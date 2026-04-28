using UnityEngine;

namespace Core.Config
{
    public static class GameDurationSettings
    {
        public const int MinDurationGameSeconds = 10;

        public static int GetDurationGameSeconds(GameConfig gameConfig)
        {
            int defaultDuration = GetDefaultDurationGameSeconds(gameConfig);
            int duration = PlayerPrefs.GetInt(PlayerPrefsKey.DurationGame, defaultDuration);

            return ClampDurationGameSeconds(duration);
        }

        public static int SetDurationGameSeconds(int duration)
        {
            int clampedDuration = ClampDurationGameSeconds(duration);
            PlayerPrefs.SetInt(PlayerPrefsKey.DurationGame, clampedDuration);
            PlayerPrefs.Save();

            return clampedDuration;
        }

        public static int GetDefaultDurationGameSeconds(GameConfig gameConfig)
        {
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
