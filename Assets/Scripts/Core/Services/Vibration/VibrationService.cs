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

        public void Play(VibrationType type = VibrationType.Light)
        {
            if (!IsEnabled)
                return;

#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidVibration.Play(type);
#elif UNITY_IOS && !UNITY_EDITOR
            IOSVibration.Play(type);
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
