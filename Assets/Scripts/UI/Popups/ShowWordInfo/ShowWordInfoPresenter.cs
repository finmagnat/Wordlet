using System;
using Core.Data;
using Core.Generated;
using Core.Services;
using Core.Services.Common;
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
        private readonly IReportWordService _reportService;
        private readonly IReportWordLimitsService _limitsService;
        private readonly LocalizationService _localization;
        
        private string _cooldownText;
        
        public ShowWordInfoPresenter(
            IUIManager ui,
            IReportWordService reportService,
            IReportWordLimitsService limitsService,
            LocalizationService localization)
        {
            _ui = ui;
            _reportService = reportService;
            _limitsService = limitsService;
            _localization = localization;
        }

        public async UniTask<ReportWordPopupFlowResult> ShowAsync(string word, string language)
        {
            var popup = await _ui.ShowPopupAsync<ShowWordInfoPopup, ShowWordInfoWindowEventData>(AssetKey.ShowWordInfoPopup, 
                new ShowWordInfoWindowEventData{ word = word });

            using var timerCts = new System.Threading.CancellationTokenSource();

            _cooldownText = _localization.Get(LocalizationConst.TableUI, "LIMIT_REPORT_WORD_SENT_TEXT");
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

            var submitResult = await _reportService.TrySubmitWordAsync(word, popup.GetSelectedReason(), language);

            return submitResult.Status switch
            {
                WordSubmissionFlowStatus.Submitted => new ReportWordPopupFlowResult(
                    ReportWordFlowStatus.Submitted,
                    submitResult.NormalizedWord),

                WordSubmissionFlowStatus.AlreadyExists => new ReportWordPopupFlowResult(
                    ReportWordFlowStatus.AlreadyExists,
                    submitResult.NormalizedWord),

                WordSubmissionFlowStatus.Cooldown => new ReportWordPopupFlowResult(
                    ReportWordFlowStatus.Cooldown,
                    remainingCooldownSeconds: submitResult.RemainingCooldownSeconds),

                WordSubmissionFlowStatus.DailyLimitReached => new ReportWordPopupFlowResult(
                    ReportWordFlowStatus.DailyLimitReached,
                    remainingDailyResetSeconds: submitResult.RemainingDailyResetSeconds),

                WordSubmissionFlowStatus.Invalid => new ReportWordPopupFlowResult(
                    ReportWordFlowStatus.Invalid),

                _ => new ReportWordPopupFlowResult(ReportWordFlowStatus.Failed)
            };
        }

        private async UniTask RunTimerLoopAsync(ShowWordInfoPopup popup, System.Threading.CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var availability = _limitsService.GetAvailability();

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