using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using Zenject;

namespace Core.Services
{
    public class PlayFabAuthService : IService , IPlayFabAuthFacade
    {
        [Inject] private AnalyticsPlayerContext _analyticsPlayerContext;
        [Inject] private IAnalyticsService _analyticsService;

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
            _analyticsPlayerContext.PlayFabId = PlayFabId;

            // ✅ если профиль был запрошен — можно получить DisplayName
            DisplayName = loginResult.InfoResultPayload?.PlayerProfile?.DisplayName;

            Debug.Log($"PlayFab login OK. New account: {NewlyCreated}. <color=yellow>PlayFabId: {PlayFabId}. Name: {DisplayName}</color>");
            _analyticsService.TrackEvent(
                AnalyticsEvents.Startup.PlayFabAuthCompleted,
                new Dictionary<string, object>
                {
                    [AnalyticsEvents.Parameter.NewUser] = NewlyCreated
                });

            if (NewlyCreated)
                Debug.Log("New account created. Starter bonus will be granted after the first finished round.");
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

    }
}
