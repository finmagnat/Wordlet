using System;
using System.Collections.Generic;
using System.Linq;
using Core.Config;
using Core.Data;
using Cysharp.Threading.Tasks;
using Inventory;
using UnityEngine;
using UnityEngine.Purchasing;
using Zenject;

namespace Core.Services.Shop
{
    /// <summary>
    /// Покупка и выдача наград через Unity IAP (Google Play).
    /// Rewarded офферы не покупаются — они триггерят показ рекламы.
    /// </summary>
    public sealed class GooglePlayShopService : IShopService, IStoreListener
    {
        private readonly ShopCatalog _catalog;
        private readonly IInventoryService _inventory;
        private readonly InventorySyncService _inventorySync;
        private readonly RewardedAdsService _ads;
        private readonly LocalizationService _localization;
        private readonly AdsEntitlementService _adsEntitlement;

        private IStoreController _controller;
        private IExtensionProvider _extensions;

        private UniTaskCompletionSource<bool> _initTcs;
        private bool _initStarted;

        private UniTaskCompletionSource<PurchaseResult> _purchaseTcs;
        private string _pendingPurchaseId;

        private bool EnablePurchasePushToServer => _catalog.EnablePurchasePushToServer;

        [Inject]
        public GooglePlayShopService(
            ShopCatalog catalog,
            IInventoryService inventory,
            InventorySyncService inventorySync,
            RewardedAdsService ads,
            LocalizationService localization,
            AdsEntitlementService adsEntitlement)
        {
            _catalog = catalog;
            _inventory = inventory;
            _inventorySync = inventorySync;
            _ads = ads;
            _localization = localization;
            _adsEntitlement = adsEntitlement;
        }

        // -------- IShopService --------

        public UniTask InitializeAsync() => UniTask.CompletedTask;

        public async UniTask<IReadOnlyList<ShopOfferDto>> GetCatalogAsync()
        {
            // Инициализируем IAP ТОЛЬКО если есть IAP офферы
            if (HasAnyIapOffers())
                await EnsureInitializedAsync();

            var list = new List<ShopOfferDto>(_catalog.Offers.Count);

            foreach (var o in _catalog.Offers)
            {
                if (o.DisableInterstitialAds && _adsEntitlement.NoInterstitialAds)
                    continue;
                
                var dto = new ShopOfferDto
                {
                    Type = (ShopOfferTypeDto)o.Type,
                    ProductId = o.ProductId,
                    RewardType = o.RewardType,

                    Title = o.Title,
                    SpriteHeader = o.SpriteHeader,
                    Description = o.Description,

                    Rewards = o.Rewards.Select(r => new ShopRewardDto
                    {
                        ItemId = r.ItemId,
                        Amount = r.Amount,
                        SpriteIcon = r.SpriteIcon
                    }).ToList(),

                    // По умолчанию
                    IsAvailable = o.DebugAvailable,
                    IsDisableInterstitialAds = o.DisableInterstitialAds,
                    CtaText = o.Type == ShopOfferType.RewardedAd ? _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyTextLook) : o.DebugPriceText
                };

                // Для IAP пробуем подставить реальную цену из Unity IAP
                if (o.Type == ShopOfferType.IapPack && !string.IsNullOrWhiteSpace(o.ProductId) && _controller != null)
                {
                    var product = _controller.products.WithID(o.ProductId);
                    if (product != null)
                    {
                        dto.IsAvailable = product.availableToPurchase;

                        var price = product.metadata?.localizedPriceString;
                        if (!string.IsNullOrWhiteSpace(price))
                            dto.CtaText = price;
                    }
                }

                list.Add(dto);
            }

            return list;
        }

        public async UniTask<PurchaseResult> ExecuteOfferAsync(ShopOfferDto offer)
        {
            if (offer == null)
                return PurchaseResult.Fail("Offer is null");

            switch (offer.Type)
            {
                case ShopOfferTypeDto.IapPack:
                    return await PurchaseAsync(offer.ProductId);

                case ShopOfferTypeDto.RewardedAd:
                    // Показ рекламы; выдача награды делается RewardedBoosterGrantService по OnRewardEarned
                    Debug.Log($"[ShopService] Execute rewarded: rewardType={offer.RewardType}");
                    _ads.ShowFor(offer.RewardType);
                    return PurchaseResult.Ok();

                default:
                    return PurchaseResult.Fail($"Unknown offer type: {offer.Type}");
            }
        }

