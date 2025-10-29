using Cysharp.Threading.Tasks;
using UI.Popups;
using UI.Screens;
using UnityEngine.AddressableAssets;

namespace Core.Services
{
    public interface IUIService : IService
    {
        UniTask<T> ShowScreenAsync<T>(AssetReferenceGameObject reference) where T : UIScreen;

        UniTask<T> ShowPopupAsync<T>(AssetReferenceGameObject reference) where T : UIPopup;

        UniTask HidePopupAsync();

        UniTask HideAllScreensAsync();
    }
}