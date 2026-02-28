namespace Core.Events
{
    public class GamePauseChangedEvent : IGameEvent { 
        public readonly bool IsPaused;
        public GamePauseChangedEvent(bool isPaused) => IsPaused = isPaused;
    }
}