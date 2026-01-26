using UnityEngine;

namespace Core.Build
{
    public static class BuildInfo
    {
        private static BuildInfoSO _cached;

        public static BuildInfoSO Data
        {
            get
            {
                if (_cached == null)
                    _cached = Resources.Load<BuildInfoSO>("BuildInfo");
                return _cached;
            }
        }

        public static string Utc => Data != null ? Data.buildUtc : "UNKNOWN";
        public static string Local => Data != null ? Data.buildLocal : "UNKNOWN";
        public static string UnityVersion => Data != null ? Data.unityVersion : "UNKNOWN";
        public static string Platform => Data != null ? Data.platform : "UNKNOWN";
        public static string Config => Data != null ? Data.configuration : "UNKNOWN";
        public static string VersionName => Data != null ? Data.versionName : Application.version;

        public static int AndroidVersionCode => Data != null ? Data.androidVersionCode : 0;
    }
}