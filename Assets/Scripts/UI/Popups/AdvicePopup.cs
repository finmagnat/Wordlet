using Core.Data;
using Core.Services;
using Zenject;

namespace UI.Popups
{
    public class AdvicePopup : MessagePopup
    {
        [Inject] private LocalizationService _locationService;
        
        protected override void Start()
        {
            base.Start();
            SetText("", "");
        }
        
        public override void SetWindowData(MessageBoxData data) {
            base.SetWindowData(data);
            
            SetText(
                _locationService.Get(LocalizationConst.TableUI, "ERROR_MSG_TITLE"), 
                _locationService.Get(LocalizationConst.TableUI, "ERROR_MSG_" + _messageBoxData.Error.ToString().ToUpper()));
        }
        
        protected override void Close()
        {
            SetText("", "");
            base.Close();
        }

    }
}