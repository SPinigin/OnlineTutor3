using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для работы с предметами
    /// </summary>
    public interface ISubjectService
    {
        Task<Subject?> GetByIdAsync(int id);
        Task<List<Subject>> GetAllAsync();
        Task<List<Subject>> GetActiveAsync();
        Task<int> CreateAsync(Subject subject);
        Task<int> UpdateAsync(Subject subject);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}

