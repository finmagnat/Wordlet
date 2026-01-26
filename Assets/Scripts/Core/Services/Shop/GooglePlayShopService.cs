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
    /// MVP: покупка и выдача наград на клиенте (Unity IAP).
    /// Опционально: пушим награды на сервер (PlayFab AddBooster) и делаем SyncFromServerAsync.
    /// </summary>
    public sealed class GooglePlayShopService : IShopService, IStoreListener
    {
        private readonly ShopCatalog _catalog;
        private readonly IInventoryService _inventory;
        private readonly InventorySyncService _inventorySync;

        private IStoreController _controller;
        private IExtensionProvider _extensions;

        private UniTaskCompletionSource<bool> _initTcs;
        private bool _initStarted;

        private UniTaskCompletionSource<PurchaseResult> _purchaseTcs;
        private string _pendingPurchaseId;

        // TODO: вынеси в конфиг (ShopCatalog/ConfigService), если хочешь.
        // MVP: выключено по умолчанию, чтобы покупки работали без PlayFab.
        private bool EnablePurchasePushToServer => _catalog.EnablePurchasePushToServer;

        [Inject]
        public GooglePlayShopService(ShopCatalog catalog, IInventoryService inventory, InventorySyncService inventorySync)
        {
            _catalog = catalog;
            _inventory = inventory;
            _inventorySync = inventorySync;
        }

        // -------- IShopService --------

        public UniTask InitializeAsync() => UniTask.CompletedTask;

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

        // -------- Unity IAP init --------

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

                // Все твои паки — расходники
                foreach (var p in _catalog.Packs)
                    builder.AddProduct(p.ProductId, ProductType.Consumable);

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

        // Держим обе сигнатуры, чтобы не зависеть от версии Unity IAP
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
                // 1) Начисляем награду локально по ShopCatalog
                var pack = _catalog.Packs.FirstOrDefault(p => p.ProductId == productId);
                if (pack == null)
                {
                    Debug.LogError($"[IAP] Pack not found in ShopCatalog: {productId}. Confirming purchase to avoid stuck.");
                }
                else
                {
                    foreach (var reward in pack.Rewards)
                    {
                        // reward.ItemId — это BoosterType (enum) у тебя в YAML: 0/1
                        _inventory.Add((BoosterType)reward.ItemId, reward.Amount);
                    }
                }

                // 2) Подтверждаем покупку (ACK/consume) — чтобы не было автоотмен и "already owned"
                _controller.ConfirmPendingPurchase(product);

                // 3) Опционально пушим награды на сервер (без валидации чека) и ресинкаемся
                if (EnablePurchasePushToServer && pack != null)
                {
                    PushPurchaseToServerAsync(pack).Forget();
                }

                // 4) Разрешаем UI (успех)
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

                // Даже при ошибке лучше подтвердить покупку, чтобы не оставлять ее "owned"/pending.
                // Награду можно не выдавать (или выдать частично), но магазин не должен ломаться.
                try { _controller.ConfirmPendingPurchase(product); } catch { /* ignore */ }

                if (_purchaseTcs != null && _pendingPurchaseId == productId)
                {
                    _purchaseTcs.TrySetResult(PurchaseResult.Fail(ex.Message));
                    _purchaseTcs = null;
                    _pendingPurchaseId = null;
                }
            }

            // В MVP мы завершаем покупку сразу.
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

        private async UniTaskVoid PushPurchaseToServerAsync(ShopPackConfig pack)
        {
            try
            {
                // Пушим именно "добавление" бустеров на сервер.
                foreach (var reward in pack.Rewards)
                {
                    await _inventorySync.GrantBoosterAsync((BoosterType)reward.ItemId, reward.Amount);
                }

                // После пуша тянем сервер -> клиент (источник истины)
                await _inventorySync.SyncFromServerAsync();

                Debug.Log($"[IAP] Purchase pushed to server and synced: {pack.ProductId}");
            }
            catch (Exception ex)
            {
                // MVP: покупка считается успешной локально, даже если серверный пуш упал.
                Debug.LogError($"[IAP] PushPurchaseToServer failed for {pack.ProductId}: {ex}");
            }
        }
    }
}
