namespace Core.Services.NewWords
{
    public sealed class SubmitNewWordFlowResult
    {
        public SubmitNewWordFlowStatus Status;
        public string NormalizedWord;
        public int RemainingCooldownSeconds;
        public int RemainingDailyResetSeconds;
    }
}