using Core.Config;
using Core.Dictionary;
using Cysharp.Threading.Tasks;
using Game.Logic;

namespace Game.AI
{
    public interface IAIAlgorithmAsync
    {
        /// <summary>
        /// Получить слово.
        /// </summary>
        /// <param name="settings">Настройки сложности ИИ</param>
        /// <param name="wordsFieldManager">Список элементов поля</param>
        /// <param name="dictionaryService">Словарь</param>
        /// <returns>Успех</returns>
        UniTask<AIWordResult> GetWordAsync(
            ComplexityAISettings settings,
            WordsFieldManager wordsFieldManager,
            DictionaryService dictionaryService);
        
        /// <summary>
        /// Время хода закончилось
        /// </summary>
        public void TimeExpired();
    }
}