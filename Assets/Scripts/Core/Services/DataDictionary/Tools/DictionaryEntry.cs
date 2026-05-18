namespace Core.DataDictionary.Tools
{
    public sealed class DictionaryEntry
    {
        public DictionaryEntry(string word, string definition, int wordLineNumber, int definitionLineNumber)
        {
            Word = word;
            Definition = definition;
            WordLineNumber = wordLineNumber;
            DefinitionLineNumber = definitionLineNumber;
        }

        public string Word { get; }
        public string Definition { get; }
        public int WordLineNumber { get; }
        public int DefinitionLineNumber { get; }
    }
}
