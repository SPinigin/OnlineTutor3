using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с ответами на классические тесты
    /// </summary>
    public interface IRegularAnswerRepository : IRepository<RegularAnswer>
    {
        Task<List<RegularAnswer>> GetByTestResultIdAsync(int testResultId);
        Task<List<RegularAnswer>> GetByQuestionIdAsync(int questionId);
        Task<int> GetCountByTestResultIdAsync(int testResultId);
    }
}

