using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Core.DataDictionary.Tools
{
    public static class DictionaryCleaner
    {
        public static List<string> RemoveEmptyLines(IReadOnlyList<string> lines)
        {
            return lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim())
                .ToList();
        }

        public static List<DictionaryEntry> CreateEntriesFromWordList(
            IReadOnlyList<string> lines,
            out int emptyLineCount,
            out List<DictionaryValidationIssue> issues)
        {
            var entries = new List<DictionaryEntry>(lines.Count);
            issues = new List<DictionaryValidationIssue>();
            emptyLineCount = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                string word = lines[i]?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(word))
                {
                    emptyLineCount++;
                    continue;
                }

                if (StartsWithDictionaryPrefix(word, out var prefix))
                {
                    issues.Add(new DictionaryValidationIssue(
                        DictionaryValidationSeverity.Error,
                        i + 1,
                        $"Исходный список содержит ключ '{prefix}' на строке {i + 1}. Операция ожидает простой список слов."));
                    continue;
                }

                entries.Add(new DictionaryEntry(word, string.Empty, i + 1, 0));
            }

            return entries;
        }

        public static List<DictionaryEntry> RemoveDuplicates(
            IReadOnlyList<DictionaryEntry> entries,
            out List<DictionaryEntry> removedEntries,
            out List<DictionaryValidationIssue> warnings)
        {
            var result = new List<DictionaryEntry>(entries.Count);
            removedEntries = new List<DictionaryEntry>();
            warnings = new List<DictionaryValidationIssue>();
            var seenWords = new Dictionary<string, DictionaryEntry>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in entries)
            {
                string normalizedWord = NormalizeWord(entry.Word);
                if (seenWords.TryGetValue(normalizedWord, out var existingEntry))
                {
                    removedEntries.Add(entry);

                    if (!string.Equals(existingEntry.Definition, entry.Definition, StringComparison.Ordinal))
                    {
                        warnings.Add(new DictionaryValidationIssue(
                            DictionaryValidationSeverity.Warning,
                            entry.WordLineNumber,
                            $"Дубликат '{entry.Word}' на строке {entry.WordLineNumber} имеет другое определение."));
                    }

                    continue;
                }

                seenWords[normalizedWord] = entry;
                result.Add(entry);
            }

            return result;
        }

        public static List<DictionaryEntry> RemoveWordsLongerThan(
            IReadOnlyList<DictionaryEntry> entries,
            int maxLength,
            out List<DictionaryEntry> removedEntries)
        {
            var result = new List<DictionaryEntry>(entries.Count);
            removedEntries = new List<DictionaryEntry>();

            foreach (var entry in entries)
            {
                if (entry.Word.Length > maxLength)
                {
                    removedEntries.Add(entry);
                    continue;
                }

                result.Add(entry);
            }

            return result;
        }

        public static List<DictionaryEntry> SortByWord(IReadOnlyList<DictionaryEntry> entries, string cultureName)
        {
            var comparer = CreateComparer(cultureName);
            return entries
                .OrderBy(entry => entry.Word, comparer)
                .ToList();
        }

        public static string NormalizeWord(string word)
        {
            return word?.Trim().ToUpperInvariant() ?? string.Empty;
        }

        private static bool StartsWithDictionaryPrefix(string line, out string prefix)
        {
            if (line.StartsWith(DictionaryFileParser.WordPrefix, StringComparison.OrdinalIgnoreCase))
            {
                prefix = DictionaryFileParser.WordPrefix;
                return true;
            }

            if (line.StartsWith(DictionaryFileParser.DefinitionPrefix, StringComparison.OrdinalIgnoreCase))
            {
                prefix = DictionaryFileParser.DefinitionPrefix;
                return true;
            }

            prefix = string.Empty;
            return false;
        }

        private static StringComparer CreateComparer(string cultureName)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
                return StringComparer.CurrentCultureIgnoreCase;

            try
            {
                return StringComparer.Create(CultureInfo.GetCultureInfo(cultureName), true);
            }
            catch (CultureNotFoundException)
            {
                return StringComparer.CurrentCultureIgnoreCase;
            }
        }
    }
}
