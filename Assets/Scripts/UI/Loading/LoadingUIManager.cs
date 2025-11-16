using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UI.Screens;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

namespace Core.UI
{
    public class LoadingUIManager : MonoBehaviour, ILoadingUI
    {
        [SerializeField] private Transform _loadingRoot;

        private readonly Dictionary<AssetReferenceGameObject, UIScreen> _loaded = new();
        private DiContainer _container;

        [Inject]
        public void Construct(DiContainer container)
        {
            _container = container;
        }

        public async UniTask<T> ShowLoadingAsync<T>(AssetReferenceGameObject prefabRef)
            where T : UIScreen
        {
            if (_loaded.TryGetValue(prefabRef, out var existing))
            {
                existing.gameObject.SetActive(true);
                return existing as T;
            }

            var handle = prefabRef.InstantiateAsync(_loadingRoot);
            await handle.ToUniTask();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"❌ Failed to load loading screen: {prefabRef.AssetGUID}");
                return null;
            }

            var instance = handle.Result;
            var screen = instance.GetComponent<T>();
            _container.InjectGameObject(instance);

            _loaded[prefabRef] = screen;
            await screen.ShowAsync();

            return screen;
        }

        public async UniTask HideLoadingAsync()
        {
            foreach (var screen in _loaded.Values)
                await screen.HideAsync();
        }
    }
}