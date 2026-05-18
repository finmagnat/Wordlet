using UI.Components;

namespace Core.Events
{
    public class CellSelectSuccessEvent : IGameEvent
    {
        public SelectableLetter letter;
        public bool isEraserSuccess;
        public string erasedLetter;
        public bool isSwapSuccess;
        public int swapFirstIndex;
        public int swapSecondIndex;
        public string[] boardBeforeSwap;
        public string[] boardAfterSwap;
    }

}
