using Core.Services.Common;
using Zenject;

namespace Core.Services.NewWords
{
    public sealed class NewWordsLimitsService : PlayerPrefsWordSubmissionLimitsServiceBase, INewWordsLimitsService
    {
        [Inject] private IConfigService _configs;

        protected override int DailyLimit => !disableLimits ? _configs.Game.newWordsDailyLimit : 0;
        protected override int CooldownSeconds => !disableLimits ? _configs.Game.newWordsCooldownSeconds : 0;
        protected override string DailyCountKeyPrefix => "new_words_daily";
        protected override string LastSubmitTsKey => "new_words_last_submit_ts";
    }
}