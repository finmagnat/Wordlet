namespace Core.Services
{
    public static class AnalyticsEvents
    {
        public static class Startup
        {
            public const string MainMenuShown = "startup:main_menu_shown";
            public const string WaitTimeMs = "wait_time_ms";
            public const string WaitTimeSeconds = "wait_time_seconds";
        }
    }
}
