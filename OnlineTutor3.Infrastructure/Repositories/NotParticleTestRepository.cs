using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с тестами на правописание частицы "не"
    /// </summary>
    public class NotParticleTestRepository : BaseRepository<NotParticleTest>, INotParticleTestRepository
    {
        public NotParticleTestRepository(IDatabaseConnection db) : base(db, "NotParticleTests")
        {
        }

        public async Task<List<NotParticleTest>> GetByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT * FROM NotParticleTests WHERE TeacherId = @TeacherId ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<NotParticleTest>(sql, new { TeacherId = teacherId });
        }

        public async Task<List<NotParticleTest>> GetByAssignmentIdAsync(int assignmentId)
        {
            var sql = "SELECT * FROM NotParticleTests WHERE AssignmentId = @AssignmentId ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<NotParticleTest>(sql, new { AssignmentId = assignmentId });
        }

        public async Task<List<NotParticleTest>> GetActiveByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT * FROM NotParticleTests WHERE TeacherId = @TeacherId AND IsActive = 1 ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<NotParticleTest>(sql, new { TeacherId = teacherId });
        }

        public async Task<int> GetCountByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT COUNT(*) FROM NotParticleTests WHERE TeacherId = @TeacherId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TeacherId = teacherId });
            return result ?? 0;
        }

        public async Task<int> GetCountByAssignmentIdAsync(int assignmentId)
        {
            var sql = "SELECT COUNT(*) FROM NotParticleTests WHERE AssignmentId = @AssignmentId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { AssignmentId = assignmentId });
            return result ?? 0;
        }

        /// <summary>
        /// Получает доступные тесты для студента с JOIN для оптимизации (один запрос вместо N+1)
        /// </summary>
        public async Task<List<NotParticleTest>> GetAvailableForStudentAsync(int studentId, int classId, string teacherId)
        {
            var now = DateTime.Now;
            var sql = @"
                SELECT DISTINCT npt.*
                FROM NotParticleTests npt
                INNER JOIN Assignments a ON npt.AssignmentId = a.Id
                INNER JOIN AssignmentClasses ac ON a.Id = ac.AssignmentId
                WHERE npt.TeacherId = @TeacherId
                  AND npt.IsActive = 1
                  AND a.IsActive = 1
                  AND ac.ClassId = @ClassId
                  AND (npt.StartDate IS NULL OR npt.StartDate <= @Now)
                  AND (npt.EndDate IS NULL OR npt.EndDate >= @Now)
                ORDER BY npt.CreatedAt DESC";
            
            return await _db.QueryAsync<NotParticleTest>(sql, new 
            { 
                TeacherId = teacherId,
                ClassId = classId,
                Now = now
            });
        }
    }
}

