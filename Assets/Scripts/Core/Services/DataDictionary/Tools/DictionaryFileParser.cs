using System;
using System.Collections.Generic;

namespace Core.DataDictionary.Tools
{
    public sealed class DictionaryFileParser
    {
        public const string WordPrefix = "WORD:";
        public const string DefinitionPrefix = "DEFINITION:";

        private readonly struct LineRecord
        {
            public LineRecord(string text, int lineNumber)
            {
                Text = text;
                LineNumber = lineNumber;
            }

            public string Text { get; }
            public int LineNumber { get; }
        }

        public DictionaryParseResult Parse(IReadOnlyList<string> lines)
        {
            var records = new List<LineRecord>();
            var issues = new List<DictionaryValidationIssue>();
            int emptyLineCount = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                string trimmed = lines[i].Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    emptyLineCount++;
                    continue;
                }

                records.Add(new LineRecord(trimmed, i + 1));
            }

            var entries = new List<DictionaryEntry>();
            int index = 0;

            while (index < records.Count)
            {
                LineRecord wordLine = records[index];
                if (!TryReadPrefixedValue(wordLine.Text, WordPrefix, out var word))
                {
                    issues.Add(new DictionaryValidationIssue(
                        DictionaryValidationSeverity.Error,
                        wordLine.LineNumber,
                        $"Структура нарушена на строке {wordLine.LineNumber}: ожидался ключ '{WordPrefix}'."));
                    index++;
                    continue;
                }

                if (index + 1 >= records.Count)
                {
                    issues.Add(new DictionaryValidationIssue(
                        DictionaryValidationSeverity.Error,
                        wordLine.LineNumber,
                        $"Структура нарушена на строке {wordLine.LineNumber}: у слова '{word}' нет строки '{DefinitionPrefix}'."));
                    index++;
                    continue;
                }

                LineRecord definitionLine = records[index + 1];
                if (!TryReadPrefixedValue(definitionLine.Text, DefinitionPrefix, out var definition))
                {
                    issues.Add(new DictionaryValidationIssue(
                        DictionaryValidationSeverity.Error,
                        definitionLine.LineNumber,
                        $"Структура нарушена на строке {definitionLine.LineNumber}: ожидался ключ '{DefinitionPrefix}' для слова '{word}'."));
                    index++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(word))
                {
                    issues.Add(new DictionaryValidationIssue(
                        DictionaryValidationSeverity.Error,
                        wordLine.LineNumber,
                        $"Структура нарушена на строке {wordLine.LineNumber}: значение '{WordPrefix}' пустое."));
                }
                else
                {
                    entries.Add(new DictionaryEntry(word.Trim(), definition.Trim(), wordLine.LineNumber, definitionLine.LineNumber));
                }

                index += 2;
            }

            if (emptyLineCount > 0)
            {
                issues.Add(new DictionaryValidationIssue(
                    DictionaryValidationSeverity.Info,
                    0,
                    $"Найдено пустых строк: {emptyLineCount}."));
            }

            return new DictionaryParseResult(entries, issues, emptyLineCount);
        }

        private static bool TryReadPrefixedValue(string line, string prefix, out string value)
        {
            if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                value = line.Substring(prefix.Length).Trim();
                return true;
            }

            value = string.Empty;
            return false;
        }
    }
}
