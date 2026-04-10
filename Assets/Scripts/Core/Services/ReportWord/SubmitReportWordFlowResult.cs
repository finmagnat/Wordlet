namespace Core.Services.ReportWord
{
    public sealed class SubmitReportWordFlowResult
    {
        public SubmitReportWordFlowStatus Status;
        public string NormalizedWord;
        public int RemainingCooldownSeconds;
        public int RemainingDailyResetSeconds;
    }
}