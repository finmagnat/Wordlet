using System;
using System.Collections.Generic;
using Core.Services.Common;
using Cysharp.Threading.Tasks;
using Inventory;
using UnityEngine;
using Zenject;

namespace Core.Services
{
    /// <summary>
    /// Связка: rewarded earned -> PlayFab CloudScript AddBooster -> обновить локальный инвентарь
    /// + локальные лимиты/cooldown (после успешного начисления)
    /// + событие для UI pop
    /// </summary>
    public sealed class RewardedBoosterGrantService : IService
    {
        [Inject] private RewardedAdsService _ads;
        [Inject] private InventorySyncService _inventorySync;
        [Inject] private RewardedLimitsService _limits;
        
        public event Action<RewardType, int> OnRewardGranted; // для UI pop

        private readonly Dictionary<RewardType, int> _pendingPop = new();
        
        public UniTask InitializeAsync()
        {
            _ads.OnRewardEarned += OnRewardEarned;
            _ads.OnClosed += OnAdClosed;
            
            return UniTask.CompletedTask;
        }

        private void OnRewardEarned(RewardType rewardType)
        {
            GrantAsync(rewardType).Forget();
        }
        
        private void OnAdClosed(RewardType type)
        {
            if (_pendingPop.TryGetValue(type, out var amount) && amount > 0)
            {
                _pendingPop[type] = 0;
                OnRewardGranted?.Invoke(type, amount);
            }
        }

        private async UniTaskVoid GrantAsync(RewardType rewardType)
        {
            if (!RewardedBoosterCatalog.TryGetBoosterType(rewardType, out var booster))
            {
                Debug.LogWarning($"[Reward] Unknown rewardType={rewardType}");
                return;
            }

            // ✅ Server-authoritative: CloudScript AddBooster
            bool ok = await _inventorySync.GrantBoosterAsync(booster, 1);

            if (!ok)
            {
                Debug.LogWarning($"[Reward] Failed to grant {booster}.");
                return;
            }

            Debug.Log($"[Reward] Granted +1 {booster} (server).");

            // ✅ важно: cooldown/daily лимит — только после успешного начисления
            _limits.RegisterSuccessfulClaim(rewardType);
            
            // ⚠️ не показываем поп "+N" сейчас, только “помечаем”
            _pendingPop[rewardType] = _pendingPop.TryGetValue(rewardType, out var v) ? v + 1 : 1;

            // ✅ UI pop +1
            OnRewardGranted?.Invoke(rewardType, 1);
        }

    }
}
