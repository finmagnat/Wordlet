using System.Collections.Generic;
using Core.Services.DataDictionary;

namespace Game.Logic.Mixer
{
    public sealed class MixerPatternEngine
    {
        private const int MaxAttemptsPerPattern = 8;

        private readonly List<IMixerPattern> _patterns;
        private readonly IMixerLetterArranger _letterArranger;

        public MixerPatternEngine(IReadOnlyList<IMixerPattern> patterns)
            : this(patterns, new AlternatingMixerLetterArranger(new MixerLetterClassifier()))
        {
        }

        public MixerPatternEngine(IReadOnlyList<IMixerPattern> patterns, IMixerLetterArranger letterArranger)
        {
            _patterns = new List<IMixerPattern>(patterns);
            _letterArranger = letterArranger;
        }

        public bool TryMix(MixerBoard board, LanguageDictionaryConfig dictionaryConfig, out MixerResult result)
        {
            result = null;

            if (board == null || board.FilledCount <= 1 || _letterArranger == null)
                return false;

            var patterns = GetAvailablePatterns(board);
            MixerRandom.Shuffle(patterns);

            for (int i = 0; i < patterns.Count; ++i)
            {
                var pattern = patterns[i];

                for (int attempt = 0; attempt < MaxAttemptsPerPattern; ++attempt)
                {
                    if (!pattern.TryBuild(board, out var targetIndexes))
                        continue;

                    if (!MixerPatternValidator.IsValidTarget(board, targetIndexes))
                        continue;

                    if (!_letterArranger.TryArrange(board, targetIndexes, pattern.Id, dictionaryConfig, out string[] mixedBoard))
                        continue;

                    if (board.IsSameBoard(mixedBoard))
                        continue;

                    result = new MixerResult(pattern.Id, _letterArranger.Id, mixedBoard, targetIndexes);
                    return true;
                }
            }

            return false;
        }

        private List<IMixerPattern> GetAvailablePatterns(MixerBoard board)
        {
            var availablePatterns = new List<IMixerPattern>();

            for (int i = 0; i < _patterns.Count; ++i)
            {
                if (_patterns[i].CanApply(board))
                    availablePatterns.Add(_patterns[i]);
            }

            return availablePatterns;
        }
    }
}
