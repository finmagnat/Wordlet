using System;
using System.Collections.Generic;
using Inventory;

namespace Core.Data
{
    [Serializable]
    public sealed class ShopPackDto
    {
        public string ProductId;
        public string Title;
        public string Description;

        public string PriceText;   // "114,99 грн" / "$1.99" / "—" если не загружено
        public bool IsAvailable;   // купится ли сейчас

        public IReadOnlyList<ShopRewardDto> Rewards;
    }
    
    [Serializable]
    public sealed class ShopRewardDto
    {
        //public string ItemId;  // "booster_hint", "booster_slowtime", "no_ads"
        public BoosterType ItemId;  // "booster_hint", "booster_slowtime", "no_ads"
        public int Amount;
    }
}