using Cysharp.Threading.Tasks;
using UI.Popups;
using UI.Screens;
using UnityEngine.AddressableAssets;

namespace Core.UI
{
    public interface IUIManager
    {
        UniTask<T> ShowScreenAsync<T>(AssetReferenceGameObject prefabRef) where T : UIScreen;
        UniTask HideAllScreensAsync();
        UniTask<T> ShowPopupAsync<T>(AssetReferenceGameObject prefabRef) where T : UIPopup;
        UniTask HidePopupAsync<T>() where T : UIPopup;
    }
}