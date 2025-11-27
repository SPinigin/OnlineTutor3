using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для работы с учениками
    /// </summary>
    public interface IStudentService
    {
        Task<Student?> GetByIdAsync(int id);
        Task<Student?> GetByUserIdAsync(string userId);
        Task<List<Student>> GetByClassIdAsync(int classId);
        Task<List<Student>> GetByTeacherIdAsync(string teacherId);
        Task<int> CreateAsync(Student student);
        Task<int> UpdateAsync(Student student);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}

