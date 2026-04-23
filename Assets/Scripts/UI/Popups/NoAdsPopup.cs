using Core.Data;
using Core.Services;
using Cysharp.Threading.Tasks;
using Zenject;

namespace UI.Popups
{
    public class NoAdsPopup : MessagePopup<MessageBoxData>
    {
        [Inject] private AnalyticsService _analytics;

        public override UniTask PrepareAsync(MessageBoxData data)
        {
            SetWindowData(data);
            return UniTask.CompletedTask;
        }

        public override async UniTask ShowAsync()
        {
            await base.ShowAsync();
            _analytics.TrackEvent(AnalyticsEvents.Navigation.NoAdsPopupShown);
        }

        protected override void OnExitClicked() // Button "OK"
        {
            _analytics.TrackEvent(AnalyticsEvents.Navigation.OkNoAdsClicked);
            base.OnExitClicked();
        }

        protected override void OnCloseClicked()
        {
            _analytics.TrackEvent(AnalyticsEvents.Navigation.CloseNoAdsClicked);
            base.OnCloseClicked();
        }
    }
}
