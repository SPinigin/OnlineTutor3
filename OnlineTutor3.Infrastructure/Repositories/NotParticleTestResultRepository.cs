using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с результатами теста на правописание частицы "не"
    /// </summary>
    public class NotParticleTestResultRepository : BaseRepository<NotParticleTestResult>, INotParticleTestResultRepository
    {
        public NotParticleTestResultRepository(IDatabaseConnection db) : base(db, "NotParticleTestResults")
        {
        }

        public async Task<List<NotParticleTestResult>> GetByTestIdAsync(int testId)
        {
            var sql = "SELECT * FROM NotParticleTestResults WHERE NotParticleTestId = @TestId ORDER BY StartedAt DESC";
            return await _db.QueryAsync<NotParticleTestResult>(sql, new { TestId = testId });
        }

        public async Task<List<NotParticleTestResult>> GetByStudentIdAsync(int studentId)
        {
            var sql = "SELECT * FROM NotParticleTestResults WHERE StudentId = @StudentId ORDER BY StartedAt DESC";
            return await _db.QueryAsync<NotParticleTestResult>(sql, new { StudentId = studentId });
        }

        public async Task<List<NotParticleTestResult>> GetCompletedByTestIdAsync(int testId)
        {
            var sql = "SELECT * FROM NotParticleTestResults WHERE NotParticleTestId = @TestId AND IsCompleted = 1 ORDER BY CompletedAt DESC";
            return await _db.QueryAsync<NotParticleTestResult>(sql, new { TestId = testId });
        }

        public async Task<List<NotParticleTestResult>> GetByStudentAndTestIdAsync(int studentId, int testId)
        {
            var sql = "SELECT * FROM NotParticleTestResults WHERE StudentId = @StudentId AND NotParticleTestId = @TestId ORDER BY StartedAt DESC";
            return await _db.QueryAsync<NotParticleTestResult>(sql, new { StudentId = studentId, TestId = testId });
        }

        public async Task<NotParticleTestResult?> GetLatestByStudentAndTestIdAsync(int studentId, int testId)
        {
            var sql = "SELECT TOP 1 * FROM NotParticleTestResults WHERE StudentId = @StudentId AND NotParticleTestId = @TestId ORDER BY StartedAt DESC";
            return await _db.QueryFirstOrDefaultAsync<NotParticleTestResult>(sql, new { StudentId = studentId, TestId = testId });
        }

        public async Task<int> GetCountByTestIdAsync(int testId)
        {
            var sql = "SELECT COUNT(*) FROM NotParticleTestResults WHERE NotParticleTestId = @TestId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TestId = testId });
            return result ?? 0;
        }
    }
}

