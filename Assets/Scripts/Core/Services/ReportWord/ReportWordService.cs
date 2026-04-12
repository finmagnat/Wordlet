using System.Collections.Generic;
using Core.Services.Common;
using Cysharp.Threading.Tasks;

namespace Core.Services.ReportWord
{
    public sealed class ReportWordService : IReportWordService
    {
        private readonly IReportWordProvider _provider;
        private readonly IReportWordLimitsService _limits;

        public ReportWordService(IReportWordProvider provider, IReportWordLimitsService limits)
        {
            _provider = provider;
            _limits = limits;
        }

        public UniTask<AddPendingWordResponseDto> SubmitWordAsync(string rawWord, ReportReason reason, string language)
        {
            if (!WordSubmissionUtils.TryNormalizeWord(rawWord, out var normalizedWord) || reason == ReportReason.None)
            {
                return UniTask.FromResult(new AddPendingWordResponseDto
                {
                    success = false,
                    status = "Invalid",
                    normalizedWord = null
                });
            }

            var normalizedLanguage = WordSubmissionUtils.NormalizeLanguage(language);
            var normalizedReason = ReportReasonExtensions.ToId(reason);
            return _provider.AddWordAsync(normalizedWord, normalizedReason, normalizedLanguage);
        }

        public async UniTask<WordSubmissionFlowResult> TrySubmitWordAsync(string rawWord, ReportReason reason, string language)
        {
            if (!WordSubmissionUtils.TryNormalizeWord(rawWord, out var normalizedWord) || reason == ReportReason.None)
            {
                return new WordSubmissionFlowResult
                {
                    Status = WordSubmissionFlowStatus.Invalid
                };
            }

            var availability = _limits.GetAvailability();
            if (!availability.CanSubmit)
            {
                return new WordSubmissionFlowResult
                {
                    Status = availability.DailyLimitReached
                        ? WordSubmissionFlowStatus.DailyLimitReached
                        : WordSubmissionFlowStatus.Cooldown,
                    RemainingCooldownSeconds = availability.RemainingCooldownSeconds,
                    RemainingDailyResetSeconds = availability.RemainingDailyResetSeconds
                };
            }

            var normalizedLanguage = WordSubmissionUtils.NormalizeLanguage(language);
            var normalizedReason = ReportReasonExtensions.ToId(reason);
            var response = await _provider.AddWordAsync(normalizedWord, normalizedReason, normalizedLanguage);

            if (!response.success)
            {
                return new WordSubmissionFlowResult
                {
                    Status = WordSubmissionFlowStatus.Failed
                };
            }

            if (response.status == "AlreadyExists")
            {
                return new WordSubmissionFlowResult
                {
                    Status = WordSubmissionFlowStatus.AlreadyExists,
                    NormalizedWord = response.normalizedWord
                };
            }

            if (response.status == "Added")
            {
                _limits.RegisterSuccessfulSubmit();

                return new WordSubmissionFlowResult
                {
                    Status = WordSubmissionFlowStatus.Submitted,
                    NormalizedWord = response.normalizedWord
                };
            }

            return new WordSubmissionFlowResult
            {
                Status = WordSubmissionFlowStatus.Failed
            };
        }

        public UniTask<IReadOnlyList<ReportWordEntryDto>> GetPendingWordsAsync(string language)
        {
            var normalizedLanguage = WordSubmissionUtils.NormalizeLanguage(language);
            return _provider.GetWordsAsync(normalizedLanguage);
        }

        public UniTask<DeletePendingWordResponseDto> RemoveWordAsync(string rawWord, string language)
        {
            if (!WordSubmissionUtils.TryNormalizeWord(rawWord, out var normalizedWord))
            {
                return UniTask.FromResult(new DeletePendingWordResponseDto
                {
                    success = false,
                    status = "Invalid",
                    normalizedWord = null
                });
            }

            var normalizedLanguage = WordSubmissionUtils.NormalizeLanguage(language);
            return _provider.DeleteWordAsync(normalizedWord, normalizedLanguage);
        }

        public UniTask<ClearPendingWordsResponseDto> ClearPendingWordsAsync(string language)
        {
            var normalizedLanguage = WordSubmissionUtils.NormalizeLanguage(language);
            return _provider.ClearWordsAsync(normalizedLanguage);
        }
    }
}