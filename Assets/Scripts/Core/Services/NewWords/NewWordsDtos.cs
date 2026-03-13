using System;
using System.Collections.Generic;

namespace Core.Services.NewWords
{
    [Serializable]
    public sealed class NewWordEntryDto
    {
        public string word;
    }

    [Serializable]
    public sealed class NewWordsCollectionDto
    {
        public List<NewWordEntryDto> words = new();
    }

    [Serializable]
    public sealed class AddPendingWordResponseDto
    {
        public bool success;
        public string status;          // Added / AlreadyExists / Invalid
        public string normalizedWord;
    }

    [Serializable]
    public sealed class DeletePendingWordResponseDto
    {
        public bool success;
        public string status;          // Deleted / NotFound / Invalid
        public string normalizedWord;
    }

    [Serializable]
    public sealed class GetPendingWordsResponseDto
    {
        public bool success;
        public string language;
        public List<NewWordEntryDto> words = new();
    }
}