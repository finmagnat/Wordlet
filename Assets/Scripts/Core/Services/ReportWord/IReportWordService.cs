using System.Collections.Generic;
using Core.Services.ReportWords;
using Cysharp.Threading.Tasks;

namespace Core.Services.ReportWord
{
    public interface IReportWordService
    {
        UniTask<AddPendingWordResponseDto> SubmitWordAsync(string rawWord, ReportReason reason, string language);
        UniTask<IReadOnlyList<ReportWordEntryDto>> GetPendingWordsAsync(string language);
        UniTask<DeletePendingWordResponseDto> RemoveWordAsync(string rawWord, string language);
        UniTask<ClearPendingWordsResponseDto> ClearPendingWordsAsync(string language);
        UniTask<SubmitReportWordFlowResult> TrySubmitWordAsync(string rawWord, ReportReason reason, string language);
    }
}