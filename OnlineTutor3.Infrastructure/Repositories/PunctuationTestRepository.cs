using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с тестами по пунктуации
    /// </summary>
    public class PunctuationTestRepository : BaseRepository<PunctuationTest>, IPunctuationTestRepository
    {
        public PunctuationTestRepository(IDatabaseConnection db) : base(db, "PunctuationTests")
        {
        }

        public async Task<List<PunctuationTest>> GetByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT * FROM PunctuationTests WHERE TeacherId = @TeacherId ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<PunctuationTest>(sql, new { TeacherId = teacherId });
        }

        public async Task<List<PunctuationTest>> GetByAssignmentIdAsync(int assignmentId)
        {
            var sql = "SELECT * FROM PunctuationTests WHERE AssignmentId = @AssignmentId ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<PunctuationTest>(sql, new { AssignmentId = assignmentId });
        }

        public async Task<List<PunctuationTest>> GetActiveByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT * FROM PunctuationTests WHERE TeacherId = @TeacherId AND IsActive = 1 ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<PunctuationTest>(sql, new { TeacherId = teacherId });
        }

        public async Task<int> GetCountByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT COUNT(*) FROM PunctuationTests WHERE TeacherId = @TeacherId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TeacherId = teacherId });
            return result ?? 0;
        }

        public async Task<int> GetCountByAssignmentIdAsync(int assignmentId)
        {
            var sql = "SELECT COUNT(*) FROM PunctuationTests WHERE AssignmentId = @AssignmentId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { AssignmentId = assignmentId });
            return result ?? 0;
        }
    }
}

