namespace Core.Services.ReportWord
{
    public enum SubmitReportWordFlowStatus
    {
        Submitted,
        Invalid,
        Cooldown,
        DailyLimitReached,
        AlreadyExists,
        Failed
    }
}