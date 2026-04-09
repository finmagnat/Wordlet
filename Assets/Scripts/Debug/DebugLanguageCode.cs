namespace Core.DebugTools
{
    public enum DebugLanguage
    {
        Ru,
        En,
        Uk
    }
    
    public class DebugLanguageCode
    {
        public static DebugLanguage SelectedLanguage { get; set; } = DebugLanguage.Ru;
        public static string Get()
        {
            return SelectedLanguage switch
            {
                DebugLanguage.Ru => "ru",
                DebugLanguage.En => "en",
                DebugLanguage.Uk => "uk",
                _ => "ru"
            };
        }
    }
}