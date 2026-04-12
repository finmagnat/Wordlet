namespace Core.Services.Common
{
    public static class WordSubmissionUtils
    {
        public static bool TryNormalizeWord(string rawWord, out string normalizedWord)
        {
            normalizedWord = null;

            if (string.IsNullOrWhiteSpace(rawWord))
                return false;

            var value = rawWord.Trim().ToUpperInvariant();

            // MVP-валидация
            if (value.Length < 2 || value.Length > 32)
                return false;

            if (value.Contains(" "))
                return false;

            normalizedWord = value;
            return true;
        }

        public static string NormalizeLanguage(string language)
        {
            return string.IsNullOrWhiteSpace(language)
                ? "ru"
                : language.Trim().ToLowerInvariant();
        }
    }
}