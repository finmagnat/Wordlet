using UnityEngine;

namespace Core.Services
{
    public static class AppLaunchTracker
    {
        private static float _launchRealtimeSinceStartup;
        private static bool _isInitialized;

        public static float LaunchRealtimeSinceStartup
        {
            get
            {
                EnsureInitialized();
                return _launchRealtimeSinceStartup;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            _launchRealtimeSinceStartup = Time.realtimeSinceStartup;
            _isInitialized = true;
        }

        private static void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            Initialize();
        }
    }
}
