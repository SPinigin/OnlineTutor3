using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с заданиями
    /// </summary>
    public interface IAssignmentRepository : IRepository<Assignment>
    {
        Task<List<Assignment>> GetByTeacherIdAsync(string teacherId);
        Task<List<Assignment>> GetBySubjectIdAsync(int subjectId);
        Task<List<Assignment>> GetByTeacherIdAndSubjectIdAsync(string teacherId, int subjectId);
        Task<List<Assignment>> GetByTeacherSubjectsAsync(string teacherId); // Только по предметам, которые ведет учитель
    }
}