        public async UniTask<PurchaseResult> PurchaseAsync(string productId)
        {
            await EnsureInitializedAsync();

            if (_purchaseTcs != null)
                return PurchaseResult.Fail("Purchase already in progress");

            var product = _controller.products.WithID(productId);
            if (product == null)
                return PurchaseResult.Fail($"Unknown productId: {productId}");
            if (!product.availableToPurchase)
                return PurchaseResult.Fail("Product not available");

            _pendingPurchaseId = productId;
            _purchaseTcs = new UniTaskCompletionSource<PurchaseResult>();

            _controller.InitiatePurchase(product);

            return await _purchaseTcs.Task;
        }

        // -------- Unity IAP init --------

        private bool HasAnyIapOffers()
        {
            return _catalog.Offers.Any(o =>
                o.Type == ShopOfferType.IapPack &&
                !string.IsNullOrWhiteSpace(o.ProductId));
        }

        private UniTask EnsureInitializedAsync()
        {
            if (_controller != null)
                return UniTask.CompletedTask;

            _initTcs ??= new UniTaskCompletionSource<bool>();

            if (!_initStarted)
            {
                _initStarted = true;

                var module = StandardPurchasingModule.Instance(AppStore.GooglePlay);
                var builder = ConfigurationBuilder.Instance(module);

                // Добавляем ТОЛЬКО IAP офферы
                foreach (var o in _catalog.Offers)
                {
                    if (o.Type != ShopOfferType.IapPack) continue;
                    if (string.IsNullOrWhiteSpace(o.ProductId)) continue;
                    
                    var type = o.IsNonConsumable ? ProductType.NonConsumable : ProductType.Consumable;
                    builder.AddProduct(o.ProductId, type);
                }

                UnityPurchasing.Initialize(this, builder);
            }

            return _initTcs.Task.AsUniTask();
        }

        // -------- IStoreListener --------

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;

            _initTcs?.TrySetResult(true);
            _initTcs = null;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            _initTcs?.TrySetException(new Exception($"IAP init failed: {error}"));
            _initTcs = null;
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            _initTcs?.TrySetException(new Exception($"IAP init failed: {error}. {message}"));
            _initTcs = null;
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            var product = e.purchasedProduct;
            var productId = product.definition.id;

            try
            {
                // Начисляем награду локально по ShopCatalog (только для IAP офферов)
                var pack = _catalog.Offers.FirstOrDefault(p =>
                    p.Type == ShopOfferType.IapPack &&
                    p.ProductId == productId);

                if (pack == null)
                {
                    Debug.LogError($"[IAP] Offer not found in ShopCatalog: {productId}. Confirming purchase.");
                }
                else if (pack != null && pack.DisableInterstitialAds)
                {
                    // Ставим entitlement (локально + PlayFab user data)
                    _adsEntitlement.SetNoInterstitialAdsLocal(true);
                    _adsEntitlement.SetNoInterstitialAdsAsync(true).Forget();
                }
                else
                {
                    foreach (var reward in pack.Rewards)
                        _inventory.Add(reward.ItemId, reward.Amount);
                }
                
                // Confirm/consume
                _controller.ConfirmPendingPurchase(product);
                Debug.Log($"[IAP] ConfirmPendingPurchase OK: {productId}");

                // Optional server push + sync
                if (EnablePurchasePushToServer && pack != null)
                    PushPurchaseToServerAsync(pack).Forget();

                if (_purchaseTcs != null && _pendingPurchaseId == productId)
                {
                    _purchaseTcs.TrySetResult(PurchaseResult.Ok());
                    _purchaseTcs = null;
                    _pendingPurchaseId = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IAP] ProcessPurchase exception for {productId}: {ex}");

                try { _controller.ConfirmPendingPurchase(product); } catch { /* ignore */ }

                if (_purchaseTcs != null && _pendingPurchaseId == productId)
                {
                    _purchaseTcs.TrySetResult(PurchaseResult.Fail(ex.Message));
                    _purchaseTcs = null;
                    _pendingPurchaseId = null;
                }
            }

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogError($"[IAP] Purchase failed: {product?.definition?.id} reason={failureReason}");

            if (_purchaseTcs != null)
            {
                _purchaseTcs.TrySetResult(PurchaseResult.Fail(failureReason.ToString()));
                _purchaseTcs = null;
                _pendingPurchaseId = null;
            }
        }

        // -------- Optional server sync --------

        private async UniTaskVoid PushPurchaseToServerAsync(ShopOfferConfig pack)
        {
            try
            {
                foreach (var reward in pack.Rewards)
                    await _inventorySync.GrantBoosterAsync((BoosterType)reward.ItemId, reward.Amount);

                await _inventorySync.SyncFromServerAsync();

                Debug.Log($"[IAP] Purchase pushed to server and synced: {pack.ProductId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IAP] PushPurchaseToServer failed for {pack.ProductId}: {ex}");
            }
        }
    }
}
