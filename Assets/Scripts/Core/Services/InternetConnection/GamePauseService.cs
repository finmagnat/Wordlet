using System.Collections.Generic;
using Core.Events;

namespace Core.Services
{
    public class GamePauseService : IGamePauseService
    {
        private readonly HashSet<object> _tokens = new ();

        public bool IsPaused { get; private set; }

        public GamePauseService()
        {
            // Чтобы знать реальное состояние, если паузу кто-то дергает ещё
            EventBus.Subscribe<GamePauseEvent>(OnPauseEvent);
        }

        public void PushPause(object token)
        {
            if (token == null) token = this;

            bool wasEmpty = _tokens.Count == 0;
            _tokens.Add(token);

            if (wasEmpty)
                EnsurePaused(true);
        }

        public void PopPause(object token)
        {
            if (token == null) token = this;

            _tokens.Remove(token);

            if (_tokens.Count == 0)
                EnsurePaused(false);
        }

        private void EnsurePaused(bool shouldBePaused)
        {
            if (IsPaused == shouldBePaused)
                return;

            // GameController переключает по toggle-событию
            EventBus.Raise(new GamePauseEvent());

            // IsPaused обновится в OnPauseEvent, но на случай если кто-то не подписан — подстрахуемся
            IsPaused = shouldBePaused;
        }

        private void OnPauseEvent(IGameEvent e)
        {
            // GameController по событию делает toggle, значит и тут toggle
            IsPaused = !IsPaused;

            // Если вдруг внешним toggle сняли/поставили паузу, а токены говорят обратное — корректируем
            bool desired = _tokens.Count > 0;
            if (IsPaused != desired)
                EnsurePaused(desired);
        }
    }
}