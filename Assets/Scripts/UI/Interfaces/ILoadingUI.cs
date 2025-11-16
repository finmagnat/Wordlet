using Cysharp.Threading.Tasks;
using UI.Screens;
using UnityEngine.AddressableAssets;

namespace Core.UI
{
    public interface ILoadingUI
    {
        UniTask<T> ShowLoadingAsync<T>(AssetReferenceGameObject prefabRef) where T : UIScreen;
        UniTask HideLoadingAsync();
    }
}