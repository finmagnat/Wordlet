using System.Collections.Generic;
using UnityEngine;

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
        }
        
        public static class Parameter
        {
            public const string WaitTimeMs = "wait_time_ms";
            public const string WaitTimeSeconds = "wait_time_seconds";
            public const string Banner = "banner";
            public const string Locale = "locale";
            public const string Skin = "skin";
        }
        
        public static class Option
        {
            
        }
        
        public static Dictionary<string, object> GetWaitTimeParams()
        {
            int waitTimeMs = Mathf.RoundToInt((Time.realtimeSinceStartup - AppLaunchTracker.LaunchRealtimeSinceStartup) * 1000f);
            return new Dictionary<string, object>
            {
                [Parameter.WaitTimeMs] = waitTimeMs,
                [Parameter.WaitTimeSeconds] = waitTimeMs / 1000f
            };
        }
    }
}
