using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Core.Services.ReportWord
{
    public sealed class PlayFabReportWordProvider : IReportWordProvider
    {
        private readonly IPlayFabAuthFacade _playFabAuth;

        public PlayFabReportWordProvider(IPlayFabAuthFacade playFabAuth)
        {
            _playFabAuth = playFabAuth;
        }
        
        public UniTask<AddPendingWordResponseDto> AddWordAsync(string normalizedWord, string language)
        {
            if (!_playFabAuth.IsLoggedIn)
                throw new Exception("PlayFab is not logged in.");
            
            return ExecuteAsync<AddPendingWordResponseDto>(
                functionName: "AddPendingWord",
                functionParameter: new AddPendingWordRequest
                {
                    word = normalizedWord,
                    language = language
                });
        }

        public async UniTask<IReadOnlyList<ReportWordEntryDto>> GetWordsAsync(string language)
        {
            if (!_playFabAuth.IsLoggedIn)
                throw new Exception("PlayFab is not logged in.");
            
            var response = await ExecuteAsync<GetPendingWordsResponseDto>(
                functionName: "GetPendingWords",
                functionParameter: new GetPendingWordsRequest
                {
                    language = language
                });

            return response.words ?? new List<ReportWordEntryDto>();
        }

        public UniTask<DeletePendingWordResponseDto> DeleteWordAsync(string normalizedWord, string language)
        {
            if (!_playFabAuth.IsLoggedIn)
                throw new Exception("PlayFab is not logged in.");
            
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
            return ExecuteAsync<ClearPendingWordsResponseDto>(
                functionName: "ClearPendingWords",
                functionParameter: new ClearPendingWordsRequest
                {
                    language = language
                });
        }

        private static UniTask<TResponse> ExecuteAsync<TResponse>(string functionName, object functionParameter)
        {
            var tcs = new UniTaskCompletionSource<TResponse>();

            var request = new ExecuteCloudScriptRequest
            {
                FunctionName = functionName,
                FunctionParameter = functionParameter,
                GeneratePlayStreamEvent = true
            };

            PlayFabClientAPI.ExecuteCloudScript(
                request,
                result =>
                {
                    try
                    {
                        if (result.Error != null)
                        {
                            tcs.TrySetException(new Exception($"CloudScript error: {result.Error.Message}"));
                            return;
                        }

                        if (result.FunctionResult is not string json || string.IsNullOrWhiteSpace(json))
                        {
                            tcs.TrySetException(new Exception(
                                $"CloudScript '{functionName}' returned empty or non-string result."));
                            return;
                        }

                        var response = JsonUtility.FromJson<TResponse>(json);
                        if (response == null)
                        {
                            tcs.TrySetException(new Exception(
                                $"CloudScript '{functionName}' returned invalid JSON: {json}"));
                            return;
                        }

                        tcs.TrySetResult(response);
                    }
                    catch (Exception e)
                    {
                        tcs.TrySetException(e);
                    }
                },
                error => tcs.TrySetException(new Exception(error.GenerateErrorReport()))
            );

            return tcs.Task;
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