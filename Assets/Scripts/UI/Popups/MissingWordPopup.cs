using Core.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups
{
    public class MissingWordPopup : MessagePopup
    {
        [SerializeField] protected TextMeshProUGUI _newWordText;
        
        private NewWordWindowEventData _eventData;
        
        protected override void Start()
        {
            base.Start();
            
            _newWordText.text = "";
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