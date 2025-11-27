using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для работы с тестами по орфоэпии
    /// </summary>
    public interface IOrthoeopyTestService
    {
        Task<OrthoeopyTest?> GetByIdAsync(int id);
        Task<List<OrthoeopyTest>> GetByTeacherIdAsync(string teacherId);
        Task<List<OrthoeopyTest>> GetByAssignmentIdAsync(int assignmentId);
        Task<List<OrthoeopyTest>> GetActiveByTeacherIdAsync(string teacherId);
        Task<int> CreateAsync(OrthoeopyTest test);
        Task<int> UpdateAsync(OrthoeopyTest test);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> TeacherCanAccessTestAsync(string teacherId, int testId);
    }
}

