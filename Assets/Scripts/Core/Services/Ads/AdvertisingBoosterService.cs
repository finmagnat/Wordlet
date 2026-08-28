using Cysharp.Threading.Tasks;

namespace Core.Services
{
    public sealed class AdvertisingBoosterService : IService
    {
        private readonly RewardedAdsService _ads;
        
        public UniTask InitializeAsync()
        {

            return UniTask.CompletedTask;
        }

        public AdsRewardItem GetData()
        {
            throw new System.NotImplementedException();
        }

        public void Exequte(AdsRewardItem data)
        {
            throw new System.NotImplementedException();
        }
    }
}
