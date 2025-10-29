using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core.Services
{
    public class EventBus : IService
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public UniTask InitializeAsync()
        {
            // здесь можно подписать глобальные события, если нужно
            return UniTask.CompletedTask;
        }

        public void Subscribe<T>(Action<T> callback)
        {
            if (!_subscribers.TryGetValue(typeof(T), out var list))
            {
                list = new List<Delegate>();
                _subscribers[typeof(T)] = list;
            }
            list.Add(callback);
        }

        public void Publish<T>(T evt)
        {
            if (_subscribers.TryGetValue(typeof(T), out var list))
            {
                foreach (var callback in list)
                    ((Action<T>)callback)?.Invoke(evt);
            }
        }
    }
}