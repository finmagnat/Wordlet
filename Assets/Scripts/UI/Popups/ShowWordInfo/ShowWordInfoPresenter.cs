using System;
using Core.Data;
using Core.Generated;
using Core.Services;
using Core.Services.ReportWord;
using Core.UI;
using Cysharp.Threading.Tasks;

namespace UI.Popups
{
    public enum ReportWordFlowStatus
    {
        Cancelled,
        Submitted,
        AlreadyExists,
        Cooldown,
        DailyLimitReached,
        Invalid,
        Failed
    }

    public readonly struct ReportWordPopupFlowResult
    {
        public readonly ReportWordFlowStatus Status;
        public readonly string NormalizedWord;
        public readonly int RemainingCooldownSeconds;
        public readonly int RemainingDailyResetSeconds;

        public ReportWordPopupFlowResult(
            ReportWordFlowStatus status,
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
        private readonly IReportWordService _newWordsService;
        private readonly IReportWordLimitsService _newWordsLimitsService;
        private readonly LocalizationService _localization;
        
        private string _cooldownText;
        
        public ShowWordInfoPresenter(
            IUIManager ui,
            IReportWordService newWordsService,
            IReportWordLimitsService newWordsLimitsService,
            LocalizationService localization)
        {
            _ui = ui;
            _newWordsService = newWordsService;
            _newWordsLimitsService = newWordsLimitsService;
            _localization = localization;
        }

        public async UniTask<ReportWordPopupFlowResult> ShowAsync(string word, string language)
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
                return new ReportWordPopupFlowResult(ReportWordFlowStatus.Cancelled);
            }

            var submitResult = await _newWordsService.TrySubmitWordAsync(word, language);

            return submitResult.Status switch
            {
                SubmitReportWordFlowStatus.Submitted => new ReportWordPopupFlowResult(
                    ReportWordFlowStatus.Submitted,
                    submitResult.NormalizedWord),

                SubmitReportWordFlowStatus.AlreadyExists => new ReportWordPopupFlowResult(
                    ReportWordFlowStatus.AlreadyExists,
                    submitResult.NormalizedWord),

                SubmitReportWordFlowStatus.Cooldown => new ReportWordPopupFlowResult(
                    ReportWordFlowStatus.Cooldown,
                    remainingCooldownSeconds: submitResult.RemainingCooldownSeconds),

                SubmitReportWordFlowStatus.DailyLimitReached => new ReportWordPopupFlowResult(
                    ReportWordFlowStatus.DailyLimitReached,
                    remainingDailyResetSeconds: submitResult.RemainingDailyResetSeconds),

                SubmitReportWordFlowStatus.Invalid => new ReportWordPopupFlowResult(
                    ReportWordFlowStatus.Invalid),

                _ => new ReportWordPopupFlowResult(ReportWordFlowStatus.Failed)
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