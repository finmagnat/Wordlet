using System.Collections.Generic;
using Core.Generated;
using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using UI.Screens;
using Unity.VisualScripting;

namespace Core.UI
{
    public class LoadingUIManager : MonoBehaviour, ILoadingUI
    {
        [SerializeField] private Transform _loadingRoot;

        private readonly Dictionary<string, UIScreen> _loaded = new();

        private AddressablesLoader _loader;
        private DiContainer _container;

        [Inject]
        public void Construct(AddressablesLoader loader, DiContainer container)
        {
            _loader = loader;
            _container = container;
        }

        public async UniTask<T> ShowLoadingAsync<T>(AssetKey assetKey) where T : UIScreen
        {
            var strAssetKey = assetKey.ToString();
            
            // Already loaded and cached?
            if (_loaded.TryGetValue(strAssetKey, out var existing))
            {
                existing.gameObject.SetActive(true);
                await existing.ShowAsync();
                return existing as T;
            }

            // Load prefab
            var prefab = await _loader.LoadAsync<GameObject>(strAssetKey);
            if (prefab == null)
            {
                Debug.LogError($"❌ Failed to load loading screen: {assetKey}");
                return null;
            }

            // Instantiate
            var instance = Instantiate(prefab, _loadingRoot);
            _container.InjectGameObject(instance);

            var screen = instance.GetComponent<T>();
            _loaded[strAssetKey] = screen;

            await screen.ShowAsync();

            return screen;
        }

        public async UniTask HideLoadingAsync()
        {
            foreach (var screen in _loaded.Values)
            {
                if (screen != null)
                    await screen.HideAsync();
            }
        }
    }
}