using Cysharp.Threading.Tasks;

namespace Core.Services
{
    public interface IDailyBonusService : IService
    {
        DailyBonusState CurrentState { get; }
        DailyBonusCycle CurrentCycle { get; }
        bool IsAvailable { get; }

        UniTask RefreshAsync();
        UniTask<DailyBonusClaimResult> TryClaimAsync();
    }
}
