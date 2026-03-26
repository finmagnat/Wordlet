using Cysharp.Threading.Tasks;

namespace UI.Screens
{
    public interface IUIScreenPreparable
    {
        UniTask PrepareAsync();
    }
}