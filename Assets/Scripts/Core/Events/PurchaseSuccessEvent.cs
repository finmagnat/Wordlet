using Core.Data;

namespace Core.Events
{
    public class PurchaseSuccessEvent : IGameEvent
    {
        public ShopOfferDto Offer { get; private set; }

        public PurchaseSuccessEvent(ShopOfferDto offerDto)
        {
            Offer = offerDto;
        }
    }
}