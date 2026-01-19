using System.Collections.Generic;
using System.Linq;
using Core.Config;
using Core.Data;
using Cysharp.Threading.Tasks;
using Inventory;
using Zenject;

namespace Core.Services.Shop
{
    public sealed class StubShopService : IShopService
    {
        private readonly IInventoryService _inventory;
        private readonly ShopCatalog _catalog;

        [Inject]
        public StubShopService(ConfigService configService, IInventoryService inventory)
        {
            _catalog = configService.Shop;
            _inventory = inventory;
        }
        
        public async UniTask InitializeAsync()
        {
            
        }

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

        public UniTask<PurchaseResult> PurchaseAsync(string productId)
        {
            var pack = _catalog.Packs.FirstOrDefault(p => p.ProductId == productId);
            if (pack == null) return UniTask.FromResult(PurchaseResult.Fail("Pack not found"));
            if (!pack.DebugAvailable) return UniTask.FromResult(PurchaseResult.Fail("Not available"));

            foreach (var r in pack.Rewards)
                _inventory.SetBoosterCount(r.ItemId, r.Amount, true);

            return UniTask.FromResult(PurchaseResult.Ok());
        }
    }
}