using System.Collections.Generic;
using Core.Data;
using Core.Services;
using Core.UI.Components;
using Cysharp.Threading.Tasks;
using Zenject;

namespace UI.Popups
{
    public class FinishGamePopup : MessagePopup<FinishGamePopupData>
    {
        public StatsTableView statsTable;
        public AdvertisingBooster advertisingBooster;

        [Inject] private AnalyticsService _analytics;

        protected FinishGamePopupData _data;

        public override async UniTask PrepareAsync(FinishGamePopupData data)
        {
            _data = data;

            statsTable.SetData(
                data.OwnerName,
                data.OpponentName,
                data.OwnerScore,
                data.OpponentScore,
                data.OwnerPass,
                data.OpponentPass,
                data.MaxPasses
            );

            await UniTask.CompletedTask;
        }

        public override async UniTask ShowAsync()
        {
            await advertisingBooster.ShowAsync();
            
            await base.ShowAsync();
            
            _analytics.TrackEvent(AnalyticsEvents.Navigation.FinishGamePopupShown, GetAnalyticsParams());
        }

        protected override void OnCloseClicked()
        {
            _analytics.TrackEvent(AnalyticsEvents.Navigation.CloseFinishGamePopupClicked, GetAnalyticsParams());
            base.OnCloseClicked();
        }

        protected override void OnExitClicked()
        {
            _analytics.TrackEvent(AnalyticsEvents.Navigation.OkFinishGamePopupClicked, GetAnalyticsParams());
            base.OnExitClicked();
        }

        protected virtual Dictionary<string, object> GetAnalyticsParams()
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Result] = _data.Result
            };
        }
    }
}
