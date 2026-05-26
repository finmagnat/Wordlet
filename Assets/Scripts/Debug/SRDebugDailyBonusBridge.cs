using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.DebugTools
{
    public sealed class SRDebugDailyBonusBridge : MonoBehaviour
    {
        public static SRDebugDailyBonusBridge Instance { get; private set; }

        [Inject] private DailyBonusService _dailyBonusService;

        private int _day = 1;
        private string _lastResult = "Idle";

        public int Day
        {
            get => _day;
            set => _day = Mathf.Clamp(value, 1, 7);
        }

        public string LastResult => _lastResult;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void SetActiveDay()
        {
            SetStateAsync(DailyBonusDebugMode.ActiveDay).Forget();
        }

        public void MarkClaimedToday()
        {
            SetStateAsync(DailyBonusDebugMode.ClaimedToday).Forget();
        }

        public void SimulateNextDayReady()
        {
            SetStateAsync(DailyBonusDebugMode.NextDayReady).Forget();
        }

        public void Reset()
        {
            SetStateAsync(DailyBonusDebugMode.Reset).Forget();
        }

        public void Refresh()
        {
            RefreshAsync().Forget();
        }

        private async UniTaskVoid SetStateAsync(DailyBonusDebugMode mode)
        {
            if (_dailyBonusService == null)
            {
                SetResult("Daily bonus service is not ready.");
                return;
            }

            var result = await _dailyBonusService.DebugSetStateAsync(mode, Day);
            SetResult(result.Success
                ? FormatState($"OK {mode}", result.State)
                : $"Failed {mode}: {result.Error}");
        }

        private async UniTaskVoid RefreshAsync()
        {
            if (_dailyBonusService == null)
            {
                SetResult("Daily bonus service is not ready.");
                return;
            }

            await _dailyBonusService.RefreshAsync();
            SetResult(FormatState("Refreshed", _dailyBonusService.CurrentState));
        }

        private void SetResult(string result)
        {
            _lastResult = result;
            Debug.Log($"Daily bonus debug: {_lastResult}");
        }

        private static string FormatState(string prefix, DailyBonusState state)
        {
            if (state == null || !state.IsUnlocked)
                return $"{prefix}: unavailable";

            var lastClaim = state.LastClaimUtc.HasValue
                ? state.LastClaimUtc.Value.ToString("u")
                : "none";

            return $"{prefix}: day={state.DailyRewardDay}, available={state.ClaimAvailable}, lastClaimUtc={lastClaim}";
        }
    }
}
