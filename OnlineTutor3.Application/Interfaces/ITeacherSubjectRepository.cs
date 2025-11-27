using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы со связями учителей и предметов
    /// </summary>
    public interface ITeacherSubjectRepository : IRepository<TeacherSubject>
    {
        Task<List<TeacherSubject>> GetByTeacherIdAsync(int teacherId);
        Task<List<TeacherSubject>> GetBySubjectIdAsync(int subjectId);
        Task<TeacherSubject?> GetByTeacherAndSubjectIdAsync(int teacherId, int subjectId);
        Task<int> DeleteByTeacherIdAsync(int teacherId);
    }
}

