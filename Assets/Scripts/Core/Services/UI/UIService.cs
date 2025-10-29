using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UI.Popups;
using UI.Screens;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Services
{
    // управляет активными экранами, умеет загружать их через Addressables:
    // await UIService.ShowScreenAsync(ScreenType.MainMenu);
    // await UIService.ShowPopupAsync(PopupType.Reward);
    public class UIService : IUIService
    {
        private readonly Transform _screenRoot;
        private readonly Transform _popupRoot;

        private readonly Dictionary<string, UIScreen> _activeScreens = new();
        private readonly Stack<UIPopup> _popupStack = new();

        public UIService(Transform screenRoot, Transform popupRoot)
        {
            _screenRoot = screenRoot;
            _popupRoot = popupRoot;
            Debug.Log($"UIService constructed {_screenRoot}, {_popupRoot}");
        }

        public async UniTask<T> ShowScreenAsync<T>(AssetReferenceGameObject reference) where T : UIScreen
        {
            var handle = await reference.InstantiateAsync(_screenRoot);
            var screen = handle.GetComponent<T>();
            _activeScreens[typeof(T).Name] = screen;
            await screen.ShowAsync();
            return screen;
        }

        public async UniTask<T> ShowPopupAsync<T>(AssetReferenceGameObject reference) where T : UIPopup
        {
            var handle = await reference.InstantiateAsync(_popupRoot);
            var popup = handle.GetComponent<T>();
            _popupStack.Push(popup);
            await popup.ShowAsync();
            return popup;
        }

        public async UniTask HidePopupAsync()
        {
            if (_popupStack.Count == 0) return;
            var popup = _popupStack.Pop();
            await popup.HideAsync();
            Object.Destroy(popup.gameObject);
        }

        public async UniTask HideAllScreensAsync()
        {
            foreach (var kvp in _activeScreens)
            {
                await kvp.Value.HideAsync();
                Object.Destroy(kvp.Value.gameObject);
            }
            _activeScreens.Clear();
        }
    }
}