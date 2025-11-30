using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для управления результатами тестов
    /// </summary>
    public interface ITestResultService
    {
        /// <summary>
        /// Создает новый результат теста по орфографии
        /// </summary>
        Task<SpellingTestResult> CreateSpellingTestResultAsync(int studentId, int testId);

        /// <summary>
        /// Создает новый результат теста по пунктуации
        /// </summary>
        Task<PunctuationTestResult> CreatePunctuationTestResultAsync(int studentId, int testId);

        /// <summary>
        /// Создает новый результат теста по орфоэпии
        /// </summary>
        Task<OrthoeopyTestResult> CreateOrthoeopyTestResultAsync(int studentId, int testId);

        /// <summary>
        /// Создает новый результат классического теста
        /// </summary>
        Task<RegularTestResult> CreateRegularTestResultAsync(int studentId, int testId);

        /// <summary>
        /// Получает незавершенный результат теста по орфографии
        /// </summary>
        Task<SpellingTestResult?> GetOngoingSpellingTestResultAsync(int studentId, int testId);

        /// <summary>
        /// Получает незавершенный результат теста по пунктуации
        /// </summary>
        Task<PunctuationTestResult?> GetOngoingPunctuationTestResultAsync(int studentId, int testId);

        /// <summary>
        /// Получает незавершенный результат теста по орфоэпии
        /// </summary>
        Task<OrthoeopyTestResult?> GetOngoingOrthoeopyTestResultAsync(int studentId, int testId);

        /// <summary>
        /// Получает незавершенный результат классического теста
        /// </summary>
        Task<RegularTestResult?> GetOngoingRegularTestResultAsync(int studentId, int testId);

        /// <summary>
        /// Получает количество попыток для теста по орфографии
        /// </summary>
        Task<int> GetAttemptCountAsync(int studentId, int testId, TestType testType);

        /// <summary>
        /// Завершает результат теста и вычисляет процент
        /// </summary>
        Task CompleteTestResultAsync<T>(T testResult) where T : TestResult;

        /// <summary>
        /// Получает все результаты тестов студента
        /// </summary>
        Task<List<T>> GetStudentResultsAsync<T>(int studentId) where T : TestResult;

        /// <summary>
        /// Получает лучший результат студента по тесту
        /// </summary>
        Task<T?> GetBestResultAsync<T>(int studentId, int testId) where T : TestResult;

        /// <summary>
        /// Обновляет оставшееся время для результата теста
        /// </summary>
        Task UpdateTimeRemainingAsync<T>(int testResultId, int timeRemainingSeconds) where T : TestResult;
    }

    /// <summary>
    /// Тип теста для определения репозитория
    /// </summary>
    public enum TestType
    {
        Spelling,
        Punctuation,
        Orthoeopy,
        Regular
    }
}

