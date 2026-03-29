using System;
using System.Collections.Generic;
using Core.Generated;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(menuName = "Wordlet/Config/Sounds", fileName = "SoundsConfig")]
    public class SoundsConfig : ScriptableObject
    {
        [TextArea]
        public string _ = "Перетащить аудиоклип в поле Clip. Запустить плеймод и проверить звук в игре. После завершения подбора звуков перенести аудио клипы в Addressables.SFX и отключить AudioService._isDebugPlayNoAsync";
        
        public List<SoundData> Sounds => _sounds;
        
        [SerializeField] private List<SoundData> _sounds = new ()
        {
            new SoundData{ Key = AssetKey.sfx_start_new_game.ToString(), Description = "Старт игры" },
            new SoundData{ Key = AssetKey.sfx_letter_put_success.ToString(), Description = "Буква установлена на поле" },
            new SoundData{ Key = AssetKey.sfx_letter_selected.ToString(), Description = "Буква выделена на поле" },
            new SoundData{ Key = AssetKey.sfx_letter_unblinking.ToString(), Description = "Буква на поле мигает (неподсвечена)" },
            new SoundData{ Key = AssetKey.sfx_letter_blinking.ToString(), Description = "Буква на поле мигает (подсвечена)" },
            new SoundData{ Key = AssetKey.sfx_i_made_move.ToString(), Description = "Игрок сделал ход" },
            new SoundData{ Key = AssetKey.sfx_opponent_made_move.ToString(), Description = "Оппонент сделал ход" },
            new SoundData{ Key = AssetKey.sfx_opponent_won.ToString(), Description = "Выиграл оппонент" },
            new SoundData{ Key = AssetKey.sfx_popup_question.ToString(), Description = "Попап с вопросом" },
            new SoundData{ Key = AssetKey.sfx_popup_worning.ToString(), Description = "Отображение попапа 'Совет'" },
            new SoundData{ Key = AssetKey.sfx_opponent_find_word_fail.ToString(), Description = "Оппонент не нашел букву и время вышло (пропуск хода)" },
            new SoundData{ Key = AssetKey.sfx_pause.ToString(), Description = " Пауза (квл/выкл)" },
            new SoundData{ Key = AssetKey.sfx_pass.ToString(), Description = "Игрок пропустил ход" },
            new SoundData{ Key = AssetKey.sfx_draw.ToString(), Description = "Ничья" },
            new SoundData{ Key = AssetKey.sfx_i_won.ToString(), Description = "Выиграл игрок" },
            new SoundData{ Key = AssetKey.sfx_skin_changed.ToString(), Description = "Скин изменился" },
            new SoundData{ Key = AssetKey.sfx_button_click.ToString(), Description = "Клик по кнопке" },
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