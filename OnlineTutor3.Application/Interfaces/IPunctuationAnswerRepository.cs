using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с ответами по пунктуации
    /// </summary>
    public interface IPunctuationAnswerRepository : IRepository<PunctuationAnswer>
    {
        Task<List<PunctuationAnswer>> GetByTestResultIdAsync(int testResultId);
        Task<List<PunctuationAnswer>> GetByQuestionIdAsync(int questionId);
        Task<int> GetCountByTestResultIdAsync(int testResultId);
    }
}

