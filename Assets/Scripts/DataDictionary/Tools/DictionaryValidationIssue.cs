namespace Core.DataDictionary.Tools
{
    public enum DictionaryValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    public sealed class DictionaryValidationIssue
    {
        public DictionaryValidationIssue(DictionaryValidationSeverity severity, int lineNumber, string message)
        {
            Severity = severity;
            LineNumber = lineNumber;
            Message = message;
        }

        public DictionaryValidationSeverity Severity { get; }
        public int LineNumber { get; }
        public string Message { get; }
    }
}
