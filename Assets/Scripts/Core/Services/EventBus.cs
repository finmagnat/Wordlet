using System;
using System.Collections.Generic;

namespace Core.Services
{
    public class EventBus : IService
    {
        private readonly Dictionary<Type, Delegate> _subscribers = new();

        public void Subscribe<T>(Action<T> handler)
        {
            if (_subscribers.TryGetValue(typeof(T), out var existing))
                _subscribers[typeof(T)] = Delegate.Combine(existing, handler);
            else
                _subscribers[typeof(T)] = handler;
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (_subscribers.TryGetValue(typeof(T), out var existing))
                _subscribers[typeof(T)] = Delegate.Remove(existing, handler);
        }

        public void Publish<T>(T message)
        {
            if (_subscribers.TryGetValue(typeof(T), out var handlers))
                ((Action<T>)handlers)?.Invoke(message);
        }
    }
}