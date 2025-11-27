using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с классами
    /// </summary>
    public interface IClassRepository : IRepository<Class>
    {
        Task<List<Class>> GetByTeacherIdAsync(string teacherId);
        Task<List<Class>> GetActiveByTeacherIdAsync(string teacherId);
    }
}

