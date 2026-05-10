using System.Collections.Generic;
using Core.Config;
using Core.Data;
using Core.Services;
using Core.Services.Shop;
using Cysharp.Threading.Tasks;
using Inventory;
using UnityEngine;
using Zenject;

namespace UI.Popups
{
    public class PurchaseConfirmationPopup : UIPopup<ShopOfferDto>
    {
        [Header("UI Elements")]
        [SerializeField] private ShopItemView _itemPrefab;
        [SerializeField] private Transform _contentRoot;

        [Inject] private AudioService _audioService;
        [Inject] private AnalyticsService _analytics;
        [Inject] private IInventoryService _inventory;

        protected UniTaskCompletionSource<PopupExitData> _completionSource;

        private ShopOfferDto _currentOffer;

        public UniTask<PopupExitData> WaitForResultAsync() => _completionSource.Task;

        public override UniTask PrepareAsync(ShopOfferDto offer)
        {
            Bind(offer);
            return UniTask.CompletedTask;
        }

        public override async UniTask ShowAsync()
        {
            await base.ShowAsync();
            _audioService?.PlaySfxAsync(SoundsConfig.StartNewGame);
            _analytics.TrackEvent(AnalyticsEvents.Navigation.RewardPopupShown, GetAnalyticsParams());
        }

        public void OnCloseButtonClicked()
        {
            _analytics.TrackEvent(AnalyticsEvents.Navigation.CloseRewardClicked);
            Close().Forget();
        }

        public void OnOkButtonClicked()
        {
            _analytics.TrackEvent(AnalyticsEvents.Navigation.OkRewardClicked);
            Close().Forget();
        }

        private void Bind(ShopOfferDto dto)
        {
            _currentOffer = dto;
            Clear();

            foreach (var item in dto.Rewards)
            {
                var view = Instantiate(_itemPrefab, _contentRoot);
                view.Bind(item);
            }
        }

        private async UniTask Close()
        {
            await HideAsync();
            Clear();
            _completionSource?.TrySetResult(new PopupExitData { Result = PopupResult.Exit });
        }

        private void Clear()
        {
            for (int i = _contentRoot.childCount - 1; i >= 0; i--)
                Destroy(_contentRoot.GetChild(i).gameObject);
        }

        private Dictionary<string, object> GetAnalyticsParams()
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.ProductId] = _currentOffer?.ProductId,
                [AnalyticsEvents.Parameter.Reward] = AnalyticsPayloadHelper.GetRewardsPayload(_currentOffer.Rewards),
                [AnalyticsEvents.Parameter.Boosters] = AnalyticsPayloadHelper.GetBoostersPayload(_inventory.Boosters)
            };
        }
    }
}
