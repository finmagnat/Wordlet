using Core.Data;
using Core.Generated;
using Core.UI;
using UI.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Screens
{
    public class MainMenuScreen : UIScreen
    {
        [SerializeField] private Button _playAIButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _infoButton;
        [SerializeField] private Button _skinsButton;

        [Inject] private IUIManager _ui;
        [Inject] private ILoadingUI _loadingUI;

        private bool _isProcessing;

        private void Start()
        {
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
    }
}
