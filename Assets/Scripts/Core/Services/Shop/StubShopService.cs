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

        [Inject]
        public StubShopService(ConfigService configService, IInventoryService inventory, InventorySyncService inventorySync)
        {
            _catalog = configService.Shop;
            _inventory = inventory;
            _inventorySync = inventorySync;
        }

        public UniTask InitializeAsync() => UniTask.CompletedTask;

        public UniTask<IReadOnlyList<ShopPackDto>> GetCatalogAsync()
        {
            var list = _catalog.Packs.Select(p => new ShopPackDto
            {
                ProductId = p.ProductId,
                Title = p.Title,
                Description = p.Description,
                PriceText = p.DebugPriceText,
                IsAvailable = p.DebugAvailable,
                Rewards = p.Rewards.Select(r => new ShopRewardDto { ItemId = r.ItemId, Amount = r.Amount }).ToList()
            }).ToList();

            return UniTask.FromResult((IReadOnlyList<ShopPackDto>)list);
        }

        public async UniTask<PurchaseResult> PurchaseAsync(string productId)
        {
            var pack = _catalog.Packs.FirstOrDefault(p => p.ProductId == productId);
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
