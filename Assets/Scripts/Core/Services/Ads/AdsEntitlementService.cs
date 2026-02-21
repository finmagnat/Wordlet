using System;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Core.Services
{
    public sealed class AdsEntitlementService : IService
    {
        private const string KeyLocal = "ads_no_interstitial";
        private const string KeyPlayFab = "no_interstitial_ads";

        public event Action Changed;

        public bool NoInterstitialAds { get; private set; }

        public async UniTask InitializeAsync()
        {
            // Локальный кэш
            NoInterstitialAds = PlayerPrefs.GetInt(KeyLocal, 0) == 1;

            // Пытаемся подтянуть с PlayFab (если залогинен) — мягко, без падений
            try
            {
                await SyncFromServerAsync();
            }
            catch
            {
                // MVP: молча
            }
        }

        public async UniTask SetNoInterstitialAdsAsync(bool value)
        {
            NoInterstitialAds = value;
            PlayerPrefs.SetInt(KeyLocal, value ? 1 : 0);
            PlayerPrefs.Save();
            Changed?.Invoke();

            // Сохраняем на PlayFab (UserData, не ReadOnly)
            var req = new UpdateUserDataRequest
            {
                Data = new System.Collections.Generic.Dictionary<string, string>
                {
                    { KeyPlayFab, value ? "1" : "0" }
                }
            };

            var tcs = new UniTaskCompletionSource();
            PlayFabClientAPI.UpdateUserData(req,
                _ => tcs.TrySetResult(),
                e => tcs.TrySetException(new Exception(e.GenerateErrorReport())));
            await tcs.Task;
        }

        public async UniTask SyncFromServerAsync()
        {
            var req = new GetUserDataRequest
            {
                Keys = new System.Collections.Generic.List<string> { KeyPlayFab }
            };

            var tcs = new UniTaskCompletionSource<GetUserDataResult>();
            PlayFabClientAPI.GetUserData(req,
                r => tcs.TrySetResult(r),
                e => tcs.TrySetException(new Exception(e.GenerateErrorReport())));

            var res = await tcs.Task;

            bool serverValue = false;
            if (res.Data != null && res.Data.TryGetValue(KeyPlayFab, out var record))
                serverValue = record.Value == "1";

            NoInterstitialAds = serverValue;
            PlayerPrefs.SetInt(KeyLocal, serverValue ? 1 : 0);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}