namespace Core.Services.NewWords
{
    public readonly struct NewWordsSubmitAvailability
    {
        public readonly bool CanSubmit;
        public readonly bool DailyLimitReached;
        public readonly int RemainingCooldownSeconds;
        public readonly int RemainingDailyResetSeconds;

        public NewWordsSubmitAvailability(
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