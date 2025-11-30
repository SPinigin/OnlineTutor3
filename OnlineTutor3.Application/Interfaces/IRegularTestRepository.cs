using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с классическими тестами
    /// </summary>
    public interface IRegularTestRepository : IRepository<RegularTest>
    {
        Task<List<RegularTest>> GetByTeacherIdAsync(string teacherId);
        Task<List<RegularTest>> GetByAssignmentIdAsync(int assignmentId);
        Task<List<RegularTest>> GetActiveByTeacherIdAsync(string teacherId);
        Task<int> GetCountByTeacherIdAsync(string teacherId);
        Task<int> GetCountByAssignmentIdAsync(int assignmentId);
        /// <summary>
        /// Получает доступные тесты для студента с JOIN для оптимизации (один запрос вместо N+1)
        /// </summary>
        Task<List<RegularTest>> GetAvailableForStudentAsync(int studentId, int classId, string teacherId);
    }
}

