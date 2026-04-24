using Core.Data;
using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Popups
{
    public class AIGameExitPopup : UIPopup
    {
        [Header("UI Elements")]
        [SerializeField] private Button _saveAndExitButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _closeButton;

        [Inject] private AnalyticsService _analytics;
        
        private UniTaskCompletionSource<GameExitData> _completionSource;

        private void Start()
        {
            _saveAndExitButton.onClick.AddListener(async () =>
            {                
                await HideAsync();
                _completionSource?.TrySetResult(new GameExitData { Result = PopupResult.SaveAndExit });
            });
            
            _exitButton.onClick.AddListener(async () =>
            {                
                await HideAsync();
                _completionSource?.TrySetResult(new GameExitData { Result = PopupResult.Exit });
            });

            _closeButton.onClick.AddListener(async () =>
            {
                _analytics.TrackEvent(AnalyticsEvents.Navigation.CloseAiGameExitPopupClicked);
                await HideAsync();
                _completionSource?.TrySetResult(new GameExitData { Result = PopupResult.Close });
            });
        }
        
        public override async UniTask ShowAsync()
        {
            _completionSource = new ();
            await base.ShowAsync();
            _analytics.TrackEvent(AnalyticsEvents.Navigation.AiGameExitPopupShown);
        }
        
        public UniTask<GameExitData> WaitForResultAsync() => _completionSource.Task;
    }
}
