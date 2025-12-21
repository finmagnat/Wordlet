using Core.Config;

namespace Core.Events
{
    public class PlayerErrorEvent : IGameEvent
    {
        public GameError GameError;
    }
}