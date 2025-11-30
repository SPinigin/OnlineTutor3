using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для проверки доступа студента к тестам
    /// </summary>
    public interface ITestAccessService
    {
        /// <summary>
        /// Проверяет, имеет ли студент доступ к тесту по орфографии
        /// </summary>
        Task<bool> CanAccessSpellingTestAsync(int studentId, int testId);

        /// <summary>
        /// Проверяет, имеет ли студент доступ к тесту по пунктуации
        /// </summary>
        Task<bool> CanAccessPunctuationTestAsync(int studentId, int testId);

        /// <summary>
        /// Проверяет, имеет ли студент доступ к тесту по орфоэпии
        /// </summary>
        Task<bool> CanAccessOrthoeopyTestAsync(int studentId, int testId);

        /// <summary>
        /// Проверяет, имеет ли студент доступ к классическому тесту
        /// </summary>
        Task<bool> CanAccessRegularTestAsync(int studentId, int testId);

        /// <summary>
        /// Получает все доступные тесты по орфографии для студента
        /// </summary>
        Task<List<SpellingTest>> GetAvailableSpellingTestsAsync(int studentId);

        /// <summary>
        /// Получает все доступные тесты по пунктуации для студента
        /// </summary>
        Task<List<PunctuationTest>> GetAvailablePunctuationTestsAsync(int studentId);

        /// <summary>
        /// Получает все доступные тесты по орфоэпии для студента
        /// </summary>
        Task<List<OrthoeopyTest>> GetAvailableOrthoeopyTestsAsync(int studentId);

        /// <summary>
        /// Получает все доступные классические тесты для студента
        /// </summary>
        Task<List<RegularTest>> GetAvailableRegularTestsAsync(int studentId);
    }
}

