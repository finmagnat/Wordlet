using System.Collections.Generic;
using Core.Generated;
using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using UI.Popups;
using UI.Screens;

namespace Core.UI
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        [Header("UI Roots")]
        [SerializeField] private Transform _screensRoot;
        [SerializeField] private Transform _popupsRoot;
        [SerializeField] private Transform _loadingRoot;

        private readonly Dictionary<string, UIScreen> _loadedScreens = new();
        private readonly Dictionary<string, UIPopup> _loadedPopups = new();

        private AddressablesLoader _loader;
        private DiContainer _container;

        [Inject]
        public void Construct(AddressablesLoader loader, DiContainer container)
        {
            _loader = loader;
            _container = container;
        }

        private void Awake()
        {
            Debug.Log("🎨 UIManager initialized.");
        }

        // ---------------------------------------------------------
        // SCREENS
        // ---------------------------------------------------------

        public async UniTask<T> ShowScreenAsync<T>(AssetKey assetKey) where T : UIScreen
        {
            var strAssetKey = assetKey.ToString();
            // Already loaded?
            if (_loadedScreens.TryGetValue(strAssetKey, out var existing))
            {
                existing.gameObject.SetActive(true);
                await existing.ShowAsync();
                return existing as T;
            }

            // Load prefab
            var prefab = await _loader.LoadAssetAsync<GameObject>(strAssetKey);
            if (prefab == null)
            {
                Debug.LogError($"❌ Failed to load screen prefab: {strAssetKey}");
                return null;
            }

            var instance = Instantiate(prefab, _screensRoot);
            _container.InjectGameObject(instance);

            var screen = instance.GetComponent<T>();
            _loadedScreens[strAssetKey] = screen;

            await screen.ShowAsync();
            return screen;
        }

        public async UniTask<T> HideScreenAsync<T>(AssetKey assetKey) where T : UIScreen
        {
            if (_loadedScreens.TryGetValue(assetKey.ToString(), out var screen) && screen != null)
            {
                await screen.HideAsync();
                return screen as T;
            }
            return null;
        }
        
        public async UniTask HideAllScreensAsync()
        {
            foreach (var pair in _loadedScreens)
            {
                if (pair.Value != null)
                    await pair.Value.HideAsync();
            }
        }

        // ---------------------------------------------------------
        // POPUPS
        // ---------------------------------------------------------

        public async UniTask<T> ShowPopupAsync<T>(AssetKey assetKey) where T : UIPopup
        {
            var strAssetKey = assetKey.ToString();
            
            if (_loadedPopups.TryGetValue(strAssetKey, out var existing))
            {
                existing.gameObject.SetActive(true);
                await existing.ShowAsync();
                return existing as T;
            }

            var prefab = await _loader.LoadAssetAsync<GameObject>(strAssetKey);
            if (prefab == null)
            {
                Debug.LogError($"❌ Failed to load popup prefab: {assetKey}");
                return null;
            }

            var instance = Instantiate(prefab, _popupsRoot);
            _container.InjectGameObject(instance);

            var popup = instance.GetComponent<T>();
            _loadedPopups[strAssetKey] = popup;

            await popup.ShowAsync();
            return popup;
        }

        public async UniTask HidePopupAsync<T>() where T : UIPopup
        {
            foreach (var pair in _loadedPopups)
            {
                if (pair.Value is T popup)
                {
                    await popup.HideAsync();
                    return;
                }
            }
        }
        
        public async UniTask HideAllPopupsAsync()
        {
            foreach (var pair in _loadedPopups)
            {
                if (pair.Value != null)
                    await pair.Value.HideAsync();
            }
        }

    }
}
