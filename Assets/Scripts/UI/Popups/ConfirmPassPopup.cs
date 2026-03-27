using Core.Config;
using Core.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups
{
    public class ConfirmPassPopup : MessagePopup<MessageBoxData>
    {
        [SerializeField] private Button _yesButton;
        [SerializeField] private Toggle _toggleDontShowAgain;
        
        private NewWordWindowEventData _eventData;
        
        protected override void Start()
        {
            base.Start();
            
            _closeButton.gameObject.SetActive(false);
            
            _yesButton.onClick.AddListener(async () =>
            {                
                await HideAsync();
                Close();

                TrySaveToggleState();

                _completionSource?.TrySetResult(new PopupExitData { Result = PopupResult.Confirm });
            });
        }
        
        public override UniTask PrepareAsync(MessageBoxData data)
        {
            return UniTask.CompletedTask;
        }
        
        protected override void OnExitClicked()
        {
            TrySaveToggleState();
            base.OnExitClicked();
        }

        private void TrySaveToggleState()
        {
            if (_toggleDontShowAgain.isOn)
            {
                PlayerPrefs.SetInt(PlayerPrefsKey.ConfirmPassDontShowAgainKey, 1);
                PlayerPrefs.Save();
            }
        }
        
    }
}