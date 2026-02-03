using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Core.Services
{
    public interface IProfileService : IService
    {
        string CurrentDisplayName { get; }
        UniTask EnsureDisplayNameAsync();

        UniTask<int> GetScoreAsync(bool forceRefresh = false);
        UniTask<int> AddScoreAsync(int delta);
        UniTask SetScoreAsync(int newScore);

        UniTask<IReadOnlyList<LeaderboardEntryDto>> GetTopAsync();
        UniTask<RankDto> GetMyRankAsync();
    }
}