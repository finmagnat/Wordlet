using System;
using System.Collections.Generic;
using Core.Services;
using Inventory;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(menuName = "Wordlet/Shop/Shop Catalog", fileName = "ShopCatalog")]
    public sealed class ShopCatalog : ScriptableObject
    {
        [Header("Config Service")]
        public bool EnablePurchasePushToServer = true;

        [Header("Offers")]
        public List<ShopOfferConfig> Offers = new();
    }

    public enum ShopOfferType
    {
        IapPack = 0,
        RewardedAd = 1
    }

    [Serializable]
    public sealed class ShopOfferConfig
    {
        public ShopOfferType Type;

        [Header("Common")]
        public string Title;
        [TextArea] public string Description;
        public List<ShopRewardConfig> Rewards = new();

        [Header("IAP only")]
        public string ProductId; // только для IAP

        // Заглушка для Этапа 1 (пока стора нет)
        public string DebugPriceText = "—";
        public bool DebugAvailable = true;

        [Header("Rewarded Ad only")]
        public RewardType RewardType;   // Letter / Slowdown (какой ad unit дергать)
        public int DailyLimit = 20;     // лимит на “фарм”
        public int CooldownSeconds = 60; // анти-спам
        
        [Header("Interstitial Ads only")]
        public bool DisableInterstitialAds; // ✅ remove ads purchase (только interstitial)
        public bool IsNonConsumable = true; // ✅ для remove ads обычно true
    }

    [Serializable]
    public sealed class ShopRewardConfig
    {
        public BoosterType ItemId;
        public int Amount;
    }
    
}