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
        [Inject] private AdvertisingBoosterService _adBoosterService;

        protected FinishGamePopupData _data;
        protected AdsRewardItem _finishAdOffer;

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
            _finishAdOffer = _adBoosterService.GetData();
            await advertisingBooster.ShowAsync(_finishAdOffer, OnAdOfferClicked);
            
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
        
        protected void OnAdOfferClicked()
        {
            _analytics.TrackEvent(AnalyticsEvents.Navigation.AdFinishGamePopupClicked, GetAnalyticsParams());
            base.OnExitClicked();
        }

        protected virtual Dictionary<string, object> GetAnalyticsParams()
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Result] = _data.Result,
                [AnalyticsEvents.Parameter.FinishAdOffer] = GetAdsRewardParam()
            };
        }

        private string GetAdsRewardParam()
        {
            return $"[{{\"item_id\":\"{_finishAdOffer.BoosterType}\",\"amount\":{_finishAdOffer.Count}}}]";
        }
    }
}
