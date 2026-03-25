using Core.Data;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups
{
    public abstract class MessagePopup<TPayload> : UIPopup<TPayload>
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
            if (_exitButton != null)
            {
                _exitButton.onClick.AddListener(OnExitClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(OnCloseClicked);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_exitButton != null)
            {
                _exitButton.onClick.RemoveListener(OnExitClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(OnCloseClicked);
            }
        }

        private void OnExitClicked()
        {
            HandleButtonClick(PopupResult.Exit).Forget();
        }

        private void OnCloseClicked()
        {
            HandleButtonClick(PopupResult.Close).Forget();
        }

        private async UniTaskVoid HandleButtonClick(PopupResult result)
        {
            await HideAsync();
            Close();
            _completionSource?.TrySetResult(new PopupExitData { Result = result });
        }

        public override async UniTask ShowAsync()
        {
            _completionSource = new UniTaskCompletionSource<PopupExitData>();
            await base.ShowAsync();
        }

        public UniTask<PopupExitData> WaitForResultAsync() => _completionSource.Task;

        public virtual void SetWindowData(MessageBoxData data)
        {
            _messageBoxData = data;
        }

        public void SetText(string title, string msg)
        {
            if (_titleText != null)
                _titleText.text = title;

            if (_messageText != null)
                _messageText.text = msg;
        }

        protected virtual void Close()
        {
            _messageBoxData?.ExecuteOnClose?.Invoke();
        }
    }
}