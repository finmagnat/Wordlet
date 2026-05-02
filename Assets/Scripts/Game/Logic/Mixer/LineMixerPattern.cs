using System.Collections.Generic;

namespace Game.Logic.Mixer
{
    public sealed class LineMixerPattern : IMixerPattern
    {
        public string Id => "line";

        public bool CanApply(MixerBoard board)
        {
            return board != null && board.FilledCount > 1 && board.FilledCount <= MixerBoard.Width;
        }

        public bool TryBuild(MixerBoard board, out List<int> targetIndexes)
        {
            targetIndexes = new List<int>(board.FilledCount);

            bool isHorizontal = MixerRandom.Range(0, 2) == 0;

            if (isHorizontal)
                BuildHorizontal(board.FilledCount, targetIndexes);
            else
                BuildVertical(board.FilledCount, targetIndexes);

            return true;
        }

        private static void BuildHorizontal(int count, List<int> targetIndexes)
        {
            int row = MixerRandom.Range(0, MixerBoard.Height);
            int startColumn = MixerRandom.Range(0, MixerBoard.Width - count + 1);

            for (int i = 0; i < count; ++i)
                targetIndexes.Add(MixerBoard.ToIndex(row, startColumn + i));
        }

        private static void BuildVertical(int count, List<int> targetIndexes)
        {
            int column = MixerRandom.Range(0, MixerBoard.Width);
            int startRow = MixerRandom.Range(0, MixerBoard.Height - count + 1);

            for (int i = 0; i < count; ++i)
                targetIndexes.Add(MixerBoard.ToIndex(startRow + i, column));
        }
    }
}
