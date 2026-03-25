using Core.Generated;
using Cysharp.Threading.Tasks;
using UI.Popups;
using UI.Screens;

namespace Core.UI
{
    public interface IUIManager
    {
        UniTask<T> ShowScreenAsync<T>(AssetKey assetKey) where T : UIScreen;
        UniTask<T> ShowScreenAsync<T, TPayload>(AssetKey assetKey, TPayload payload) where T : UIScreen<TPayload>;
        UniTask<T> HideScreenAsync<T>(AssetKey assetKey) where T : UIScreen;
        UniTask HideAllScreensAsync();

        UniTask<T> ShowPopupAsync<T>(AssetKey assetKey) where T : UIPopup;
        UniTask<T> ShowPopupAsync<T, TPayload>(AssetKey assetKey, TPayload payload) where T : UIPopup<TPayload>;
        UniTask HidePopupAsync<T>() where T : UIPopup;
        UniTask HideAllPopupsAsync();
    }
}