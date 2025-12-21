using Core.Config;
using Core.Data;
using Core.Services;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Popups
{
    public class AIGameSetupPopup : UIPopup
    {
        private const int STEP_TIME = 5;
        private const int MIN_TIME = 5;
        
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _numericTimeText;
        [SerializeField] private ToggleGroup _toggleGroup;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _closeButton;
        
        [Inject] private ConfigService _configService;
        
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
        }
        
        public UniTask<GameSetupData> WaitForResultAsync() => _completionSource.Task;
        
        public override async UniTask ShowAsync()
        {
            var gameConfig = _configService.Game;
            _durationGame = PlayerPrefs.GetInt(PlayerPrefsKey.DurationGame, gameConfig.durationGameByDefault);
            _complexityAI = (ComplexityAI)PlayerPrefs.GetInt(PlayerPrefsKey.ComplexityAI, (int)gameConfig.complexityAiByDefault);
                
            _completionSource = new UniTaskCompletionSource<GameSetupData>();
            
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
            _durationGame += STEP_TIME;
            ChangeTimeText();
        }

        public void OnDecrTimeButton()
        {
            if (_durationGame - STEP_TIME > MIN_TIME)
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