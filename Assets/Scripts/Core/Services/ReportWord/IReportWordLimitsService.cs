using System;
using Core.Services.Common;

namespace Core.Services.ReportWord
{
    public interface IReportWordLimitsService
    {
        event Action OnStateChanged;

        WordSubmitAvailability GetAvailability();
        void RegisterSuccessfulSubmit();
    }
}