using Core.Config;
using UI.Screens;

namespace Core.Events
{
    public class GameScreenStartEvent : IGameEvent
    {
        public GameScreenBase Screen;
        public GameOpponent Opponent;
        public bool AutoStart;
    }
}