using Core.Data;

namespace Core.Events
{
    public struct GameSetupConfirmedEvent : IGameEvent
    {
        public readonly GameSetupData Data;

        public GameSetupConfirmedEvent(GameSetupData data)
        {
            Data = data;
        }
    }

}