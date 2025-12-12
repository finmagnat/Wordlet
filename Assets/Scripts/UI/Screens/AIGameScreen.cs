using System.Collections.Generic;
using Core.Data;
using Core.Events;
using Core.Generated;
using Core.Services;
using Core.UI;
using Cysharp.Threading.Tasks;
using TMPro;
using UI.Components;
using UI.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Screens
{
    // TODO: экран игры с ИИ
    public class AIGameScreen : UIScreen
    {
        [Space, Header("Game Screen UI Components")] 
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _wordText;
        [SerializeField] private TimerProgressBar _progressBar;
        [SerializeField] private GameObject _gameControlsPanel;
        [SerializeField] private Button _homeButton;
        [SerializeField] private Image _mainBackground;
        [SerializeField] private PlayerPanel _playerPanelOwner;
        [SerializeField] private PlayerPanel _playerPanelOpponent;
        [SerializeField] private WordsField _wordsField;
        [SerializeField] private LettersField _lettersField;
        [SerializeField] private Canvas _renderCameraCanvas;
        
        internal TimerProgressBar TimerBar => _progressBar;
        internal PlayerPanel PlayerPanelOwner => _playerPanelOwner;
        internal PlayerPanel PlayerPanelOpponent => _playerPanelOpponent;

        [Inject] private LocalizationService _localization;
        [Inject] private SkinsService _skinsService;
        [Inject] private ISpriteService _spritesService;
        [Inject] private IUIManager _ui;
        [Inject] private ILoadingUI _loadingUI;
        [Inject] private LocalSaveService _localSaveService;
        
        private bool _isProcessing;
        
        private void Start()
        {
            EventBus.Subscribe<GoToHomeEvent>(OnGoToHome);
            EventBus.Subscribe<GameEndEvent>(OnGameEnd);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<GoToHomeEvent>(OnGoToHome);
            EventBus.Unsubscribe<GameEndEvent>(OnGameEnd);
        }
        
        public void OnPressedHome() => EventBus.Raise(new GoToHomeEvent());
        
        public void OnPressedPause() => EventBus.Raise(new GamePauseEvent());

        public void OnPressedGo() => EventBus.Raise(new GameGoEvent());
        
        public void OnPressedClear() => EventBus.Raise(new GameClearEvent());
        
        public void OnPressedCancel() => EventBus.Raise(new GameCancelEvent());

        public override UniTask ShowAsync()
        {
            _homeButton.gameObject.SetActive(false);
            _gameControlsPanel.SetActive(true);
            
            SetSkin();
            Reset();
            
            _isProcessing = true;
            
            return base.ShowAsync();
        }
        
        internal void Reset()
        {
            ResetButtons();
            
            SetStatusLocalizationKey("STATUS_LABEL_NEW_GAME");
            SetTextWord("");
            PlayerPanelOwner.Reset();
            PlayerPanelOpponent.Reset();
            TimerBar.ResetTimer();
        }
        
        internal List<SelectableLetter> InitWordsField() => _wordsField.InitField();

        internal void InitAlphabetField() => _lettersField.InitField(_renderCameraCanvas.worldCamera);

        internal void SetTextWord(string value) => _wordText.text = value;

        internal string GetTextWord() => _wordText.text;
        
        internal void AddLetterToWord(string letter) => _wordText.text += letter;

        internal void SetStatusLocalizationKey(string localizationKey) => 
            _statusText.text = _localization.Get(LocalizationConst.TableUI, localizationKey);

        private async void OnGoToHome(GoToHomeEvent eventData)
        {
            if (_isProcessing) // Игра не завершена
            {
                // Попап с предложением "Сохранить и выйти" или "Выйти без сохранения".
                var popup = await _ui.ShowPopupAsync<AIGameExitPopup>(AssetKey.AIGameExitPopup);
                var data = await popup.WaitForResultAsync();

                if (data.Result == PopupResult.Exit || data.Result == PopupResult.SaveAndExit)
                    await GoToHome(data.Result == PopupResult.SaveAndExit);
                else
                    Debug.Log("Игрок вернулся в игру");
            }
            else
                await GoToHome();
        }

        private async UniTask GoToHome(bool isSaveGame = false)
        {
            // Показ in-game loading
            await _loadingUI.ShowLoadingAsync<InGameLoadingScreen>(AssetKey.InGameLoadingScreen);

            if (isSaveGame)
                await _localSaveService.Save();

            Reset();

            // Скрываем экран игры
            await _ui.HideAllScreensAsync();

            // Переход на экран главного меню
            await _ui.ShowScreenAsync<MainMenuScreen>(AssetKey.MainMenuScreen);

            // Убираем лоадинг
            await _loadingUI.HideLoadingAsync();
        }
        
        private void ResetButtons()
        {
            _homeButton.gameObject.SetActive(true);
            _gameControlsPanel.SetActive(false);
        }
        
        private void OnGameEnd(GameEndEvent eventData)
        {
            ResetButtons();
            TimerBar.ResetTimer();
            SetStatusLocalizationKey("STATUS_LABEL_GAME_OVER");
            _isProcessing = false;
        }
        
        private async UniTask SetSkin()
        {
            var skin = _skinsService.SkinCurrent;
            _mainBackground.sprite = await _spritesService.GetSpriteAsync(skin.GameBackgroundAlias);
        }

    }
}