using Core.Services.Common;
using Zenject;

namespace Core.Services.ReportWord
{
    public sealed class ReportWordLimitsService : PlayerPrefsWordSubmissionLimitsServiceBase, IReportWordLimitsService
    {
        [Inject] private IConfigService _configs;

        protected override int DailyLimit => _configs.Game.reportWordsDailyLimit;
        protected override int CooldownSeconds => _configs.Game.reportWordsCooldownSeconds;
        protected override string DailyCountKeyPrefix => "report_words_daily";
        protected override string LastSubmitTsKey => "report_words_last_submit_ts";
    }
}