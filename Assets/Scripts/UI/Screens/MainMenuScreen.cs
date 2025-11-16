using Core.Config;
using Core.Data;
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

        [Inject] private IUIManager _ui;
        [Inject] private ILoadingUI _loadingUI;
        [Inject] private UIAddresses _addresses;

        private bool _isProcessing;

        private void Start()
        {
            _playAIButton.onClick.AddListener(async () =>
            {
                if (_isProcessing) return;
                _isProcessing = true;

                var popup = await _ui.ShowPopupAsync<GameSetupPopup>(_addresses.GameSetupPopup);
                var data = await popup.WaitForResultAsync();

                if (data.Result == PopupResult.Play)
                {
                    Debug.Log($"🎮 Начинаем игру: Difficulty={data.Difficulty}, Time={data.TurnTime}s");

                    // Показ *правильного* in-game loading
                    await _loadingUI.ShowLoadingAsync<InGameLoadingScreen>(_addresses.InGameLoadingScreen);

                    // Скрываем главное меню
                    await _ui.HideAllScreensAsync();

                    // Переход на экран игры с ИИ
                    await _ui.ShowScreenAsync<AIGameScreen>(_addresses.AIGameScreen);

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

                await _ui.ShowPopupAsync<SettingsPopup>(_addresses.SettingsPopup);

                _isProcessing = false;
            });
        }
    }
}
