using System.Collections.Generic;
using Core.Data;
using Core.Services;
using Cysharp.Threading.Tasks;
using Zenject;

namespace UI.Popups
{
    public class FinishGamePopup : MessagePopup<FinishGamePopupData>
    {
        public StatsTableView statsTable;

        [Inject] private AnalyticsService _analytics;

        private FinishGamePopupData _data;

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

        private Dictionary<string, object> GetAnalyticsParams()
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Result] = _data.Result
            };
        }
    }
}
