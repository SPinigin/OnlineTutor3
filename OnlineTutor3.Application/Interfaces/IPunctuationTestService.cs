using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для работы с тестами по пунктуации
    /// </summary>
    public interface IPunctuationTestService
    {
        Task<PunctuationTest?> GetByIdAsync(int id);
        Task<List<PunctuationTest>> GetByTeacherIdAsync(string teacherId);
        Task<List<PunctuationTest>> GetByAssignmentIdAsync(int assignmentId);
        Task<List<PunctuationTest>> GetActiveByTeacherIdAsync(string teacherId);
        Task<int> CreateAsync(PunctuationTest test);
        Task<int> UpdateAsync(PunctuationTest test);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> TeacherCanAccessTestAsync(string teacherId, int testId);
    }
}

