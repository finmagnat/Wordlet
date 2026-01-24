using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Core.Services
{
    public class PlayFabAuthService : MonoBehaviour
    {
        private void Start()
        {
            Login();
        }

        private void Login()
        {
            var request = new LoginWithCustomIDRequest
            {
                CustomId = SystemInfo.deviceUniqueIdentifier,
                CreateAccount = true
            };

            PlayFabClientAPI.LoginWithCustomID(
                request,
                OnLoginSuccess,
                OnLoginError
            );
        }

        private void OnLoginSuccess(LoginResult result)
        {
            Debug.Log($"PlayFab login OK. New account: {result.NewlyCreated}");
        }

        private void OnLoginError(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
        }
    }
}