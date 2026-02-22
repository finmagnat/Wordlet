using System.ComponentModel;
using Core.DebugTools;

public partial class SROptions
{
    [Category("Dev/Ads")]
    [DisplayName("No Interstitial Ads (local+server)")]
    public bool NoInterstitialAds
    {
        get => SRDebugAdsBridge.Instance != null && SRDebugAdsBridge.Instance.NoInterstitialAds;
        set
        {
            if (SRDebugAdsBridge.Instance == null) return;
            SRDebugAdsBridge.Instance.SetNoInterstitialAds(value);
        }
    }

    [Category("Dev/Ads")]
    [DisplayName("Sync Entitlements From Server")]
    public void SyncEntitlementsFromServer()
    {
        SRDebugAdsBridge.Instance?.SyncEntitlements();
    }

    [Category("Dev/Ads")]
    [DisplayName("Force Load Interstitial")]
    public void ForceLoadInterstitial()
    {
        SRDebugAdsBridge.Instance?.ForceLoadInterstitial();
    }

    [Category("Dev/Ads")]
    [DisplayName("Force Show Interstitial Now")]
    public void ForceShowInterstitialNow()
    {
        SRDebugAdsBridge.Instance?.ForceShowInterstitialNow();
    }

    [Category("Dev/Ads")]
    [DisplayName("Clear Local Ads Prefs")]
    public void ClearLocalAdsPrefs()
    {
        SRDebugAdsBridge.Instance?.ClearLocalAdsPrefs();
    }
}