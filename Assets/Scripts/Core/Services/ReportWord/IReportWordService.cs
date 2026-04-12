using System.Collections.Generic;
using Core.Services.Common;
using Cysharp.Threading.Tasks;

namespace Core.Services.ReportWord
{
    public interface IReportWordService
    {
        UniTask<AddPendingWordResponseDto> SubmitWordAsync(string rawWord, ReportReason reason, string language);
        UniTask<IReadOnlyList<ReportWordEntryDto>> GetPendingWordsAsync(string language);
        UniTask<DeletePendingWordResponseDto> RemoveWordAsync(string rawWord, string language);
        UniTask<ClearPendingWordsResponseDto> ClearPendingWordsAsync(string language);
        UniTask<WordSubmissionFlowResult> TrySubmitWordAsync(string rawWord, ReportReason reason, string language);
    }
}