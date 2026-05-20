using System.Collections.Generic;
using Core.Data;
using Core.Services;
using Cysharp.Threading.Tasks;
using Core.Services.Inventory;
using Zenject;

namespace UI.Popups
{
    public class StarterBonusPopup : MessagePopup<FinishGamePopupData>
    {
        [Inject] private AnalyticsService _analytics;
        [Inject] private IInventoryService _inventory;
        
        public override async UniTask PrepareAsync(FinishGamePopupData data)
        {
            await UniTask.CompletedTask;
        }
        
        public override async UniTask ShowAsync()
        {
            await base.ShowAsync();
            _analytics.TrackEvent(AnalyticsEvents.Navigation.StarterBonusPopupShown);
        }

        protected override void OnCloseClicked()
        {
            _analytics.TrackEvent(AnalyticsEvents.Navigation.StarterBonusPopupClickedClose, GetAnalyticsParams());
            base.OnCloseClicked();
        }

        protected override void OnExitClicked()
        {
            _analytics.TrackEvent(AnalyticsEvents.Navigation.StarterBonusPopupClickedGet, GetAnalyticsParams());
            base.OnExitClicked();
        }

        protected Dictionary<string, object> GetAnalyticsParams()
        {
            var dictionary = new Dictionary<string, object> {
                [AnalyticsEvents.Parameter.Boosters] =
                AnalyticsPayloadHelper.GetBoostersPayload(_inventory.Boosters)};
            return dictionary;
        }
    }
}
