using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для оценки ответов и подсчета результатов тестов
    /// </summary>
    public interface ITestEvaluationService
    {
        /// <summary>
        /// Оценивает ответ на вопрос по орфографии
        /// </summary>
        Task<(bool IsCorrect, int Points)> EvaluateSpellingAnswerAsync(SpellingQuestion question, string studentAnswer, bool noLetterNeeded, int pointsPerQuestion);

        /// <summary>
        /// Оценивает ответ на вопрос по пунктуации
        /// </summary>
        Task<(bool IsCorrect, int Points)> EvaluatePunctuationAnswerAsync(PunctuationQuestion question, string studentAnswer, int pointsPerQuestion);

        /// <summary>
        /// Оценивает ответ на вопрос по орфоэпии
        /// </summary>
        Task<(bool IsCorrect, int Points)> EvaluateOrthoeopyAnswerAsync(OrthoeopyQuestion question, int studentAnswer, int pointsPerQuestion);

        /// <summary>
        /// Оценивает ответ на вопрос классического теста
        /// </summary>
        Task<(bool IsCorrect, int Points)> EvaluateRegularAnswerAsync(RegularQuestion question, string? studentAnswer, int? selectedOptionId, int pointsPerQuestion);

        /// <summary>
        /// Вычисляет итоговый результат теста по орфографии
        /// </summary>
        Task<(int Score, int MaxScore, double Percentage)> CalculateSpellingTestResultAsync(int testResultId, int testId);

        /// <summary>
        /// Вычисляет итоговый результат теста по пунктуации
        /// </summary>
        Task<(int Score, int MaxScore, double Percentage)> CalculatePunctuationTestResultAsync(int testResultId, int testId);

        /// <summary>
        /// Вычисляет итоговый результат теста по орфоэпии
        /// </summary>
        Task<(int Score, int MaxScore, double Percentage)> CalculateOrthoeopyTestResultAsync(int testResultId, int testId);

        /// <summary>
        /// Вычисляет итоговый результат классического теста
        /// </summary>
        Task<(int Score, int MaxScore, double Percentage)> CalculateRegularTestResultAsync(int testResultId, int testId);

        /// <summary>
        /// Оценивает ответ на вопрос теста на правописание частицы "не"
        /// </summary>
        Task<(bool IsCorrect, int Points)> EvaluateNotParticleAnswerAsync(NotParticleQuestion question, bool studentAnswerIsMerged, int pointsPerQuestion);

        /// <summary>
        /// Вычисляет итоговый результат теста на правописание частицы "не"
        /// </summary>
        Task<(int Score, int MaxScore, double Percentage)> CalculateNotParticleTestResultAsync(int testResultId, int testId);
    }
}

