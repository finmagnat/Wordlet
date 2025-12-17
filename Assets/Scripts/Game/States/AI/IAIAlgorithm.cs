using System.Collections.Generic;
using Core.Config;
using Core.Dictionary;
using Game.Logic;
using UI.Components;

namespace Game.AI
{
    public interface IAIAlgorithm
    {
        /// <summary>
        /// Получить слово рекомендованной длины.
        /// </summary>
        /// <param name="word">Найденное слово</param>
        /// <param name="settings">Настройки сложности ИИ</param>
        /// <param name="wordsFieldManager">Список элементов поля</param>
        /// <param name="dictionaryService">Словарь</param>
        /// <returns>Успех</returns>
        public bool GetWord(out string word, ComplexityAISettings settings, WordsFieldManager wordsFieldManager, DictionaryService dictionaryService);
        
        /// <summary>
        /// Время хода закончилось
        /// </summary>
        public void TimeExpired();
    }
}