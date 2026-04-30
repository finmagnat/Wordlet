using System.Collections.Generic;

namespace Game.Logic.Mixer
{
    public static class MixerPatternCatalog
    {
        public static IReadOnlyList<IMixerPattern> CreateDefault()
        {
            return new IMixerPattern[]
            {
                new ClusterMixerPattern(),
                new CrossMixerPattern(),
                new FrameMixerPattern(),
                new LineMixerPattern()
            };
        }
    }
}
