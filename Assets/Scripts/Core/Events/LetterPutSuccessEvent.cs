namespace Core.Events
{
    public class LetterPutSuccessEvent : IGameEvent
    {
        public string letter;
        public int index;
    }
}
