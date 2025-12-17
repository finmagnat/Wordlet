using Core.Data;
using Core.Services;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Popups
{
    public class MessagePopup : UIPopup
    {
        [Header("UI Elements")]
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        
        [Inject] private LocalizationService _locationService;
        
        private UniTaskCompletionSource<GameExitData> _completionSource;
        
        private MessageBoxData _messageBoxData;

        private void Start()
        {
            _exitButton.onClick.AddListener(async () =>
            {                
                await HideAsync();
                Close();
                _completionSource?.TrySetResult(new GameExitData { Result = PopupResult.Exit });
            });

            _closeButton.onClick.AddListener(async () =>
            {
                await HideAsync();
                Close();
                _completionSource?.TrySetResult(new GameExitData { Result = PopupResult.Close });
            });
            
            SetText("", "");
        }
        
        public override async UniTask ShowAsync()
        {
            _completionSource = new ();
            await base.ShowAsync();
        }
        
        public UniTask<GameExitData> WaitForResultAsync() => _completionSource.Task;
        
        public void SetWindowData(MessageBoxData data) {
            _messageBoxData = data;
            
            SetText(
                _locationService.Get(LocalizationConst.TableUI, "ERROR_MSG_TITLE"), 
                _locationService.Get(LocalizationConst.TableUI, "ERROR_MSG_" + _messageBoxData.Error.ToString().ToUpper()));
        }

        private void SetText(string title, string msg)
        {
            _titleText.text = title;
            _messageText.text = msg;
        }

        private void Close()
        {
            SetText("", "");
            if (_messageBoxData.ExecuteOnClose != null)
                _messageBoxData.ExecuteOnClose();
        }
    }
}