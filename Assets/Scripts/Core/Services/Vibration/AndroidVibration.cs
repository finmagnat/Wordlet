using UnityEngine;

namespace Core.Services
{
    internal static class AndroidVibration
    {
#if UNITY_ANDROID
        private const int ApiLevelOreo = 26;
        private const int DefaultAmplitude = -1;

        public static void Play(VibrationType type)
        {
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");

                if (vibrator == null || !vibrator.Call<bool>("hasVibrator"))
                    return;

                int sdkVersion;
                using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                    sdkVersion = version.GetStatic<int>("SDK_INT");

                if (sdkVersion >= ApiLevelOreo)
                    PlayVibrationEffect(vibrator, type);
                else
                    PlayLegacy(vibrator, type);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Failed to play Android vibration: {exception.Message}");
                Handheld.Vibrate();
            }
        }

        private static void PlayVibrationEffect(
            AndroidJavaObject vibrator,
            VibrationType type)
        {
            using var vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
            using AndroidJavaObject effect = IsNotification(type)
                ? CreateWaveform(vibrationEffect, type)
                : vibrationEffect.CallStatic<AndroidJavaObject>(
                    "createOneShot",
                    GetDuration(type),
                    DefaultAmplitude);

            vibrator.Call("vibrate", effect);
        }

        private static AndroidJavaObject CreateWaveform(
            AndroidJavaClass vibrationEffect,
            VibrationType type)
        {
            long[] timings = type switch
            {
                VibrationType.Success => new long[] { 0, 50, 50, 80 },
                VibrationType.Warning => new long[] { 0, 90, 70, 90 },
                VibrationType.Error => new long[] { 0, 130, 70, 180 },
                _ => new long[] { 0, GetDuration(type) }
            };

            return vibrationEffect.CallStatic<AndroidJavaObject>(
                "createWaveform",
                timings,
                -1);
        }

        private static void PlayLegacy(AndroidJavaObject vibrator, VibrationType type)
        {
            if (IsNotification(type))
            {
                long[] pattern = type switch
                {
                    VibrationType.Success => new long[] { 0, 35, 45, 60 },
                    VibrationType.Warning => new long[] { 0, 80, 70, 80 },
                    VibrationType.Error => new long[] { 0, 120, 60, 160 },
                    _ => new long[] { 0, GetDuration(type) }
                };

                vibrator.Call("vibrate", pattern, -1);
                return;
            }

            vibrator.Call("vibrate", GetDuration(type));
        }

        private static bool IsNotification(VibrationType type) =>
            type is VibrationType.Success or VibrationType.Warning or VibrationType.Error;

        private static long GetDuration(VibrationType type) => type switch
        {
            VibrationType.Selection => 40,
            VibrationType.Light => 80,
            VibrationType.Medium => 150,
            VibrationType.Heavy => 250,
            _ => 50
        };
#else
        public static void Play(VibrationType type)
        {
        }
#endif
    }
}
