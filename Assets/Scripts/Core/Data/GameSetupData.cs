using Core.Config;

namespace Core.Data
{
    public class GameSetupData
    {
        public PopupResult Result;   // Play / Cancel / Close
        public ComplexityAI Difficulty; // 0=Легко, 1=Средне, 2=Сложно
        public int TurnTime;         // Время хода в секундах
    }
}