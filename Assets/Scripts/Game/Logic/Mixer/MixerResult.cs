using System.Collections.Generic;

namespace Game.Logic.Mixer
{
    public sealed class MixerResult
    {
        public string PatternId { get; }
        public string ArrangerId { get; }
        public string[] BoardData { get; }
        public IReadOnlyList<int> TargetIndexes { get; }

        public MixerResult(string patternId, string arrangerId, string[] boardData, IReadOnlyList<int> targetIndexes)
        {
            PatternId = patternId;
            ArrangerId = arrangerId;
            BoardData = boardData;
            TargetIndexes = targetIndexes;
        }
    }
}
