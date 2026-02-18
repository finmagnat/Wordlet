using Cysharp.Threading.Tasks;
using Inventory;
using UnityEngine;
using Zenject;

namespace Core.Services
{
    /// <summary>
    /// Связка: rewarded earned -> PlayFab CloudScript AddBooster -> обновить локальный инвентарь
    /// </summary>
    public sealed class RewardedBoosterGrantService : IService
    {
        [Inject] private RewardedAdsService _ads;
        [Inject] private InventorySyncService _inventorySync;

        public UniTask InitializeAsync()
        {
            _ads.OnRewardEarned += OnRewardEarned;
            return UniTask.CompletedTask;
        }

        private void OnRewardEarned(RewardType rewardType)
        {
            // Коллбек может прийти в “неудобный” момент — уводим в UniTask
            GrantAsync(rewardType).Forget();
        }

        private async UniTaskVoid GrantAsync(RewardType rewardType)
        {
            var booster = Map(rewardType);
            if (booster == null)
            {
                Debug.LogWarning($"[Reward] Unknown rewardType={rewardType}");
                return;
            }

            // ✅ Server-authoritative: CloudScript AddBooster
            bool ok = await _inventorySync.GrantBoosterAsync(booster.Value, 1);

            Debug.Log(ok
                ? $"[Reward] Granted +1 {booster.Value} (server)."
                : $"[Reward] Failed to grant {booster.Value}.");
        }

        private static BoosterType? Map(RewardType rewardType)
        {
            return rewardType switch
            {
                RewardType.Letter   => BoosterType.Letter,
                RewardType.Slowdown => BoosterType.Slowdown,
                _ => (BoosterType?)null
            };
        }
    }
}