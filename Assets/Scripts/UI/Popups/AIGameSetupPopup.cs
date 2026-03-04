using Core.Config;
using Core.Data;
using Core.Events;
using Core.Services;
using Cysharp.Threading.Tasks;
using TMPro;
using UI.Components;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Popups
{
    public class AIGameSetupPopup : UIPopup
    {
        private const int STEP_TIME = 5;
        
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _numericTimeText;
        [SerializeField] private ToggleGroup _toggleGroup;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private BoosterPanelUI _boosterPanel;
        
        [Inject] private ConfigService _configService;
        
        private GameConfig _gameConfig;
        private ComplexityAI _complexityAI;
        private int _durationGame;
        private Toggle[] _toggles;
        
        private UniTaskCompletionSource<GameSetupData> _completionSource;

        private void Start()
        {
            _toggles = _toggleGroup.GetComponentsInChildren<Toggle>();
            
            _startButton.onClick.AddListener(async () =>
            {
                PlayerPrefs.SetInt(PlayerPrefsKey.DurationGame, _durationGame);
                PlayerPrefs.SetInt(PlayerPrefsKey.ComplexityAI, (int)_complexityAI);
                
                await HideAsync();
                
                var data = new GameSetupData
                {
                    Result = PopupResult.Play,
                    Difficulty = _complexityAI,
                    TurnTime = _durationGame
                };
                _completionSource?.TrySetResult(data);
            });

            _closeButton.onClick.AddListener(async () =>
            {
                await HideAsync();
                _completionSource?.TrySetResult(new GameSetupData { Result = PopupResult.Close });
            });
            
            EventBus.Subscribe<GotoShopEvent>(OnGotoShopEvent);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<GotoShopEvent>(OnGotoShopEvent);
        }

        private async void OnGotoShopEvent(GotoShopEvent objEvent)
        {
            await HideAsync();
            _completionSource?.TrySetResult(new GameSetupData { Result = PopupResult.GotoShop });
        }

        public UniTask<GameSetupData> WaitForResultAsync() => _completionSource.Task;
        
        public override async UniTask ShowAsync()
        {
            _gameConfig = _configService.Game;
            _durationGame = PlayerPrefs.GetInt(PlayerPrefsKey.DurationGame, _gameConfig.durationGameByDefault);
            _complexityAI = (ComplexityAI)PlayerPrefs.GetInt(PlayerPrefsKey.ComplexityAI, (int)_gameConfig.complexityAiByDefault);
                
            _completionSource = new UniTaskCompletionSource<GameSetupData>();
            
            _boosterPanel.Refresh();
            
            await base.ShowAsync();
            
            foreach (var toggle in _toggles)
            {
                toggle.isOn = false;
            }
            _toggles[(int)_complexityAI - 1].isOn = true;
            
            ChangeTimeText();
        }
        
        public void OnIncrTimeButton()
        {
            if (_durationGame + STEP_TIME <= _gameConfig.durationGameMaximum)
            {
                _durationGame += STEP_TIME;
                ChangeTimeText();
            }
        }

        public void OnDecrTimeButton()
        {
            if (_durationGame - STEP_TIME >= _gameConfig.durationGameMinimum)
            {
                _durationGame -= STEP_TIME;
                ChangeTimeText();
            }            
        }

        public void OnSelectComplexityAI(int value)
        {
            _complexityAI = (ComplexityAI)value;
        }
        
        private void ChangeTimeText() => _numericTimeText.text = _durationGame.ToString();
    }
}