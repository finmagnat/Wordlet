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
                lines.Add($"{DictionaryFileParser.WordPrefix} {entry.Word}");
                lines.Add($"{DictionaryFileParser.DefinitionPrefix} {entry.Definition}");
            }

            return lines;
        }
    }
}
