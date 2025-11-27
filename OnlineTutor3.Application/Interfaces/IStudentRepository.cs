using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с учениками
    /// </summary>
    public interface IStudentRepository : IRepository<Student>
    {
        Task<Student?> GetByUserIdAsync(string userId);
        Task<List<Student>> GetByClassIdAsync(int classId);
        Task<List<Student>> GetByTeacherIdAsync(string teacherId);
    }
}

