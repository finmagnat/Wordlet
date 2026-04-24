using Core.Config;
using Core.Data;
using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System.Collections.Generic;

namespace UI.Popups
{
    public class ConfirmPassPopup : MessagePopup<MessageBoxData>
    {
        [SerializeField] private Button _yesButton;
        [SerializeField] private Toggle _toggleDontShowAgain;

        [Inject] private AnalyticsService _analytics;
        
        private NewWordWindowEventData _eventData;
        
        protected override void Start()
        {
            base.Start();
            
            _closeButton.gameObject.SetActive(false);
            
            _yesButton.onClick.AddListener(async () =>
            {
                _analytics.TrackEvent(AnalyticsEvents.Navigation.YesConfirmPassPopupClicked, GetAnalyticsParams());
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

        public override async UniTask ShowAsync()
        {
            await base.ShowAsync();
            _analytics.TrackEvent(AnalyticsEvents.Navigation.ConfirmPassPopupShown);
        }
        
        protected override void OnExitClicked()
        {
            _analytics.TrackEvent(AnalyticsEvents.Navigation.NoConfirmPassPopupClicked);
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

        private Dictionary<string, object> GetAnalyticsParams()
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.DontShow] = _toggleDontShowAgain != null && _toggleDontShowAgain.isOn
            };
        }
        
    }
}
