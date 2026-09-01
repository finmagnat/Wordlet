using System.Collections.Generic;
using Core.Services;

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
        public readonly FinishRewardData Reward;
        

        public FinishGamePopupData(
            string ownerName,
            string opponentName,
            uint ownerScore,
            uint opponentScore,
            uint ownerPass,
            uint opponentPass,
            uint maxPasses,
            string result,
            FinishRewardData reward)
        {
            OwnerName = ownerName;
            OpponentName = opponentName;
            OwnerScore = ownerScore;
            OpponentScore = opponentScore;
            OwnerPass = ownerPass;
            OpponentPass = opponentPass;
            MaxPasses = maxPasses;
            Result = result;
            Reward = reward;
        }
    }

    public readonly struct FinishRewardData
    {
        public readonly int WinsInSeriesCount;
        public readonly int WinsInSeriesMax;
        public readonly List<RewardDto> Rewards;
        
        public FinishRewardData(
            int winsInSeriesCount,
            int winsInSeriesMax,
            List<RewardDto> rewards
            )
        {
            WinsInSeriesCount = winsInSeriesCount;
            WinsInSeriesMax = winsInSeriesMax;
            Rewards = rewards;
        }
    }
}
