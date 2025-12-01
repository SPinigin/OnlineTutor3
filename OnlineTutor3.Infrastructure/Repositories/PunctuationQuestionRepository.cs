using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с вопросами по пунктуации
    /// </summary>
    public class PunctuationQuestionRepository : BaseRepository<PunctuationQuestion>, IPunctuationQuestionRepository
    {
        public PunctuationQuestionRepository(IDatabaseConnection db) : base(db, "PunctuationQuestions")
        {
        }

        public async Task<List<PunctuationQuestion>> GetByTestIdAsync(int testId)
        {
            var sql = "SELECT * FROM PunctuationQuestions WHERE PunctuationTestId = @TestId";
            return await _db.QueryAsync<PunctuationQuestion>(sql, new { TestId = testId });
        }

        public async Task<List<PunctuationQuestion>> GetByTestIdOrderedAsync(int testId)
        {
            var sql = "SELECT * FROM PunctuationQuestions WHERE PunctuationTestId = @TestId ORDER BY OrderIndex";
            return await _db.QueryAsync<PunctuationQuestion>(sql, new { TestId = testId });
        }

        public async Task<int> GetCountByTestIdAsync(int testId)
        {
            var sql = "SELECT COUNT(*) FROM PunctuationQuestions WHERE PunctuationTestId = @TestId";
            long? result = await _db.QueryScalarAsync<long>(sql, new { TestId = testId });
            return result.HasValue ? (int)result.Value : 0;
        }
    }
}

