using Core.Generated;
using Cysharp.Threading.Tasks;
using UI.Popups;
using UI.Screens;

namespace Core.UI
{
    public interface IUIManager
    {
        UniTask<T> ShowScreenAsync<T>(AssetKey assetKey) where T : UIScreen;
        UniTask HideAllScreensAsync();
        UniTask<T> ShowPopupAsync<T>(AssetKey assetKey) where T : UIPopup;
        UniTask HidePopupAsync<T>() where T : UIPopup;
    }
}