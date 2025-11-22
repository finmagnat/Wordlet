using Core.Generated;
using Cysharp.Threading.Tasks;
using UI.Screens;

namespace Core.UI
{
    public interface ILoadingUI
    {
        UniTask<T> ShowLoadingAsync<T>(AssetKey assetKey) where T : UIScreen;
        UniTask HideLoadingAsync();
    }
}