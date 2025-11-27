using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с результатами тестов по пунктуации
    /// </summary>
    public interface IPunctuationTestResultRepository : IRepository<PunctuationTestResult>
    {
        Task<List<PunctuationTestResult>> GetByTestIdAsync(int testId);
        Task<List<PunctuationTestResult>> GetByStudentIdAsync(int studentId);
        Task<List<PunctuationTestResult>> GetCompletedByTestIdAsync(int testId);
        Task<List<PunctuationTestResult>> GetByStudentAndTestIdAsync(int studentId, int testId);
        Task<PunctuationTestResult?> GetLatestByStudentAndTestIdAsync(int studentId, int testId);
        Task<int> GetCountByTestIdAsync(int testId);
    }
}

