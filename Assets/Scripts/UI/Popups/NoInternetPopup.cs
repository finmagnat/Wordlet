using System;
using Core.Data;
using Core.Services;
using Cysharp.Threading.Tasks;
using Zenject;

namespace UI.Popups
{
    public class NoInternetPopup : MessagePopup<MessageBoxData>, INoInternetPopup
    {
        [Inject] private LocalizationService _localization;
        
        private Action _onCheckClicked;

        protected override void Start()
        {
            // Кнопка "Проверить"
            _exitButton.onClick.RemoveAllListeners();
            _exitButton.onClick.AddListener(() => _onCheckClicked?.Invoke());

            // Крестик/закрытие скрываем навсегда
            _closeButton.gameObject.SetActive(false);
        }
        
        public override UniTask PrepareAsync(MessageBoxData data)
        {
            SetWindowData(data);
            return UniTask.CompletedTask;
        }
        
        public void ShowBlocking(Action onCheckClicked)
        {
            _onCheckClicked = onCheckClicked;

            // На всякий случай: если попап реюзается, перекидываем слушатель
            _exitButton.onClick.RemoveAllListeners();
            _exitButton.onClick.AddListener(() => _onCheckClicked?.Invoke());

            _closeButton.gameObject.SetActive(false);
        }

        public async UniTask Hide()
        {
            await HideAsync();
            Close();
            _completionSource?.TrySetResult(new PopupExitData { Result = PopupResult.Exit });
        }
    }
}