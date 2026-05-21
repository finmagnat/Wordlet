using Cysharp.Threading.Tasks;

namespace Core.Services
{
    public interface IStarterBonusService : IService
    {
        bool IsAvailable { get; }
        UniTask<bool> TryGrantAsync();
    }
}
