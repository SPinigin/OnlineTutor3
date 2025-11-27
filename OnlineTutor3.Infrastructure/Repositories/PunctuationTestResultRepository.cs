using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с результатами тестов по пунктуации
    /// </summary>
    public class PunctuationTestResultRepository : BaseRepository<PunctuationTestResult>, IPunctuationTestResultRepository
    {
        public PunctuationTestResultRepository(IDatabaseConnection db) : base(db, "PunctuationTestResults")
        {
        }

        public async Task<List<PunctuationTestResult>> GetByTestIdAsync(int testId)
        {
            var sql = "SELECT * FROM PunctuationTestResults WHERE PunctuationTestId = @TestId ORDER BY StartedAt DESC";
            return await _db.QueryAsync<PunctuationTestResult>(sql, new { TestId = testId });
        }

        public async Task<List<PunctuationTestResult>> GetByStudentIdAsync(int studentId)
        {
            var sql = "SELECT * FROM PunctuationTestResults WHERE StudentId = @StudentId ORDER BY StartedAt DESC";
            return await _db.QueryAsync<PunctuationTestResult>(sql, new { StudentId = studentId });
        }

        public async Task<List<PunctuationTestResult>> GetCompletedByTestIdAsync(int testId)
        {
            var sql = "SELECT * FROM PunctuationTestResults WHERE PunctuationTestId = @TestId AND IsCompleted = 1 ORDER BY CompletedAt DESC";
            return await _db.QueryAsync<PunctuationTestResult>(sql, new { TestId = testId });
        }

        public async Task<List<PunctuationTestResult>> GetByStudentAndTestIdAsync(int studentId, int testId)
        {
            var sql = "SELECT * FROM PunctuationTestResults WHERE StudentId = @StudentId AND PunctuationTestId = @TestId ORDER BY StartedAt DESC";
            return await _db.QueryAsync<PunctuationTestResult>(sql, new { StudentId = studentId, TestId = testId });
        }

        public async Task<PunctuationTestResult?> GetLatestByStudentAndTestIdAsync(int studentId, int testId)
        {
            var sql = "SELECT TOP 1 * FROM PunctuationTestResults WHERE StudentId = @StudentId AND PunctuationTestId = @TestId ORDER BY StartedAt DESC";
            return await _db.QueryFirstOrDefaultAsync<PunctuationTestResult>(sql, new { StudentId = studentId, TestId = testId });
        }

        public async Task<int> GetCountByTestIdAsync(int testId)
        {
            var sql = "SELECT COUNT(*) FROM PunctuationTestResults WHERE PunctuationTestId = @TestId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TestId = testId });
            return result ?? 0;
        }
    }
}

