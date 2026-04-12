namespace Core.Services.Common
{
    public enum WordSubmissionFlowStatus
    {
        Invalid = 0,
        Cooldown = 1,
        DailyLimitReached = 2,
        AlreadyExists = 3,
        Submitted = 4,
        Failed = 5
    }
}