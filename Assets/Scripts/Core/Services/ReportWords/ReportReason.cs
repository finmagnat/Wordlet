namespace Core.Services.ReportWords
{
    public enum ReportReason
    {
        None = 0,                    // Пустой выбор (для UI)

        NotAWord = 1,                // Слова не существует
        TypoOrError = 2,             // Опечатка или ошибка
        IncorrectForm = 3,           // Неправильная форма слова
        ProperNoun = 4,              // Имя / название
        RareOrOutdated = 5,          // Редкое / устаревшее
        Offensive = 6,               // Оскорбительное
        Other = 7                    // Другое
    }
}