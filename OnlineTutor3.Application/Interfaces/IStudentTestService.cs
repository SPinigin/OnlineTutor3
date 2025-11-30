using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Основной сервис для работы студента с тестами
    /// </summary>
    public interface IStudentTestService
    {
        /// <summary>
        /// Получает все доступные тесты для студента с информацией о статусе
        /// </summary>
        Task<StudentAvailableTestsViewModel> GetAvailableTestsAsync(int studentId, string? category = null);

        /// <summary>
        /// Получает историю прохождения тестов студента
        /// </summary>
        Task<StudentTestHistoryViewModel> GetTestHistoryAsync(int studentId, string? testType = null);

        /// <summary>
        /// Начинает новый тест по орфографии
        /// </summary>
        Task<SpellingTestResult> StartSpellingTestAsync(int studentId, int testId);

        /// <summary>
        /// Начинает новый тест по пунктуации
        /// </summary>
        Task<PunctuationTestResult> StartPunctuationTestAsync(int studentId, int testId);

        /// <summary>
        /// Начинает новый тест по орфоэпии
        /// </summary>
        Task<OrthoeopyTestResult> StartOrthoeopyTestAsync(int studentId, int testId);

        /// <summary>
        /// Начинает новый классический тест
        /// </summary>
        Task<RegularTestResult> StartRegularTestAsync(int studentId, int testId);
    }

    /// <summary>
    /// ViewModel для доступных тестов студента
    /// </summary>
    public class StudentAvailableTestsViewModel
    {
        public Student Student { get; set; } = null!;
        public List<SpellingTest> SpellingTests { get; set; } = new();
        public List<PunctuationTest> PunctuationTests { get; set; } = new();
        public List<OrthoeopyTest> OrthoeopyTests { get; set; } = new();
        public List<RegularTest> RegularTests { get; set; } = new();
    }

    /// <summary>
    /// ViewModel для истории тестов студента
    /// </summary>
    public class StudentTestHistoryViewModel
    {
        public Student Student { get; set; } = null!;
        public List<SpellingTestResult> SpellingResults { get; set; } = new();
        public List<PunctuationTestResult> PunctuationResults { get; set; } = new();
        public List<OrthoeopyTestResult> OrthoeopyResults { get; set; } = new();
        public List<RegularTestResult> RegularResults { get; set; } = new();
    }
}

