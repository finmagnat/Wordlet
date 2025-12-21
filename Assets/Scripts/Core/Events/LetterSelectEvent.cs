using UI.Components;

namespace Core.Events
{
    public struct LetterSelectEvent : IGameEvent
    {
        public SelectableLetter letter;
    }

}