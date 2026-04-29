using System.Collections.Generic;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(menuName = "Wordlet/Config/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        //---------------------------------
        [Header("Address")]
        public string Privacy = "https://finmagnat.github.io/wordlet-privacy/";
        public string Terms = "https://finmagnat.github.io/wordlet-terms/";
        public string Support = "semantica.dev@gmail.com";
        
        //---------------------------------
        [Header("UI")]
        [Tooltip("Базовое разрешение для Canvas Scaler (ландшафт)")]
        public Vector2Int referenceResolution = new Vector2Int(1080, 1920);

        [Range(0f, 1f)]
        [Tooltip("Матчинг ширины/высоты для Canvas Scaler (0=по ширине, 1=по высоте)")]
        public float screenMatch = 0.5f;

        //---------------------------------
        [Header("APPLICATION")]
        public int targetFps = 60;
        
        //---------------------------------
        [Header("SERVICES")]
        public bool enableAnalytics = true;
        public bool enableAds = true;
        public bool useDebugLogs = true;

        [Tooltip("Firebase Realtime DB URL")]
        public string firebaseRealtimeDbUrl;

        [Header("ADS (Unity Ads, при необходимости)")]
        public string unityAdsGameIdAndroid;
        public bool unityAdsTestMode = true;

        //---------------------------------
        [Header("GAMEPLAY")]
        [Tooltip("Версия игровой механики (влияет на пользовательскиие сейвы)")]
        public int version = 1;
        
        [Tooltip("Размер поля по умолчанию")]
        public uint defaultBoardSize = 5;
        
        [Space(10)]
        [Tooltip("Время хода игрока (секунды)")]
        public int durationGameSeconds = 120;
        
        [Tooltip("Максимум пропуска ходов (в режиме игры с человеком). 0 = безлимит")]
        public uint maxPassesByDefault = 2;
        
        [Tooltip("Кулдаун между переключением паузы (секунды). 0 = выкл")]
        public int pauseCooldownSeconds = 1;
        
        [Tooltip("Задержка перед ходом ИИ (секунды). 0 = выкл")]
        public float delayAIPlaySeconds = 1.0f;
        
        //---------------------------------
        [Header("СЛОЖНОСТЬ ИГРЫ С ИИ")]
        [Tooltip("Сложность игры с ИИ по умолчанию")]
        public ComplexityAI complexityAiByDefault = ComplexityAI.NORMAL;
       
        [Space(10), Tooltip("Уровни сложности игры с ИИ")]
        public List<ComplexityAISettings> _сomplexityAIList;
        
        public ComplexityAISettings GetComplexityAIItem(ComplexityAI сomplexityAI) =>
            _сomplexityAIList.Find(item => item.СomplexityAiLevel == сomplexityAI);
        
        //---------------------------------
        [Header("БУСТЕРЫ")]
        [Tooltip("Время замедления игрового таймера (секунды)")]
        public int slowdownDelay;
        [Tooltip("Время перед переходом хода, после найденного бустером слова (секунды)")]
        public float autoApplyDelay = 2;
        [Tooltip("Настройки алгоритма поиска слов для бустера")]
        public ComplexityAISettings boosterLetterAiSettings;
        
        //---------------------------------
        [Header("LEADERBOARD")]
        [Tooltip("Сколько строк показывать в Top-N")]
        public int leaderboardTopN = 10;

        [Tooltip("Кэш Score (секунды), чтобы не спамить PlayFab")]
        public int scoreCacheSeconds = 15;
        
        //---------------------------------
        [Header("NEW WORDS MODERATION")]
        [Tooltip("Максимум отправок новых слов в сутки. 0 = безлимит")]
        public int newWordsDailyLimit = 5;

        [Tooltip("Кулдаун между отправками новых слов (секунды). 0 = выкл")]
        public int newWordsCooldownSeconds = 90;
        
        //---------------------------------
        [Header("REPORT WORDS MODERATION")]
        [Tooltip("Максимум отправок жалоб на слова в сутки. 0 = безлимит")]
        public int reportWordsDailyLimit = 5;

        [Tooltip("Кулдаун между отправками жалоб на слова (секунды). 0 = выкл")]
        public int reportWordsCooldownSeconds = 90;

        //---------------------------------
        [Tooltip("Минимальное время отображения загрузочного экрана с баннерами (миллисекунды)")]
        public int minLoadingScreenDurationMs = 2000;
    }
}