using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.DebugTools
{
    /// <summary>
    /// Мост между SRDebugger (SROptions) и Zenject-сервисами.
    /// </summary>
    public sealed class SRDebugAdsBridge : MonoBehaviour
    {
        public static SRDebugAdsBridge Instance { get; private set; }

        [Inject] private AdsEntitlementService _entitlement;
        [Inject] private InterstitialAdsService _interstitialAds;
        [Inject] private InterstitialPolicyService _policy;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool NoInterstitialAds => _entitlement.NoInterstitialAds;

        public void SetNoInterstitialAds(bool value)
        {
            // Важно: применится локально сразу, а сервер запишется async.
            _entitlement.SetNoInterstitialAdsLocal(value);
            _entitlement.SetNoInterstitialAdsAsync(value).Forget();
        }

        public void SyncEntitlements()
        {
            _entitlement.SyncFromServerAsync().Forget();
        }

        public void ForceLoadInterstitial()
        {
            _interstitialAds.EnsureLoaded();
        }

        public void ForceShowInterstitialNow()
        {
            // Policy должен уважать NoInterstitialAds.
            _policy.TryShow("srdebug_force");
        }

        public void ClearLocalAdsPrefs()
        {
            // Если у тебя ключи другие — поправим тут.
            PlayerPrefs.DeleteKey(AdsEntitlementService.KeyNoInterstitialAds);  // локальный кэш entitlements
            PlayerPrefs.Save();
        }
    }
}