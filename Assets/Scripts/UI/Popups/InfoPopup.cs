using Core.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups
{
    public class InfoPopup : UIPopup
    {
        [Header("UI Elements")]
        [SerializeField] protected Button _exitButton;
        
        protected UniTaskCompletionSource<PopupExitData> _completionSource;
        
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
            await base.ShowAsync();
        }
        
        public UniTask<PopupExitData> WaitForResultAsync() => _completionSource.Task;
        
        public virtual void SetWindowData(MessageBoxData data) {
        }
    }
}