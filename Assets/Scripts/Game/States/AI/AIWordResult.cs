namespace Game.AI
{
    public readonly struct AIWordResult
    {
        public readonly bool Success;
        public readonly string Word;

        public AIWordResult(bool success, string word)
        {
            Success = success;
            Word = word;
        }

        public static AIWordResult Fail() => new(false, string.Empty);
        public static AIWordResult Ok(string word) => new(true, word);
    }

}