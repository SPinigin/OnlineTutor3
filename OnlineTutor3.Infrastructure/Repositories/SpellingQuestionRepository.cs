using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с вопросами по орфографии
    /// </summary>
    public class SpellingQuestionRepository : BaseRepository<SpellingQuestion>, ISpellingQuestionRepository
    {
        public SpellingQuestionRepository(IDatabaseConnection db) : base(db, "SpellingQuestions")
        {
        }

        public async Task<List<SpellingQuestion>> GetByTestIdAsync(int testId)
        {
            var sql = "SELECT * FROM SpellingQuestions WHERE SpellingTestId = @TestId";
            return await _db.QueryAsync<SpellingQuestion>(sql, new { TestId = testId });
        }

        public async Task<List<SpellingQuestion>> GetByTestIdOrderedAsync(int testId)
        {
            var sql = "SELECT * FROM SpellingQuestions WHERE SpellingTestId = @TestId ORDER BY OrderIndex";
            return await _db.QueryAsync<SpellingQuestion>(sql, new { TestId = testId });
        }

        public async Task<int> GetCountByTestIdAsync(int testId)
        {
            var sql = "SELECT COUNT(*) FROM SpellingQuestions WHERE SpellingTestId = @TestId";
            long? result = await _db.QueryScalarAsync<long>(sql, new { TestId = testId });
            return result.HasValue ? (int)result.Value : 0;
        }
    }
}

