using System.Collections.Generic;
using Core.Services.ReportWords;
using Cysharp.Threading.Tasks;

namespace Core.Services.ReportWord
{
    public interface IReportWordProvider
    {
        UniTask<AddPendingWordResponseDto> AddWordAsync(string normalizedWord, string normalizedReason, string language);
        UniTask<IReadOnlyList<ReportWordEntryDto>> GetWordsAsync(string language);
        UniTask<DeletePendingWordResponseDto> DeleteWordAsync(string normalizedWord, string language);
        UniTask<ClearPendingWordsResponseDto> ClearWordsAsync(string language);
    }
}