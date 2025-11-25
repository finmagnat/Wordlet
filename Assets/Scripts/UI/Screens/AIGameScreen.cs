using System.Collections.Generic;
using Core.Events;
using Core.Services;
using Cysharp.Threading.Tasks;
using TMPro;
using UI.Components;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Screens
{
    // TODO: экран игры с ИИ
    public class AIGameScreen : UIScreen
    {
        [Space, Header("Gme Screen UI Components")] 
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Button _playAIButton;
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
        
        private void OnGoToHome(GoToHomeEvent eventData)
        {
            HideAsync();
            Reset();
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
        }
        
        private async UniTask SetSkin()
        {
            var skin = _skinsService.SkinCurrent;
            _mainBackground.sprite = await _spritesService.GetSpriteAsync(skin.GameBackgroundAlias);
        }

    }
}