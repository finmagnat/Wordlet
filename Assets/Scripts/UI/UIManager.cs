using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using Core.Config;
using UI.Screens;

namespace Core.UI
{
    public interface IUIManager
    {
        UniTask<T> ShowScreenAsync<T>(AssetReferenceGameObject prefabRef) where T : UIScreen;
        UniTask HideAllScreensAsync();
    }

    public class UIManager : MonoBehaviour, IUIManager
    {
        [SerializeField] private Transform _screensRoot;
        [SerializeField] private Transform _popupsRoot;

        private readonly Dictionary<AssetReferenceGameObject, UIScreen> _loadedScreens = new();
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
