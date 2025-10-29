using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(menuName = "Config/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("UI")]
        [Tooltip("Базовое разрешение для Canvas Scaler (ландшафт)")]
        public Vector2Int referenceResolution = new Vector2Int(1920, 1080);

        [Range(0f, 1f)]
        [Tooltip("Матчинг ширины/высоты для Canvas Scaler (0=по ширине, 1=по высоте)")]
        public float screenMatch = 0.5f;

        [Header("Application")]
        public int targetFps = 60;

        [Header("Services")]
        public bool enableAnalytics = true;
        public bool enableAds = true;
        public bool useDebugLogs = true;

        [Tooltip("PlayFab Title Id")]
        public string playFabTitleId;

        [Tooltip("Firebase Realtime DB URL")]
        public string firebaseRealtimeDbUrl;

        [Header("Ads (Unity Ads, при необходимости)")]
        public string unityAdsGameIdAndroid;
        public bool unityAdsTestMode = true;

        [Header("Gameplay")]
        [Tooltip("Размер поля 'Балды' по умолчанию")]
        public int defaultBoardSize = 5;

        [Header("Content (Addressables keys / labels)")]
        [Tooltip("Ключ/лейбл для словаря, если храним его через Addressables")]
        public string dictionaryAddress;
    }
}