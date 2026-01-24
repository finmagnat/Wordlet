using System;
using System.Collections.Generic;
using System.Linq;
using Core.Config;
using Core.Data;
using Cysharp.Threading.Tasks;
using Inventory;
using UnityEngine.Purchasing;
using Zenject;

namespace Core.Services.Shop
{
    public sealed class GooglePlayShopService : IShopService, IStoreListener
    {
    private readonly ShopCatalog _catalog;
    private readonly IInventoryService _inventory; // или твой интерфейс инвентаря

    private IStoreController _controller;
    private IExtensionProvider _extensions;

    private UniTaskCompletionSource<bool> _initTcs;
    private UniTaskCompletionSource<PurchaseResult> _purchaseTcs;
    private string _pendingPurchaseId;

    [Inject]
    public GooglePlayShopService(ShopCatalog catalog, IInventoryService inventory)
    {
        _catalog = catalog;
        _inventory = inventory;
    }

    // --- IShopService ---

    public async UniTask InitializeAsync()
    {
        
    }

    public async UniTask<IReadOnlyList<ShopPackDto>> GetCatalogAsync()
    {
        await EnsureInitializedAsync();

        var list = new List<ShopPackDto>(_catalog.Packs.Count);

        foreach (var p in _catalog.Packs)
        {
            var dto = new ShopPackDto
            {
                ProductId = p.ProductId,
                Title = p.Title,
                Description = p.Description,
                IsAvailable = true,
                PriceText = p.DebugPriceText, // fallback
                Rewards = p.Rewards.Select(r => new ShopRewardDto { ItemId = r.ItemId, Amount = r.Amount }).ToList()
            };

            var product = _controller?.products?.WithID(p.ProductId);
            if (product != null)
            {
                dto.IsAvailable = product.availableToPurchase;
                if (product.metadata != null && !string.IsNullOrWhiteSpace(product.metadata.localizedPriceString))
                    dto.PriceText = product.metadata.localizedPriceString;
            }

            list.Add(dto);
        }

        return list;
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

    // --- Initialization ---

    private UniTask EnsureInitializedAsync()
    {
        if (_controller != null) return UniTask.CompletedTask;

        _initTcs ??= new UniTaskCompletionSource<bool>();

        // Инициализируем ровно один раз
        if (_controller == null && !_initTcs.Task.Status.IsCompleted())
        {
            var module = StandardPurchasingModule.Instance(AppStore.GooglePlay);
            var builder = ConfigurationBuilder.Instance(module);

            // Сейчас все твои паки — consumable
            foreach (var p in _catalog.Packs)
                builder.AddProduct(p.ProductId, ProductType.Consumable);

            UnityPurchasing.Initialize(this, builder);
        }

        return _initTcs.Task.AsUniTask();
    }

    // --- IStoreListener ---

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

    public void OnInitializeFailed(InitializationFailureReason error, string message = null)
    {
        throw new NotImplementedException();
    }

#if UNITY_PURCHASING_4_OR_NEWER
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        _initTcs?.TrySetException(new Exception($"IAP init failed: {error}. {message}"));
        _initTcs = null;
    }
#endif

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        try
        {
            var id = e.purchasedProduct.definition.id;

            // Выдаём награду ТОЛЬКО здесь (а не в момент клика)
            var pack = _catalog.Packs.FirstOrDefault(x => x.ProductId == id);
            if (pack != null)
            {
                foreach (var r in pack.Rewards)
                {
                    // ВАЖНО: начислять, а не "set"
                    _inventory.Add(r.ItemId, r.Amount);
                }
            }

            if (_purchaseTcs != null && _pendingPurchaseId == id)
            {
                _purchaseTcs.TrySetResult(PurchaseResult.Ok());
                _purchaseTcs = null;
                _pendingPurchaseId = null;
            }
        }
        catch (Exception ex)
        {
            if (_purchaseTcs != null)
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
        if (_purchaseTcs != null)
        {
            _purchaseTcs.TrySetResult(PurchaseResult.Fail(failureReason.ToString()));
            _purchaseTcs = null;
            _pendingPurchaseId = null;
        }
    }
}
}