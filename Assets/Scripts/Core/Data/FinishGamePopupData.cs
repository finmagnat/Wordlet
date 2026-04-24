namespace Core.Data
{
    public readonly struct FinishGamePopupData
    {
        public readonly string OwnerName;
        public readonly string OpponentName;
        public readonly uint OwnerScore;
        public readonly uint OpponentScore;
        public readonly uint OwnerPass;
        public readonly uint OpponentPass;
        public readonly uint MaxPasses;
        public readonly string Result;

        public FinishGamePopupData(
            string ownerName,
            string opponentName,
            uint ownerScore,
            uint opponentScore,
            uint ownerPass,
            uint opponentPass,
            uint maxPasses,
            string result)
        {
            OwnerName = ownerName;
            OpponentName = opponentName;
            OwnerScore = ownerScore;
            OpponentScore = opponentScore;
            OwnerPass = ownerPass;
            OpponentPass = opponentPass;
            MaxPasses = maxPasses;
            Result = result;
        }
    }
}
