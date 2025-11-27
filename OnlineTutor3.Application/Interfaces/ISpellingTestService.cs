using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для работы с тестами по орфографии
    /// </summary>
    public interface ISpellingTestService
    {
        Task<SpellingTest?> GetByIdAsync(int id);
        Task<List<SpellingTest>> GetByTeacherIdAsync(string teacherId);
        Task<List<SpellingTest>> GetByAssignmentIdAsync(int assignmentId);
        Task<List<SpellingTest>> GetActiveByTeacherIdAsync(string teacherId);
        Task<int> CreateAsync(SpellingTest test);
        Task<int> UpdateAsync(SpellingTest test);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> TeacherCanAccessTestAsync(string teacherId, int testId);
    }
}

