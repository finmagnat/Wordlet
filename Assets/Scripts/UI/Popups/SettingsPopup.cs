using System.Collections.Generic;
using Core.Config;
using Core.Events;
using Core.Services;
using Core.UI.Components;
using Cysharp.Threading.Tasks;
using PlayFab;
using UI.Parallax;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;
using UnityEngine.UI;
using Zenject;

namespace UI.Popups
{
    public class SettingsPopup : UIPopup
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _soundOn;
        [SerializeField] private GameObject _giroParallaxOn;
        [SerializeField] private LanguageButton _languageButtonPrefab;
        [SerializeField] private GameObject _content;
        [SerializeField] private GameObject _languageBar;
        [SerializeField] private Button _closeButton;
        
        [Inject] private LocalizationService _localization;
        [Inject] private AudioService _audioService;
        [Inject] private PlayFabAuthService _playFabAuthService;
        [Inject] private ConfigService _configService;
        
        private readonly Dictionary<string, LanguageButton> _buttons = new ();
        private Locale _newLanguage;
        private Locale _oldLanguage;
        private bool _gyroEnabled;
        

        public override async UniTask ShowAsync()
        {
            if (!_languageBar.activeSelf) return;

            _oldLanguage = _localization.CurrentLocale;
            if (_buttons == null || _buttons.Count == 0)
            {
                var locales = _localization.GetAvailableLocales();
                foreach (var locale in locales)
                {
                    LanguageButton languageButton = Instantiate(_languageButtonPrefab, _content.transform, false);
                    languageButton.language = locale.Identifier.Code;
                    languageButton.SetText(locale.LocaleName);
                    languageButton.button.onClick.AddListener(() =>
                    {
                        SelectLanguage(locale);
                        //Dictionary<string, string> paramDictionary = new() { { Constants.Type, s.lang } };
                        //_analyticsManager.SendEvent(Constants.LanguagePressedEvent, paramDictionary);
                    });

                    _buttons[locale.Identifier.Code] = languageButton;
                }
            }
            
            SelectLanguage(_oldLanguage);
            UpdateSoundView();
            
            await base.ShowAsync();
        }

        private void Start()
        {
            _closeButton.onClick.AddListener(async () =>
            {
                await HideAsync();
                //_completionSource?.TrySetResult(new GameSetupData { Result = PopupResult.Close });
            });
            
            _gyroEnabled = PlayerPrefs.GetInt(PlayerPrefsKey.GyroKey, 1) == 1;
            UpdateGyroView();
        }
        
        public void OnSoundButtonClick()
        {
            _audioService.MasterVolume = _audioService.MasterVolume < 0.1f ? 1f : 0f;
            _audioService.SetSfxVolume(_audioService.MasterVolume);
           
            UpdateSoundView();
        }
        
        private void SelectLanguage(Locale locale)
        {
            _newLanguage = locale;

            foreach (string buttonsKey in _buttons.Keys)
                _buttons[buttonsKey].SetActiveStatus(buttonsKey == locale.Identifier.Code);

            if (_localization.CurrentLocale == _newLanguage) return;

            _localization.SetLocale(_newLanguage.Identifier.Code);

            OnCloseButtonClick();
        }
        
        public void OnTermsButtonClick()
        {
            //_analyticsManager.SendEvent(Constants.SettingsTOSEvent);
            OnCloseButtonClick(false);

            Application.OpenURL(_configService.Game.Terms);
        }

        public void OnPrivacyButtonClick()
        {
            //_analyticsManager.SendEvent(Constants.SettingsPPEvent);
            OnCloseButtonClick(false);

            Application.OpenURL(_configService.Game.Privacy);
        }

        public void OnSupportButtonClick()
        {
            //_analyticsManager.SendEvent(Constants.SettingsSupportEvent);

            OnCloseButtonClick(false);

            string body = string.Empty;
            if (PlayFabClientAPI.IsClientLoggedIn())
            {
                body += "Cloud ID: " + _playFabAuthService.PlayFabId;
            }

            Application.OpenURL("mailto:" + _configService.Game.Support + "?&body=" + MyEscapeURL(body));
        }

        private string MyEscapeURL(string url)
        {
            return UnityWebRequest.EscapeURL(url).Replace("+","%20");
        }

        public void OnCloseButtonClick(bool sendAnalytics = true)
        {
            /*
            if (sendAnalytics)
            {
                _analyticsManager.SendEvent(Constants.SettingsBackEvent);
            }*/

            HideAsync();
        }
        
        private void UpdateSoundView() =>
            _soundOn.SetActive(_audioService.MasterVolume > 0.1f);
        
        private void UpdateGyroView() =>
            _giroParallaxOn.SetActive(_gyroEnabled);
        
        public void OnGyroButtonClick()
        {
            _gyroEnabled = !_gyroEnabled;
            
            PlayerPrefs.SetInt(PlayerPrefsKey.GyroKey, _gyroEnabled ? 1 : 0);
            PlayerPrefs.Save();
            
            EventBus.Raise(new GyroEnableEvent{ ParallaxMode = _gyroEnabled
                ? UIParallaxMode.TouchAndGyro
                : UIParallaxMode.Touch});
           
            UpdateGyroView();
        }
        
    }
}