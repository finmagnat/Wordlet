using Core.Data;
using Core.Services.Shop;
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
        
        [Header("UI Elements")]
        [SerializeField] protected Button _exitButton;
        [SerializeField] private ShopPackItemView _itemPrefab;
        [SerializeField] protected Transform _contentRoot;
        
        protected UniTaskCompletionSource<PopupExitData> _completionSource;
        
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
        
        public virtual void SetWindowData(MessageBoxData data) {
        }
        
        private async UniTask InitializeAsync()
        {
            // очистка
            for (int i = _contentRoot.childCount - 1; i >= 0; i--)
                Destroy(_contentRoot.GetChild(i).gameObject);

            var catalog = await _shop.GetCatalogAsync();

            foreach (var pack in catalog)
            {
                var view = _container.InstantiatePrefabForComponent<ShopPackItemView>(_itemPrefab, _contentRoot);
                view.Bind(pack, OnBuyClicked);
            }
        }

        private async void OnBuyClicked(string productId)
        {
            var result = await _shop.PurchaseAsync(productId);
            if (!result.Success)
            {
                Debug.LogWarning($"Purchase failed: {result.Error}");
                // позже: показать попап
                return;
            }

            Debug.Log($"Purchased: {productId}");
            // позже: pop-up “Успешно”, обновить инвентарь UI и т.п.
        }
    }
}