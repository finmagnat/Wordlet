namespace Core.Config
{
    public enum GameOpponent
    {
        AI = 1,
        FRIEND
    }
    
    public enum ComplexityAI
    {
        EASY = 1,
        NORMAL,
        HARD
    }

    public enum ResultGame
    {
        DEAD_HEAT,
        OWNER_WIN,
        OWNER_LOSE
    }

    public enum GameError
    {
        NONE = 0, // Ошибок нет
        NO_SETTED_LETTER, // Не установлена новая буква на поле
        SET_LETTER_NO_SELECTED, // Установленная буква не использована в слове
        WORD_NO_SELECTED, // Не выделено ни одной буквы на поле
        WORD_ALREADY_BEEN // Такое слово уже было в текущем сеансе игры
    }

    public enum PopupType
    {
        MESSAGE_BOX_ERROR = 1,       
        MESSAGE_BOX_WIN,
        MESSAGE_BOX_LOSE,
        MESSAGE_BOX_DEAD_HEAT,
        NEW_GAME,
        NEW_WORD,
        LIDER_BOARD
    }
    
    public enum SkinType
    {
        RED = 1,       
        BLUE
    }
}