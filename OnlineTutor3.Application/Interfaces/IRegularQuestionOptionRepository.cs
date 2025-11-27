using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с вариантами ответов классических тестов
    /// </summary>
    public interface IRegularQuestionOptionRepository : IRepository<RegularQuestionOption>
    {
        Task<List<RegularQuestionOption>> GetByQuestionIdAsync(int questionId);
        Task<List<RegularQuestionOption>> GetByQuestionIdOrderedAsync(int questionId);
        Task<int> GetCountByQuestionIdAsync(int questionId);
    }
}

