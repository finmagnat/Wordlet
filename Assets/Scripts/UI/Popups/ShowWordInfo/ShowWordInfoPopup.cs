using Core.Data;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups
{
    public class ShowWordInfoPopup : UIPopup<ShowWordInfoWindowEventData>
    {
        [SerializeField] protected Button _closeButton;
        [SerializeField] protected TextMeshProUGUI _wordText;
        [SerializeField] protected TextMeshProUGUI _infoText;
        [SerializeField] protected TextMeshProUGUI _cooldownText;
        [SerializeField] protected Button _reportButton;
        
        private ShowWordInfoWindowEventData _eventData;
        protected UniTaskCompletionSource<PopupExitData> _completionSource;
        
        protected void Start()
        {
            _cooldownText.text = "";
            _cooldownText.gameObject.SetActive(false);
            
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(OnCloseClicked);
            }
            
            _reportButton.onClick.AddListener(async () =>
            {                
                await HideAsync();
                Close();
                _completionSource?.TrySetResult(new PopupExitData { Result = PopupResult.SaveAndExit });
            });
        }
        
        public override UniTask PrepareAsync(ShowWordInfoWindowEventData data)
        {
            _wordText.text = data.newWord;
            // _infoText.text = ""; // TODO: получить значение слова для текущей локализации из базы данных и отобразить. 
            return UniTask.CompletedTask;
        }
        
        public override async UniTask ShowAsync()
        {
            _completionSource = new UniTaskCompletionSource<PopupExitData>();
            await base.ShowAsync();
        }
        
        public void SetSubmitState(bool canSubmit, string message)
        {
            _reportButton.interactable = canSubmit;

            if (_cooldownText == null)
                return;

            bool showMessage = !string.IsNullOrWhiteSpace(message);
            _cooldownText.gameObject.SetActive(showMessage);

            if (showMessage)
                _cooldownText.text = message;
        }
        
        protected virtual void OnCloseClicked()
        {
            HandleButtonClick(PopupResult.Close).Forget();
        }

        protected virtual async UniTaskVoid HandleButtonClick(PopupResult result)
        {
            await HideAsync();
            Close();
            _completionSource?.TrySetResult(new PopupExitData { Result = result });
        }
        
        private void Close()
        {
            _wordText.text = "";
            _cooldownText.text = "";
            _cooldownText.gameObject.SetActive(false);
        }
    }
}