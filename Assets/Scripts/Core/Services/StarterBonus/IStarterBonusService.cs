using Cysharp.Threading.Tasks;

namespace Core.Services
{
    public interface IStarterBonusService : IService
    {
        bool IsAvailable { get; }
        bool IsGranted { get; }
        UniTask<bool> TryGrantAsync();
    }
}
