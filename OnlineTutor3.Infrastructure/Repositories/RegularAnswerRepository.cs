using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с ответами на классические тесты
    /// </summary>
    public class RegularAnswerRepository : BaseRepository<RegularAnswer>, IRegularAnswerRepository
    {
        public RegularAnswerRepository(IDatabaseConnection db) : base(db, "RegularAnswers")
        {
        }

        public async Task<List<RegularAnswer>> GetByTestResultIdAsync(int testResultId)
        {
            var sql = "SELECT * FROM RegularAnswers WHERE TestResultId = @TestResultId ORDER BY Id";
            return await _db.QueryAsync<RegularAnswer>(sql, new { TestResultId = testResultId });
        }

        public async Task<List<RegularAnswer>> GetByQuestionIdAsync(int questionId)
        {
            var sql = "SELECT * FROM RegularAnswers WHERE RegularQuestionId = @QuestionId ORDER BY Id";
            return await _db.QueryAsync<RegularAnswer>(sql, new { QuestionId = questionId });
        }

        public async Task<int> GetCountByTestResultIdAsync(int testResultId)
        {
            var sql = "SELECT COUNT(*) FROM RegularAnswers WHERE TestResultId = @TestResultId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TestResultId = testResultId });
            return result ?? 0;
        }
    }
}

