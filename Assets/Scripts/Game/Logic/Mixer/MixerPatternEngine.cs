using System.Collections.Generic;

namespace Game.Logic.Mixer
{
    public sealed class MixerPatternEngine
    {
        private const int MaxAttemptsPerPattern = 8;

        private readonly List<IMixerPattern> _patterns;

        public MixerPatternEngine(IReadOnlyList<IMixerPattern> patterns)
        {
            _patterns = new List<IMixerPattern>(patterns);
        }

        public bool TryMix(MixerBoard board, out MixerResult result)
        {
            result = null;

            if (board == null || board.FilledCount <= 1)
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

                    string[] mixedBoard = BuildMixedBoard(board, targetIndexes);
                    if (board.IsSameBoard(mixedBoard))
                        continue;

                    result = new MixerResult(pattern.Id, mixedBoard, targetIndexes);
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

        private static string[] BuildMixedBoard(MixerBoard board, IReadOnlyList<int> targetIndexes)
        {
            string[] mixedBoard = new string[MixerBoard.CellCount];
            var letters = new List<string>(board.FilledCount);

            for (int i = 0; i < mixedBoard.Length; ++i)
                mixedBoard[i] = string.Empty;

            for (int i = 0; i < board.Letters.Count; ++i)
                letters.Add(board.Letters[i].Value);

            MixerRandom.Shuffle(letters);

            if (HasSameLetterOrder(board, targetIndexes, letters) && HasDifferentLetters(letters))
                MixerRandom.Swap(letters, 0, FindFirstDifferentLetterIndex(letters));

            for (int i = 0; i < targetIndexes.Count; ++i)
                mixedBoard[targetIndexes[i]] = letters[i];

            return mixedBoard;
        }

        private static bool HasSameLetterOrder(MixerBoard board, IReadOnlyList<int> targetIndexes, IReadOnlyList<string> letters)
        {
            for (int i = 0; i < targetIndexes.Count; ++i)
            {
                if (board.GetLetter(targetIndexes[i]) != letters[i])
                    return false;
            }

            return true;
        }

        private static bool HasDifferentLetters(IReadOnlyList<string> letters)
        {
            return FindFirstDifferentLetterIndex(letters) > 0;
        }

        private static int FindFirstDifferentLetterIndex(IReadOnlyList<string> letters)
        {
            string firstLetter = letters[0];

            for (int i = 1; i < letters.Count; ++i)
            {
                if (letters[i] != firstLetter)
                    return i;
            }

            return -1;
        }
    }
}
