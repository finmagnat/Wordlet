using System.Collections.Generic;

namespace Game.Logic.Mixer
{
    public sealed class CrossMixerPattern : IMixerPattern
    {
        private const int MaxCells = 9;
        private const int CenterIndex = MixerBoard.CellCount / 2;

        private static readonly int[] NearArmIndexes =
        {
            MixerBoard.ToIndex(1, 2),
            MixerBoard.ToIndex(2, 1),
            MixerBoard.ToIndex(2, 3),
            MixerBoard.ToIndex(3, 2)
        };

        private static readonly int[] FarArmIndexes =
        {
            MixerBoard.ToIndex(0, 2),
            MixerBoard.ToIndex(2, 0),
            MixerBoard.ToIndex(2, 4),
            MixerBoard.ToIndex(4, 2)
        };

        public string Id => "cross";

        public bool CanApply(MixerBoard board)
        {
            return board != null && board.FilledCount > 1 && board.FilledCount <= MaxCells;
        }

        public bool TryBuild(MixerBoard board, out List<int> targetIndexes)
        {
            targetIndexes = new List<int>(board.FilledCount) { CenterIndex };

            AddRandomIndexes(targetIndexes, NearArmIndexes, board.FilledCount);
            AddRandomIndexes(targetIndexes, FarArmIndexes, board.FilledCount);

            return targetIndexes.Count == board.FilledCount;
        }

        private static void AddRandomIndexes(List<int> targetIndexes, int[] sourceIndexes, int targetCount)
        {
            if (targetIndexes.Count >= targetCount)
                return;

            var indexes = new List<int>(sourceIndexes);
            MixerRandom.Shuffle(indexes);

            for (int i = 0; i < indexes.Count && targetIndexes.Count < targetCount; ++i)
                targetIndexes.Add(indexes[i]);
        }
    }
}
