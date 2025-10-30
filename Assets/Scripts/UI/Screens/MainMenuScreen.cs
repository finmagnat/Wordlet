using Core.Config;
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
        
        [Inject] private IUIManager _ui;
        [Inject] private UIAddresses _addresses;

        private void Start()
        {
            _playAIButton.onClick.AddListener(async () =>
            {
                var popup = await _ui.ShowPopupAsync<GameSetupPopup>(_addresses.GameSetupPopup);
                bool confirmed = await popup.WaitForResultAsync();

                if (confirmed)
                {
                    Debug.Log("✅ Игрок подтвердил запуск игры с ИИ!");
                    await _ui.ShowInGameLoadingAsync();

                    await _ui.HideAllScreensAsync();
                    await _ui.ShowScreenAsync<AIGameScreen>(_addresses.GameScreen);

                    await _ui.HideInGameLoadingAsync();
                }
                else
                {
                    Debug.Log("❎ Игрок отменил запуск.");
                }
            });
        }
        
        public void OnPlayClicked()
        {
            Debug.Log("Play button clicked!");
        }

        public void OnSettingsClicked()
        {
            Debug.Log("Settings clicked!");
        }
    }
}