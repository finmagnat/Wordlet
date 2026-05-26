using System;
using System.Collections.Generic;

namespace Core.Data
{
    [Serializable]
    public sealed class RewardPopupData
    {
        public const string SourceShop = "shop";
        public const string SourceDailyBonus = "daily_bonus";

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
    }
}
