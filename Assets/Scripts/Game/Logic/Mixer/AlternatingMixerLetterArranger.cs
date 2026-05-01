using System.Collections.Generic;
using Core.DataDictionary;

namespace Game.Logic.Mixer
{
    public sealed class AlternatingMixerLetterArranger : IMixerLetterArranger
    {
        private const string FramePatternId = "frame";

        private readonly MixerLetterClassifier _classifier;

        public string Id => "alternating_vowel_consonant";

        public AlternatingMixerLetterArranger(MixerLetterClassifier classifier)
        {
            _classifier = classifier;
        }

        public bool TryArrange(MixerBoard board, IReadOnlyList<int> targetIndexes, string patternId,
            LanguageDictionaryConfig dictionaryConfig, out string[] boardData)
        {
            boardData = CreateEmptyBoardData();

            if (board == null || targetIndexes == null || _classifier == null ||
                targetIndexes.Count != board.FilledCount)
                return false;

            var consonants = new List<string>();
            var vowelOrNeutral = new List<string>();

            for (int i = 0; i < board.Letters.Count; ++i)
            {
                var letter = board.Letters[i];
                if (_classifier.GetGroup(letter.Value, dictionaryConfig) == MixerLetterGroup.Consonant)
                    consonants.Add(letter.Value);
                else
                    vowelOrNeutral.Add(letter.Value);
            }

            MixerRandom.Shuffle(consonants);
            MixerRandom.Shuffle(vowelOrNeutral);

            var orderedTargets = GetOrderedTargets(targetIndexes, patternId);
            int consonantParity = GetBestConsonantParity(orderedTargets, consonants.Count, vowelOrNeutral.Count);
            var placedGroups = new Dictionary<int, MixerLetterGroup>();

            for (int i = 0; i < orderedTargets.Count; ++i)
            {
                int targetIndex = orderedTargets[i];
                MixerLetterGroup preferredGroup = GetPreferredGroup(targetIndex, placedGroups, consonantParity);
                string letter = TakeLetter(preferredGroup, consonants, vowelOrNeutral, out var placedGroup);

                if (string.IsNullOrEmpty(letter))
                    return false;

                boardData[targetIndex] = letter;
                placedGroups[targetIndex] = placedGroup;
            }

            return true;
        }

        private static string[] CreateEmptyBoardData()
        {
            string[] boardData = new string[MixerBoard.CellCount];

            for (int i = 0; i < boardData.Length; ++i)
                boardData[i] = string.Empty;

            return boardData;
        }

        private static List<int> GetOrderedTargets(IReadOnlyList<int> targetIndexes, string patternId)
        {
            var orderedTargets = new List<int>(targetIndexes);

            if (patternId == FramePatternId)
                return orderedTargets;

            orderedTargets.Sort(CompareIndexesTopLeft);
            return orderedTargets;
        }

        private static int CompareIndexesTopLeft(int firstIndex, int secondIndex)
        {
            int firstRow = MixerBoard.GetRow(firstIndex);
            int secondRow = MixerBoard.GetRow(secondIndex);

            if (firstRow != secondRow)
                return firstRow.CompareTo(secondRow);

            return MixerBoard.GetColumn(firstIndex).CompareTo(MixerBoard.GetColumn(secondIndex));
        }

        private static int GetBestConsonantParity(IReadOnlyList<int> targetIndexes, int consonantsCount,
            int vowelOrNeutralCount)
        {
            if (targetIndexes.Count == 0)
                return 0;

            int firstParity = GetParity(targetIndexes[0]);
            int firstParitySlots = CountParitySlots(targetIndexes, firstParity);
            int secondParitySlots = targetIndexes.Count - firstParitySlots;
            int firstParityCost = GetPlacementCost(consonantsCount, vowelOrNeutralCount, firstParitySlots, secondParitySlots);
            int secondParityCost = GetPlacementCost(consonantsCount, vowelOrNeutralCount, secondParitySlots, firstParitySlots);

            if (firstParityCost <= secondParityCost)
                return firstParity;

            return 1 - firstParity;
        }

        private static int CountParitySlots(IReadOnlyList<int> targetIndexes, int parity)
        {
            int count = 0;

            for (int i = 0; i < targetIndexes.Count; ++i)
            {
                if (GetParity(targetIndexes[i]) == parity)
                    count++;
            }

            return count;
        }

        private static int GetPlacementCost(int consonantsCount, int vowelOrNeutralCount,
            int consonantsSlots, int vowelOrNeutralSlots)
        {
            int consonantsOverflow = consonantsCount > consonantsSlots ? consonantsCount - consonantsSlots : 0;
            int vowelOrNeutralOverflow = vowelOrNeutralCount > vowelOrNeutralSlots
                ? vowelOrNeutralCount - vowelOrNeutralSlots
                : 0;

            return consonantsOverflow + vowelOrNeutralOverflow;
        }

        private static MixerLetterGroup GetPreferredGroup(int targetIndex,
            Dictionary<int, MixerLetterGroup> placedGroups, int consonantParity)
        {
            int consonantNeighbors = 0;
            int vowelOrNeutralNeighbors = 0;

            foreach (int neighborIndex in MixerBoard.GetOrthogonalNeighbors(targetIndex))
            {
                if (!placedGroups.TryGetValue(neighborIndex, out var neighborGroup))
                    continue;

                if (neighborGroup == MixerLetterGroup.Consonant)
                    consonantNeighbors++;
                else
                    vowelOrNeutralNeighbors++;
            }

            if (consonantNeighbors > vowelOrNeutralNeighbors)
                return MixerLetterGroup.VowelOrNeutral;

            if (vowelOrNeutralNeighbors > consonantNeighbors)
                return MixerLetterGroup.Consonant;

            return GetParity(targetIndex) == consonantParity
                ? MixerLetterGroup.Consonant
                : MixerLetterGroup.VowelOrNeutral;
        }

        private static string TakeLetter(MixerLetterGroup preferredGroup, List<string> consonants,
            List<string> vowelOrNeutral, out MixerLetterGroup placedGroup)
        {
            if (preferredGroup == MixerLetterGroup.Consonant && consonants.Count > 0)
                return TakeFromGroup(consonants, MixerLetterGroup.Consonant, out placedGroup);

            if (preferredGroup == MixerLetterGroup.VowelOrNeutral && vowelOrNeutral.Count > 0)
                return TakeFromGroup(vowelOrNeutral, MixerLetterGroup.VowelOrNeutral, out placedGroup);

            if (consonants.Count > 0)
                return TakeFromGroup(consonants, MixerLetterGroup.Consonant, out placedGroup);

            return TakeFromGroup(vowelOrNeutral, MixerLetterGroup.VowelOrNeutral, out placedGroup);
        }

        private static string TakeFromGroup(List<string> letters, MixerLetterGroup group, out MixerLetterGroup placedGroup)
        {
            placedGroup = group;

            if (letters.Count == 0)
                return string.Empty;

            int lastIndex = letters.Count - 1;
            string letter = letters[lastIndex];
            letters.RemoveAt(lastIndex);
            return letter;
        }

        private static int GetParity(int index)
        {
            return (MixerBoard.GetRow(index) + MixerBoard.GetColumn(index)) % 2;
        }
    }
}
