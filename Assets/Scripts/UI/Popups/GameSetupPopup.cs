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
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _closeButton;
        
        private UniTaskCompletionSource<bool> _completionSource;
        public UniTask<bool> WaitForResultAsync() => _completionSource.Task;

        private void Start()
        {
            _closeButton.onClick.AddListener(async () =>
            {
                await HideAsync();
                _completionSource?.TrySetResult(false); // отмена
            });

            _startButton.onClick.AddListener(async () =>
            {
                Debug.Log($"▶ Начинаем игру: " +
                          //$"ИИ={_opponentDropdown.captionText.text}, " +
                          $"Сложность={_difficultyDropdown.captionText.text}, " +
                          $"Время={_turnTimeDropdown.captionText.text}");
                await HideAsync();
                _completionSource?.TrySetResult(true); // начать игру
            });
        }

        public override async UniTask ShowAsync()
        {
            _completionSource = new UniTaskCompletionSource<bool>();
            await base.ShowAsync();
        }
    }
}