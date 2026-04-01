using Core.Config;
using Core.Data;
using Core.Events;
using Core.Generated;
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
        
        [Header("UI Elements")]
        [SerializeField] private Button _exitButton;
        [SerializeField] private ShopPackItemView _itemPrefab;
        [SerializeField] private Transform _contentRoot;
        
        private UniTaskCompletionSource<PopupExitData> _completionSource;
        
        private bool _isInitialized;
        
        protected virtual void Start()
        {
            _exitButton.onClick.AddListener(async () =>
            {                
                await HideAsync();
                _completionSource?.TrySetResult(new PopupExitData { Result = PopupResult.Exit });
            });
        }
        
        public override async UniTask ShowAsync()
        {
            _completionSource = new ();
            
            if (!_isInitialized)
            {
                await InitializeAsync();
                
                _isInitialized = true;
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
            var result = await _shop.ExecuteOfferAsync(offer);
            if (!result.Success)
            {
                Debug.LogWarning($"Offer failed: {result.Error}");
                return;
            }

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
                EventBus.Raise(new PurchaseSuccessEvent(offer));
                await _ui.ShowPopupAsync<PurchaseConfirmationPopup, ShopOfferDto>(AssetKey.PurchaseConfirmationPopup, offer);
                
                HideAsync();
            }
        }
        
        private async UniTask RebuildCatalogAsync()
        {
            // очистка
            for (int i = _contentRoot.childCount - 1; i >= 0; i--)
                Destroy(_contentRoot.GetChild(i).gameObject);

            var offers = await _shop.GetCatalogAsync();

            foreach (var offer in offers)
            {
                var view = _container.InstantiatePrefabForComponent<ShopPackItemView>(_itemPrefab, _contentRoot);
                view.Bind(offer, OnOfferClicked);
            }
        }

    }
}