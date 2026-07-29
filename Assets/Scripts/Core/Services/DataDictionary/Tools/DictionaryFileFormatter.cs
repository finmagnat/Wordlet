using System.Collections.Generic;

namespace Core.DataDictionary.Tools
{
    public static class DictionaryFileFormatter
    {
        public static List<string> FormatEntries(IReadOnlyList<DictionaryEntry> entries)
        {
            var lines = new List<string>(entries.Count * 2);

            foreach (var entry in entries)
            {
                lines.Add(FormatPrefixedLine(DictionaryFileParser.WordPrefix, entry.Word));
                lines.Add(FormatPrefixedLine(DictionaryFileParser.DefinitionPrefix, entry.Definition));
            }

            return lines;
        }

        public static List<string> FormatWords(IReadOnlyList<DictionaryEntry> entries)
        {
            var lines = new List<string>(entries.Count);

            foreach (var entry in entries)
                lines.Add(entry.Word);

            return lines;
        }

        private static string FormatPrefixedLine(string prefix, string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? prefix
                : $"{prefix} {value.Trim()}";
        }
    }
}
