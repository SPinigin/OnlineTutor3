using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для работы с тестами на правописание частицы "не"
    /// </summary>
    public interface INotParticleTestService
    {
        Task<NotParticleTest?> GetByIdAsync(int id);
        Task<List<NotParticleTest>> GetByTeacherIdAsync(string teacherId);
        Task<List<NotParticleTest>> GetByAssignmentIdAsync(int assignmentId);
        Task<List<NotParticleTest>> GetActiveByTeacherIdAsync(string teacherId);
        Task<int> CreateAsync(NotParticleTest test);
        Task<int> UpdateAsync(NotParticleTest test);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> TeacherCanAccessTestAsync(string teacherId, int testId);
    }
}

