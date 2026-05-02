using System.Collections.Generic;

namespace Game.Logic.Mixer
{
    public sealed class FrameMixerPattern : IMixerPattern
    {
        private static readonly int[] OuterFrameIndexes =
        {
            0, 1, 2, 3, 4,
            9, 14, 19, 24,
            23, 22, 21, 20,
            15, 10, 5
        };

        public string Id => "frame";

        public bool CanApply(MixerBoard board)
        {
            return board != null && board.FilledCount > 1 && board.FilledCount <= OuterFrameIndexes.Length;
        }

        public bool TryBuild(MixerBoard board, out List<int> targetIndexes)
        {
            targetIndexes = new List<int>(board.FilledCount);

            int startIndex = MixerRandom.Range(0, OuterFrameIndexes.Length);

            for (int i = 0; i < board.FilledCount; ++i)
            {
                int frameIndex = (startIndex + i) % OuterFrameIndexes.Length;
                targetIndexes.Add(OuterFrameIndexes[frameIndex]);
            }

            return true;
        }
    }
}
