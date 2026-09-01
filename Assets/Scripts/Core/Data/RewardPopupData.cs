using System;
using System.Collections.Generic;
using Core.Services;

namespace Core.Data
{
    [Serializable]
    public sealed class RewardPopupData
    {
        public const string SourceShop = "shop";
        public const string SourceDailyBonus = "daily_bonus";
        public const string SourceFinishRound = "finish_round";

        public string Source;
        public string ProductId;
        public int DailyBonusDay;
        public bool DailyBonusJackpot;
        public List<RewardDto> Rewards = new();

        public static RewardPopupData FromShopOffer(ShopOfferDto offer)
        {
            return new RewardPopupData
            {
                Source = SourceShop,
                ProductId = offer?.ProductId,
                Rewards = offer?.Rewards != null
                    ? new List<RewardDto>(offer.Rewards)
                    : new List<RewardDto>()
            };
        }
        
        public static RewardPopupData FromAdsReward(AdsRewardItem data)
        {
            return new RewardPopupData
            {
                Source = SourceFinishRound,
                Rewards = new List<RewardDto>
                { new () {ItemId = data.BoosterType, Amount = data.Count} }
            };
        }
    }
}
