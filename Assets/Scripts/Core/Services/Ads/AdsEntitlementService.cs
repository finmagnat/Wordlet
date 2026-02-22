using System;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Core.Services
{
    public sealed class AdsEntitlementService : IService
    {
        public const string KeyNoInterstitialAds = "no_interstitial_ads";

        public event Action Changed;

        public bool NoInterstitialAds { get; private set; }

        public async UniTask InitializeAsync()
        {
            // Локальный кэш
            NoInterstitialAds = PlayerPrefs.GetInt(KeyNoInterstitialAds, 0) == 1;

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

        public void SetNoInterstitialAdsLocal(bool value)
        {
            NoInterstitialAds = value;
            PlayerPrefs.SetInt(KeyNoInterstitialAds, value ? 1 : 0);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
        
        public async UniTask SetNoInterstitialAdsAsync(bool value)
        {
            // Сохраняем на PlayFab (UserData, не ReadOnly)
            var req = new UpdateUserDataRequest
            {
                Data = new System.Collections.Generic.Dictionary<string, string>
                {
                    { KeyNoInterstitialAds, value ? "1" : "0" }
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
                Keys = new System.Collections.Generic.List<string> { KeyNoInterstitialAds }
            };

            var tcs = new UniTaskCompletionSource<GetUserDataResult>();
            PlayFabClientAPI.GetUserData(req,
                r => tcs.TrySetResult(r),
                e => tcs.TrySetException(new Exception(e.GenerateErrorReport())));

            var res = await tcs.Task;

            bool serverValue = false;
            if (res.Data != null && res.Data.TryGetValue(KeyNoInterstitialAds, out var record))
                serverValue = record.Value == "1";

            NoInterstitialAds = serverValue;
            PlayerPrefs.SetInt(KeyNoInterstitialAds, serverValue ? 1 : 0);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}