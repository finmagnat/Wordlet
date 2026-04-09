namespace Core.Services.ReportWords
{
    public static class ReportReasonExtensions
    {
        public static string ToId(this ReportReason reason)
        {
            return reason switch
            {
                ReportReason.NotAWord => "not_a_word",
                ReportReason.TypoOrError => "typo_or_error",
                ReportReason.IncorrectForm => "incorrect_form",
                ReportReason.ProperNoun => "proper_noun",
                ReportReason.RareOrOutdated => "rare_or_outdated",
                ReportReason.Offensive => "offensive",
                ReportReason.Other => "other",
                _ => "none"
            };
        }
    }
}