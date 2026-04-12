using System;
using System.Collections.Generic;
using Core.Services.Common;
using Cysharp.Threading.Tasks;

namespace Core.Services.ReportWord
{
    public sealed class PlayFabReportWordProvider : PlayFabCloudScriptProviderBase, IReportWordProvider
    {
        public PlayFabReportWordProvider(IPlayFabAuthFacade playFabAuth) : base(playFabAuth)
        {
        }

        public UniTask<AddPendingWordResponseDto> AddWordAsync(string normalizedWord, string normalizedReason, string language)
        {
            EnsureLoggedIn();

            return ExecuteAsync<AddPendingWordResponseDto>(
                functionName: "AddReportWord",
                functionParameter: new AddPendingWordRequest
                {
                    word = normalizedWord,
                    reason = normalizedReason,
                    language = language
                });
        }

        public async UniTask<IReadOnlyList<ReportWordEntryDto>> GetWordsAsync(string language)
        {
            EnsureLoggedIn();

            var response = await ExecuteAsync<GetPendingWordsResponseDto>(
                functionName: "GetReportWords",
                functionParameter: new GetPendingWordsRequest
                {
                    language = language
                });

            return response.words ?? new List<ReportWordEntryDto>();
        }

        public UniTask<DeletePendingWordResponseDto> DeleteWordAsync(string normalizedWord, string language)
        {
            EnsureLoggedIn();

            return ExecuteAsync<DeletePendingWordResponseDto>(
                functionName: "DeleteReportWord",
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
                functionName: "ClearReportWords",
                functionParameter: new ClearPendingWordsRequest
                {
                    language = language
                });
        }

        [Serializable]
        private sealed class AddPendingWordRequest
        {
            public string word;
            public string reason;
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