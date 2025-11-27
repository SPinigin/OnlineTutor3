using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с предметами
    /// </summary>
    public interface ISubjectRepository : IRepository<Subject>
    {
        Task<List<Subject>> GetActiveAsync();
    }
}

