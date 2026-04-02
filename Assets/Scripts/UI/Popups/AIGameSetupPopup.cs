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
    public class AIGameSetupPopup : PopupBase<GameSetupData>
    {
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
        private bool _isInitializing;

        private void Awake()
        {
            _toggles = _toggleGroup.GetComponentsInChildren<Toggle>(true);

            BindToggles();

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

        public override UniTask PrepareAsync(GameSetupData data)
        {
            _completionSource = new UniTaskCompletionSource<GameSetupData>();
            _gameConfig = _configService.Game;

            _isInitializing = true;

            // Сложность
            _complexityAI = (ComplexityAI)PlayerPrefs.GetInt(
                PlayerPrefsKey.ComplexityAI,
                (int)_gameConfig.complexityAiByDefault);

            ApplyComplexityToUI(_complexityAI);

            // Время хода
            _durationGame = PlayerPrefs.GetInt(
                PlayerPrefsKey.DurationGame,
                _gameConfig.durationGameByDefault);

            _durationGameSlider.minValue = 0;
            _durationGameSlider.maxValue = _gameConfig.durationGameMaximum;
            ChangeTimeText();

            _boosterPanel.Refresh();

            _isInitializing = false;

            return UniTask.CompletedTask;
        }

        private void BindToggles()
        {
            for (int i = 0; i < _toggles.Length; i++)
            {
                int index = i;
                _toggles[i].onValueChanged.AddListener(isOn =>
                {
                    if (_isInitializing || !isOn)
                        return;

                    _complexityAI = (ComplexityAI)(index + 1);
                });
            }
        }

        private void ApplyComplexityToUI(ComplexityAI complexity)
        {
            int targetIndex = (int)complexity - 1;
            targetIndex = Mathf.Clamp(targetIndex, 0, _toggles.Length - 1);

            for (int i = 0; i < _toggles.Length; i++)
            {
                _toggles[i].SetIsOnWithoutNotify(i == targetIndex);
            }
        }

        private async void OnGotoShopEvent(GotoShopEvent objEvent)
        {
            await HideAsync();
            _completionSource?.TrySetResult(new GameSetupData { Result = PopupResult.GotoShop });
        }

        public UniTask<GameSetupData> WaitForResultAsync() => _completionSource.Task;

        public void OnDurationGameSlider()
        {
            _durationGame = Mathf.RoundToInt(_durationGameSlider.value);

            if (_durationGame <= _gameConfig.durationGameMinimum)
            {
                _durationGame = _gameConfig.durationGameMinimum;
                _durationGameSlider.value = _durationGame;
            }

            SetFormatMMSS(_durationGame);
        }

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