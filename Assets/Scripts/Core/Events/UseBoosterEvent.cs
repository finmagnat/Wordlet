using Inventory;

namespace Core.Events
{
    public class UseBoosterEvent : IGameEvent
    {
        public BoosterType boosterType;
    }
}