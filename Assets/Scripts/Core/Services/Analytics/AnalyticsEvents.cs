namespace Core.Services
{
    public static class AnalyticsEvents
    {
        public static class Startup
        {
            public const string Group = "startup:";
            
            public const string LoadingStarted = Group + "start_loading_started";
            public const string LoadingInternetConnectionRestored = Group + "internet_connection_restored";
            public const string LoadingCompleted = Group + "start_loading_completed";
        }
        
        public static class Navigation
        {
            public const string Group = "navigation:";
            
            public const string MainMenuShown = Group + "main_menu_shown";
            public const string PlayMainMenuClicked = Group + "play_main_menu_clicked";
            public const string ContinueMainMenuClicked = Group + "continue_main_menu_clicked";
            public const string SettingsMainMenuClicked = Group + "settings_main_menu_clicked";
            public const string SkinsMainMenuClicked = Group + "skins_main_menu_clicked";
            public const string InfoMainMenuClicked = Group + "info_main_menu_clicked";
            public const string ShopMainMenuClicked = Group + "shop_main_menu_clicked";
            
            public const string SettingsPopupShown = Group + "settings_popup_shown";
            public const string CloseSettingsClicked = Group + "close_settings_clicked";
            public const string LocaleSettingsClicked = Group + "locale_settings_clicked";
            public const string PrivacyPolicySettingsClicked = Group + "privacy_policy_settings_clicked";
            public const string TermsOfServiceSettingsClicked = Group + "terms_of_service_settings_clicked";
            public const string SupportSettingsClicked = Group + "support_settings_clicked";
            
            public const string SkinsPopupShown = Group + "skins_popup_shown";
            public const string CloseSkinsClicked = Group + "close_skins_clicked";
            public const string ApplySkinsClicked = Group + "apply_skins_clicked";
            
            public const string InfoPopupShown = Group + "info_popup_shown";
            public const string CloseInfoClicked = Group + "close_info_clicked";
            
            public const string ShopPopupShown = Group + "shop_popup_shown";
            public const string CloseShopClicked = Group + "close_shop_clicked";
            public const string RewardPopupShown = Group + "reward_popup_shown";
            public const string CloseRewardClicked = Group + "close_reward_clicked";
            public const string OkRewardClicked = Group + "ok_reward_clicked";
            public const string NoAdsPopupShown = Group + "no_ads_popup_shown";
            public const string CloseNoAdsClicked = Group + "close_no_ads_clicked";
            public const string OkNoAdsClicked = Group + "ok_no_ads_clicked";
        }

        public static class Monetization
        {
            public const string Group = "monetization:";
            
            public const string IapOfferShopClicked = Group + "iap_offer_shop_clicked";
            public const string AdOfferShopClicked = Group + "ad_offer_shop_clicked";
            public const string RemoveAdOfferShopClicked = Group + "remove_ad_offer_shop_clicked";
        }
        
        public static class Ads
        {
            public const string Group = "ads:";
            
            public const string RewardedAvailability = Group + "rewarded_availability";
            public const string RewardedLoadStart = Group + "rewarded_load_start";
            public const string RewardedLoadSuccess = Group + "rewarded_load_success";
            public const string RewardedLoadFailed = Group + "rewarded_load_failed";
            public const string RewardedShowAttempt = Group + "rewarded_show_attempt";
            public const string RewardedShowStart = Group + "rewarded_show_start";
            public const string RewardedShowFailed = Group + "rewarded_show_failed";
            public const string RewardedEarned = Group + "rewarded_earned";
            public const string RewardedClosed = Group + "rewarded_closed";

            public const string InterstitialLoadStart = Group + "interstitial_load_start";
            public const string InterstitialLoadSuccess = Group + "interstitial_load_success";
            public const string InterstitialLoadFailed = Group + "interstitial_load_failed";
            public const string InterstitialShowAttempt = Group + "interstitial_show_attempt";
            public const string InterstitialShowStart = Group + "interstitial_show_start";
            public const string InterstitialShowFailed = Group + "interstitial_show_failed";
            public const string InterstitialClosed = Group + "interstitial_closed";
        }
        
        public static class Parameter
        {
            public const string WaitTimeMs = "wait_time_ms";
            public const string WaitTimeSeconds = "wait_time_seconds";
            public const string Banner = "banner";
            public const string Locale = "locale";
            public const string Skin = "skin";
            public const string Sound = "sound";
            public const string Giro = "giro";
            public const string ProductId = "product_id";
            public const string Reward = "reward";
            public const string Price = "price";
            public const string LimitRemain = "limit_remain";
            public const string Result = "result";
            public const string RewardType = "reward_type";
            public const string Placement = "placement";
            public const string IsReady = "is_ready";
            public const string IsLoading = "is_loading";
            public const string Cooldown = "cooldown";
            public const string DailyLimitReached = "daily_limit_reached";
            public const string LoadTimeMs = "load_time_ms";
            public const string LoadReason = "load_reason";
            public const string Error = "error";
            public const string WasRewarded = "was_rewarded";
        }
        
        public static class Option
        {
            public const string On = "on";
            public const string Off = "off";
            public const string Success = "success";
            public const string NotReady = "not_ready";
            public const string Cooldown = "cooldown";
            public const string Limit = "limit";
            public const string NoFill = "no_fill";
            public const string Timeout = "timeout";
            public const string Network = "network";
            public const string InternalError = "internal_error";
            public const string InvalidRequest = "invalid_request";
            public const string Disabled = "disabled";
            public const string TooEarly = "too_early";
            public const string Unknown = "unknown";
            public const string Initial = "initial";
            public const string ReloadOnDemand = "reload_on_demand";
            public const string ReloadAfterClose = "reload_after_close";
            public const string ReloadAfterFail = "reload_after_fail";
        }

        public static class Placement
        {
            public const string RepeatGame = "repeat_game";
            public const string ExitGame = "exit_game";
            public const string Global = "global";
        }
    }
}
