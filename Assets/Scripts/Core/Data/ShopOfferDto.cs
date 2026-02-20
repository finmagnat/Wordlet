using System;
using System.Collections.Generic;
using Core.Services;
using Inventory;

namespace Core.Data
{
    public enum ShopOfferTypeDto { IapPack = 0, RewardedAd = 1 }
    
    [Serializable]
    public sealed class ShopOfferDto
    {
        public ShopOfferTypeDto Type;

        public string ProductId;      // для IAP
        public RewardType RewardType; // для Rewarded

        public string Title;
        public string Description;

        public string CtaText;        // "19.99" или "Смотреть"
        public bool IsAvailable;

        public List<ShopRewardDto> Rewards = new();
    }
    
    [Serializable]
    public sealed class ShopRewardDto
    {
        //public string ItemId;  // "booster_hint", "booster_slowtime", "no_ads"
        public BoosterType ItemId;  // "booster_hint", "booster_slowtime", "no_ads"
        public int Amount;
    }
}