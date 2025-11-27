using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с результатами классических тестов
    /// </summary>
    public class RegularTestResultRepository : BaseRepository<RegularTestResult>, IRegularTestResultRepository
    {
        public RegularTestResultRepository(IDatabaseConnection db) : base(db, "RegularTestResults")
        {
        }

        public async Task<List<RegularTestResult>> GetByTestIdAsync(int testId)
        {
            var sql = "SELECT * FROM RegularTestResults WHERE RegularTestId = @TestId ORDER BY StartedAt DESC";
            return await _db.QueryAsync<RegularTestResult>(sql, new { TestId = testId });
        }

        public async Task<List<RegularTestResult>> GetByStudentIdAsync(int studentId)
        {
            var sql = "SELECT * FROM RegularTestResults WHERE StudentId = @StudentId ORDER BY StartedAt DESC";
            return await _db.QueryAsync<RegularTestResult>(sql, new { StudentId = studentId });
        }

        public async Task<List<RegularTestResult>> GetCompletedByTestIdAsync(int testId)
        {
            var sql = "SELECT * FROM RegularTestResults WHERE RegularTestId = @TestId AND IsCompleted = 1 ORDER BY CompletedAt DESC";
            return await _db.QueryAsync<RegularTestResult>(sql, new { TestId = testId });
        }

        public async Task<List<RegularTestResult>> GetByStudentAndTestIdAsync(int studentId, int testId)
        {
            var sql = "SELECT * FROM RegularTestResults WHERE StudentId = @StudentId AND RegularTestId = @TestId ORDER BY StartedAt DESC";
            return await _db.QueryAsync<RegularTestResult>(sql, new { StudentId = studentId, TestId = testId });
        }

        public async Task<RegularTestResult?> GetLatestByStudentAndTestIdAsync(int studentId, int testId)
        {
            var sql = "SELECT TOP 1 * FROM RegularTestResults WHERE StudentId = @StudentId AND RegularTestId = @TestId ORDER BY StartedAt DESC";
            return await _db.QueryFirstOrDefaultAsync<RegularTestResult>(sql, new { StudentId = studentId, TestId = testId });
        }

        public async Task<int> GetCountByTestIdAsync(int testId)
        {
            var sql = "SELECT COUNT(*) FROM RegularTestResults WHERE RegularTestId = @TestId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TestId = testId });
            return result ?? 0;
        }
    }
}

