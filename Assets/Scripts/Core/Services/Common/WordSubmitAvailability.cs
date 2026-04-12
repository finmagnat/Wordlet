namespace Core.Services.Common
{
    public readonly struct WordSubmitAvailability
    {
        public readonly bool CanSubmit;
        public readonly bool DailyLimitReached;
        public readonly int RemainingCooldownSeconds;
        public readonly int RemainingDailyResetSeconds;

        public WordSubmitAvailability(
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