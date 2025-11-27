using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с ответами по пунктуации
    /// </summary>
    public class PunctuationAnswerRepository : BaseRepository<PunctuationAnswer>, IPunctuationAnswerRepository
    {
        public PunctuationAnswerRepository(IDatabaseConnection db) : base(db, "PunctuationAnswers")
        {
        }

        public async Task<List<PunctuationAnswer>> GetByTestResultIdAsync(int testResultId)
        {
            var sql = "SELECT * FROM PunctuationAnswers WHERE TestResultId = @TestResultId ORDER BY Id";
            return await _db.QueryAsync<PunctuationAnswer>(sql, new { TestResultId = testResultId });
        }

        public async Task<List<PunctuationAnswer>> GetByQuestionIdAsync(int questionId)
        {
            var sql = "SELECT * FROM PunctuationAnswers WHERE PunctuationQuestionId = @QuestionId ORDER BY Id";
            return await _db.QueryAsync<PunctuationAnswer>(sql, new { QuestionId = questionId });
        }

        public async Task<int> GetCountByTestResultIdAsync(int testResultId)
        {
            var sql = "SELECT COUNT(*) FROM PunctuationAnswers WHERE TestResultId = @TestResultId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TestResultId = testResultId });
            return result ?? 0;
        }
    }
}

