using Core.Config;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Services
{
    public class VibrationService : IVibrationService
    {
        public bool IsEnabled { get; private set; }

        public async UniTask InitializeAsync()
        {
            IsEnabled = PlayerPrefs.GetInt(PlayerPrefsKey.VibrationEnabled, 1) == 1;
        }

        public void Play()
        {
            if (!IsEnabled)
                return;

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            if (SystemInfo.supportsVibration)
                Handheld.Vibrate();
#endif
        }

        public void EnableVibration(bool value)
        {
            IsEnabled = value;
            PlayerPrefs.SetInt(PlayerPrefsKey.VibrationEnabled, value ? 1 : 0);

            Play();
        }
    }
}
