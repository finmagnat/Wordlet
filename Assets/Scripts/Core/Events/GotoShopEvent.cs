using Core.Services.Common;

namespace Core.Events
{
    public class GotoShopEvent : IGameEvent
    {
        public BoosterType BoosterType;
    }
}
