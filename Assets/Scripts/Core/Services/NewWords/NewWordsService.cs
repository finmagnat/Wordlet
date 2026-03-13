using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core.Services.NewWords
{
    public sealed class NewWordsService : INewWordsService
    {
        private readonly INewWordsProvider _provider;
        private readonly INewWordsLimitsService _limits;

        public NewWordsService(INewWordsProvider provider, INewWordsLimitsService limits)
        {
            _provider = provider;
            _limits = limits;
        }

        public UniTask<AddPendingWordResponseDto> SubmitWordAsync(string rawWord, string language)
        {
            if (!TryNormalizeWord(rawWord, out var normalizedWord))
            {
                return UniTask.FromResult(new AddPendingWordResponseDto
                {
                    success = false,
                    status = "Invalid",
                    normalizedWord = null
                });
            }

            var normalizedLanguage = NormalizeLanguage(language);
            return _provider.AddWordAsync(normalizedWord, normalizedLanguage);
        }
        
        public async UniTask<SubmitNewWordFlowResult> TrySubmitWordAsync(string rawWord, string language)
        {
            if (!TryNormalizeWord(rawWord, out var normalizedWord))
            {
                return new SubmitNewWordFlowResult
                {
                    Status = SubmitNewWordFlowStatus.Invalid
                };
            }

            var availability = _limits.GetAvailability();
            if (!availability.CanSubmit)
            {
                return new SubmitNewWordFlowResult
                {
                    Status = availability.DailyLimitReached
                        ? SubmitNewWordFlowStatus.DailyLimitReached
                        : SubmitNewWordFlowStatus.Cooldown,
                    RemainingCooldownSeconds = availability.RemainingCooldownSeconds,
                    RemainingDailyResetSeconds = availability.RemainingDailyResetSeconds
                };
            }

            var normalizedLanguage = NormalizeLanguage(language);
            var response = await _provider.AddWordAsync(normalizedWord, normalizedLanguage);

            if (!response.success)
            {
                return new SubmitNewWordFlowResult
                {
                    Status = SubmitNewWordFlowStatus.Failed
                };
            }

            if (response.status == "AlreadyExists")
            {
                return new SubmitNewWordFlowResult
                {
                    Status = SubmitNewWordFlowStatus.AlreadyExists,
                    NormalizedWord = response.normalizedWord
                };
            }

            if (response.status == "Added")
            {
                _limits.RegisterSuccessfulSubmit();

                return new SubmitNewWordFlowResult
                {
                    Status = SubmitNewWordFlowStatus.Submitted,
                    NormalizedWord = response.normalizedWord
                };
            }

            return new SubmitNewWordFlowResult
            {
                Status = SubmitNewWordFlowStatus.Failed
            };
        }

        public UniTask<IReadOnlyList<NewWordEntryDto>> GetPendingWordsAsync(string language)
        {
            var normalizedLanguage = NormalizeLanguage(language);
            return _provider.GetWordsAsync(normalizedLanguage);
        }

        public UniTask<DeletePendingWordResponseDto> RemoveWordAsync(string rawWord, string language)
        {
            if (!TryNormalizeWord(rawWord, out var normalizedWord))
            {
                return UniTask.FromResult(new DeletePendingWordResponseDto
                {
                    success = false,
                    status = "Invalid",
                    normalizedWord = null
                });
            }

            var normalizedLanguage = NormalizeLanguage(language);
            return _provider.DeleteWordAsync(normalizedWord, normalizedLanguage);
        }
        
        public UniTask<ClearPendingWordsResponseDto> ClearPendingWordsAsync(string language)
        {
            var normalizedLanguage = NormalizeLanguage(language);
            return _provider.ClearWordsAsync(normalizedLanguage);
        }

        private static bool TryNormalizeWord(string rawWord, out string normalizedWord)
        {
            normalizedWord = null;

            if (string.IsNullOrWhiteSpace(rawWord))
                return false;

            var value = rawWord.Trim().ToUpperInvariant();

            // Для MVP: без пробелов внутри, длина в разумных пределах.
            if (value.Length < 2 || value.Length > 32)
                return false;

            if (value.Contains(" "))
                return false;

            normalizedWord = value;
            return true;
        }

        private static string NormalizeLanguage(string language)
        {
            return string.IsNullOrWhiteSpace(language)
                ? "ru"
                : language.Trim().ToLowerInvariant();
        }
    }
}