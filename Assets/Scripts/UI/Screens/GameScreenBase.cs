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
    public abstract class GameScreenBase : UIScreen
    {
        [Space, Header("Game Screen UI Components")]
        [SerializeField] protected TextMeshProUGUI _statusText;
        [SerializeField] protected TextMeshProUGUI _wordText;
        [SerializeField] protected TimerProgressBar _progressBar;

        [SerializeField] protected Button _homeButton;
        [SerializeField] protected Button _pauseButton;
        [SerializeField] protected Button _cancelButton;
        [SerializeField] protected Button _goButton;
        [SerializeField] protected Button _repeatGame;
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
        [SerializeField] protected FocusHoleOverlay _eraserOverlay;

        internal TimerProgressBar TimerBar => _progressBar;
        internal PlayerPanel PlayerPanelOwner => _playerPanelOwner;
        internal PlayerPanel PlayerPanelOpponent => _playerPanelOpponent;
        internal StatisticsPanel StatisticsPanel => _statisticsPanel;
        internal KeyboardPanel KeyboardPanel => _keyboardPanel;
        internal FocusHoleOverlay EraserOverlay => _eraserOverlay;
        internal BoosterPanelIngameScreen BoosterPanel => _boosterPanel;
        internal Button PauseButton => _pauseButton;
        internal Button PassButton => _passButton;
        internal GameObject CancelButton => _cancelButton.gameObject;
        internal GameObject GoButton => _goButton.gameObject;
        internal GameObject RepeatGame => _repeatGame.gameObject;

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

        protected virtual void Start()
        {
            EventBus.Subscribe<GoToHomeEvent>(OnGoToHome);
            EventBus.Subscribe<GameEndEvent>(OnGameEnd);
        }

        protected virtual void OnDestroy()
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
        public void OnPressedRepeatGame() => EventBus.Raise(new RepeatGameEvent());
        public void OnPressedCancel() => EventBus.Raise(new GameCancelEvent());
        public void OnPressedSkip() => EventBus.Raise(new GameSkipEvent());
        public void OnOpenStatistic() => _statisticsPanel.ShowAsync().Forget();

        public override async UniTask ShowAsync()
        {
            _isProcessing = true;
            await base.ShowAsync();
        }

        internal virtual void Reset()
        {
            SetStatusLocalizationKey("STATUS_LABEL_NEW_GAME");
            SetTextWord(string.Empty);
            _statisticsPanel.Reset();
            _playerPanelOwner.Reset();
            _playerPanelOpponent.Reset();
            _progressBar.ResetTimer();
            _repeatGame.gameObject.SetActive(false);
            
            if (_isPaused)
            {
                _isPaused = false;
                _pauseButtonAnimator.SetPaused(_isPaused);
                _pauseService.SetUserPause(_isPaused);
            }
        }

        internal virtual List<SelectableLetter> InitWordsField() => _wordsField.InitField();
        internal virtual void InitAlphabetField() => _lettersField.InitField();
        internal virtual void SetTextWord(string value) => _wordText.text = value;
        internal virtual string GetTextWord() => _wordText.text;
        internal virtual void AddLetterToWord(string letter) => _wordText.text += letter;

        internal virtual void SetStatusLocalizationKey(string localizationKey)
        {
            _statusText.text = _localization.Get(LocalizationConst.TableUI, localizationKey);
        }

        internal virtual void RemoveLastLetter()
        {
            if (!string.IsNullOrEmpty(_wordText.text))
                _wordText.text = _wordText.text[..^1];
        }

        protected virtual async void OnGoToHome(GoToHomeEvent eventData)
        {
            await GoToHome();
        }

        protected async UniTask GoToHome(bool isSaveGame = false)
        {
            await _interstitialService.TryShowAndWaitAsync("exit_game");
            await _loadingUI.ShowLoadingAsync<InGameLoadingScreen>(AssetKey.InGameLoadingScreen);
            
            if (isSaveGame)
                await _saveService.SaveAsync();

            Reset();

            await _ui.HideAllScreensAsync();
            await _ui.ShowScreenAsync<MainMenuScreen>(AssetKey.MainMenuScreen);
            
            _isProcessing = false;
            
            await _loadingUI.HideLoadingAsync();
        }

        protected virtual void OnGameEnd(GameEndEvent eventData)
        {
            TimerBar.ResetTimer();
            SetStatusLocalizationKey("STATUS_LABEL_GAME_OVER");
        }

        protected async UniTask UpdateSkinAsync()
        {
            var skin = _skinsService.SkinCurrent;

            _mainBackground.sprite = await _spritesService.GetSpriteAsync(skin.MainBackgroundAlias);
            _homeButton.image.sprite = await _spritesService.GetSpriteAsync(skin.HomeButtonAlias);
            _pauseButton.image.sprite = await _spritesService.GetSpriteAsync(skin.PauseButtonAlias);
            _cancelButton.image.sprite = await _spritesService.GetSpriteAsync(skin.CancelButtonAlias);
            _goButton.image.sprite = await _spritesService.GetSpriteAsync(skin.GoButtonAlias);
            _repeatGame.image.sprite = await _spritesService.GetSpriteAsync(skin.RepeatGameButtonAlias);
            _passButton.image.sprite = await _spritesService.GetSpriteAsync(skin.PassButtonAlias);
            _statisticButton.image.sprite = await _spritesService.GetSpriteAsync(skin.StatisticButtonAlias);

            await _progressBar.UpdateSkin();
            await _playerPanelOwner.UpdateSkin();
            await _playerPanelOpponent.UpdateSkin();
            await _wordsField.UpdateSkin();
            await _statisticsPanel.UpdateSkin();
            await _keyboardPanel.UpdateSkin();
        }

        protected virtual async UniTask PrepareCommonAsync()
        {
            Reset();
            await UpdateSkinAsync();
        }
    }
}