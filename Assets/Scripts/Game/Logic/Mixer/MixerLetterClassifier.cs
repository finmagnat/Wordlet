using Core.Services.DataDictionary;

namespace Game.Logic.Mixer
{
    public sealed class MixerLetterClassifier
    {
        public MixerLetterGroup GetGroup(string letter, LanguageDictionaryConfig dictionaryConfig)
        {
            if (string.IsNullOrEmpty(letter) || dictionaryConfig == null)
                return MixerLetterGroup.VowelOrNeutral;

            string normalizedLetter = letter.ToLowerInvariant();
            string consonants = dictionaryConfig.consonants?.ToLowerInvariant() ?? string.Empty;

            return IsConsonant(normalizedLetter, consonants);
        }

        private static MixerLetterGroup IsConsonant(string letter, string consonants)
        {
            return letter.Length > 0 && consonants.IndexOf(letter[0]) >= 0
                ? MixerLetterGroup.Consonant
                : MixerLetterGroup.VowelOrNeutral;
        }
    }
}
