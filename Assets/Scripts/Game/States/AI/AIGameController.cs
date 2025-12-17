using System.Collections.Generic;
using Core.Config;
using Core.Dictionary;
using Core.Events;
using Game.Logic;

namespace Game.AI
{
    /*
     * Локальный ИИ использует тот же API, что и PvP.
       [Опционально] Добавляем систему “псевдо-оффлайна”: если Firebase недоступен, матч переключается на ИИ.
     */
    
    /// <summary>
    /// Контроллер алгоритмов для ИИ.
    /// 
    /// Алгоритмы регистрируются в массиве _algoritms и поочереди вызываются до тех пор, 
    /// пока не будет подобрано и успешно вставлено слово.
    /// В случае успеха публикуется событие OpponentFindWordEvent, 
    /// либо OpponentFindWordFailEvent в случае провала.
    /// </summary>
    public class AIGameController : IState
    {
        private ComplexityAISettings _settings; // Настройки текущего уровня ИИ
        private WordsFieldManager _wordsFieldManager;
        private DictionaryService _dictionaryService;
        private bool _isPlay;

        // Регистрация алгоритмов
        private readonly IAIAlgorithm[] _algorithms = { 
            new FirstLastCharacter() 
        };

        public AIGameController()
        {
            EventBus.Subscribe<TimeExpiredEvent>(OnTimeExpired);
        }

        public void Destroy()
        {
            EventBus.Unsubscribe<TimeExpiredEvent>(OnTimeExpired);
        }

        internal void Init(WordsFieldManager wordsFieldManager, DictionaryService dictionaryService)
        {
            _wordsFieldManager = wordsFieldManager;
            _dictionaryService = dictionaryService;
        }

        internal void SetSettings(ComplexityAISettings settings)
        {
            _settings = settings;
        }

        internal void Play()
        {
            string word;
            _isPlay = true;
            foreach (var algoritm in _algorithms)
            {
                if (algoritm.GetWord(out word, _settings, _wordsFieldManager, _dictionaryService))
                {
                    _isPlay = false;
                    EventBus.Raise(new OpponentFindWordEvent { word = word });
                    return;
                }
            }
            _isPlay = false;
            
            EventBus.Raise(new OpponentFindWordFailEvent());
        }
        void OnTimeExpired(IGameEvent eventData)
        {
            if (_isPlay)
            {
                _isPlay = false;
                foreach (var algoritm in _algorithms)
                {
                    algoritm.TimeExpired();
                }
                
                EventBus.Raise(new OpponentFindWordFailEvent());
            }
        }
    }
}