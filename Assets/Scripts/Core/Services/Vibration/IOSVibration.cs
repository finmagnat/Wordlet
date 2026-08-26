using System.Runtime.InteropServices;

namespace Core.Services
{
    internal static class IOSVibration
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void PlayHaptic(int type);
#endif

        public static void Play(VibrationType type)
        {
#if UNITY_IOS && !UNITY_EDITOR
            PlayHaptic((int)type);
#endif
        }
    }
}
