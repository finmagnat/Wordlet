using System.Collections.Generic;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(menuName = "Wordlet/Config/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Address")]
        public string Privacy = "https://finmagnat.github.io/wordlet-privacy/";
        public string Terms = "https://finmagnat.github.io/wordlet-terms/";
        public string Support = "semantica.dev@gmail.com";
        
        [Header("UI")]
        [Tooltip("Базовое разрешение для Canvas Scaler (ландшафт)")]
        public Vector2Int referenceResolution = new Vector2Int(1920, 1080);

        [Range(0f, 1f)]
        [Tooltip("Матчинг ширины/высоты для Canvas Scaler (0=по ширине, 1=по высоте)")]
        public float screenMatch = 0.5f;

        [Header("APPLICATION")]
        public int targetFps = 60;
        
        [Header("SERVICES")]
        public bool enableAnalytics = true;
        public bool enableAds = true;
        public bool useDebugLogs = true;

        [Tooltip("Firebase Realtime DB URL")]
        public string firebaseRealtimeDbUrl;

        [Header("ADS (Unity Ads, при необходимости)")]
        public string unityAdsGameIdAndroid;
        public bool unityAdsTestMode = true;

        [Header("GAMEPLAY")]
        [Tooltip("Версия игровой механики (влияет на пользовательскиие сейвы)")]
        public int version = 1;
        
        [Tooltip("Размер поля по умолчанию")]
        public uint defaultBoardSize = 5;
        
        [Space(10)]
        [Tooltip("Время хода игрока по умолчанию (секунды)")]
        public int durationGameByDefault = 60;
        [Tooltip("Время хода игрока минимальное (секунды)")]
        public int durationGameMinimum = 10;
        [Tooltip("Время хода игрока максимальное (секунды)")]
        public int durationGameMaximum = 120;
        
        [Tooltip("Оппонент игрока по умолчанию")]
        public GameOpponent gameOpponentByDefault = GameOpponent.AI;
        
        [Tooltip("Максимум пропуска ходов (в режиме игры с человеком)")]
        public uint maxPassesByDefault = 2;
        
        [Header("СЛОЖНОСТЬ ИГРЫ С ИИ")]
        [Tooltip("Сложность игры с ИИ по умолчанию")]
        public ComplexityAI complexityAiByDefault = ComplexityAI.NORMAL;
        
        [Space(10), Tooltip("Уровни сложности игры с ИИ")]
        public List<ComplexityAISettings> _сomplexityAIList;
        
        public ComplexityAISettings GetComplexityAIItem(ComplexityAI сomplexityAI) =>
            _сomplexityAIList.Find(item => item.СomplexityAiLevel == сomplexityAI);
        
        [Header("БУСТЕРЫ")]
        [Tooltip("Время замедления игрового таймера (секунды)")]
        public int slowdownDelay;
        
        [Header("LEADERBOARD")]
        [Tooltip("Сколько строк показывать в Top-N")]
        public int leaderboardTopN = 10;

        [Tooltip("Кэш Score (секунды), чтобы не спамить PlayFab")]
        public int scoreCacheSeconds = 15;
        
        [Header("NEW WORDS MODERATION")]
        [Tooltip("Максимум отправок новых слов в сутки. 0 = безлимит")]
        public int newWordsDailyLimit = 5;

        [Tooltip("Кулдаун между отправками новых слов (секунды). 0 = выкл")]
        public int newWordsCooldownSeconds = 90;

        [Tooltip("Минимальная длина нового слова для отправки")]
        public int newWordsMinLength = 2;

        [Tooltip("Максимальная длина нового слова для отправки")]
        public int newWordsMaxLength = 32;
    }
}