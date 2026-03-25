using Cysharp.Threading.Tasks;

namespace Core.UI
{
    public interface IUIElement<in TPayload>
    {
        UniTask PrepareAsync(TPayload payload);
        UniTask ShowAsync();
        UniTask HideAsync();
    }
}