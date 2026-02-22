using System;
using System.Globalization;
using Core.Config;
using UnityEngine;
using Zenject;

namespace Core.Services
{
    /// <summary>
    /// Локальные лимиты Rewarded (MVP):
    /// - DailyLimit: 0 = безлимит
    /// - CooldownSeconds: 0 = выкл
    /// Хранение: PlayerPrefs
    /// </summary>
    public sealed class RewardedLimitsService : IService
    {
        [Inject] private IConfigService _configs;
        private ShopCatalog _shop => _configs.Shop;

        public event Action<RewardType> OnStateChanged;

        public Cysharp.Threading.Tasks.UniTask InitializeAsync()
            => Cysharp.Threading.Tasks.UniTask.CompletedTask;

        public bool CanClaim(RewardType type, out int remainingCooldownSeconds, out bool dailyLimitReached)
        {
            remainingCooldownSeconds = 0;
            dailyLimitReached = false;

            var cfg = GetConfig(type);
            if (cfg == null)
                return true; // если оффер не найден — не блокируем

            // Daily limit
            if (cfg.DailyLimit > 0)
            {
                int used = GetDailyCount(type);
                if (used >= cfg.DailyLimit)
                {
                    dailyLimitReached = true;
                    return false;
                }
            }

            // Cooldown
            if (cfg.CooldownSeconds > 0)
            {
                int now = NowUnix();
                int last = GetLastClaimTs(type);
                int elapsed = now - last;
                int remain = cfg.CooldownSeconds - elapsed;
                if (remain > 0)
                {
                    remainingCooldownSeconds = remain;
                    return false;
                }
            }

            return true;
        }

        /// <summary>Вызывать только после УСПЕШНОГО начисления награды (у тебя: GrantBoosterAsync ok).</summary>
        public void RegisterSuccessfulClaim(RewardType type)
        {
            var cfg = GetConfig(type);
            if (cfg == null)
                return;

            // Daily count
            if (cfg.DailyLimit > 0)
                IncDailyCount(type);

            // Cooldown
            if (cfg.CooldownSeconds > 0)
                SetLastClaimTs(type, NowUnix());

            OnStateChanged?.Invoke(type);
        }

        // ---------------- internals ----------------

        private ShopOfferConfig GetConfig(RewardType type)
        {
            // Берём конфиг из ShopCatalog: offer.Type == RewardedAd && offer.RewardType == type
            var offers = _shop?.Offers;
            if (offers == null) return null;

            for (int i = 0; i < offers.Count; i++)
            {
                var o = offers[i];
                if (o == null) continue;
                if (o.Type != ShopOfferType.RewardedAd) continue;
                if (o.RewardType != type) continue;
                return o;
            }
            return null;
        }

        private static int NowUnix()
            => (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        private static string TodayKey()
            => DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        private static string KeyDailyCount(RewardType type)
            => $"rewarded_daily_{type}_{TodayKey()}";

        private static string KeyLastClaimTs(RewardType type)
            => $"rewarded_last_claim_ts_{type}";

        private static int GetDailyCount(RewardType type)
            => PlayerPrefs.GetInt(KeyDailyCount(type), 0);

        private static void IncDailyCount(RewardType type)
        {
            var key = KeyDailyCount(type);
            int v = PlayerPrefs.GetInt(key, 0);
            PlayerPrefs.SetInt(key, v + 1);
            PlayerPrefs.Save();
        }

        private static int GetLastClaimTs(RewardType type)
            => PlayerPrefs.GetInt(KeyLastClaimTs(type), 0);

        private static void SetLastClaimTs(RewardType type, int unix)
        {
            PlayerPrefs.SetInt(KeyLastClaimTs(type), unix);
            PlayerPrefs.Save();
        }
    }
}