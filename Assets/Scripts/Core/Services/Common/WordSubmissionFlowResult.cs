namespace Core.Services.Common
{
    public sealed class WordSubmissionFlowResult
    {
        public WordSubmissionFlowStatus Status;
        public string NormalizedWord;
        public int RemainingCooldownSeconds;
        public int RemainingDailyResetSeconds;
    }
}