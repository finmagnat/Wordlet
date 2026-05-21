using System;
using System.Collections.Generic;
using System.Globalization;
using Core.Config;
using Core.Services.Common;
using Core.Services.Inventory;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Services
{
    public sealed class DailyBonusService : PlayFabCloudScriptProviderBase, IDailyBonusService
    {
        private const string FunctionRefreshDailyBonus = "RefreshDailyBonus";
        private const string FunctionClaimDailyBonus = "ClaimDailyBonus";

        private readonly IStarterBonusService _starterBonusService;
        private readonly InventorySyncService _inventorySync;

        private bool _starterBonusGrantedAtLaunch;

        public event Action<DailyBonusState> StateChanged;

        public DailyBonusState CurrentState { get; private set; } = DailyBonusState.Unavailable;
        public DailyBonusCycle CurrentCycle { get; private set; } = DailyBonusCycle.Empty;
        public bool IsAvailable => CurrentState is { IsUnlocked: true, ClaimAvailable: true };

        public DailyBonusService(
            IPlayFabAuthFacade playFabAuth,
            IStarterBonusService starterBonusService,
            InventorySyncService inventorySync)
            : base(playFabAuth)
        {
            _starterBonusService = starterBonusService;
            _inventorySync = inventorySync;
        }

        public async UniTask InitializeAsync()
        {
            _starterBonusGrantedAtLaunch = _starterBonusService.IsGranted;

            if (!_starterBonusGrantedAtLaunch)
            {
                SetState(DailyBonusState.Unavailable);
                return;
            }

            await RefreshAsync();
        }

        public async UniTask RefreshAsync()
        {
            if (!_starterBonusGrantedAtLaunch)
            {
                SetState(DailyBonusState.Unavailable);
                return;
            }

            try
            {
                EnsureLoggedIn();
                var response = await ExecuteAsync<DailyBonusRefreshResponse>(FunctionRefreshDailyBonus, null);
                CurrentCycle = ToCycle(response.config);

                if (!response.ok || !response.starterGranted)
                {
                    SetState(DailyBonusState.Unavailable);
                    return;
                }

                SetState(ToState(response.state, true));
            }
            catch (Exception exception)
            {
                Debug.LogError($"Daily bonus refresh failed: {exception}");
                SetState(DailyBonusState.Unavailable);
            }
        }

        public async UniTask<DailyBonusClaimResult> TryClaimAsync()
        {
            if (!_starterBonusGrantedAtLaunch)
                return DailyBonusClaimResult.NotAvailable(CurrentState, "starter_bonus_not_granted_at_launch");

            if (!IsAvailable)
                return DailyBonusClaimResult.NotAvailable(CurrentState, "claim_not_available");

            DailyBonusClaimResponse response;
            try
            {
                EnsureLoggedIn();
                response = await ExecuteAsync<DailyBonusClaimResponse>(FunctionClaimDailyBonus, null);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Daily bonus claim failed: {exception}");
                return DailyBonusClaimResult.NotAvailable(CurrentState, exception.Message);
            }

            var state = ToState(response.state, response.ok);
            CurrentCycle = ToCycle(response.config);
            SetState(state);

            if (!response.ok || !response.granted)
                return DailyBonusClaimResult.NotAvailable(state, response.error);

            try
            {
                await _inventorySync.SyncFromServerAsync();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Daily bonus inventory sync failed: {exception}");
            }

            var grantedRewards = ToRewards(response.rewards);
            Debug.Log(
                $"Daily bonus reward granted: day={response.day}, rewards={FormatRewards(grantedRewards)}, jackpot={response.jackpot}, multiplier={response.multiplier}, selectedBooster={response.selectedBooster}");

            return DailyBonusClaimResult.Granted(
                state,
                response.day,
                grantedRewards,
                response.jackpot,
                response.multiplier,
                ParseBoosterType(response.selectedBooster));
        }

        private void SetState(DailyBonusState state)
        {
            bool wasAvailable = IsAvailable;
            CurrentState = state ?? DailyBonusState.Unavailable;

            if (IsAvailable && !wasAvailable)
                Debug.Log($"Daily bonus reward available: day={CurrentState.DailyRewardDay}");

            StateChanged?.Invoke(CurrentState);
        }

        private static DailyBonusState ToState(DailyBonusStateDto dto, bool isUnlocked)
        {
            if (dto == null)
                return DailyBonusState.Unavailable;

            return new DailyBonusState(
                dto.dailyRewardDay,
                ParseUtc(dto.lastClaimUtc),
                dto.claimAvailable,
                isUnlocked);
        }

        private static IReadOnlyList<DailyBonusRewardItem> ToRewards(List<DailyBonusRewardDto> rewards)
        {
            if (rewards == null || rewards.Count == 0)
                return Array.Empty<DailyBonusRewardItem>();

            var result = new List<DailyBonusRewardItem>(rewards.Count);
            foreach (var reward in rewards)
            {
                if (reward == null || reward.amount <= 0)
                    continue;

                var boosterType = ParseBoosterType(reward.boosterType);
                if (boosterType == BoosterType.None)
                    continue;

                result.Add(new DailyBonusRewardItem(boosterType, reward.amount));
            }

            return result;
        }

        private static DailyBonusCycle ToCycle(DailyBonusConfigDto config)
        {
            if (config == null)
                return DailyBonusCycle.Empty;

            var days = new List<DailyBonusCycleDay>();
            if (config.days != null)
            {
                foreach (var day in config.days)
                {
                    if (day == null)
                        continue;

                    days.Add(new DailyBonusCycleDay(
                        day.day,
                        day.rewardKind,
                        ToRewards(day.rewards),
                        ToChestDrops(day.chestDrops)));
                }
            }

            return new DailyBonusCycle(config.cycleLength, days);
        }

        private static IReadOnlyList<DailyBonusChestDrop> ToChestDrops(List<DailyBonusChestDropDto> chestDrops)
        {
            if (chestDrops == null || chestDrops.Count == 0)
                return Array.Empty<DailyBonusChestDrop>();

            var result = new List<DailyBonusChestDrop>(chestDrops.Count);
            foreach (var drop in chestDrops)
            {
                if (drop == null)
                    continue;

                result.Add(new DailyBonusChestDrop(
                    drop.weight,
                    drop.mode,
                    drop.multiplier,
                    ToBoosterPool(drop.pool),
                    ToRewards(drop.rewards)));
            }

            return result;
        }

        private static IReadOnlyList<BoosterType> ToBoosterPool(List<string> pool)
        {
            if (pool == null || pool.Count == 0)
                return Array.Empty<BoosterType>();

            var result = new List<BoosterType>(pool.Count);
            foreach (var item in pool)
            {
                var boosterType = ParseBoosterType(item);
                if (boosterType != BoosterType.None)
                    result.Add(boosterType);
            }

            return result;
        }

        private static BoosterType ParseBoosterType(string value)
        {
            return Enum.TryParse(value, ignoreCase: true, out BoosterType boosterType)
                ? boosterType
                : BoosterType.None;
        }

        private static DateTime? ParseUtc(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (!DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dateTime))
                return null;

            return dateTime;
        }

        private static string FormatRewards(IReadOnlyList<DailyBonusRewardItem> rewards)
        {
            if (rewards == null || rewards.Count == 0)
                return "none";

            var parts = new List<string>(rewards.Count);
            foreach (var reward in rewards)
            {
                if (reward == null)
                    continue;

                parts.Add($"{reward.BoosterType}x{reward.Amount}");
            }

            return parts.Count > 0 ? string.Join(", ", parts) : "none";
        }

        [Serializable]
        private sealed class DailyBonusRefreshResponse
        {
            public bool ok;
            public bool starterGranted;
            public DailyBonusStateDto state;
            public DailyBonusConfigDto config;
        }

        [Serializable]
        private sealed class DailyBonusClaimResponse
        {
            public bool ok;
            public bool granted;
            public string error;
            public int day;
            public string selectedBooster;
            public int multiplier;
            public bool jackpot;
            public List<DailyBonusRewardDto> rewards;
            public DailyBonusStateDto state;
            public DailyBonusConfigDto config;
        }

        [Serializable]
        private sealed class DailyBonusStateDto
        {
            public int dailyRewardDay;
            public string lastClaimUtc;
            public bool claimAvailable;
        }

        [Serializable]
        private sealed class DailyBonusRewardDto
        {
            public string boosterType;
            public int amount;
        }

        [Serializable]
        private sealed class DailyBonusConfigDto
        {
            public int cycleLength;
            public List<DailyBonusDayDto> days;
        }

        [Serializable]
        private sealed class DailyBonusDayDto
        {
            public int day;
            public string rewardKind;
            public List<DailyBonusRewardDto> rewards;
            public List<DailyBonusChestDropDto> chestDrops;
        }

        [Serializable]
        private sealed class DailyBonusChestDropDto
        {
            public int weight;
            public string mode;
            public int multiplier;
            public List<string> pool;
            public List<DailyBonusRewardDto> rewards;
        }
    }
}
