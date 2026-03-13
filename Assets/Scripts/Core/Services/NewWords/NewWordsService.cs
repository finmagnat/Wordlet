using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core.Services.NewWords
{
    public sealed class NewWordsService : INewWordsService
    {
        private readonly INewWordsProvider _provider;

        public NewWordsService(INewWordsProvider provider)
        {
            _provider = provider;
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