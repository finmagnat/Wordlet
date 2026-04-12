using Core.Services.Common;
using Zenject;

namespace Core.Services.NewWords
{
    public sealed class NewWordsLimitsService : PlayerPrefsWordSubmissionLimitsServiceBase, INewWordsLimitsService
    {
        [Inject] private IConfigService _configs;

        protected override int DailyLimit => _configs.Game.newWordsDailyLimit;
        protected override int CooldownSeconds => _configs.Game.newWordsCooldownSeconds;
        protected override string DailyCountKeyPrefix => "new_words_daily";
        protected override string LastSubmitTsKey => "new_words_last_submit_ts";
    }
}