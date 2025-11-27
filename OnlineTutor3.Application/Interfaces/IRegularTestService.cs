using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для работы с классическими тестами
    /// </summary>
    public interface IRegularTestService
    {
        Task<RegularTest?> GetByIdAsync(int id);
        Task<List<RegularTest>> GetByTeacherIdAsync(string teacherId);
        Task<List<RegularTest>> GetByAssignmentIdAsync(int assignmentId);
        Task<List<RegularTest>> GetActiveByTeacherIdAsync(string teacherId);
        Task<int> CreateAsync(RegularTest test);
        Task<int> UpdateAsync(RegularTest test);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> TeacherCanAccessTestAsync(string teacherId, int testId);
    }
}

