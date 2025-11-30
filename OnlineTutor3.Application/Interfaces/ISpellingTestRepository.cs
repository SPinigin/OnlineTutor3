using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с тестами по орфографии
    /// </summary>
    public interface ISpellingTestRepository : IRepository<SpellingTest>
    {
        Task<List<SpellingTest>> GetByTeacherIdAsync(string teacherId);
        Task<List<SpellingTest>> GetByAssignmentIdAsync(int assignmentId);
        Task<List<SpellingTest>> GetActiveByTeacherIdAsync(string teacherId);
        Task<int> GetCountByTeacherIdAsync(string teacherId);
        Task<int> GetCountByAssignmentIdAsync(int assignmentId);
        /// <summary>
        /// Получает доступные тесты для студента с JOIN для оптимизации (один запрос вместо N+1)
        /// </summary>
        Task<List<SpellingTest>> GetAvailableForStudentAsync(int studentId, int classId, string teacherId);
    }
}

