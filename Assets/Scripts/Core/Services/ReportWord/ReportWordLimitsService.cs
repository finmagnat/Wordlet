using System;
using System.Globalization;
using Core.Config;
using Zenject;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Services.ReportWord
{
    public sealed class ReportWordLimitsService : IService, IReportWordLimitsService
    {
        [Inject] private IConfigService _configs;
        private GameConfig _game => _configs.Game;

        public event Action OnStateChanged;

        public UniTask InitializeAsync() => UniTask.CompletedTask;

        public ReportWordSubmitAvailability GetAvailability()
        {
            int cooldownRemain = 0;
            bool dailyReached = false;
            int dailyResetRemain = 0;

            int dailyLimit = _game.reportWordsDailyLimit;
            int cooldownSeconds = _game.reportWordsCooldownSeconds;

            // Daily limit
            if (dailyLimit > 0)
            {
                int usedToday = GetDailyCount();
                if (usedToday >= dailyLimit)
                {
                    dailyReached = true;
                    dailyResetRemain = GetSecondsUntilNextUtcDay();
                    return new ReportWordSubmitAvailability(
                        canSubmit: false,
                        dailyLimitReached: true,
                        remainingCooldownSeconds: 0,
                        remainingDailyResetSeconds: dailyResetRemain);
                }
            }

            // Cooldown
            if (cooldownSeconds > 0)
            {
                int now = NowUnix();
                int last = GetLastSubmitTs();
                int elapsed = now - last;
                int remain = cooldownSeconds - elapsed;

                if (remain > 0)
                {
                    cooldownRemain = remain;
                    return new ReportWordSubmitAvailability(
                        canSubmit: false,
                        dailyLimitReached: false,
                        remainingCooldownSeconds: cooldownRemain,
                        remainingDailyResetSeconds: 0);
                }
            }

            return new ReportWordSubmitAvailability(
                canSubmit: true,
                dailyLimitReached: false,
                remainingCooldownSeconds: 0,
                remainingDailyResetSeconds: 0);
        }

        public void RegisterSuccessfulSubmit()
        {
            if (_game.reportWordsDailyLimit > 0)
                IncDailyCount();

            if (_game.reportWordsCooldownSeconds > 0)
                SetLastSubmitTs(NowUnix());

            OnStateChanged?.Invoke();
        }

        private static int NowUnix()
            => (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        private static string TodayKey()
            => DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        private static string KeyDailyCount()
            => $"new_words_daily_{TodayKey()}";

        private static string KeyLastSubmitTs()
            => "new_words_last_submit_ts";

        private static int GetDailyCount()
            => PlayerPrefs.GetInt(KeyDailyCount(), 0);

        private static void IncDailyCount()
        {
            var key = KeyDailyCount();
            int value = PlayerPrefs.GetInt(key, 0);
            PlayerPrefs.SetInt(key, value + 1);
            PlayerPrefs.Save();
        }

        private static int GetLastSubmitTs()
            => PlayerPrefs.GetInt(KeyLastSubmitTs(), 0);

        private static void SetLastSubmitTs(int unix)
        {
            PlayerPrefs.SetInt(KeyLastSubmitTs(), unix);
            PlayerPrefs.Save();
        }

        private static int GetSecondsUntilNextUtcDay()
        {
            var now = DateTimeOffset.UtcNow;
            var next = now.Date.AddDays(1);
            return Mathf.Max(0, (int)(next - now).TotalSeconds);
        }
    }
}