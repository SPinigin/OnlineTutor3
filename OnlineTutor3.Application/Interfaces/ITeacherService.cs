using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для работы с учителями
    /// </summary>
    public interface ITeacherService
    {
        Task<Teacher?> GetByIdAsync(int id);
        Task<Teacher?> GetByUserIdAsync(string userId);
        Task<List<Teacher>> GetApprovedAsync();
        Task<int> CreateAsync(Teacher teacher);
        Task<int> UpdateAsync(Teacher teacher);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<List<Subject>> GetTeacherSubjectsAsync(int teacherId);
        Task<List<Subject>> GetTeacherSubjectsByUserIdAsync(string userId);
        Task<bool> TeacherTeachesSubjectAsync(string userId, int subjectId);
        Task<int> AddSubjectToTeacherAsync(int teacherId, int subjectId);
        Task<int> RemoveSubjectFromTeacherAsync(int teacherId, int subjectId);
    }
}

