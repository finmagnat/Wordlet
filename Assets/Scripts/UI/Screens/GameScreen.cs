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
        
        [SerializeField] protected Button _homeButton;
        [SerializeField] protected Button _pauseButton;
        [SerializeField] protected Button _cancelButton;
        [SerializeField] protected Button _goButton;
        [SerializeField] protected Button _passButton;
        [SerializeField] protected Button _statisticButton;
        
        [SerializeField] protected Image _mainBackground;
        [SerializeField] protected PlayerPanel _playerPanelOwner;
        [SerializeField] protected PlayerPanel _playerPanelOpponent;
        [SerializeField] protected WordsField _wordsField;
        [SerializeField] protected LettersField _lettersField;
        [SerializeField] protected BoosterPanelIngameScreen _boosterPanel;
        [SerializeField] protected PauseButtonAnimator _pauseButtonAnimator;
        [SerializeField] protected StatisticsPanel _statisticsPanel;
        [SerializeField] protected KeyboardPanel _keyboardPanel;
        
        internal TimerProgressBar TimerBar => _progressBar;
        internal PlayerPanel PlayerPanelOwner => _playerPanelOwner;
        internal PlayerPanel PlayerPanelOpponent => _playerPanelOpponent;
        internal StatisticsPanel StatisticsPanel => _statisticsPanel;
        internal KeyboardPanel KeyboardPanel => _keyboardPanel;
        internal BoosterPanelIngameScreen BoosterPanel => _boosterPanel;
        internal GameObject PauseButton => _pauseButton.gameObject;
        internal GameObject PassButton => _passButton.gameObject;
        internal GameObject CancelButton => _cancelButton.gameObject;
        internal GameObject GoButton => _goButton.gameObject;
        
        [Inject] protected LocalizationService _localization;
        [Inject] protected SkinsService _skinsService;
        [Inject] protected ISpriteService _spritesService;
        [Inject] protected IUIManager _ui;
        [Inject] protected ILoadingUI _loadingUI;
        [Inject] protected ISaveService _saveService;
        [Inject] protected InterstitialPolicyService _interstitialService;
        [Inject] protected IGamePauseService _pauseService;
        
        protected bool _isProcessing;
        protected bool _isPaused;

        protected void Start()
        {
            EventBus.Subscribe<GoToHomeEvent>(OnGoToHome);
            EventBus.Subscribe<GameEndEvent>(OnGameEnd);

            //UpdateSkin();
        }

        protected void OnDestroy()
        {
            EventBus.Unsubscribe<GoToHomeEvent>(OnGoToHome);
            EventBus.Unsubscribe<GameEndEvent>(OnGameEnd);
        }
        
        public void OnPressedHome() => EventBus.Raise(new GoToHomeEvent());

        public void OnPressedPause()
        {
            _isPaused = !_isPaused;
            _pauseButtonAnimator.SetPaused(_isPaused);
            _pauseService.SetUserPause(!_pauseService.IsPaused);
        }

        public void OnPressedGo() => EventBus.Raise(new GameGoEvent());
        
        public void OnPressedCancel() => EventBus.Raise(new GameCancelEvent());
        public void OnPressedSkip() => EventBus.Raise(new GameSkipEvent());
        public void OnOpenStatistic() => _statisticsPanel.ShowAsync().Forget();

        public override UniTask ShowAsync()
        {
            UpdateSkin();
            Reset();
            
            _isProcessing = true;
            
            return base.ShowAsync();
        }
        
        internal void Reset()
        {
            SetStatusLocalizationKey("STATUS_LABEL_NEW_GAME");
            SetTextWord("");
            _statisticsPanel.Reset();
            _playerPanelOwner.Reset();
            _playerPanelOpponent.Reset();
            _progressBar.ResetTimer();
        }
        
        internal List<SelectableLetter> InitWordsField() => _wordsField.InitField();

        internal void InitAlphabetField() => _lettersField.InitField();

        internal void SetTextWord(string value) => _wordText.text = value;

        internal string GetTextWord() => _wordText.text;
        
        
        internal void AddLetterToWord(string letter) => _wordText.text += letter;

        internal void SetStatusLocalizationKey(string localizationKey) => 
            _statusText.text = _localization.Get(LocalizationConst.TableUI, localizationKey);
        
        internal void RemoveLastLetter()
        {
            if (_wordText.text.Length > 0)
                _wordText.text = _wordText.text[..^1]; // C# range operator
        }
        
        protected virtual async void OnGoToHome(GoToHomeEvent eventData)
        {
            await GoToHome();
        }

        protected async UniTask GoToHome(bool isSaveGame = false)
        {
            // Пытаемся показать interstitial и ждём закрытия (если показалась)
            await _interstitialService.TryShowAndWaitAsync("exit_game");
            
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
            TimerBar.ResetTimer();
            SetStatusLocalizationKey("STATUS_LABEL_GAME_OVER");
            _isProcessing = false;
        }
        
        protected async UniTask UpdateSkin()
        {
            var skin = _skinsService.SkinCurrent;
            _mainBackground.sprite = await _spritesService.GetSpriteAsync(skin.MaidBackgroundAlias);
            _homeButton.image.sprite = await _spritesService.GetSpriteAsync(skin.HomeButtonAlias);
            _pauseButton.image.sprite = await _spritesService.GetSpriteAsync(skin.PauseButtonAlias);
            _cancelButton.image.sprite = await _spritesService.GetSpriteAsync(skin.CancelButtonAlias);
            _goButton.image.sprite = await _spritesService.GetSpriteAsync(skin.GoButtonAlias);
            _passButton.image.sprite = await _spritesService.GetSpriteAsync(skin.PassButtonAlias);
            _statisticButton.image.sprite = await _spritesService.GetSpriteAsync(skin.StatisticButtonAlias);
            
            await _playerPanelOwner.UpdateSkin();
            await _playerPanelOpponent.UpdateSkin();
            await _wordsField.UpdateSkin();
            await _statisticsPanel.UpdateSkin();
            await _keyboardPanel.UpdateSkin();
        }
    }
}