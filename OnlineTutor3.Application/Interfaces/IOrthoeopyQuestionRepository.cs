using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с вопросами по орфоэпии
    /// </summary>
    public interface IOrthoeopyQuestionRepository : IRepository<OrthoeopyQuestion>
    {
        Task<List<OrthoeopyQuestion>> GetByTestIdAsync(int testId);
        Task<List<OrthoeopyQuestion>> GetByTestIdOrderedAsync(int testId);
        Task<int> GetCountByTestIdAsync(int testId);
    }
}

