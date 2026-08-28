using Core.Data;
using Core.Services;
using Cysharp.Threading.Tasks;
using Zenject;
using System.Collections.Generic;
using Core.Config;

namespace UI.Popups
{
    public class AdvicePopup : MessagePopup<MessageBoxData>
    {
        [Inject] private LocalizationService _localization;
        [Inject] private AnalyticsService _analytics;
        
        public override UniTask PrepareAsync(MessageBoxData data)
        {
            SetWindowData(data);
            return UniTask.CompletedTask;
        }

        public override async UniTask ShowAsync()
        {
            await base.ShowAsync();
            _analytics.TrackEvent(AnalyticsEvents.Navigation.AdvicePopupShown, GetAnalyticsParams());
        }
        
        public override void SetWindowData(MessageBoxData data) {
            base.SetWindowData(data);

            string key = data.Error switch
            {
                GameError.NoLetterInstalled => LocalizationConst.KeyErrorMsgNoLetterInstalled,
                GameError.SetLetterNoSelected => LocalizationConst.KeyErrorMsgSetLetterNoSelected,
                GameError.WordNoSelected => LocalizationConst.KeyErrorMsgWordNoSelected,
                GameError.WordAlreadyBeen => LocalizationConst.KeyErrorMsgWordAlreadyBeen,
                _  => ""
            };
            
            SetText(
                _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyErrorMsgTitle), 
                _localization.Get(LocalizationConst.TableUI, key));
        }
        
        protected override void Close()
        {
            SetText("", "");
            base.Close();
        }

        protected override void OnCloseClicked()
        {
            _analytics.TrackEvent(AnalyticsEvents.Navigation.CloseAdvicePopupClicked);
            base.OnCloseClicked();
        }

        protected override void OnExitClicked()
        {
            _analytics.TrackEvent(AnalyticsEvents.Navigation.OkAdvicePopupClicked);
            base.OnExitClicked();
        }

        private Dictionary<string, object> GetAnalyticsParams()
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Error] = _messageBoxData?.Error.ToString() ?? string.Empty
            };
        }

    }
}
