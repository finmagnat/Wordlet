using System;
using System.Collections.Generic;

namespace Core.Events
{
    /// <summary>
    /// Вызываем событие:
    /// EventBus.Raise(new GameSetupConfirmedEvent(setupData));
    ///
    /// Подписываемся:
    /// private void OnEnable()
    /// {
    ///     EventBus.Subscribe<GameSetupConfirmedEvent>(OnGameSetup);
    /// }
    ///
    /// private void OnDisable()
    /// {
    ///     EventBus.Unsubscribe<GameSetupConfirmedEvent>(OnGameSetup);
    /// }
    ///
    /// Обрабатываем:
    /// private void OnGameSetup(GameSetupConfirmedEvent evt)
    /// {
    ///     Debug.Log("Game setup received!");
    ///     StartGame(evt.Data);
    /// }
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public static void Subscribe<T>(Action<T> callback) where T : IGameEvent
        {
            var type = typeof(T);

            if (!_subscribers.ContainsKey(type))
                _subscribers[type] = new List<Delegate>();

            _subscribers[type].Add(callback);
        }

        public static void Unsubscribe<T>(Action<T> callback) where T : IGameEvent
        {
            var type = typeof(T);

            if (_subscribers.ContainsKey(type))
                _subscribers[type].Remove(callback);
        }

        public static void Raise<T>(T evt) where T : IGameEvent
        {
            var type = typeof(T);

            if (!_subscribers.ContainsKey(type))
                return;

            foreach (var del in _subscribers[type])
                (del as Action<T>)?.Invoke(evt);
        }
    }
}