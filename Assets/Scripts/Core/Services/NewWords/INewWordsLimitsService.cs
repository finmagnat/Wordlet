using System;
using Core.Services.Common;

namespace Core.Services.NewWords
{
    public interface INewWordsLimitsService
    {
        event Action OnStateChanged;

        WordSubmitAvailability GetAvailability();
        void RegisterSuccessfulSubmit();
        void ResetLimits(bool disableLimits = false);
    }
}