using Core.Data;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups
{
    public class NewWordPopup : MessagePopup<MessageBoxData>
    {
        [SerializeField] protected TextMeshProUGUI _newWordText;
        [SerializeField] protected Button _yesButton;
        
        private NewWordWindowEventData _eventData;
        
        protected override void Start()
        {
            base.Start();
            
            _newWordText.text = "";
            
            _yesButton.onClick.AddListener(async () =>
            {                
                await HideAsync();
                Close();
                _completionSource?.TrySetResult(new PopupExitData { Result = PopupResult.SaveAndExit });
            });
        }
        
        public override UniTask PrepareAsync(MessageBoxData data)
        {
            SetWindowData(data);
            return UniTask.CompletedTask;
        }
        
        public void SetWindowData(string newWord)
        {
            _newWordText.text = newWord;
        }
        
        protected override void Close()
        {
            _newWordText.text = "";
            base.Close();
        }

    }
}