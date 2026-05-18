using Core.Services.Common;

namespace Core.Events
{
    public class UseBoosterEvent : IGameEvent
    {
        public BoosterType boosterType;
        public bool isEmpty;
    }
}