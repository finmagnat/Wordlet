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
        
        private readonly IInventoryService _inventory;
        private readonly ShopCatalog _catalog;
        private readonly InventorySyncService _inventorySync;
        private readonly RewardedAdsService _ads;
        private readonly LocalizationService _localization;

        [Inject]
        public StubShopService(ConfigService configService, IInventoryService inventory, InventorySyncService inventorySync, RewardedAdsService ads, LocalizationService localization)
        {
            _catalog = configService.Shop;
            _inventory = inventory;
            _inventorySync = inventorySync;
            _ads = ads;
            _localization = localization;
        }

        public UniTask InitializeAsync() => UniTask.CompletedTask;

        public UniTask<IReadOnlyList<ShopOfferDto>> GetCatalogAsync()
        {
            var list = _catalog.Offers.Select(o => new ShopOfferDto
            {
                Type = (ShopOfferTypeDto)o.Type,
                ProductId = o.ProductId,
                RewardType = o.RewardType,
                Title = o.Title,
                Description = o.Description,
                CtaText = o.Type == ShopOfferType.IapPack ? o.DebugPriceText : _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyTextLook),
                IsAvailable = o.DebugAvailable, // + позже сюда добавим cooldown/limit для Rewarded
                Rewards = o.Rewards.Select(r => new ShopRewardDto { ItemId = r.ItemId, Amount = r.Amount }).ToList()
            }).ToList();

            return UniTask.FromResult((IReadOnlyList<ShopOfferDto>)list);
        }
        
        public async UniTask<PurchaseResult> ExecuteOfferAsync(ShopOfferDto offer)
        {
            if (offer == null) return PurchaseResult.Fail("Offer is null");

            switch (offer.Type)
            {
                case ShopOfferTypeDto.IapPack:
                    return await PurchaseAsync(offer.ProductId);

                case ShopOfferTypeDto.RewardedAd:
                    // тут позже воткнём лимиты/cooldown
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
