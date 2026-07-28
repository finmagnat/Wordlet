using UnityEngine;

namespace Core
{
    public static class NativeSplashScreen
    {
        private const string ActivityClassName = "com.unity3d.player.CustomSplashActivity";

        public static void Hide()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var activityClass = new AndroidJavaClass(ActivityClassName))
                {
                    activityClass.CallStatic("hideSplashScreen");
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Failed to hide native splash screen: {exception.Message}");
            }
#endif
        }
    }
}
