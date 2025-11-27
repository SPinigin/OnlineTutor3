using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с результатами тестов по орфоэпии
    /// </summary>
    public class OrthoeopyTestResultRepository : BaseRepository<OrthoeopyTestResult>, IOrthoeopyTestResultRepository
    {
        public OrthoeopyTestResultRepository(IDatabaseConnection db) : base(db, "OrthoeopyTestResults")
        {
        }

        public async Task<List<OrthoeopyTestResult>> GetByTestIdAsync(int testId)
        {
            var sql = "SELECT * FROM OrthoeopyTestResults WHERE OrthoeopyTestId = @TestId ORDER BY StartedAt DESC";
            return await _db.QueryAsync<OrthoeopyTestResult>(sql, new { TestId = testId });
        }

        public async Task<List<OrthoeopyTestResult>> GetByStudentIdAsync(int studentId)
        {
            var sql = "SELECT * FROM OrthoeopyTestResults WHERE StudentId = @StudentId ORDER BY StartedAt DESC";
            return await _db.QueryAsync<OrthoeopyTestResult>(sql, new { StudentId = studentId });
        }

        public async Task<List<OrthoeopyTestResult>> GetCompletedByTestIdAsync(int testId)
        {
            var sql = "SELECT * FROM OrthoeopyTestResults WHERE OrthoeopyTestId = @TestId AND IsCompleted = 1 ORDER BY CompletedAt DESC";
            return await _db.QueryAsync<OrthoeopyTestResult>(sql, new { TestId = testId });
        }

        public async Task<List<OrthoeopyTestResult>> GetByStudentAndTestIdAsync(int studentId, int testId)
        {
            var sql = "SELECT * FROM OrthoeopyTestResults WHERE StudentId = @StudentId AND OrthoeopyTestId = @TestId ORDER BY StartedAt DESC";
            return await _db.QueryAsync<OrthoeopyTestResult>(sql, new { StudentId = studentId, TestId = testId });
        }

        public async Task<OrthoeopyTestResult?> GetLatestByStudentAndTestIdAsync(int studentId, int testId)
        {
            var sql = "SELECT TOP 1 * FROM OrthoeopyTestResults WHERE StudentId = @StudentId AND OrthoeopyTestId = @TestId ORDER BY StartedAt DESC";
            return await _db.QueryFirstOrDefaultAsync<OrthoeopyTestResult>(sql, new { StudentId = studentId, TestId = testId });
        }

        public async Task<int> GetCountByTestIdAsync(int testId)
        {
            var sql = "SELECT COUNT(*) FROM OrthoeopyTestResults WHERE OrthoeopyTestId = @TestId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TestId = testId });
            return result ?? 0;
        }
    }
}

