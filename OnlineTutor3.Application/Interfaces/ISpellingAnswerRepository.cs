using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с ответами по орфографии
    /// </summary>
    public interface ISpellingAnswerRepository : IRepository<SpellingAnswer>
    {
        Task<List<SpellingAnswer>> GetByTestResultIdAsync(int testResultId);
        Task<List<SpellingAnswer>> GetByQuestionIdAsync(int questionId);
        Task<int> GetCountByTestResultIdAsync(int testResultId);
    }
}

