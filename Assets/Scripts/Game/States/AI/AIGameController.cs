using Core.Config;
using Core.DataDictionary;
using Cysharp.Threading.Tasks;
using Game.Logic;

namespace Game.AI
{
    public class AIGameController : IState
    {
        private WordsFieldManager _wordsFieldManager;
        private DictionaryService _dictionaryService;

        private readonly SmartWordAlgorithmAsync _algorithm = new();

        public void Destroy() { }

        internal void Init(WordsFieldManager wordsFieldManager, DictionaryService dictionaryService)
        {
            _wordsFieldManager = wordsFieldManager;
            _dictionaryService = dictionaryService;
        }

        /// <summary>
        /// Универсальный поиск слова (для хода ИИ, подсказок, бустеров).
        /// Важно: применяет найденный ход к полю (SetLetter/Highlight) внутри алгоритма.
        /// </summary>
        internal UniTask<AIWordResult> FindWordAsync(ComplexityAISettings settings)
        {
            return _algorithm.GetWordAsync(settings, _wordsFieldManager, _dictionaryService);
        }

        /// <summary>
        /// Прервать поиск слова (не важно почему: таймер, смена сцены, отмена игроком и т.п.).
        /// </summary>
        internal void AbortSearch()
        {
            _algorithm.Cancel(); // можно переименовать в алгоритме в Cancel()
        }
    }
}