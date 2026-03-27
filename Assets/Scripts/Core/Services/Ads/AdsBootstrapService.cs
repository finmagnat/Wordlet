using GoogleMobileAds.Api;
using UnityEngine;

namespace Core.Services
{
    public sealed class AdsBootstrapService : IInitializable
    {
        public void Initialize()
        {
#if UNITY_ANDROID || UNITY_IOS
            MobileAds.Initialize(_ =>
            {
                Debug.Log("[Ads] MobileAds initialized.");
            });
#else
            Debug.Log("[Ads] Skipping MobileAds init on this platform.");
#endif
        }
    }
}