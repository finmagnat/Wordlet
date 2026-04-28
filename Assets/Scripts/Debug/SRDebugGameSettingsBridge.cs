using Core.Config;
using Core.Services;
using UnityEngine;
using Zenject;

namespace Core.DebugTools
{
    public sealed class SRDebugGameSettingsBridge : MonoBehaviour
    {
        public static SRDebugGameSettingsBridge Instance { get; private set; }

        [Inject] private ConfigService _configService;

        public int DurationGameSeconds
        {
            get => GameDurationSettings.GetDurationGameSeconds(_configService != null ? _configService.Game : null);
            set => GameDurationSettings.SetDurationGameSeconds(value);
        }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
