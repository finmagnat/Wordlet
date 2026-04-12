using System;
using System.Collections.Generic;
using Core.Services.Common;
using Cysharp.Threading.Tasks;

namespace Core.Services.NewWords
{
    public sealed class PlayFabNewWordsProvider : PlayFabCloudScriptProviderBase, INewWordsProvider
    {
        public PlayFabNewWordsProvider(IPlayFabAuthFacade playFabAuth) : base(playFabAuth)
        {
        }

        public UniTask<AddPendingWordResponseDto> AddWordAsync(string normalizedWord, string language)
        {
            EnsureLoggedIn();

            return ExecuteAsync<AddPendingWordResponseDto>(
                functionName: "AddPendingWord",
                functionParameter: new AddPendingWordRequest
                {
                    word = normalizedWord,
                    language = language
                });
        }

        public async UniTask<IReadOnlyList<NewWordEntryDto>> GetWordsAsync(string language)
        {
            EnsureLoggedIn();

            var response = await ExecuteAsync<GetPendingWordsResponseDto>(
                functionName: "GetPendingWords",
                functionParameter: new GetPendingWordsRequest
                {
                    language = language
                });

            return response.words ?? new List<NewWordEntryDto>();
        }

        public UniTask<DeletePendingWordResponseDto> DeleteWordAsync(string normalizedWord, string language)
        {
            EnsureLoggedIn();

            return ExecuteAsync<DeletePendingWordResponseDto>(
                functionName: "DeletePendingWord",
                functionParameter: new DeletePendingWordRequest
                {
                    word = normalizedWord,
                    language = language
                });
        }

        public UniTask<ClearPendingWordsResponseDto> ClearWordsAsync(string language)
        {
            EnsureLoggedIn();

            return ExecuteAsync<ClearPendingWordsResponseDto>(
                functionName: "ClearPendingWords",
                functionParameter: new ClearPendingWordsRequest
                {
                    language = language
                });
        }

        [Serializable]
        private sealed class AddPendingWordRequest
        {
            public string word;
            public string language;
        }

        [Serializable]
        private sealed class GetPendingWordsRequest
        {
            public string language;
        }

        [Serializable]
        private sealed class DeletePendingWordRequest
        {
            public string word;
            public string language;
        }

        [Serializable]
        private sealed class ClearPendingWordsRequest
        {
            public string language;
        }
    }
}