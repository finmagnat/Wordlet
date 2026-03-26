using Core.Data;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups
{
    public class MissingWordPopup : MessagePopup<NewWordWindowEventData>
    {
        [SerializeField] protected TextMeshProUGUI _newWordText;
        [SerializeField] protected TextMeshProUGUI _cooldownText;
        [SerializeField] protected Button _yesButton;
        
        private NewWordWindowEventData _eventData;
        
        protected override void Start()
        {
            base.Start();
            
            _cooldownText.text = "";
            _cooldownText.gameObject.SetActive(false);
            
            _yesButton.onClick.AddListener(async () =>
            {                
                await HideAsync();
                Close();
                _completionSource?.TrySetResult(new PopupExitData { Result = PopupResult.SaveAndExit });
            });
        }
        
        public override UniTask PrepareAsync(NewWordWindowEventData data)
        {
            _newWordText.text = data.newWord;
            return UniTask.CompletedTask;
        }
        
        public void SetSubmitState(bool canSubmit, string message)
        {
            _yesButton.interactable = canSubmit;

            if (_cooldownText == null)
                return;

            bool showMessage = !string.IsNullOrWhiteSpace(message);
            _cooldownText.gameObject.SetActive(showMessage);

            if (showMessage)
                _cooldownText.text = message;
        }
        
        protected override void Close()
        {
            _newWordText.text = "";
            _cooldownText.text = "";
            _cooldownText.gameObject.SetActive(false);
            
            base.Close();
        }
    }
}