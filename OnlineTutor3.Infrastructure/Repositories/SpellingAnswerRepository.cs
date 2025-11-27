using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с ответами по орфографии
    /// </summary>
    public class SpellingAnswerRepository : BaseRepository<SpellingAnswer>, ISpellingAnswerRepository
    {
        public SpellingAnswerRepository(IDatabaseConnection db) : base(db, "SpellingAnswers")
        {
        }

        public async Task<List<SpellingAnswer>> GetByTestResultIdAsync(int testResultId)
        {
            var sql = "SELECT * FROM SpellingAnswers WHERE TestResultId = @TestResultId ORDER BY Id";
            return await _db.QueryAsync<SpellingAnswer>(sql, new { TestResultId = testResultId });
        }

        public async Task<List<SpellingAnswer>> GetByQuestionIdAsync(int questionId)
        {
            var sql = "SELECT * FROM SpellingAnswers WHERE SpellingQuestionId = @QuestionId ORDER BY Id";
            return await _db.QueryAsync<SpellingAnswer>(sql, new { QuestionId = questionId });
        }

        public async Task<int> GetCountByTestResultIdAsync(int testResultId)
        {
            var sql = "SELECT COUNT(*) FROM SpellingAnswers WHERE TestResultId = @TestResultId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TestResultId = testResultId });
            return result ?? 0;
        }
    }
}

