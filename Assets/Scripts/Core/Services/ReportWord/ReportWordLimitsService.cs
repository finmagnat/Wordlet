using Core.Services.Common;
using Zenject;

namespace Core.Services.ReportWord
{
    public sealed class ReportWordLimitsService : PlayerPrefsWordSubmissionLimitsServiceBase, IReportWordLimitsService
    {
        [Inject] private IConfigService _configs;

        protected override int DailyLimit => !disableLimits ? _configs.Game.reportWordsDailyLimit : 0;
        protected override int CooldownSeconds => !disableLimits ? _configs.Game.reportWordsCooldownSeconds : 0;
        protected override string DailyCountKeyPrefix => "report_words_daily";
        protected override string LastSubmitTsKey => "report_words_last_submit_ts";
    }
}