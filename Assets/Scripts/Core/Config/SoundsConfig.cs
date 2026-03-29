using System;
using System.Collections.Generic;
using Core.Generated;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(menuName = "Wordlet/Config/Sounds", fileName = "SoundsConfig")]
    public class SoundsConfig : ScriptableObject
    {
        public static string StartNewGame => AssetKey.sfx_start_new_game.ToString();
        public static string LetterPutSuccess => AssetKey.sfx_letter_put_success.ToString();
        public static string LetterSelected => AssetKey.sfx_letter_selected.ToString();
        public static string LetterUnblinking => AssetKey.sfx_letter_unblinking.ToString();
        public static string LetterBlinking => AssetKey.sfx_letter_blinking.ToString();
        public static string IMadeMove => AssetKey.sfx_i_made_move.ToString();
        public static string OpponentMadeMove => AssetKey.sfx_opponent_made_move.ToString();
        public static string OpponentWon => AssetKey.sfx_opponent_won.ToString();
        public static string PopupQuestion => AssetKey.sfx_popup_question.ToString();
        public static string PopupWarning => AssetKey.sfx_popup_worning.ToString();
        public static string OpponentFindWordFail => AssetKey.sfx_opponent_find_word_fail.ToString();
        public static string Pause => AssetKey.sfx_pause.ToString();
        public static string Pass => AssetKey.sfx_pass.ToString();
        public static string Draw => AssetKey.sfx_draw.ToString();
        public static string IWon => AssetKey.sfx_i_won.ToString();
        public static string SkinChanged => AssetKey.sfx_skin_changed.ToString();
        public static string ButtonClick => AssetKey.sfx_button_click.ToString();
        public static string BoosterFoundWord => AssetKey.sfx_booster_found_word.ToString();
        public static string BoosterNotFoundWord => AssetKey.sfx_booster_not_found_word.ToString();
        public static string BoosterSlowdownLaunch => AssetKey.sfx_booster_slowdown_launch.ToString();
       
        
        [TextArea]
        public string _ = "Перетащить аудиоклип в поле Clip. Запустить плеймод и проверить звук в игре. После завершения подбора звуков перенести аудио клипы в Addressables.SFX и отключить AudioService._isDebugPlayNoAsync";
        
        public List<SoundData> Sounds => _sounds;
        
        [SerializeField] private List<SoundData> _sounds = new ()
        {
            new SoundData{ Key = StartNewGame, Description = "Старт игры" },
            new SoundData{ Key = LetterPutSuccess, Description = "Буква установлена на поле" },
            new SoundData{ Key = LetterSelected, Description = "Буква выделена на поле" },
            new SoundData{ Key = LetterUnblinking, Description = "Буква на поле мигает (неподсвечена)" },
            new SoundData{ Key = LetterBlinking, Description = "Буква на поле мигает (подсвечена)" },
            new SoundData{ Key = IMadeMove, Description = "Игрок сделал ход" },
            new SoundData{ Key = OpponentMadeMove, Description = "Оппонент сделал ход" },
            new SoundData{ Key = OpponentWon, Description = "Выиграл оппонент" },
            new SoundData{ Key = PopupQuestion, Description = "Попап с вопросом" },
            new SoundData{ Key = PopupWarning, Description = "Отображение попапа 'Совет'" },
            new SoundData{ Key = OpponentFindWordFail, Description = "Оппонент не нашел букву и время вышло (пропуск хода)" },
            new SoundData{ Key = Pause, Description = "Пауза (квл/выкл)" },
            new SoundData{ Key = Pass, Description = "Игрок пропустил ход" },
            new SoundData{ Key = Draw, Description = "Ничья" },
            new SoundData{ Key = IWon, Description = "Выиграл игрок" },
            new SoundData{ Key = SkinChanged, Description = "Скин изменился" },
            new SoundData{ Key = ButtonClick, Description = "Клик по кнопке" },
            new SoundData{ Key = BoosterFoundWord, Description = "Бустер 'Буковка' нашел слово" },
            new SoundData{ Key = BoosterNotFoundWord, Description = "Бустер 'Буковка' не нашел слово" },
            new SoundData{ Key = BoosterSlowdownLaunch, Description = "Запуск бустера 'Замедление'" },
        };

        
    }
    
    [Serializable]
    public struct SoundData
    {
        public string Key;
        public string Description;
        public AudioClip Clip;
    }
}