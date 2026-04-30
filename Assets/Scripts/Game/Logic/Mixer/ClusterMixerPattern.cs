using System.Collections.Generic;

namespace Game.Logic.Mixer
{
    public sealed class ClusterMixerPattern : IMixerPattern
    {
        private static readonly int[] Anchors =
        {
            0,
            MixerBoard.Width - 1,
            MixerBoard.CellCount - MixerBoard.Width,
            MixerBoard.CellCount - 1,
            MixerBoard.CellCount / 2
        };

        public string Id => "cluster";

        public bool CanApply(MixerBoard board)
        {
            return board != null && board.FilledCount > 1;
        }

        public bool TryBuild(MixerBoard board, out List<int> targetIndexes)
        {
            targetIndexes = new List<int>(board.FilledCount);

            int anchor = MixerRandom.Pick(Anchors);
            var visited = new HashSet<int> { anchor };
            var queue = new Queue<int>();

            targetIndexes.Add(anchor);
            queue.Enqueue(anchor);

            while (queue.Count > 0 && targetIndexes.Count < board.FilledCount)
            {
                int currentIndex = queue.Dequeue();
                var neighbors = new List<int>(MixerBoard.GetOrthogonalNeighbors(currentIndex));
                MixerRandom.Shuffle(neighbors);

                for (int i = 0; i < neighbors.Count && targetIndexes.Count < board.FilledCount; ++i)
                {
                    int neighborIndex = neighbors[i];

                    if (!visited.Add(neighborIndex))
                        continue;

                    targetIndexes.Add(neighborIndex);
                    queue.Enqueue(neighborIndex);
                }
            }

            return targetIndexes.Count == board.FilledCount;
        }
    }
}
