using System.Collections.Generic;

namespace Game.Logic.Mixer
{
    public static class MixerPatternValidator
    {
        public static bool IsValidTarget(MixerBoard board, IReadOnlyList<int> targetIndexes)
        {
            if (board == null || targetIndexes == null)
                return false;

            if (targetIndexes.Count != board.FilledCount || targetIndexes.Count <= 1)
                return false;

            var uniqueIndexes = new HashSet<int>();

            for (int i = 0; i < targetIndexes.Count; ++i)
            {
                int index = targetIndexes[i];

                if (!MixerBoard.IsValidIndex(index) || !uniqueIndexes.Add(index))
                    return false;
            }

            foreach (int index in uniqueIndexes)
            {
                bool hasNeighbor = false;

                foreach (int neighborIndex in MixerBoard.GetOrthogonalNeighbors(index))
                {
                    if (uniqueIndexes.Contains(neighborIndex))
                    {
                        hasNeighbor = true;
                        break;
                    }
                }

                if (!hasNeighbor)
                    return false;
            }

            return true;
        }
    }
}
