namespace Core.Audio
{
    public class Sounds
    {
        public const string SoundSfx_StartNewGame = "sfx_start_new_game"; // Старт игры
        public const string SoundSfx_LetterPutSuccess = "sfx_letter_put_success"; // Буква установлена на поле
        public const string SoundSfx_LetterSelected = "sfx_letter_selected"; // Буква выделена на поле
        public const string SoundSfx_LetterBlinking = "sfx_letter_blinking"; // Буква на поле мигает (подсвечена)
        public const string SoundSfx_LetterUnblinking = "sfx_letter_unblinking"; // Буква на поле мигает (неподсвечена)
        public const string SoundSfx_PopupWorning = "sfx_popup_worning"; // Отображение попапа "Совет"
        public const string SoundSfx_Pass = "sfx_pass"; // Игрок пропустил ход
        public const string SoundSfx_OpponentFindWordFail = "sfx_opponent_find_word_fail"; // Оппонент не нашел букву и время вышло (пропуск хода) 
        public const string SoundSfx_PopupQuestion = "sfx_popup_question"; // Попап с вопросом
        public const string SoundSfx_Pause = "sfx_pause"; // Пауза (квл/выкл)
        public const string SoundSfx_IMadeMove = "sfx_i_made_move"; // Игрок сделал ход
        public const string SoundSfx_OpponentMadeMove = "sfx_opponent_made_move"; // Оппонент сделал ход 
        public const string SoundSfx_SkinChanged = "sfx_skin_changed"; // Скин изменился
        public const string SoundSfx_OpponentWon = "sfx_opponent_won"; // Выиграл оппонент
        public const string SoundSfx_IWon = "sfx_i_won"; // Выиграл игрок
        public const string SoundSfx_Draw = "sfx_draw"; // Ничья
    }
}