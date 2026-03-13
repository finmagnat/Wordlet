using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core.Services.NewWords
{
    public interface INewWordsProvider
    {
        UniTask<AddPendingWordResponseDto> AddWordAsync(string normalizedWord, string language);
        UniTask<IReadOnlyList<NewWordEntryDto>> GetWordsAsync(string language);
        UniTask<DeletePendingWordResponseDto> DeleteWordAsync(string normalizedWord, string language);
        UniTask<ClearPendingWordsResponseDto> ClearWordsAsync(string language);
    }
}