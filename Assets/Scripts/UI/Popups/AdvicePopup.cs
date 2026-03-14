using Core.Data;
using Core.Services;
using Zenject;

namespace UI.Popups
{
    public class AdvicePopup : MessagePopup
    {
        [Inject] private LocalizationService _localization;
        
        protected override void Start()
        {
            base.Start();
            SetText("", "");
        }
        
        public override void SetWindowData(MessageBoxData data) {
            base.SetWindowData(data);
            
            SetText(
                _localization.Get(LocalizationConst.TableUI, "ERROR_MSG_TITLE"), 
                _localization.Get(LocalizationConst.TableUI, "ERROR_MSG_" + _messageBoxData.Error.ToString().ToUpper()));
        }
        
        protected override void Close()
        {
            SetText("", "");
            base.Close();
        }

    }
}