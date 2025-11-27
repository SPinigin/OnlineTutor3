using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с результатами тестов по орфографии
    /// </summary>
    public interface ISpellingTestResultRepository : IRepository<SpellingTestResult>
    {
        Task<List<SpellingTestResult>> GetByTestIdAsync(int testId);
        Task<List<SpellingTestResult>> GetByStudentIdAsync(int studentId);
        Task<List<SpellingTestResult>> GetCompletedByTestIdAsync(int testId);
        Task<List<SpellingTestResult>> GetByStudentAndTestIdAsync(int studentId, int testId);
        Task<SpellingTestResult?> GetLatestByStudentAndTestIdAsync(int studentId, int testId);
        Task<int> GetCountByTestIdAsync(int testId);
    }
}

