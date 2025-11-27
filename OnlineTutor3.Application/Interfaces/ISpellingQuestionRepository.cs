using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с вопросами по орфографии
    /// </summary>
    public interface ISpellingQuestionRepository : IRepository<SpellingQuestion>
    {
        Task<List<SpellingQuestion>> GetByTestIdAsync(int testId);
        Task<List<SpellingQuestion>> GetByTestIdOrderedAsync(int testId);
        Task<int> GetCountByTestIdAsync(int testId);
    }
}

