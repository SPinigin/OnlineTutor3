using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с тестами на правописание частицы "не"
    /// </summary>
    public interface INotParticleTestRepository : IRepository<NotParticleTest>
    {
        Task<List<NotParticleTest>> GetByTeacherIdAsync(string teacherId);
        Task<List<NotParticleTest>> GetByAssignmentIdAsync(int assignmentId);
        Task<List<NotParticleTest>> GetActiveByTeacherIdAsync(string teacherId);
        Task<int> GetCountByTeacherIdAsync(string teacherId);
        Task<int> GetCountByAssignmentIdAsync(int assignmentId);
        /// <summary>
        /// Получает доступные тесты для студента с JOIN для оптимизации (один запрос вместо N+1)
        /// </summary>
        Task<List<NotParticleTest>> GetAvailableForStudentAsync(int studentId, int classId, string teacherId);
    }
}

