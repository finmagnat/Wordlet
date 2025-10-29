using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using Core.Config;
using UI.Popups;
using UI.Screens;

namespace Core.UI
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        [SerializeField] private Transform _screensRoot;
        [SerializeField] private Transform _popupsRoot;

        private readonly Dictionary<AssetReferenceGameObject, UIScreen> _loadedScreens = new();
        private readonly Dictionary<AssetReferenceGameObject, UIPopup> _loadedPopups = new();
        
        private UIAddresses _addresses;
        private DiContainer _container;

        [Inject]
        public void Construct(UIAddresses addresses, DiContainer container)
        {
            _addresses = addresses;
            _container = container;
        }

        private void Awake()
        {
            Debug.Log("🎨 UIManager initialized.");
        }

        public async UniTask<T> ShowScreenAsync<T>(AssetReferenceGameObject prefabRef) where T : UIScreen
        {
            if (_loadedScreens.TryGetValue(prefabRef, out var existing))
            {
                existing.gameObject.SetActive(true);
                return existing as T;
            }

            AsyncOperationHandle<GameObject> handle = prefabRef.InstantiateAsync(_screensRoot);
            await handle.ToUniTask();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"❌ Failed to load screen: {prefabRef.AssetGUID}");
                return null;
            }

            var instance = handle.Result;
            var screen = instance.GetComponent<T>();
            _container.InjectGameObject(instance);

            _loadedScreens[prefabRef] = screen;
            await screen.ShowAsync();

            return screen;
        }

        public async UniTask HideAllScreensAsync()
        {
            foreach (var kvp in _loadedScreens)
            {
                if (kvp.Value)
                    await kvp.Value.HideAsync();
            }
        }
        
        public async UniTask<T> ShowPopupAsync<T>(AssetReferenceGameObject prefabRef) where T : UIPopup
        {
            if (_loadedPopups.TryGetValue(prefabRef, out var existing))
            {
                existing.gameObject.SetActive(true);
                await existing.ShowAsync();
                return existing as T;
            }

            var handle = prefabRef.InstantiateAsync(_popupsRoot);
            await handle.ToUniTask();
            var instance = handle.Result;
            var popup = instance.GetComponent<T>();
            _container.InjectGameObject(instance);

            _loadedPopups[prefabRef] = popup;
            await popup.ShowAsync();

            return popup;
        }

        public async UniTask HidePopupAsync<T>() where T : UIPopup
        {
            var kvp = _loadedPopups.FirstOrDefault(p => p.Value is T);
            if (kvp.Value != null)
                await kvp.Value.HideAsync();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_screensRoot || !_popupsRoot)
            {
                var roots = GetComponentsInChildren<RectTransform>();
                foreach (var rt in roots)
                {
                    if (rt.name.Contains("ScreensRoot")) _screensRoot = rt;
                    else if (rt.name.Contains("PopupsRoot")) _popupsRoot = rt;
                }
            }
        }
#endif
    }
}
