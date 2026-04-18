using System.Diagnostics;
using UnityEngine;

namespace Core.Services
{
    internal static class AnalyticsDebug
    {
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Log(string message)
        {
            UnityEngine.Debug.Log($"<color=green>[Analytics]</color> <color=white>{message}</color>");
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Warn(string message)
        {
            UnityEngine.Debug.LogWarning($"[Analytics] {message}");
        }
    }
}
