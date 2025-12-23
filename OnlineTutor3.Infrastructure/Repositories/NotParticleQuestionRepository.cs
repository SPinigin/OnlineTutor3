using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с вопросами теста на правописание частицы "не"
    /// </summary>
    public class NotParticleQuestionRepository : BaseRepository<NotParticleQuestion>, INotParticleQuestionRepository
    {
        public NotParticleQuestionRepository(IDatabaseConnection db) : base(db, "NotParticleQuestions")
        {
        }

        public async Task<List<NotParticleQuestion>> GetByTestIdAsync(int testId)
        {
            var sql = "SELECT * FROM NotParticleQuestions WHERE NotParticleTestId = @TestId";
            return await _db.QueryAsync<NotParticleQuestion>(sql, new { TestId = testId });
        }

        public async Task<List<NotParticleQuestion>> GetByTestIdOrderedAsync(int testId)
        {
            var sql = "SELECT * FROM NotParticleQuestions WHERE NotParticleTestId = @TestId ORDER BY OrderIndex";
            return await _db.QueryAsync<NotParticleQuestion>(sql, new { TestId = testId });
        }

        public async Task<int> GetCountByTestIdAsync(int testId)
        {
            var sql = "SELECT COUNT(*) FROM NotParticleQuestions WHERE NotParticleTestId = @TestId";
            long? result = await _db.QueryScalarAsync<long>(sql, new { TestId = testId });
            return result.HasValue ? (int)result.Value : 0;
        }
    }
}

