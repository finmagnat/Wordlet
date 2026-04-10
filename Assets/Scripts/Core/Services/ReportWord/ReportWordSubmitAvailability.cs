namespace Core.Services.ReportWord
{
    public readonly struct ReportWordSubmitAvailability
    {
        public readonly bool CanSubmit;
        public readonly bool DailyLimitReached;
        public readonly int RemainingCooldownSeconds;
        public readonly int RemainingDailyResetSeconds;

        public ReportWordSubmitAvailability(
            bool canSubmit,
            bool dailyLimitReached,
            int remainingCooldownSeconds,
            int remainingDailyResetSeconds)
        {
            CanSubmit = canSubmit;
            DailyLimitReached = dailyLimitReached;
            RemainingCooldownSeconds = remainingCooldownSeconds;
            RemainingDailyResetSeconds = remainingDailyResetSeconds;
        }
    }
}