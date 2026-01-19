using System.Collections.Generic;
using Core.Data;
using Cysharp.Threading.Tasks;

namespace Core.Services.Shop
{
    public interface IShopService : IService
    {
        UniTask InitializeAsync();
        UniTask<IReadOnlyList<ShopPackDto>> GetCatalogAsync();
        UniTask<PurchaseResult> PurchaseAsync(string productId);
    }
}