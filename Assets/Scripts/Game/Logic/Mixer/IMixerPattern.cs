using System.Collections.Generic;

namespace Game.Logic.Mixer
{
    public interface IMixerPattern
    {
        string Id { get; }
        bool CanApply(MixerBoard board);
        bool TryBuild(MixerBoard board, out List<int> targetIndexes);
    }
}
