using Core.Events;

namespace UI.Components
{
    public class StartPopupBoosterUI : BoosterUI
    {
        public void OnClick()
        {
            if (IsEmpty)
                EventBus.Raise(new GotoShopEvent());
        }
    }
}