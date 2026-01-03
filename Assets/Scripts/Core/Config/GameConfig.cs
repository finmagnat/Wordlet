using System.Collections.Generic;
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
        [Tooltip("Версия игровой механики (влияет на пользовательскиие сейвы)")]
        public int version = 1;
        
        [Tooltip("Размер поля по умолчанию")]
        public uint defaultBoardSize = 5;
        
        [Space(10)]
        [Tooltip("Время хода игрока по умолчанию (секунды)")]
        public int durationGameByDefault = 60;
        
        [Tooltip("Оппонент игрока по умолчанию")]
        public GameOpponent gameOpponentByDefault = GameOpponent.AI;
        
        [Tooltip("Максимум пропуска ходов (в режиме игры с человеком)")]
        public uint maxPassesByDefault = 2;
        
        [Header("Сложность игры с ИИ")]
        [Tooltip("Сложность игры с ИИ по умолчанию")]
        public ComplexityAI complexityAiByDefault = ComplexityAI.NORMAL;
        
        [Space(10), Tooltip("Уровни сложности игры с ИИ")]
        public List<ComplexityAISettings> _сomplexityAIList;
        
        public ComplexityAISettings GetComplexityAIItem(ComplexityAI сomplexityAI) =>
            _сomplexityAIList.Find(item => item.СomplexityAiLevel == сomplexityAI);
    }
}