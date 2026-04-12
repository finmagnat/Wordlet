using System;
using System.Collections.Generic;
using Core.Config;
using Core.Events;
using Core.Services;
using Cysharp.Threading.Tasks;
using PlayFab;
using TMPro;
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
        [SerializeField] private GameObject _languageBar;
        [SerializeField] private TMP_Dropdown _landDropdown;
        [SerializeField] private GameObject _soundOn;
        [SerializeField] private GameObject _giroParallaxOn;
        [SerializeField] private Button _closeButton;
        
        [Inject] private LocalizationService _localization;
        [Inject] private AudioService _audioService;
        [Inject] private PlayFabAuthService _playFabAuthService;
        [Inject] private ConfigService _configService;
        [Inject] private DiContainer _container;
        
        private Locale _newLanguage;
        private Locale _oldLanguage;
        private bool _gyroEnabled;
        private List<Locale> _locales;

        public override async UniTask ShowAsync()
        {
            if (!_languageBar.activeSelf) return;

            _oldLanguage = _localization.CurrentLocale;

            if (_locales == null)
            {
                _locales = _localization.GetAvailableLocales();
                var options = new List<TMP_Dropdown.OptionData>(_locales.Count);
                int curIndex = 0, i = 0;
                foreach (var locale in _locales)
                {
                    options.Add(new TMP_Dropdown.OptionData(locale.LocaleName));
                    if (locale.LocaleName == _oldLanguage.LocaleName)
                        curIndex = i;
                    i++;
                }

                _landDropdown.ClearOptions();
                _landDropdown.AddOptions(options);
                _landDropdown.value = curIndex;
                _landDropdown.RefreshShownValue();
                _landDropdown.onValueChanged.AddListener(OnDropdownChanged);
            }

            UpdateSoundView();
            
            await base.ShowAsync();
        }

        private void OnDestroy()
        {
            _landDropdown.onValueChanged.RemoveAllListeners();
        }

        private void OnDropdownChanged(int index)
        {
            Debug.Log($"Выбран пункт: {index}");
            SelectLanguage(_locales[index]);
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