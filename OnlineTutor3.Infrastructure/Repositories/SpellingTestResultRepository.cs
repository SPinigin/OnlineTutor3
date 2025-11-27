using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с результатами тестов по орфографии
    /// </summary>
    public class SpellingTestResultRepository : BaseRepository<SpellingTestResult>, ISpellingTestResultRepository
    {
        public SpellingTestResultRepository(IDatabaseConnection db) : base(db, "SpellingTestResults")
        {
        }

        public async Task<List<SpellingTestResult>> GetByTestIdAsync(int testId)
        {
            var sql = "SELECT * FROM SpellingTestResults WHERE SpellingTestId = @TestId ORDER BY StartedAt DESC";
            return await _db.QueryAsync<SpellingTestResult>(sql, new { TestId = testId });
        }

        public async Task<List<SpellingTestResult>> GetByStudentIdAsync(int studentId)
        {
            var sql = "SELECT * FROM SpellingTestResults WHERE StudentId = @StudentId ORDER BY StartedAt DESC";
            return await _db.QueryAsync<SpellingTestResult>(sql, new { StudentId = studentId });
        }

        public async Task<List<SpellingTestResult>> GetCompletedByTestIdAsync(int testId)
        {
            var sql = "SELECT * FROM SpellingTestResults WHERE SpellingTestId = @TestId AND IsCompleted = 1 ORDER BY CompletedAt DESC";
            return await _db.QueryAsync<SpellingTestResult>(sql, new { TestId = testId });
        }

        public async Task<List<SpellingTestResult>> GetByStudentAndTestIdAsync(int studentId, int testId)
        {
            var sql = "SELECT * FROM SpellingTestResults WHERE StudentId = @StudentId AND SpellingTestId = @TestId ORDER BY StartedAt DESC";
            return await _db.QueryAsync<SpellingTestResult>(sql, new { StudentId = studentId, TestId = testId });
        }

        public async Task<SpellingTestResult?> GetLatestByStudentAndTestIdAsync(int studentId, int testId)
        {
            var sql = "SELECT TOP 1 * FROM SpellingTestResults WHERE StudentId = @StudentId AND SpellingTestId = @TestId ORDER BY StartedAt DESC";
            return await _db.QueryFirstOrDefaultAsync<SpellingTestResult>(sql, new { StudentId = studentId, TestId = testId });
        }

        public async Task<int> GetCountByTestIdAsync(int testId)
        {
            var sql = "SELECT COUNT(*) FROM SpellingTestResults WHERE SpellingTestId = @TestId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TestId = testId });
            return result ?? 0;
        }
    }
}

