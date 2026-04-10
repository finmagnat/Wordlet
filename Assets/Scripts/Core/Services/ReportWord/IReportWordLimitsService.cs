using System;

namespace Core.Services.ReportWord
{
    public interface IReportWordLimitsService
    {
        event Action OnStateChanged;

        ReportWordSubmitAvailability GetAvailability();
        void RegisterSuccessfulSubmit();
    }
}