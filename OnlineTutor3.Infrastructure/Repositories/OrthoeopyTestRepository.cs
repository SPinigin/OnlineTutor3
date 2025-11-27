using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с тестами по орфоэпии
    /// </summary>
    public class OrthoeopyTestRepository : BaseRepository<OrthoeopyTest>, IOrthoeopyTestRepository
    {
        public OrthoeopyTestRepository(IDatabaseConnection db) : base(db, "OrthoeopyTests")
        {
        }

        public async Task<List<OrthoeopyTest>> GetByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT * FROM OrthoeopyTests WHERE TeacherId = @TeacherId ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<OrthoeopyTest>(sql, new { TeacherId = teacherId });
        }

        public async Task<List<OrthoeopyTest>> GetByAssignmentIdAsync(int assignmentId)
        {
            var sql = "SELECT * FROM OrthoeopyTests WHERE AssignmentId = @AssignmentId ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<OrthoeopyTest>(sql, new { AssignmentId = assignmentId });
        }

        public async Task<List<OrthoeopyTest>> GetActiveByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT * FROM OrthoeopyTests WHERE TeacherId = @TeacherId AND IsActive = 1 ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<OrthoeopyTest>(sql, new { TeacherId = teacherId });
        }

        public async Task<int> GetCountByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT COUNT(*) FROM OrthoeopyTests WHERE TeacherId = @TeacherId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TeacherId = teacherId });
            return result ?? 0;
        }

        public async Task<int> GetCountByAssignmentIdAsync(int assignmentId)
        {
            var sql = "SELECT COUNT(*) FROM OrthoeopyTests WHERE AssignmentId = @AssignmentId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { AssignmentId = assignmentId });
            return result ?? 0;
        }
    }
}

