namespace Core.Services.NewWords
{
    public enum SubmitNewWordFlowStatus
    {
        Submitted,
        Invalid,
        Cooldown,
        DailyLimitReached,
        AlreadyExists,
        Failed
    }
}