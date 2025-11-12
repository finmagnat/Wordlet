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
        [Inject] private UIAddresses _addresses;

        private void Start()
        {
            _playAIButton.onClick.AddListener(async () =>
            {
                var popup = await _ui.ShowPopupAsync<GameSetupPopup>(_addresses.GameSetupPopup);
                var data = await popup.WaitForResultAsync();

                switch (data.Result)
                {
                    case PopupResult.Play:
                        Debug.Log($"🎮 Игрок выбрал начать игру: Difficulty={data.Difficulty}, Time={data.TurnTime}s");
                        await _ui.ShowInGameLoadingAsync();
                        await _ui.HideAllScreensAsync();
                        await _ui.ShowScreenAsync<AIGameScreen>(_addresses.AIGameScreen);
                        await _ui.HideInGameLoadingAsync();
                        break;

                    case PopupResult.Close:
                        Debug.Log("🚪 Игрок отменил настройку или закрыл окно");
                        break;
                }
            });
            
            _settingsButton.onClick.AddListener(async () =>
            {
                await _ui.ShowPopupAsync<SettingsPopup>(_addresses.SettingsPopup);
            });

        }

    }
}