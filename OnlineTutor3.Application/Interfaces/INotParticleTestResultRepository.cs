using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с результатами теста на правописание частицы "не"
    /// </summary>
    public interface INotParticleTestResultRepository : IRepository<NotParticleTestResult>
    {
        Task<List<NotParticleTestResult>> GetByTestIdAsync(int testId);
        Task<List<NotParticleTestResult>> GetByStudentIdAsync(int studentId);
        Task<List<NotParticleTestResult>> GetCompletedByTestIdAsync(int testId);
        Task<List<NotParticleTestResult>> GetByStudentAndTestIdAsync(int studentId, int testId);
        Task<NotParticleTestResult?> GetLatestByStudentAndTestIdAsync(int studentId, int testId);
        Task<int> GetCountByTestIdAsync(int testId);
    }
}

