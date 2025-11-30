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

        /// <summary>
        /// Получает доступные тесты для студента с JOIN для оптимизации (один запрос вместо N+1)
        /// </summary>
        public async Task<List<OrthoeopyTest>> GetAvailableForStudentAsync(int studentId, int classId, string teacherId)
        {
            var now = DateTime.Now;
            var sql = @"
                SELECT DISTINCT ot.*
                FROM OrthoeopyTests ot
                INNER JOIN Assignments a ON ot.AssignmentId = a.Id
                INNER JOIN AssignmentClasses ac ON a.Id = ac.AssignmentId
                WHERE ot.TeacherId = @TeacherId
                  AND ot.IsActive = 1
                  AND a.IsActive = 1
                  AND ac.ClassId = @ClassId
                  AND (ot.StartDate IS NULL OR ot.StartDate <= @Now)
                  AND (ot.EndDate IS NULL OR ot.EndDate >= @Now)
                ORDER BY ot.CreatedAt DESC";
            
            return await _db.QueryAsync<OrthoeopyTest>(sql, new 
            { 
                TeacherId = teacherId,
                ClassId = classId,
                Now = now
            });
        }
    }
}

