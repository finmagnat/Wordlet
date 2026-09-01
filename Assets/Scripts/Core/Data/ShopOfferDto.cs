using System;
using System.Collections.Generic;
using Core.Config;
using UnityEngine;

namespace Core.Data
{
    public enum ShopOfferTypeDto
    {
        IapPack = 0, 
        RewardedAd = 1
    }
    
    [Serializable]
    public sealed class ShopOfferDto
    {
        public ShopOfferTypeDto Type;

        public string ProductId;      // для IAP
        public RewardType RewardType; // для Rewarded

        public string Title;
        public Sprite SpriteHeader;
        public string Description;

        public string CtaText;        // "19.99" или "Смотреть"
        public bool IsAvailable;
        public bool IsDisableInterstitialAds;
        
        public List<RewardDto> Rewards = new();
    }
    
    [Serializable]
    public sealed class RewardDto
    {
        public BoosterType ItemId;
        public int Amount;
    }
}
