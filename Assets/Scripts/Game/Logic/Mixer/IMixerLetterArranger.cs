using System.Collections.Generic;
using Core.Services.DataDictionary;

namespace Game.Logic.Mixer
{
    public interface IMixerLetterArranger
    {
        string Id { get; }
        bool TryArrange(MixerBoard board, IReadOnlyList<int> targetIndexes, string patternId,
            LanguageDictionaryConfig dictionaryConfig, out string[] boardData);
    }
}
