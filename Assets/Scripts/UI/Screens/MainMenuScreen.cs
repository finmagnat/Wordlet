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
                await _ui.ShowPopupAsync<GameSetupPopup>(_addresses.GameSetupPopup);
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