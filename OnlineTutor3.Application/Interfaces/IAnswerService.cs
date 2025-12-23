using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для работы с ответами студентов
    /// </summary>
    public interface IAnswerService
    {
        /// <summary>
        /// Сохраняет ответ на вопрос по орфографии
        /// </summary>
        Task<SpellingAnswer> SaveSpellingAnswerAsync(int testResultId, int questionId, string studentAnswer, bool noLetterNeeded = false);

        /// <summary>
        /// Сохраняет ответ на вопрос по пунктуации
        /// </summary>
        Task<PunctuationAnswer> SavePunctuationAnswerAsync(int testResultId, int questionId, string studentAnswer);

        /// <summary>
        /// Сохраняет ответ на вопрос по орфоэпии
        /// </summary>
        Task<OrthoeopyAnswer> SaveOrthoeopyAnswerAsync(int testResultId, int questionId, int studentAnswer);

        /// <summary>
        /// Сохраняет ответ на вопрос классического теста
        /// </summary>
        Task<RegularAnswer> SaveRegularAnswerAsync(int testResultId, int questionId, string? studentAnswer, int? selectedOptionId);

        /// <summary>
        /// Сохраняет ответ на вопрос теста на правописание частицы "не"
        /// </summary>
        Task<NotParticleAnswer> SaveNotParticleAnswerAsync(int testResultId, int questionId, string studentAnswer);

        /// <summary>
        /// Получает все ответы для результата теста по орфографии
        /// </summary>
        Task<List<SpellingAnswer>> GetSpellingAnswersAsync(int testResultId);

        /// <summary>
        /// Получает все ответы для результата теста по пунктуации
        /// </summary>
        Task<List<PunctuationAnswer>> GetPunctuationAnswersAsync(int testResultId);

        /// <summary>
        /// Получает все ответы для результата теста по орфоэпии
        /// </summary>
        Task<List<OrthoeopyAnswer>> GetOrthoeopyAnswersAsync(int testResultId);

        /// <summary>
        /// Получает все ответы для результата классического теста
        /// </summary>
        Task<List<RegularAnswer>> GetRegularAnswersAsync(int testResultId);

        /// <summary>
        /// Получает все ответы для результата теста на правописание частицы "не"
        /// </summary>
        Task<List<NotParticleAnswer>> GetNotParticleAnswersAsync(int testResultId);

        /// <summary>
        /// Обновляет существующий ответ
        /// </summary>
        Task UpdateAnswerAsync<T>(T answer) where T : Answer;
    }
}

