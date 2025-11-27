using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с ответами по орфоэпии
    /// </summary>
    public interface IOrthoeopyAnswerRepository : IRepository<OrthoeopyAnswer>
    {
        Task<List<OrthoeopyAnswer>> GetByTestResultIdAsync(int testResultId);
        Task<List<OrthoeopyAnswer>> GetByQuestionIdAsync(int questionId);
        Task<int> GetCountByTestResultIdAsync(int testResultId);
    }
}

