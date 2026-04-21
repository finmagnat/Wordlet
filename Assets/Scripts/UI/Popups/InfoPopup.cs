using Core.Data;
using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Popups
{
    public class InfoPopup : UIPopup
    {
        [Header("UI Elements")]
        [SerializeField] protected Button _exitButton;
        
        [Inject] private AnalyticsService _analytics;
        
        protected UniTaskCompletionSource<PopupExitData> _completionSource;
        
        protected virtual void Start()
        {
            _exitButton.onClick.AddListener(async () =>
            {    
                _analytics.TrackEvent(AnalyticsEvents.Navigation.CloseInfoClicked);
                await HideAsync();
                _completionSource?.TrySetResult(new PopupExitData { Result = PopupResult.Exit });
            });
        }
        
        public override async UniTask ShowAsync()
        {
            _completionSource = new ();
            
            _analytics.TrackEvent(AnalyticsEvents.Navigation.InfoPopupShown);
            await base.ShowAsync();
        }
        
        public UniTask<PopupExitData> WaitForResultAsync() => _completionSource.Task;
        
        public virtual void SetWindowData(MessageBoxData data) {
        }
    }
}