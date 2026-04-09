using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core.Services.ReportWord
{
    public interface IReportWordService
    {
        UniTask<AddPendingWordResponseDto> SubmitWordAsync(string rawWord, string language);
        UniTask<IReadOnlyList<ReportWordEntryDto>> GetPendingWordsAsync(string language);
        UniTask<DeletePendingWordResponseDto> RemoveWordAsync(string rawWord, string language);
        UniTask<ClearPendingWordsResponseDto> ClearPendingWordsAsync(string language);
        UniTask<SubmitReportWordFlowResult> TrySubmitWordAsync(string rawWord, string language);
    }
}