using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с вопросами теста на правописание частицы "не"
    /// </summary>
    public interface INotParticleQuestionRepository : IRepository<NotParticleQuestion>
    {
        Task<List<NotParticleQuestion>> GetByTestIdAsync(int testId);
        Task<List<NotParticleQuestion>> GetByTestIdOrderedAsync(int testId);
        Task<int> GetCountByTestIdAsync(int testId);
    }
}

