using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с результатами классических тестов
    /// </summary>
    public interface IRegularTestResultRepository : IRepository<RegularTestResult>
    {
        Task<List<RegularTestResult>> GetByTestIdAsync(int testId);
        Task<List<RegularTestResult>> GetByStudentIdAsync(int studentId);
        Task<List<RegularTestResult>> GetCompletedByTestIdAsync(int testId);
        Task<List<RegularTestResult>> GetByStudentAndTestIdAsync(int studentId, int testId);
        Task<RegularTestResult?> GetLatestByStudentAndTestIdAsync(int studentId, int testId);
        Task<int> GetCountByTestIdAsync(int testId);
    }
}

