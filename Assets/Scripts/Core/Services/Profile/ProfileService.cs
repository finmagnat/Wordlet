using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using PlayFab.ClientModels;
using UnityEngine;

namespace Core.Services
{
    public sealed class ProfileService : IProfileService
    {
        public const string StatScore = "Score";

        private readonly PlayFabAuthService _auth;
        private readonly ConfigService _configService;

        private int? _cachedScore;
        private DateTime _scoreCacheTimeUtc;

        public string CurrentDisplayName { get; private set; }

        public ProfileService(PlayFabAuthService auth, ConfigService configService)
        {
            _auth = auth;
            _configService = configService;
        }

        public async UniTask InitializeAsync()
        {
            // Инициализация профиля после логина
            await EnsureDisplayNameAsync();

            // Подтянем score один раз (кэш), чтобы UI мог быстро показать
            await GetScoreAsync(forceRefresh: true);
        }

        public async UniTask EnsureDisplayNameAsync()
        {
            EnsureLoggedIn();

            if (!string.IsNullOrWhiteSpace(_auth.DisplayName))
            {
                CurrentDisplayName = _auth.DisplayName;
                return;
            }

            var generated = GenerateNicknameFromPlayFabId(_auth.PlayFabId);

            var res = await PlayFabAsync.UpdateUserTitleDisplayNameAsync(new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = generated
            });

            CurrentDisplayName = res.DisplayName;
            _auth.SetDisplayNameLocal(CurrentDisplayName);

            Debug.Log($"DisplayName generated: {CurrentDisplayName}");
        }

        public async UniTask<int> GetScoreAsync(bool forceRefresh = false)
        {
            EnsureLoggedIn();

            var cfg = _configService.Game;
            if (!forceRefresh && _cachedScore.HasValue)
            {
                var age = DateTime.UtcNow - _scoreCacheTimeUtc;
                if (age.TotalSeconds <= cfg.scoreCacheSeconds)
                    return _cachedScore.Value;
            }

            var stats = await PlayFabAsync.GetPlayerStatisticsAsync(new GetPlayerStatisticsRequest
            {
                StatisticNames = new List<string> { StatScore }
            });

            var score = stats.Statistics.FirstOrDefault(s => s.StatisticName == StatScore)?.Value ?? 0;
            CacheScore(score);
            return score;
        }

        public async UniTask<int> AddScoreAsync(int delta)
        {
            EnsureLoggedIn();

            var current = await GetScoreAsync();
            var next = Math.Max(0, current + delta);

            await SetScoreAsync(next);
            return next;
        }

        public async UniTask SetScoreAsync(int newScore)
        {
            EnsureLoggedIn();

            try
            {
                await PlayFabAsync.UpdatePlayerStatisticsAsync(new UpdatePlayerStatisticsRequest
                {
                    Statistics = new List<StatisticUpdate>
                    {
                        new StatisticUpdate { StatisticName = StatScore, Value = newScore }
                    }
                });

                CacheScore(newScore);
            }
            catch (Exception e)
            {
                Debug.LogError($"SetScoreAsync failed: {e}");
                throw; // или НЕ throw, если хочешь “тихо” жить в MVP
            }
        }

        public async UniTask<IReadOnlyList<LeaderboardEntryDto>> GetTopAsync()
        {
            EnsureLoggedIn();

            var topN = Mathf.Max(1, _configService.Game.leaderboardTopN);

            var lb = await PlayFabAsync.GetLeaderboardAsync(new GetLeaderboardRequest
            {
                StatisticName = StatScore,
                MaxResultsCount = topN
            });

            return lb.Leaderboard.Select(x => new LeaderboardEntryDto
            {
                Rank = x.Position + 1,
                DisplayName = string.IsNullOrWhiteSpace(x.DisplayName) ? "Player" : x.DisplayName,
                Score = x.StatValue,
                IsMe = x.PlayFabId == _auth.PlayFabId
            }).ToList();
        }

        public async UniTask<RankDto> GetMyRankAsync()
        {
            EnsureLoggedIn();

            var around = await PlayFabAsync.GetLeaderboardAroundPlayerAsync(new GetLeaderboardAroundPlayerRequest
            {
                StatisticName = StatScore,
                MaxResultsCount = 1
            });

            var me = around.Leaderboard?.FirstOrDefault(x => x.PlayFabId == _auth.PlayFabId)
                     ?? around.Leaderboard?.FirstOrDefault();

            if (me == null)
            {
                return new RankDto { Rank = null, Score = await GetScoreAsync() };
            }

            CacheScore(me.StatValue);

            return new RankDto
            {
                Rank = me.Position + 1,
                Score = me.StatValue
            };
        }

        // ----------------- helpers -----------------

        private void EnsureLoggedIn()
        {
            if (!_auth.IsLoggedIn)
                throw new InvalidOperationException("ProfileService used before PlayFab login.");
        }

        private void CacheScore(int score)
        {
            _cachedScore = score;
            _scoreCacheTimeUtc = DateTime.UtcNow;
        }

        private static string GenerateNicknameFromPlayFabId(string playFabId)
        {
            using var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(playFabId ?? "null"));

            ulong value = BitConverter.ToUInt64(hash, 0);
            var code = ToBase36(value).ToUpperInvariant();

            if (code.Length > 6) code = code.Substring(0, 6);
            if (code.Length < 6) code = code.PadLeft(6, '0');

            return $"Player-{code}";
        }

        private static string ToBase36(ulong value)
        {
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            if (value == 0) return "0";

            var sb = new StringBuilder();
            while (value > 0)
            {
                sb.Insert(0, chars[(int)(value % 36)]);
                value /= 36;
            }
            return sb.ToString();
        }

    }

}
