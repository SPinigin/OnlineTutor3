using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с ответами по орфоэпии
    /// </summary>
    public class OrthoeopyAnswerRepository : BaseRepository<OrthoeopyAnswer>, IOrthoeopyAnswerRepository
    {
        public OrthoeopyAnswerRepository(IDatabaseConnection db) : base(db, "OrthoeopyAnswers")
        {
        }

        public async Task<List<OrthoeopyAnswer>> GetByTestResultIdAsync(int testResultId)
        {
            var sql = "SELECT * FROM OrthoeopyAnswers WHERE TestResultId = @TestResultId ORDER BY Id";
            return await _db.QueryAsync<OrthoeopyAnswer>(sql, new { TestResultId = testResultId });
        }

        public async Task<List<OrthoeopyAnswer>> GetByQuestionIdAsync(int questionId)
        {
            var sql = "SELECT * FROM OrthoeopyAnswers WHERE OrthoeopyQuestionId = @QuestionId ORDER BY Id";
            return await _db.QueryAsync<OrthoeopyAnswer>(sql, new { QuestionId = questionId });
        }

        public async Task<int> GetCountByTestResultIdAsync(int testResultId)
        {
            var sql = "SELECT COUNT(*) FROM OrthoeopyAnswers WHERE TestResultId = @TestResultId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TestResultId = testResultId });
            return result ?? 0;
        }
    }
}

