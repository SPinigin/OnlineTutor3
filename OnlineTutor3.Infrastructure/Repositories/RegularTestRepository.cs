using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с классическими тестами
    /// </summary>
    public class RegularTestRepository : BaseRepository<RegularTest>, IRegularTestRepository
    {
        public RegularTestRepository(IDatabaseConnection db) : base(db, "RegularTests")
        {
        }

        public async Task<List<RegularTest>> GetByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT * FROM RegularTests WHERE TeacherId = @TeacherId ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<RegularTest>(sql, new { TeacherId = teacherId });
        }

        public async Task<List<RegularTest>> GetByAssignmentIdAsync(int assignmentId)
        {
            var sql = "SELECT * FROM RegularTests WHERE AssignmentId = @AssignmentId ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<RegularTest>(sql, new { AssignmentId = assignmentId });
        }

        public async Task<List<RegularTest>> GetActiveByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT * FROM RegularTests WHERE TeacherId = @TeacherId AND IsActive = 1 ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<RegularTest>(sql, new { TeacherId = teacherId });
        }

        public async Task<int> GetCountByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT COUNT(*) FROM RegularTests WHERE TeacherId = @TeacherId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TeacherId = teacherId });
            return result ?? 0;
        }

        public async Task<int> GetCountByAssignmentIdAsync(int assignmentId)
        {
            var sql = "SELECT COUNT(*) FROM RegularTests WHERE AssignmentId = @AssignmentId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { AssignmentId = assignmentId });
            return result ?? 0;
        }
    }
}

