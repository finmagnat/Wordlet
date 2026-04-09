using System;
using Core.Data;
using Core.Generated;
using Core.Services;
using Core.Services.NewWords;
using Core.UI;
using Cysharp.Threading.Tasks;

namespace UI.Popups
{
    public enum ShowWordInfoPopupFlowStatus
    {
        Cancelled,
        Submitted,
        AlreadyExists,
        Cooldown,
        DailyLimitReached,
        Invalid,
        Failed
    }

    public readonly struct ShowWordInfoPopupFlowResult
    {
        public readonly ShowWordInfoPopupFlowStatus Status;
        public readonly string NormalizedWord;
        public readonly int RemainingCooldownSeconds;
        public readonly int RemainingDailyResetSeconds;

        public ShowWordInfoPopupFlowResult(
            ShowWordInfoPopupFlowStatus status,
            string normalizedWord = null,
            int remainingCooldownSeconds = 0,
            int remainingDailyResetSeconds = 0)
        {
            Status = status;
            NormalizedWord = normalizedWord;
            RemainingCooldownSeconds = remainingCooldownSeconds;
            RemainingDailyResetSeconds = remainingDailyResetSeconds;
        }
    }

    public sealed class ShowWordInfoPresenter
    {
        private readonly IUIManager _ui;
        private readonly INewWordsService _newWordsService;
        private readonly INewWordsLimitsService _newWordsLimitsService;
        private readonly LocalizationService _localization;
        
        private string _cooldownText;
        
        public ShowWordInfoPresenter(
            IUIManager ui,
            INewWordsService newWordsService,
            INewWordsLimitsService newWordsLimitsService,
            LocalizationService localization)
        {
            _ui = ui;
            _newWordsService = newWordsService;
            _newWordsLimitsService = newWordsLimitsService;
            _localization = localization;
        }

        public async UniTask<ShowWordInfoPopupFlowResult> ShowAsync(string word, string language)
        {
            var popup = await _ui.ShowPopupAsync<MissingWordPopup, NewWordWindowEventData>(AssetKey.MissingWordPopup, 
                new NewWordWindowEventData{ newWord = word });

            using var timerCts = new System.Threading.CancellationTokenSource();

            _cooldownText = _localization.Get(LocalizationConst.TableUI, "LIMIT_NEW_WORD_SENT_TEXT");
            var timerTask = RunTimerLoopAsync(popup, timerCts.Token);

            PopupExitData popupResult;
            try
            {
                popupResult = await popup.WaitForResultAsync();
            }
            finally
            {
                timerCts.Cancel();
                try
                {
                    await timerTask;
                }
                catch (OperationCanceledException)
                {
                    // Нормально, таймер остановили.
                }
            }

            if (popupResult.Result != PopupResult.SaveAndExit)
            {
                return new ShowWordInfoPopupFlowResult(ShowWordInfoPopupFlowStatus.Cancelled);
            }

            var submitResult = await _newWordsService.TrySubmitWordAsync(word, language);

            return submitResult.Status switch
            {
                SubmitNewWordFlowStatus.Submitted => new ShowWordInfoPopupFlowResult(
                    ShowWordInfoPopupFlowStatus.Submitted,
                    submitResult.NormalizedWord),

                SubmitNewWordFlowStatus.AlreadyExists => new ShowWordInfoPopupFlowResult(
                    ShowWordInfoPopupFlowStatus.AlreadyExists,
                    submitResult.NormalizedWord),

                SubmitNewWordFlowStatus.Cooldown => new ShowWordInfoPopupFlowResult(
                    ShowWordInfoPopupFlowStatus.Cooldown,
                    remainingCooldownSeconds: submitResult.RemainingCooldownSeconds),

                SubmitNewWordFlowStatus.DailyLimitReached => new ShowWordInfoPopupFlowResult(
                    ShowWordInfoPopupFlowStatus.DailyLimitReached,
                    remainingDailyResetSeconds: submitResult.RemainingDailyResetSeconds),

                SubmitNewWordFlowStatus.Invalid => new ShowWordInfoPopupFlowResult(
                    ShowWordInfoPopupFlowStatus.Invalid),

                _ => new ShowWordInfoPopupFlowResult(ShowWordInfoPopupFlowStatus.Failed)
            };
        }

        private async UniTask RunTimerLoopAsync(MissingWordPopup popup, System.Threading.CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var availability = _newWordsLimitsService.GetAvailability();

                string message = null;

                if (!availability.CanSubmit)
                {
                    message = availability.DailyLimitReached
                        ? $"{_cooldownText}{FormatTime(availability.RemainingDailyResetSeconds)}"
                        : $"{_cooldownText}{FormatTime(availability.RemainingCooldownSeconds)}";
                }

                popup.SetSubmitState(availability.CanSubmit, message);

                await UniTask.Delay(1000, ignoreTimeScale: true, cancellationToken: ct);
            }
        }

        private static string FormatTime(int totalSeconds)
        {
            if (totalSeconds < 0)
                totalSeconds = 0;

            var ts = TimeSpan.FromSeconds(totalSeconds);

            return ts.TotalHours >= 1
                ? ts.ToString(@"hh\:mm\:ss")
                : ts.ToString(@"mm\:ss");
        }
    }
}