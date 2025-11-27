using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с тестами по орфографии
    /// </summary>
    public class SpellingTestRepository : BaseRepository<SpellingTest>, ISpellingTestRepository
    {
        public SpellingTestRepository(IDatabaseConnection db) : base(db, "SpellingTests")
        {
        }

        public async Task<List<SpellingTest>> GetByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT * FROM SpellingTests WHERE TeacherId = @TeacherId ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<SpellingTest>(sql, new { TeacherId = teacherId });
        }

        public async Task<List<SpellingTest>> GetByAssignmentIdAsync(int assignmentId)
        {
            var sql = "SELECT * FROM SpellingTests WHERE AssignmentId = @AssignmentId ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<SpellingTest>(sql, new { AssignmentId = assignmentId });
        }

        public async Task<List<SpellingTest>> GetActiveByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT * FROM SpellingTests WHERE TeacherId = @TeacherId AND IsActive = 1 ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<SpellingTest>(sql, new { TeacherId = teacherId });
        }

        public async Task<int> GetCountByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT COUNT(*) FROM SpellingTests WHERE TeacherId = @TeacherId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TeacherId = teacherId });
            return result ?? 0;
        }

        public async Task<int> GetCountByAssignmentIdAsync(int assignmentId)
        {
            var sql = "SELECT COUNT(*) FROM SpellingTests WHERE AssignmentId = @AssignmentId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { AssignmentId = assignmentId });
            return result ?? 0;
        }
    }
}

