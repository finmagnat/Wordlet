using Core.Data;
using Core.Generated;
using Core.Services;
using Core.UI;
using Cysharp.Threading.Tasks;
using Game.Logic;
using UI.Popups;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Zenject;

namespace UI.Screens
{
    public class MainMenuScreen : UIScreen
    {
        [SerializeField] private Button _playAIButton;
        [SerializeField] private Button _loadAndplayAIButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _infoButton;
        [SerializeField] private Button _skinsButton;

        [Inject] private IUIManager _ui;
        [Inject] private ILoadingUI _loadingUI;
        [Inject] private ISaveService _saveService;
        [Inject] private GameController _gameController;
        [Inject] private LocalizationService _localization;

        private bool _isProcessing;

        private void Start()
        {
            _localization.OnLocaleChanged += OnLocaleChanged; 
            
            _playAIButton.onClick.AddListener(async () =>
            {
                if (_isProcessing) return;
                _isProcessing = true;

                var popup = await _ui.ShowPopupAsync<AIGameSetupPopup>(AssetKey.AIGameSetupPopup);
                var data = await popup.WaitForResultAsync();

                if (data.Result == PopupResult.Play)
                {
                    Debug.Log($"🎮 Начинаем игру: Difficulty={data.Difficulty}, Time={data.TurnTime}s");

                    // Показ *правильного* in-game loading
                    await _loadingUI.ShowLoadingAsync<InGameLoadingScreen>(AssetKey.InGameLoadingScreen);

                    // Скрываем главное меню
                    await _ui.HideAllScreensAsync();

                    // Переход на экран игры с ИИ
                    await _ui.ShowScreenAsync<AIGameScreen>(AssetKey.AIGameScreen);

                    // Убираем лоадинг
                    await _loadingUI.HideLoadingAsync();
                }
                else
                {
                    Debug.Log("❌ Игрок отменил или закрыл попап");
                }

                _isProcessing = false;
            });
            
            _loadAndplayAIButton.onClick.AddListener(async () =>
            {
                if (_isProcessing) return;
                _isProcessing = true;

                var popup = await _ui.ShowPopupAsync<LoadSavedGamePopup>(AssetKey.LoadSavedGamePopup);
                var data = await popup.WaitForResultAsync();

                if (data.Result == PopupResult.Play)
                {
                    Debug.Log($"🎮 Продолжаем сохраненную игру");

                    // Показ *правильного* in-game loading
                    await _loadingUI.ShowLoadingAsync<InGameLoadingScreen>(AssetKey.InGameLoadingScreen);
                    
                    SaveGameData gameData = await _saveService.LoadAsync();
                    _gameController.SetGameData(gameData);

                    // Скрываем главное меню
                    await _ui.HideAllScreensAsync();

                    // Переход на экран игры с ИИ
                    await _ui.ShowScreenAsync<AIGameScreen>(AssetKey.AIGameScreen);

                    // Убираем лоадинг
                    await _loadingUI.HideLoadingAsync();
                }
                else if (data.Result == PopupResult.RemoveAndExit)
                {
                    Debug.Log($"❌ Удаляем сохраненную игру");
                    await _saveService.ClearAsync();
                    _loadAndplayAIButton.gameObject.SetActive(false);
                }
                else
                {
                    Debug.Log("❌ Игрок отменил или закрыл попап");
                }

                _isProcessing = false;
            });

            _settingsButton.onClick.AddListener(async () =>
            {
                if (_isProcessing) return;
                _isProcessing = true;

                await _ui.ShowPopupAsync<SettingsPopup>(AssetKey.SettingsPopup);

                _isProcessing = false;
            });
            
            _skinsButton.onClick.AddListener(async () =>
            {
                if (_isProcessing) return;
                _isProcessing = true;

                await _ui.ShowPopupAsync<SkinsPopup>(AssetKey.SkinsPopup);

                _isProcessing = false;
            });
            
            _infoButton.onClick.AddListener(async () =>
            {
                if (_isProcessing) return;
                _isProcessing = true;

                await _ui.ShowPopupAsync<InfoPopup>(AssetKey.InfoPopup);

                _isProcessing = false;
            });
        }
        
        private void OnDestroy() => _localization.OnLocaleChanged -= OnLocaleChanged;

        private void OnLocaleChanged(Locale locale)
        {
            _loadAndplayAIButton.gameObject.SetActive(_saveService.HasSave());
        }

        public override UniTask ShowAsync()
        {
            _loadAndplayAIButton.gameObject.SetActive(_saveService.HasSave());
            return base.ShowAsync();
        } 
    }
}
