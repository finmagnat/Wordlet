namespace Core.Events
{
    public class CellSelectCancelEvent : IGameEvent
    {
        public int index;
        public bool keepKeyboardOpen;
    }

}
