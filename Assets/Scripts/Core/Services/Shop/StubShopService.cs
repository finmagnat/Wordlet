using System;
using System.Collections.Generic;
using System.Linq;
using Core.Config;
using Core.Data;
using Cysharp.Threading.Tasks;
using Inventory;
using PlayFab;
using PlayFab.ClientModels;
using Zenject;

namespace Core.Services.Shop
{
    public sealed class StubShopService : IShopService
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string DEBUG_SECRET = "SOME_LONG_RANDOM_SECRET_123";
#else
        private const string DEBUG_SECRET = "";
#endif
        
        private readonly ShopCatalog _catalog;
        private readonly InventorySyncService _inventorySync;
        private readonly RewardedAdsService _ads;
        private readonly LocalizationService _localization;
        private readonly AdsEntitlementService _adsEntitlement;
        
        [Inject]
        public StubShopService(ConfigService configService, 
            InventorySyncService inventorySync, 
            RewardedAdsService ads, 
            LocalizationService localization,
            AdsEntitlementService adsEntitlement)
        {
            _catalog = configService.Shop;
            _inventorySync = inventorySync;
            _ads = ads;
            _localization = localization;
            _adsEntitlement = adsEntitlement;
        }

        public UniTask InitializeAsync() => UniTask.CompletedTask;

        public UniTask<IReadOnlyList<ShopOfferDto>> GetCatalogAsync()
        {
            var list = new List<ShopOfferDto>(_catalog.Offers.Count);

            foreach (var o in _catalog.Offers)
            {
                if (o.DisableInterstitialAds && _adsEntitlement.NoInterstitialAds)
                    continue;

                list.Add(new ShopOfferDto
                {
                    Type = (ShopOfferTypeDto)o.Type,
                    ProductId = o.ProductId,
                    RewardType = o.RewardType,
                    Title = o.Title,
                    SpriteHeader = o.SpriteHeader,
                    Description = o.Description,
                    CtaText = o.Type == ShopOfferType.IapPack
                        ? o.DebugPriceText
                        : _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyTextLook),
                    IsAvailable = o.DebugAvailable,
                    Rewards = o.Rewards.Select(r => new ShopRewardDto { ItemId = r.ItemId, Amount = r.Amount, SpriteIcon = r.SpriteIcon }).ToList()
                });
            }

            return UniTask.FromResult((IReadOnlyList<ShopOfferDto>)list);
        }
        
        public async UniTask<PurchaseResult> ExecuteOfferAsync(ShopOfferDto offer)
        {
            if (offer == null) return PurchaseResult.Fail("Offer is null");
            
            if (offer.Type == ShopOfferTypeDto.IapPack && offer.ProductId == ShopCatalog.RemoveInterstitialProductId)
            {
                // ✅ применяем сразу в этой сессии
                _adsEntitlement.SetNoInterstitialAdsLocal(true);

                // опционально: пишем на сервер (если PlayFab доступен и ты хочешь чтобы в Editor тоже записывалось)
                _adsEntitlement.SetNoInterstitialAdsAsync(true).Forget();

                return PurchaseResult.Ok();
            }
            
            switch (offer.Type)
            {
                case ShopOfferTypeDto.IapPack:
                    return await PurchaseAsync(offer.ProductId);

                case ShopOfferTypeDto.RewardedAd:
                    _ads.ShowFor(offer.RewardType);
                    return PurchaseResult.Ok();

                default:
                    return PurchaseResult.Fail("Unknown offer type");
            }
        }


        public async UniTask<PurchaseResult> PurchaseAsync(string productId)
        {
            var pack = _catalog.Offers.FirstOrDefault(p => p.ProductId == productId);
            if (pack == null) return PurchaseResult.Fail("Pack not found");
            if (!pack.DebugAvailable) return PurchaseResult.Fail("Not available");

            try
            {
#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
                return PurchaseResult.Fail("Debug purchases disabled");
#endif

                await PlayFabShopGateway.GrantPackOnServerAsync(
                    source: "debug",
                    productId: productId,
                    debugSecret: DEBUG_SECRET
                );

                // подтягиваем серверный источник истины
                await _inventorySync.SyncFromServerAsync();

                return PurchaseResult.Ok();

            }
            catch (Exception e)
            {
                return PurchaseResult.Fail(e.Message);
            }
        }

        private static UniTask<ExecuteCloudScriptResult> ExecuteCloudScriptAsync(ExecuteCloudScriptRequest request)
        {
            var tcs = new UniTaskCompletionSource<ExecuteCloudScriptResult>();

            PlayFabClientAPI.ExecuteCloudScript(
                request,
                r =>
                {
                    if (r.Error != null)
                    {
                        var msg = r.Error.Message ?? r.Error.Message ?? "Unknown CloudScript error";
                        tcs.TrySetException(new Exception($"CloudScript error: {msg}"));
                        return;
                    }
                    tcs.TrySetResult(r);
                },
                e => tcs.TrySetException(new Exception(e.GenerateErrorReport()))
            );

            return tcs.Task;
        }
    }
}
