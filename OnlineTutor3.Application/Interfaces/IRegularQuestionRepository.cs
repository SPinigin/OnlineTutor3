using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с вопросами классических тестов
    /// </summary>
    public interface IRegularQuestionRepository : IRepository<RegularQuestion>
    {
        Task<List<RegularQuestion>> GetByTestIdAsync(int testId);
        Task<List<RegularQuestion>> GetByTestIdOrderedAsync(int testId);
        Task<int> GetCountByTestIdAsync(int testId);
    }
}

