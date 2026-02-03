using System;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Core.Services
{
    public class PlayFabAuthService : IService , IPlayFabAuthFacade
    {
        public bool IsLoggedIn { get; private set; }
        public bool NewlyCreated { get; private set; }
        public string PlayFabId { get; private set; }
        public string DisplayName { get; private set; }

        public async UniTask InitializeAsync()
        {
            // Пока для smoke-test используем CustomId.
            // Перед релизом: AndroidDeviceID / IOSDeviceID, а CustomID запретим в настройках.
            var loginResult = await LoginWithCustomIdAsync(SystemInfo.deviceUniqueIdentifier);

            IsLoggedIn = true;
            NewlyCreated = loginResult.NewlyCreated;
            PlayFabId = loginResult.PlayFabId;

            // ✅ если профиль был запрошен — можно получить DisplayName
            DisplayName = loginResult.InfoResultPayload?.PlayerProfile?.DisplayName;

            Debug.Log($"PlayFab login OK. New account: {NewlyCreated}. PlayFabId: {PlayFabId}. Name: {DisplayName}");

            if (NewlyCreated)
            {
                await GrantStarterGiftAsync();
                Debug.Log("Starter gift granted");
            }
        }

        public void SetDisplayNameLocal(string name)
        {
            DisplayName = name;
        }

        private static UniTask<LoginResult> LoginWithCustomIdAsync(string customId)
        {
            var tcs = new UniTaskCompletionSource<LoginResult>();

            var request = new LoginWithCustomIDRequest
            {
                CustomId = customId,
                CreateAccount = true,

                // ✅ просим профиль, чтобы сразу знать DisplayName
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true,
                    ProfileConstraints = new PlayerProfileViewConstraints
                    {
                        ShowDisplayName = true
                    }
                }
            };

            PlayFabClientAPI.LoginWithCustomID(
                request,
                r => tcs.TrySetResult(r),
                e => tcs.TrySetException(new Exception(e.GenerateErrorReport()))
            );

            return tcs.Task;
        }

        private static UniTask GrantStarterGiftAsync()
        {
            var tcs = new UniTaskCompletionSource();

            var request = new ExecuteCloudScriptRequest
            {
                FunctionName = "GrantStarterGift",
                GeneratePlayStreamEvent = true
            };

            PlayFabClientAPI.ExecuteCloudScript(
                request,
                r =>
                {
                    if (r.Error != null)
                    {
                        tcs.TrySetException(new Exception($"CloudScript error: {r.Error.Message}"));
                        return;
                    }
                    tcs.TrySetResult();
                },
                e => tcs.TrySetException(new Exception(e.GenerateErrorReport()))
            );

            return tcs.Task;
        }
    }
}
