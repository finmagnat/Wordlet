using System;
using System.Collections.Generic;

namespace Core.Services.ReportWord
{
    [Serializable]
    public sealed class ReportWordEntryDto
    {
        public string word;
    }

    [Serializable]
    public sealed class ReportWordCollectionDto
    {
        public List<ReportWordEntryDto> words = new();
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
        public List<ReportWordEntryDto> words = new();
    }
    
    [Serializable]
    public sealed class ClearPendingWordsResponseDto
    {
        public bool success;
        public string status;   // Cleared
        public string language;
    }
}