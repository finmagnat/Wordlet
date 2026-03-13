using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core.Services.NewWords
{
    public interface INewWordsService
    {
        UniTask<AddPendingWordResponseDto> SubmitWordAsync(string rawWord, string language);
        UniTask<IReadOnlyList<NewWordEntryDto>> GetPendingWordsAsync(string language);
        UniTask<DeletePendingWordResponseDto> RemoveWordAsync(string rawWord, string language);
        UniTask<ClearPendingWordsResponseDto> ClearPendingWordsAsync(string language);
    }
}