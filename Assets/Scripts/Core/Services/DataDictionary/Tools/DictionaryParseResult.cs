using System.Collections.Generic;
using System.Linq;

namespace Core.DataDictionary.Tools
{
    public sealed class DictionaryParseResult
    {
        public DictionaryParseResult(
            List<DictionaryEntry> entries,
            List<DictionaryValidationIssue> issues,
            int emptyLineCount)
        {
            Entries = entries;
            Issues = issues;
            EmptyLineCount = emptyLineCount;
        }

        public List<DictionaryEntry> Entries { get; }
        public List<DictionaryValidationIssue> Issues { get; }
        public int EmptyLineCount { get; }
        public bool HasErrors => Issues.Any(issue => issue.Severity == DictionaryValidationSeverity.Error);
    }
}
