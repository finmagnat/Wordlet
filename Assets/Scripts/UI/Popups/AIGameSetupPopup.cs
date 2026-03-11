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
        [SerializeField] private ToggleGroup _toggleGroup;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private BoosterPanelUI _boosterPanel;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private Slider _durationGameSlider;
        
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
            _completionSource = new UniTaskCompletionSource<GameSetupData>();
            
            _gameConfig = _configService.Game;
            
            // Сложность игры
            _complexityAI = (ComplexityAI)PlayerPrefs.GetInt(PlayerPrefsKey.ComplexityAI, (int)_gameConfig.complexityAiByDefault);
            
            await base.ShowAsync();
            
            foreach (var toggle in _toggles)
            {
                toggle.isOn = false;
            }
            _toggles[(int)_complexityAI - 1].isOn = true;
            
            // Время хода
            _durationGame = PlayerPrefs.GetInt(PlayerPrefsKey.DurationGame, _gameConfig.durationGameByDefault);
            _durationGameSlider.minValue = 0;
            _durationGameSlider.maxValue = _gameConfig.durationGameMaximum;
            ChangeTimeText();
            
            _boosterPanel.Refresh();
        }

        public void OnDurationGameSlider()
        {
            _durationGame = Mathf.RoundToInt(_durationGameSlider.value);
            if (_durationGame <= _gameConfig.durationGameMinimum)
            {
                _durationGame = _gameConfig.durationGameMinimum;
                _durationGameSlider.value = _durationGame;
            }
            
            SetFormatMMSS(_durationGame);
            //Debug.Log($"OnDurationGameSlider: {_durationGameSlider.value} = {_durationGame}");
        }
        
        /*public void OnIncrTimeButton()
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
        }*/

        public void OnSelectComplexityAI(int value)
        {
            _complexityAI = (ComplexityAI)value;
        }
        
        //private void ChangeTimeText() => _timeText.text = _durationGame.ToString();
        
        private void ChangeTimeText()
        {
            _durationGameSlider.value = _durationGame;
            SetFormatMMSS(_durationGame);
        }
        
        private void SetFormatMMSS(int seconds)
        {
            if (seconds < 0) seconds = 0;
            int m = seconds / 60;
            int s = seconds % 60;
            _timeText.text = $"{m:00}:{s:00}";
        }
    }
}