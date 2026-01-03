using Core.Config;
using Core.Dictionary;
using Core.Events;
using Cysharp.Threading.Tasks;
using Game.Logic;

namespace Game.AI
{
    public class AIGameController : IState
    {
        private ComplexityAISettings _settings;
        private WordsFieldManager _wordsFieldManager;
        private DictionaryService _dictionaryService;

        private bool _isPlay;

        // ✅ теперь async-алгоритмы
        private readonly IAIAlgorithmAsync[] _algorithms =
        {
            new SmartWordAlgorithmAsync()
        };

        public AIGameController()
        {
            EventBus.Subscribe<TimeExpiredEvent>(OnTimeExpired);
        }

        public void Destroy()
        {
            EventBus.Unsubscribe<TimeExpiredEvent>(OnTimeExpired);
        }

        internal void Init(WordsFieldManager wordsFieldManager, DictionaryService dictionaryService, ComplexityAISettings settings)
        {
            _wordsFieldManager = wordsFieldManager;
            _dictionaryService = dictionaryService;
            _settings = settings;
        }

        internal void PlayAsync()
        {
            // fire-and-forget, чтобы не ломать текущие сигнатуры GameController
            RunAsync().Forget();
        }

        private async UniTaskVoid RunAsync()
        {
            _isPlay = true;

            foreach (var algorithm in _algorithms)
            {
                var res = await algorithm.GetWordAsync(_settings, _wordsFieldManager, _dictionaryService);

                if (!_isPlay) // время уже вышло и нас “срубили” событием
                    return;

                if (res.Success)
                {
                    _isPlay = false;
                    EventBus.Raise(new OpponentFindWordEvent { word = res.Word });
                    return;
                }
            }

            _isPlay = false;
            EventBus.Raise(new OpponentFindWordFailEvent());
        }

        private void OnTimeExpired(IGameEvent eventData)
        {
            if (!_isPlay) return;

            _isPlay = false;
            foreach (var algorithm in _algorithms)
                algorithm.TimeExpired();

            EventBus.Raise(new OpponentFindWordFailEvent());
        }
    }
}
