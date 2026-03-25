using Core.Data;
using Core.Services.Shop;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UI.Popups
{
    public class PurchaseConfirmationPopup : UIPopup
    {
        [Header("UI Elements")]
        [SerializeField] private ShopItemView _itemPrefab;
        [SerializeField] private Transform _contentRoot;
        
        protected UniTaskCompletionSource<PopupExitData> _completionSource;
        
        private bool _isInitialized;

        private void Start()
        {
            Clear();
        }
        
        public UniTask<PopupExitData> WaitForResultAsync() => _completionSource.Task;
        
        public virtual void SetWindowData(ShopOfferDto offer) {
            Bind(offer);
        }
        
        public void OnClose()
        {
            Close().Forget();
        }
        
        private void Bind(ShopOfferDto dto)
        {
            foreach (var item in dto.Rewards)
            {
                var view = Instantiate(_itemPrefab, _contentRoot);
                view.Bind(item);
            }
        }
        
        private async UniTask Close()
        {
            await HideAsync();
            Clear();
            _completionSource?.TrySetResult(new PopupExitData { Result = PopupResult.Exit });
        }

        private void Clear()
        {
            // очистка наград
            for (int i = _contentRoot.childCount - 1; i >= 0; i--)
                Destroy(_contentRoot.GetChild(i).gameObject);
        }
    }
}