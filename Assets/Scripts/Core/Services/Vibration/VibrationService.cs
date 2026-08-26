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
        }

        public void EnableVibration(bool value)
        {
            IsEnabled = value;
            PlayerPrefs.SetInt(PlayerPrefsKey.MasterVolume, value ? 1 : 0);

            Play();
        }
    }
}