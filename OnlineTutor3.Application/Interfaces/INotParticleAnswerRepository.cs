using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с ответами на вопросы теста на правописание частицы "не"
    /// </summary>
    public interface INotParticleAnswerRepository : IRepository<NotParticleAnswer>
    {
        Task<List<NotParticleAnswer>> GetByTestResultIdAsync(int testResultId);
        Task<List<NotParticleAnswer>> GetByQuestionIdAsync(int questionId);
        Task<int> GetCountByTestResultIdAsync(int testResultId);
    }
}

