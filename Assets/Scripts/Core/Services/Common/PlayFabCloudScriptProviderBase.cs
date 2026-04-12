using System;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Core.Services.Common
{
    public abstract class PlayFabCloudScriptProviderBase
    {
        private readonly IPlayFabAuthFacade _playFabAuth;

        protected PlayFabCloudScriptProviderBase(IPlayFabAuthFacade playFabAuth)
        {
            _playFabAuth = playFabAuth;
        }

        protected void EnsureLoggedIn()
        {
            if (!_playFabAuth.IsLoggedIn)
                throw new Exception("PlayFab is not logged in.");
        }

        protected static UniTask<TResponse> ExecuteAsync<TResponse>(string functionName, object functionParameter)
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
    }
}