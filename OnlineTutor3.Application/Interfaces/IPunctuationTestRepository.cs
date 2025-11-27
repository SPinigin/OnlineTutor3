using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с тестами по пунктуации
    /// </summary>
    public interface IPunctuationTestRepository : IRepository<PunctuationTest>
    {
        Task<List<PunctuationTest>> GetByTeacherIdAsync(string teacherId);
        Task<List<PunctuationTest>> GetByAssignmentIdAsync(int assignmentId);
        Task<List<PunctuationTest>> GetActiveByTeacherIdAsync(string teacherId);
        Task<int> GetCountByTeacherIdAsync(string teacherId);
        Task<int> GetCountByAssignmentIdAsync(int assignmentId);
    }
}

