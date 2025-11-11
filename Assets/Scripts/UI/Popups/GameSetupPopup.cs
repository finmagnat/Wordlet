using Core.Data;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups
{
    public class GameSetupPopup : UIPopup
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Dropdown _difficultyDropdown;
        [SerializeField] private TMP_Dropdown _turnTimeDropdown;
        [SerializeField] private TMP_Text _turnTimeLabel;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _closeButton;
        
        private UniTaskCompletionSource<GameSetupData> _completionSource;

        public UniTask<GameSetupData> WaitForResultAsync() => _completionSource.Task;

        private void Awake()
        {
            if (_turnTimeDropdown != null)
            {
                _turnTimeDropdown.onValueChanged.AddListener(v =>
                {
                    if (_turnTimeLabel != null)
                        _turnTimeLabel.text = $"{Mathf.RoundToInt(v)} сек.";
                });
            }
        }

        public override async UniTask ShowAsync()
        {
            _completionSource = new UniTaskCompletionSource<GameSetupData>();
            await base.ShowAsync();
        }

        private void Start()
        {
            _startButton.onClick.AddListener(async () =>
            {
                await HideAsync();
                var data = new GameSetupData
                {
                    Result = PopupResult.Play,
                    Difficulty = _difficultyDropdown?.value ?? 0,
                    TurnTime = Mathf.RoundToInt(_turnTimeDropdown?.value ?? 30)
                };
                _completionSource?.TrySetResult(data);
            });

            _closeButton.onClick.AddListener(async () =>
            {
                await HideAsync();
                _completionSource?.TrySetResult(new GameSetupData { Result = PopupResult.Close });
            });
        }
    }
}