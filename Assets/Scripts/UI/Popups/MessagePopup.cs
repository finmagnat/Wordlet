using Core.Data;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups
{
    public class MessagePopup : UIPopup
    {
        [Header("UI Elements")]
        [SerializeField] protected Button _exitButton;
        [SerializeField] protected Button _closeButton;
        [SerializeField] protected TextMeshProUGUI _titleText;
        [SerializeField] protected TextMeshProUGUI _messageText;
        
        protected UniTaskCompletionSource<PopupExitData> _completionSource;
        
        protected MessageBoxData _messageBoxData;

        protected virtual void Start()
        {
            _exitButton.onClick.AddListener(async () =>
            {                
                await HideAsync();
                Close();
                _completionSource?.TrySetResult(new PopupExitData { Result = PopupResult.Exit });
            });

            _closeButton.onClick.AddListener(async () =>
            {
                await HideAsync();
                Close();
                _completionSource?.TrySetResult(new PopupExitData { Result = PopupResult.Close });
            });
        }
        
        public override async UniTask ShowAsync()
        {
            _completionSource = new ();
            await base.ShowAsync();
        }
        
        public UniTask<PopupExitData> WaitForResultAsync() => _completionSource.Task;
        
        public virtual void SetWindowData(MessageBoxData data) {
            _messageBoxData = data;
        }

        public void SetText(string title, string msg)
        {
            _titleText.text = title;
            _messageText.text = msg;
        }

        protected virtual void Close()
        {
            if (_messageBoxData != null && _messageBoxData.ExecuteOnClose != null)
                _messageBoxData.ExecuteOnClose();
        }
    }
}