using System;
using System.Globalization;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Services.Common
{
    public abstract class PlayerPrefsWordSubmissionLimitsServiceBase : IService
    {
        public event Action OnStateChanged;

        protected abstract int DailyLimit { get; }
        protected abstract int CooldownSeconds { get; }
        protected abstract string DailyCountKeyPrefix { get; }
        protected abstract string LastSubmitTsKey { get; }

        protected bool disableLimits;
        
        public UniTask InitializeAsync() => UniTask.CompletedTask;

        public WordSubmitAvailability GetAvailability()
        {
            if (DailyLimit > 0)
            {
                int usedToday = GetDailyCount();
                if (usedToday >= DailyLimit)
                {
                    return new WordSubmitAvailability(
                        canSubmit: false,
                        dailyLimitReached: true,
                        remainingCooldownSeconds: 0,
                        remainingDailyResetSeconds: GetSecondsUntilNextUtcDay());
                }
            }

            if (CooldownSeconds > 0)
            {
                int now = NowUnix();
                int last = GetLastSubmitTs();
                int elapsed = now - last;
                int remain = CooldownSeconds - elapsed;

                if (remain > 0)
                {
                    return new WordSubmitAvailability(
                        canSubmit: false,
                        dailyLimitReached: false,
                        remainingCooldownSeconds: remain,
                        remainingDailyResetSeconds: 0);
                }
            }

            return new WordSubmitAvailability(
                canSubmit: true,
                dailyLimitReached: false,
                remainingCooldownSeconds: 0,
                remainingDailyResetSeconds: 0);
        }

        public void RegisterSuccessfulSubmit()
        {
            if (DailyLimit > 0)
                IncrementDailyCount();

            if (CooldownSeconds > 0)
                SetLastSubmitTs(NowUnix());

            OnStateChanged?.Invoke();
        }

        public void ResetLimits(bool disableLimits = false)
        {
            PlayerPrefs.DeleteKey(GetDailyCountKey());
            PlayerPrefs.DeleteKey(LastSubmitTsKey);
            PlayerPrefs.Save();
            
            this.disableLimits = disableLimits;

            OnStateChanged?.Invoke();
        }

        private int GetDailyCount()
            => PlayerPrefs.GetInt(GetDailyCountKey(), 0);

        private void IncrementDailyCount()
        {
            var key = GetDailyCountKey();
            int value = PlayerPrefs.GetInt(key, 0);
            PlayerPrefs.SetInt(key, value + 1);
            PlayerPrefs.Save();
        }

        private int GetLastSubmitTs()
            => PlayerPrefs.GetInt(LastSubmitTsKey, 0);

        private void SetLastSubmitTs(int unix)
        {
            PlayerPrefs.SetInt(LastSubmitTsKey, unix);
            PlayerPrefs.Save();
        }

        private string GetDailyCountKey()
            => $"{DailyCountKeyPrefix}_{TodayKey()}";

        private static int NowUnix()
            => (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        private static string TodayKey()
            => DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        private static int GetSecondsUntilNextUtcDay()
        {
            var now = DateTimeOffset.UtcNow;
            var next = now.Date.AddDays(1);
            return Mathf.Max(0, (int)(next - now).TotalSeconds);
        }
    }
}