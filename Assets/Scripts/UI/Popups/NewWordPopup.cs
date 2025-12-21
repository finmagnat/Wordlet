using Core.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups
{
    public class NewWordPopup : MessagePopup
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