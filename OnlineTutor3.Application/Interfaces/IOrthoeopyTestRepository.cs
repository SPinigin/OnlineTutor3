using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с тестами по орфоэпии
    /// </summary>
    public interface IOrthoeopyTestRepository : IRepository<OrthoeopyTest>
    {
        Task<List<OrthoeopyTest>> GetByTeacherIdAsync(string teacherId);
        Task<List<OrthoeopyTest>> GetByAssignmentIdAsync(int assignmentId);
        Task<List<OrthoeopyTest>> GetActiveByTeacherIdAsync(string teacherId);
        Task<int> GetCountByTeacherIdAsync(string teacherId);
        Task<int> GetCountByAssignmentIdAsync(int assignmentId);
    }
}

