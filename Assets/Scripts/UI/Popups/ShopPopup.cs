using System;
using System.Collections.Generic;
using Core.Config;
using Core.Data;
using Core.Events;
using Core.Generated;
using Core.Services;
using Core.Services.Shop;
using Core.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Popups
{
    public class ShopPopup : UIPopup
    {
        [Inject] private IShopService _shop;
        [Inject] private DiContainer _container;
        [Inject] private IUIManager _ui;
        [Inject] private RewardedAdsService _ads;
        [Inject] private AnalyticsService _analytics;
        
        [Header("UI Elements")]
        [SerializeField] private Button _exitButton;
        [SerializeField] private ShopPackItemView _itemPrefab;
        [SerializeField] private Transform _contentRoot;
        
        private UniTaskCompletionSource<PopupExitData> _completionSource;
        
        private bool _isInitialized;
        private List<ShopPackItemView> _packItems = new ();
        
        protected virtual void Start()
        {
            _exitButton.onClick.AddListener(async () =>
            {         
                _analytics.TrackEvent(AnalyticsEvents.Navigation.CloseShopClicked);
                await HideAsync();
                _completionSource?.TrySetResult(new PopupExitData { Result = PopupResult.Exit });
            });
        }
        
        public override async UniTask ShowAsync()
        {
            _completionSource = new ();
            _analytics.TrackEvent(AnalyticsEvents.Navigation.ShopPopupShown);
            
            if (!_isInitialized)
            {
                await InitializeAsync();
                
                _isInitialized = true;
            }
            
            foreach (var packItem in _packItems)
            {
                packItem.RefreshLocalizedState();

                if (packItem.Dto.Type == ShopOfferTypeDto.RewardedAd)
                {
                    _analytics.TrackEvent(AnalyticsEvents.Ads.RewardedAvailability,
                        new Dictionary<string, object>
                        {
                            [AnalyticsEvents.Parameter.RewardType] = packItem.Dto.RewardType.ToString(),
                            [AnalyticsEvents.Parameter.IsReady] = _ads.IsReady(packItem.Dto.RewardType),
                            [AnalyticsEvents.Parameter.IsLoading] = _ads.IsLoading(packItem.Dto.RewardType),
                            [AnalyticsEvents.Parameter.Cooldown] = packItem.Cooldown,
                            [AnalyticsEvents.Parameter.DailyLimitReached] = packItem.IsLimitDailyReached,
                        });
                }
            }
            
            await base.ShowAsync();
        }
        
        public UniTask<PopupExitData> WaitForResultAsync() => _completionSource.Task;
        
        public virtual void SetWindowData(MessageBoxData data) { }
        
        private async UniTask InitializeAsync()
        {
            await RebuildCatalogAsync();
        }
        
        private async void OnOfferClicked(ShopOfferDto offer)
        {
            PurchaseResult result;
            try
            {
                result = await _shop.ExecuteOfferAsync(offer);
            }
            catch (Exception exception)
            {
                TrackPurchaseResult(offer, AnalyticsEvents.Monetization.PurchaseError, exception.Message);
                Debug.LogWarning($"Offer error: {exception}");
                return;
            }
            if (!result.Success)
            {
                TrackPurchaseResult(
                    offer,
                    result.IsError ? AnalyticsEvents.Monetization.PurchaseError : AnalyticsEvents.Monetization.PurchaseFailed,
                    result.Error);
                Debug.LogWarning($"Offer failed: {result.Error}");
                return;
            }

            TrackPurchaseResult(offer, AnalyticsEvents.Monetization.PurchaseSuccess);

            // 1) Уведомление
            if (offer.Type == ShopOfferTypeDto.IapPack && offer.ProductId == ShopCatalog.RemoveInterstitialProductId)
            {
                // MVP-уведомление (без зависимости от других попапов)
                Debug.Log("[Shop] Interstitial-реклама отключена");

                // 2) Обновляем витрину, чтобы remove_ads исчез сразу
                await RebuildCatalogAsync();
                
                await _ui.ShowPopupAsync<NoAdsPopup>(AssetKey.NoAdsPopup);
                
                HideAsync();
            }
            else
            {
                await _ui.ShowPopupAsync<RewardPopup, RewardPopupData>(AssetKey.RewardPopup, RewardPopupData.FromShopOffer(offer));
                Debug.Log($"[ShopPopup][OnOfferClicked] PurchaseSuccessEvent, ProductId = {offer.ProductId}");
                EventBus.Raise(new PurchaseSuccessEvent(offer));
                
                HideAsync();
            }
        }

        private void TrackPurchaseResult(ShopOfferDto offer, string eventName, string error = null)
        {
            if (offer == null || offer.Type != ShopOfferTypeDto.IapPack)
                return;

            var parameters = new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.ProductId] = offer.ProductId,
                [AnalyticsEvents.Parameter.Reward] = AnalyticsPayloadHelper.GetRewardsPayload(offer.Rewards),
                [AnalyticsEvents.Parameter.Price] = offer.CtaText,
            };

            if (!string.IsNullOrWhiteSpace(error))
                parameters[AnalyticsEvents.Parameter.Error] = error;

            _analytics.TrackEvent(eventName, parameters);
        }
        
        private async UniTask RebuildCatalogAsync()
        {
            // очистка
            for (int i = _contentRoot.childCount - 1; i >= 0; i--)
                Destroy(_contentRoot.GetChild(i).gameObject);

            var offers = await _shop.GetCatalogAsync();

            foreach (var offer in offers)
            {
                var packItemView = _container.InstantiatePrefabForComponent<ShopPackItemView>(_itemPrefab, _contentRoot);
                packItemView.Bind(offer, OnOfferClicked);
                
                _packItems.Add(packItemView);
            }
        }
    }
}
