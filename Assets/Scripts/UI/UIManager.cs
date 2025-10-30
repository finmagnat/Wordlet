using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Zenject;
using Core.Config;
using DG.Tweening;
using UI.Popups;
using UI.Screens;

namespace Core.UI
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        [SerializeField] private Transform _screensRoot;
        [SerializeField] private Transform _popupsRoot;
        [SerializeField] private Transform _loadingRoot;

        private readonly Dictionary<AssetReferenceGameObject, UIScreen> _loadedScreens = new();
        private readonly Dictionary<AssetReferenceGameObject, UIPopup> _loadedPopups = new();

        private UIAddresses _addresses;
        private DiContainer _container;

        // === Лоадинг ===
        private GameObject _loadingScreen;
        private CanvasGroup _loadingCanvasGroup;
        private bool _isLoadingVisible;

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

        // =========================
        //        SCREENS
        // =========================
        public async UniTask<T> ShowScreenAsync<T>(AssetReferenceGameObject prefabRef) where T : UIScreen
        {
            if (_loadedScreens.TryGetValue(prefabRef, out var existing))
            {
                existing.gameObject.SetActive(true);
                return existing as T;
            }

            var handle = prefabRef.InstantiateAsync(_screensRoot);
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
            await screen.ShowAsync(); //После нажатия в попапе GameSetupPopup кнопки играть нужно запустить экран игры

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

        // =========================
        //        POPUPS
        // =========================
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
            
            //await UniTask.Yield(PlayerLoopTiming.PostLateUpdate); // гарантирует вызов Awake()
            
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

        // =========================
        //        LOADING
        // =========================
        public async UniTask ShowInGameLoadingAsync(Color? overlayColor = null)
        {
            if (_isLoadingVisible)
                return;

            if (_loadingScreen == null)
            {
                _loadingScreen = new GameObject("LoadingOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
                _loadingScreen.transform.SetParent(_loadingRoot, false);

                var rect = _loadingScreen.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;

                var img = _loadingScreen.GetComponent<Image>();
                img.color = overlayColor ?? new Color(0, 0, 0, 0.85f);

                _loadingCanvasGroup = _loadingScreen.GetComponent<CanvasGroup>();
                _loadingCanvasGroup.alpha = 0;
                _loadingCanvasGroup.blocksRaycasts = true;
            }

            // Без AsyncWaitForCompletion:
            var tween = _loadingCanvasGroup.DOFade(1f, 0.25f);
            await UniTask.WaitUntil(() => !tween.IsActive() || !tween.IsPlaying());

            _isLoadingVisible = true;
        }

        public async UniTask HideInGameLoadingAsync()
        {
            if (!_isLoadingVisible || _loadingCanvasGroup == null)
                return;

            var tween = _loadingCanvasGroup.DOFade(0f, 0.25f);
            await UniTask.WaitUntil(() => !tween.IsActive() || !tween.IsPlaying());

            _loadingCanvasGroup.blocksRaycasts = false;
            _isLoadingVisible = false;
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_screensRoot || !_popupsRoot || !_loadingRoot)
            {
                var roots = GetComponentsInChildren<RectTransform>(true);
                foreach (var rt in roots)
                {
                    if (rt.name.Contains("ScreensRoot")) _screensRoot = rt;
                    else if (rt.name.Contains("PopupsRoot")) _popupsRoot = rt;
                    else if (rt.name.Contains("LoadingRoot")) _loadingRoot = rt;
                }
            }
        }
#endif
    }
}
