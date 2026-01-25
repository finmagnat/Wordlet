using System;
using System.Collections.Generic;
using System.Linq;
using Core.Config;
using Core.Data;
using Cysharp.Threading.Tasks;
using Inventory;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Purchasing;
using Zenject;

namespace Core.Services.Shop
{
    public sealed class GooglePlayShopService : IShopService, IStoreListener
    {
        private readonly ShopCatalog _catalog;
        private readonly IInventoryService _inventory; // или твой интерфейс инвентаря
        private readonly InventorySyncService _inventorySync;

        private IStoreController _controller;
        private IExtensionProvider _extensions;

        private UniTaskCompletionSource<bool> _initTcs;
        private UniTaskCompletionSource<PurchaseResult> _purchaseTcs;
        private string _pendingPurchaseId;

        [Inject]
        public GooglePlayShopService(ShopCatalog catalog, IInventoryService inventory,
            InventorySyncService inventorySync)
        {
            _catalog = catalog;
            _inventory = inventory;
            _inventorySync = inventorySync;
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
            var product = e.purchasedProduct;
            var productId = product.definition.id;

            // Запускаем async-валидацию и выдачу награды на сервере
            ValidateAndGrantAsync(product).Forget();

            // ВАЖНО: Pending — пока не подтвердим покупку после сервера
            return PurchaseProcessingResult.Pending;
        }

        private async UniTaskVoid ValidateAndGrantAsync(Product product)
        {
            try
            {
                // 1) Достаём receipt+signature для Google Play
                // Unity IAP receipt для Google: JSON, внутри payload содержит json + signature.
                // Чтобы не городить парсер на 200 строк — вытащим минимально нужное.
                var (receiptJson, signature, transactionId) = GoogleReceiptExtractor.Extract(product);
                
                Debug.Log($"IAP receipt extracted for {product.definition.id}");

                // 2) Цена/валюта (PlayFab ожидает minor units - обычно "центы")
                var currency = product.metadata?.isoCurrencyCode ?? "USD";
                var priceMinor = (int)Math.Round((double)product.metadata.localizedPrice * 100.0);

                // 3) Server-side validate + grant через CloudScript
                var exec = await ExecuteCloudScriptAsync(new ExecuteCloudScriptRequest
                {
                    FunctionName = "ProcessGooglePlayPurchase",
                    FunctionParameter = new Dictionary<string, object>
                    {
                        { "productId", product.definition.id },
                        { "receiptJson", receiptJson },
                        { "signature", signature },
                        { "currencyCode", currency },
                        { "purchasePrice", priceMinor },
                        { "transactionId", transactionId }
                    },
                    GeneratePlayStreamEvent = true
                });

                // 4) Если сервер ок — подтверждаем покупку в IAP
                _controller.ConfirmPendingPurchase(product);

                // 5) Обновляем локальный инвентарь из PlayFab (источник истины)
                await _inventorySync.SyncFromServerAsync();

                // 6) Разрешаем UI
                if (_purchaseTcs != null && _pendingPurchaseId == product.definition.id)
                {
                    _purchaseTcs.TrySetResult(PurchaseResult.Ok());
                    _purchaseTcs = null;
                    _pendingPurchaseId = null;
                }
            }
            catch (Exception ex)
            {
                // Если сервер не подтвердил — НЕ ConfirmPendingPurchase.
                // Unity IAP повторит ProcessPurchase позже (при следующем запуске), пока ты не подтвердишь.
                if (_purchaseTcs != null)
                {
                    _purchaseTcs.TrySetResult(PurchaseResult.Fail(ex.Message));
                    _purchaseTcs = null;
                    _pendingPurchaseId = null;
                }
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