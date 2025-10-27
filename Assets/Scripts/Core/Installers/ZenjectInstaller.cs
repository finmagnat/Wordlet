using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Services;

namespace Core.Installers
{
    // регистрирует все сервисы.
    public class ZenjectInstaller
    {
        private static readonly Dictionary<Type, object> _services = new();

        
        public static void Register<T>(T service)
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
                throw new Exception($"Service {type.Name} already registered");
            _services[type] = service;
        }

        public static T Get<T>()
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
                return (T)service;
            throw new Exception($"Service {type.Name} not found");
        }

        public static async Task InitializeAsync()
        {
            foreach (var service in _services.Values)
            {
                if (service is IInitializable initializable)
                    await initializable.InitializeAsync();
            }
        }

        public static void DisposeAll()
        {
            foreach (var service in _services.Values)
            {
                if (service is IDisposableService disposable)
                    disposable.Dispose();
            }
            _services.Clear();
        }
    }
}