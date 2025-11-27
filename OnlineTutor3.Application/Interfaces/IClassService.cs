using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для работы с классами
    /// </summary>
    public interface IClassService
    {
        Task<Class?> GetByIdAsync(int id);
        Task<List<Class>> GetByTeacherIdAsync(string teacherId);
        Task<List<Class>> GetActiveByTeacherIdAsync(string teacherId);
        Task<int> CreateAsync(Class @class);
        Task<int> UpdateAsync(Class @class);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}

