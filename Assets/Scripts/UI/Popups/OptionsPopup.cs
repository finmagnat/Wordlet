using System.Collections.Generic;
using Core.Data;
using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Popups
{
    public class OptionsPopup : UIPopup
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _soundOn;
        [SerializeField] private GameObject _vibrationOn;
        [SerializeField] private Button _closeButton;
        
        [Inject] private LocalizationService _localization;
        [Inject] private AudioService _audioService;
        [Inject] private IVibrationService _vibrationService;
        [Inject] private PlayFabAuthService _playFabAuthService;
        [Inject] private ConfigService _configService;
        [Inject] private DiContainer _container;
        [Inject] private AnalyticsService _analytics;
        
        private UniTaskCompletionSource<PopupExitData> _completionSource;
        
        private void Start()
        {
            _closeButton.onClick.AddListener(async () =>
            {
                SendAnalytics(AnalyticsEvents.Navigation.CloseSettingsClicked);
                await HideAsync();
                _completionSource?.TrySetResult(new PopupExitData { Result = PopupResult.Exit });
            });
        }

        public override async UniTask ShowAsync()
        {
            _completionSource = new UniTaskCompletionSource<PopupExitData>();
            UpdateSoundView();
            
            SendAnalytics(AnalyticsEvents.Navigation.OptionsPopupShown);
            
            await base.ShowAsync();
        }
        
        public UniTask<PopupExitData> WaitForResultAsync() => _completionSource.Task;
        
        public void OnSoundButtonClick()
        {
            _audioService.MasterVolume = _audioService.MasterVolume < 0.1f ? 1f : 0f;
            _audioService.SetSfxVolume(_audioService.MasterVolume);
           
            UpdateSoundView();
        }
        
        public void OnVibrationOnButtonClick()
        {
            _vibrationService.EnableVibration(!_vibrationService.IsEnabled);
           
            UpdateVibrationOnView();
        }
        
        private void UpdateSoundView() =>
            _soundOn.SetActive(_audioService.MasterVolume > 0.1f);
        
        private void UpdateVibrationOnView() =>
            _vibrationOn.SetActive(_vibrationService.IsEnabled);
        
        
        private void SendAnalytics(string eventName)
        {
            Dictionary<string, object> parameters = null;
            switch (eventName)
            {
                case AnalyticsEvents.Navigation.OptionsPopupShown:
                case AnalyticsEvents.Navigation.CloseOptionsClicked:
                    parameters = new Dictionary<string, object>
                    {
                        [AnalyticsEvents.Parameter.Sound] = _audioService.MasterVolume > 0.1f ? AnalyticsEvents.Option.On : AnalyticsEvents.Option.Off,
                        [AnalyticsEvents.Parameter.Vibration] = _vibrationService.IsEnabled ? AnalyticsEvents.Option.On : AnalyticsEvents.Option.Off,
                    };
                    break;
            }
            
            _analytics.TrackEvent(eventName, parameters);
        }
    }
}