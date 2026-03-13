using System;

namespace Core.Services.NewWords
{
    public interface INewWordsLimitsService
    {
        event Action OnStateChanged;

        NewWordsSubmitAvailability GetAvailability();
        void RegisterSuccessfulSubmit();
    }
}