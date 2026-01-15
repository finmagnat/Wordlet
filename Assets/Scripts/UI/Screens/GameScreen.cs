using System.Collections.Generic;
using Core.Events;
using Core.Generated;
using Core.Services;
using Core.UI;
using Cysharp.Threading.Tasks;
using TMPro;
using UI.Components;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Screens
{
    // Абстрактный базовый класс игрового экрана.
    public abstract class GameScreen : UIScreen
    {
        [Space, Header("Game Screen UI Components")] 
        [SerializeField] protected TextMeshProUGUI _statusText;
        [SerializeField] protected TextMeshProUGUI _wordText;
        [SerializeField] protected TimerProgressBar _progressBar;
        [SerializeField] protected GameObject _gameControlsPanel;
        [SerializeField] protected Image _mainBackground;
        [SerializeField] protected PlayerPanel _playerPanelOwner;
        [SerializeField] protected PlayerPanel _playerPanelOpponent;
        [SerializeField] protected WordsField _wordsField;
        [SerializeField] protected LettersField _lettersField;
        [SerializeField] protected BoosterPanel _boosterPanel;
        
        internal TimerProgressBar TimerBar => _progressBar;
        internal PlayerPanel PlayerPanelOwner => _playerPanelOwner;
        internal PlayerPanel PlayerPanelOpponent => _playerPanelOpponent;
        internal BoosterPanel BoosterPanel => _boosterPanel;
        
        [Inject] protected LocalizationService _localization;
        [Inject] protected SkinsService _skinsService;
        [Inject] protected ISpriteService _spritesService;
        [Inject] protected IUIManager _ui;
        [Inject] protected ILoadingUI _loadingUI;
        [Inject] protected ISaveService _saveService;
        
        protected bool _isProcessing;
        
        protected void Start()
        {
            EventBus.Subscribe<GoToHomeEvent>(OnGoToHome);
            EventBus.Subscribe<GameEndEvent>(OnGameEnd);
        }

        protected void OnDestroy()
        {
            EventBus.Unsubscribe<GoToHomeEvent>(OnGoToHome);
            EventBus.Unsubscribe<GameEndEvent>(OnGameEnd);
        }
        
        public void OnPressedHome() => EventBus.Raise(new GoToHomeEvent());
        
        public void OnPressedPause() => EventBus.Raise(new GamePauseEvent());

        public void OnPressedGo() => EventBus.Raise(new GameGoEvent());
        
        public void OnPressedClear() => EventBus.Raise(new GameClearEvent());
        
        public void OnPressedCancel() => EventBus.Raise(new GameCancelEvent());
        public void OnPressedSkip() => EventBus.Raise(new GameSkipEvent());

        public override UniTask ShowAsync()
        {
            _gameControlsPanel.SetActive(true);
            
            SetSkin();
            Reset();
            _boosterPanel.Refresh();
            
            _isProcessing = true;
            
            return base.ShowAsync();
        }
        
        internal void Reset()
        {
            SetStatusLocalizationKey("STATUS_LABEL_NEW_GAME");
            SetTextWord("");
            PlayerPanelOwner.Reset();
            PlayerPanelOpponent.Reset();
            TimerBar.ResetTimer();
        }
        
        internal List<SelectableLetter> InitWordsField() => _wordsField.InitField();

        internal List<DraggedLetter> InitAlphabetField() => _lettersField.InitField();

        internal void SetTextWord(string value) => _wordText.text = value;

        internal string GetTextWord() => _wordText.text;
        
        internal void AddLetterToWord(string letter) => _wordText.text += letter;

        internal void SetStatusLocalizationKey(string localizationKey) => 
            _statusText.text = _localization.Get(LocalizationConst.TableUI, localizationKey);

        protected virtual async void OnGoToHome(GoToHomeEvent eventData)
        {
            await GoToHome();
        }

        protected async UniTask GoToHome(bool isSaveGame = false)
        {
            // Показ in-game loading
            await _loadingUI.ShowLoadingAsync<InGameLoadingScreen>(AssetKey.InGameLoadingScreen);

            if (isSaveGame)
                await _saveService.SaveAsync();

            Reset();

            // Скрываем экран игры
            await _ui.HideAllScreensAsync();

            // Переход на экран главного меню
            await _ui.ShowScreenAsync<MainMenuScreen>(AssetKey.MainMenuScreen);

            // Убираем лоадинг
            await _loadingUI.HideLoadingAsync();
        }
        
        protected void OnGameEnd(GameEndEvent eventData)
        {
            _gameControlsPanel.SetActive(false);
            TimerBar.ResetTimer();
            SetStatusLocalizationKey("STATUS_LABEL_GAME_OVER");
            _isProcessing = false;
        }
        
        protected async UniTask SetSkin()
        {
            var skin = _skinsService.SkinCurrent;
            _mainBackground.sprite = await _spritesService.GetSpriteAsync(skin.GameBackgroundAlias);
        }

    }
}