using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с вопросами классических тестов
    /// </summary>
    public class RegularQuestionRepository : BaseRepository<RegularQuestion>, IRegularQuestionRepository
    {
        public RegularQuestionRepository(IDatabaseConnection db) : base(db, "RegularQuestions")
        {
        }

        public async Task<List<RegularQuestion>> GetByTestIdAsync(int testId)
        {
            var sql = "SELECT * FROM RegularQuestions WHERE RegularTestId = @TestId";
            return await _db.QueryAsync<RegularQuestion>(sql, new { TestId = testId });
        }

        public async Task<List<RegularQuestion>> GetByTestIdOrderedAsync(int testId)
        {
            var sql = "SELECT * FROM RegularQuestions WHERE RegularTestId = @TestId ORDER BY OrderIndex";
            return await _db.QueryAsync<RegularQuestion>(sql, new { TestId = testId });
        }

        public async Task<int> GetCountByTestIdAsync(int testId)
        {
            var sql = "SELECT COUNT(*) FROM RegularQuestions WHERE RegularTestId = @TestId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TestId = testId });
            return result ?? 0;
        }
    }
}

