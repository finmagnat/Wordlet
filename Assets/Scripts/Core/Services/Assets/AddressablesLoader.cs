using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Core.Services
{
    public class AddressablesLoader
    {
        // Кэш загруженных ресурсов
        private readonly Dictionary<string, AsyncOperationHandle> _loaded = new();

        /// <summary>
        /// Универсальная загрузка ресурсов Addressables по ключу.
        /// </summary>
        public async UniTask<T> LoadAsync<T>(string key) where T : class
        {
            // Если уже есть в кэше
            if (_loaded.TryGetValue(key, out var cached))
            {
                if (cached.Result is T result)
                    return result;
            }

            // Загружаем
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            await handle.ToUniTask();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"❌ Addressables failed to load key: {key}");
                return null;
            }

            _loaded[key] = handle;
            return handle.Result;
        }

        /// <summary>
        /// Проверка — загружен ли ключ.
        /// </summary>
        public bool IsLoaded(string key)
        {
            return _loaded.ContainsKey(key);
        }

        /// <summary>
        /// Выгрузка конкретного ключа.
        /// </summary>
        public void Unload(string key)
        {
            if (_loaded.TryGetValue(key, out var handle))
            {
                Addressables.Release(handle);
                _loaded.Remove(key);
            }
        }

        /// <summary>
        /// Выгрузка всех ресурсов.
        /// </summary>
        public void ReleaseAll()
        {
            foreach (var handle in _loaded.Values)
                Addressables.Release(handle);

            _loaded.Clear();
        }
    }
}