using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с вопросами по орфоэпии
    /// </summary>
    public class OrthoeopyQuestionRepository : BaseRepository<OrthoeopyQuestion>, IOrthoeopyQuestionRepository
    {
        public OrthoeopyQuestionRepository(IDatabaseConnection db) : base(db, "OrthoeopyQuestions")
        {
        }

        public async Task<List<OrthoeopyQuestion>> GetByTestIdAsync(int testId)
        {
            var sql = "SELECT * FROM OrthoeopyQuestions WHERE OrthoeopyTestId = @TestId";
            return await _db.QueryAsync<OrthoeopyQuestion>(sql, new { TestId = testId });
        }

        public async Task<List<OrthoeopyQuestion>> GetByTestIdOrderedAsync(int testId)
        {
            var sql = "SELECT * FROM OrthoeopyQuestions WHERE OrthoeopyTestId = @TestId ORDER BY OrderIndex";
            return await _db.QueryAsync<OrthoeopyQuestion>(sql, new { TestId = testId });
        }

        public async Task<int> GetCountByTestIdAsync(int testId)
        {
            var sql = "SELECT COUNT(*) FROM OrthoeopyQuestions WHERE OrthoeopyTestId = @TestId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TestId = testId });
            return result ?? 0;
        }
    }
}

