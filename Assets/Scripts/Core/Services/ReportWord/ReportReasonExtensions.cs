using System.Collections.Generic;

namespace Core.Services.ReportWord
{
    public static class ReportReasonExtensions
    {
        public static readonly List<ReportReason> Reasons = new()
        {
            ReportReason.None,
            ReportReason.NotAWord,
            ReportReason.TypoOrError,
            ReportReason.IncorrectForm,
            ReportReason.ProperNoun,
            ReportReason.RareOrOutdated,
            ReportReason.Offensive,
            ReportReason.IncorrectDescription,
            ReportReason.Other
        };
        
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
                ReportReason.IncorrectDescription => "incorrect_description",
                ReportReason.Other => "other",
                _ => "none"
            };
        }
        
        public static string ToLocaleKey(this ReportReason reason)
        {
            return reason switch
            {
                ReportReason.None => "REPORT_WORD_SELECT_OPTION",
                ReportReason.NotAWord => "REPORT_WORD_NOT_A_WORD",
                ReportReason.TypoOrError => "REPORT_WORD_TYPO_OR_ERROR",
                ReportReason.IncorrectForm => "REPORT_WORD_INCORRECT_FORM",
                ReportReason.ProperNoun => "REPORT_WORD_PROPER_NOUN",
                ReportReason.RareOrOutdated => "REPORT_WORD_RARE_OR_OUTDATED",
                ReportReason.Offensive => "REPORT_WORD_OFFENSIVE",
                ReportReason.IncorrectDescription => "REPORT_WORD_INCORRECT_DESCRIPTION",
                ReportReason.Other => "REPORT_WORD_OTHER",
                _ => "none"
            };
        }
    }
}
