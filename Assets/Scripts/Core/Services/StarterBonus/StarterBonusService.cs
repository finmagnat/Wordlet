using System;
using System.Collections.Generic;
using Core.Services.Inventory;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using UnityEngine;

namespace Core.Services
{
    public sealed class StarterBonusService : IStarterBonusService
    {
        private const string StateKey = "starter_bonus_state";
        private const string StateAvailable = "available";
        private const string StateGranted = "granted";
        private const string FunctionPrepareStarterBonus = "PrepareStarterBonus";
        private const string FunctionGrantStarterGift = "GrantStarterGift";
        
        private readonly PlayFabAuthService _auth;
        private readonly InventorySyncService _inventorySync;

        public bool IsAvailable { get; private set; }
        public bool IsGranted { get; private set; }

        public StarterBonusService(PlayFabAuthService auth, InventorySyncService inventorySync)
        {
            _auth = auth;
            _inventorySync = inventorySync;
        }

        public async UniTask InitializeAsync()
        {
            if (_auth.NewlyCreated)
            {
                await PrepareStarterBonusAsync();
                return;
            }

            await RefreshAvailabilityAsync();
        }

        public async UniTask<bool> TryGrantAsync()
        {
            if (!IsAvailable)
                return false;

            ExecuteCloudScriptResult exec;
            try
            {
                exec = await ExecuteCloudScriptAsync(FunctionGrantStarterGift);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Starter bonus grant failed: {exception}");
                return false;
            }

            var result = exec.FunctionResult;
            bool granted = TryGetBool(result, "granted", out var value) && value;
            bool alreadyGranted = TryGetBool(result, "alreadyGranted", out var alreadyGrantedValue) && alreadyGrantedValue;
            bool isAvailable = TryGetBool(result, "isAvailable", out var available) && available;

            IsAvailable = isAvailable;
            IsGranted = granted || alreadyGranted || IsGranted;

            if (!granted)
                return false;

            try
            {
                await _inventorySync.SyncFromServerAsync();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Starter bonus inventory sync failed: {exception}");
            }

            return true;
        }

        private async UniTask PrepareStarterBonusAsync()
        {
            try
            {
                var exec = await ExecuteCloudScriptAsync(FunctionPrepareStarterBonus);
                ApplyAvailability(exec.FunctionResult);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Starter bonus preparation failed: {exception}");
                IsAvailable = false;
            }
        }

        private async UniTask RefreshAvailabilityAsync()
        {
            try
            {
                var result = await GetUserReadOnlyDataAsync(new List<string> { StateKey });
                string state = null;
                if (result.Data != null && result.Data.TryGetValue(StateKey, out var record))
                    state = record.Value;

                IsAvailable = state == StateAvailable;
                IsGranted = state == StateGranted;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Starter bonus state sync failed: {exception}");
                IsAvailable = false;
                IsGranted = false;
            }
        }

        private void ApplyAvailability(object result)
        {
            IsAvailable = TryGetBool(result, "isAvailable", out var isAvailable) && isAvailable;
            IsGranted = TryGetString(result, "state", out var state) && state == StateGranted;
        }

        private static UniTask<GetUserDataResult> GetUserReadOnlyDataAsync(List<string> keys)
        {
            var tcs = new UniTaskCompletionSource<GetUserDataResult>();

            PlayFabClientAPI.GetUserReadOnlyData(
                new GetUserDataRequest { Keys = keys },
                r => tcs.TrySetResult(r),
                e => tcs.TrySetException(new Exception(e.GenerateErrorReport()))
            );

            return tcs.Task;
        }

        private static UniTask<ExecuteCloudScriptResult> ExecuteCloudScriptAsync(string functionName)
        {
            var tcs = new UniTaskCompletionSource<ExecuteCloudScriptResult>();

            var request = new ExecuteCloudScriptRequest
            {
                FunctionName = functionName,
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

                    tcs.TrySetResult(r);
                },
                e => tcs.TrySetException(new Exception(e.GenerateErrorReport()))
            );

            return tcs.Task;
        }

        private static bool TryGetBool(object result, string key, out bool value)
        {
            value = false;

            if (result is Dictionary<string, object> dictionary &&
                dictionary.TryGetValue(key, out var dictionaryValue))
            {
                value = Convert.ToBoolean(dictionaryValue);
                return true;
            }

            if (result is JsonObject json &&
                json.TryGetValue(key, out var jsonValue))
            {
                value = Convert.ToBoolean(jsonValue);
                return true;
            }

            return false;
        }

        private static bool TryGetString(object result, string key, out string value)
        {
            value = null;

            if (result is Dictionary<string, object> dictionary &&
                dictionary.TryGetValue(key, out var dictionaryValue))
            {
                value = Convert.ToString(dictionaryValue);
                return true;
            }

            if (result is JsonObject json &&
                json.TryGetValue(key, out var jsonValue))
            {
                value = Convert.ToString(jsonValue);
                return true;
            }

            return false;
        }
    }
}
