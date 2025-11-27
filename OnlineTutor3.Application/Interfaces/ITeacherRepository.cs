using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с учителями
    /// </summary>
    public interface ITeacherRepository : IRepository<Teacher>
    {
        Task<Teacher?> GetByUserIdAsync(string userId);
        Task<List<Teacher>> GetApprovedAsync();
    }
}

