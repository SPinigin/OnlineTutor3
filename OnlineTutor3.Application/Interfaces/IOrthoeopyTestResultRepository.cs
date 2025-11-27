using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с результатами тестов по орфоэпии
    /// </summary>
    public interface IOrthoeopyTestResultRepository : IRepository<OrthoeopyTestResult>
    {
        Task<List<OrthoeopyTestResult>> GetByTestIdAsync(int testId);
        Task<List<OrthoeopyTestResult>> GetByStudentIdAsync(int studentId);
        Task<List<OrthoeopyTestResult>> GetCompletedByTestIdAsync(int testId);
        Task<List<OrthoeopyTestResult>> GetByStudentAndTestIdAsync(int studentId, int testId);
        Task<OrthoeopyTestResult?> GetLatestByStudentAndTestIdAsync(int studentId, int testId);
        Task<int> GetCountByTestIdAsync(int testId);
    }
}

