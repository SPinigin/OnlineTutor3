using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с вопросами по пунктуации
    /// </summary>
    public interface IPunctuationQuestionRepository : IRepository<PunctuationQuestion>
    {
        Task<List<PunctuationQuestion>> GetByTestIdAsync(int testId);
        Task<List<PunctuationQuestion>> GetByTestIdOrderedAsync(int testId);
        Task<int> GetCountByTestIdAsync(int testId);
    }
}

