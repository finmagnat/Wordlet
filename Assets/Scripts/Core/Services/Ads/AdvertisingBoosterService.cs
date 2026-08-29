using System;
using Core.Config;
using Core.Services.Inventory;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.Services
{
    public sealed class AdvertisingBoosterService : IService
    {
        [Inject] private RewardedAdsService _ads;
        [Inject] private IConfigService _configs;
        [Inject] private InventorySyncService _inventorySync;
        
        public UniTask InitializeAsync()
        {

            return UniTask.CompletedTask;
        }

        public AdsRewardItem GetData()
        {
            int totalWeight = 0;

            foreach (AdsRewardItem item in _configs.Ads.AdsRewardItems)
                totalWeight += Mathf.Max(0, item.Weight);

            if (totalWeight == 0)
                throw new InvalidOperationException(
                    "At least one reward must have a positive weight.");

            int roll = UnityEngine.Random.Range(0, totalWeight);

            foreach (AdsRewardItem item in _configs.Ads.AdsRewardItems)
            {
                int weight = Mathf.Max(0, item.Weight);

                if (roll < weight)
                    return item;

                roll -= weight;
            }

            throw new InvalidOperationException("Failed to select a reward.");
        }

        public void Execute(AdsRewardItem data)
        {
            if (data == null)
            {
                Debug.LogWarning("[AdvertisingBooster] Cannot show an ad for a null reward.");
                return;
            }

            if (data.RewardType == RewardType.None || data.BoosterType == BoosterType.None || data.Count <= 0)
            {
                Debug.LogWarning(
                    $"[AdvertisingBooster] Invalid reward config: rewardType={data.RewardType}, " +
                    $"boosterType={data.BoosterType}, count={data.Count}.");
                return;
            }

            if (!RewardedBoosterCatalog.TryGetPlayFabKey(data.BoosterType, out _))
            {
                Debug.LogError(
                    $"[AdvertisingBooster] No PlayFab inventory key configured for {data.BoosterType}. " +
                    "The ad will not be shown because its reward cannot be granted.");
                return;
            }

            BoosterType boosterType = data.BoosterType;
            int count = data.Count;

            _ads.ShowFor(data.RewardType, _ => GrantRewardAsync(boosterType, count).Forget());
        }

        private async UniTaskVoid GrantRewardAsync(BoosterType boosterType, int count)
        {
            try
            {
                bool granted = await _inventorySync.GrantBoosterAsync(boosterType, count);

                if (granted)
                {
                    Debug.Log($"[AdvertisingBooster] Granted +{count} {boosterType} (server).");
                    return;
                }

                Debug.LogWarning($"[AdvertisingBooster] Failed to grant +{count} {boosterType}.");
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[AdvertisingBooster] Failed to grant +{count} {boosterType}: {exception}");
            }
        }
    }
}
