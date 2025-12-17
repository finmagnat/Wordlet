using Core.Config;
using UI.Screens;

namespace Core.Events
{
    public class GameScreenStartEvent : IGameEvent
    {
        public GameScreen Screen;
        public GameOpponent Opponent;
    }
}