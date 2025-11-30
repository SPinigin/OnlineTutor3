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

        /// <summary>
        /// Получает доступные тесты для студента с JOIN для оптимизации (один запрос вместо N+1)
        /// </summary>
        public async Task<List<SpellingTest>> GetAvailableForStudentAsync(int studentId, int classId, string teacherId)
        {
            var now = DateTime.Now;
            var sql = @"
                SELECT DISTINCT st.*
                FROM SpellingTests st
                INNER JOIN Assignments a ON st.AssignmentId = a.Id
                INNER JOIN AssignmentClasses ac ON a.Id = ac.AssignmentId
                WHERE st.TeacherId = @TeacherId
                  AND st.IsActive = 1
                  AND a.IsActive = 1
                  AND ac.ClassId = @ClassId
                  AND (st.StartDate IS NULL OR st.StartDate <= @Now)
                  AND (st.EndDate IS NULL OR st.EndDate >= @Now)
                ORDER BY st.CreatedAt DESC";
            
            return await _db.QueryAsync<SpellingTest>(sql, new 
            { 
                TeacherId = teacherId,
                ClassId = classId,
                Now = now
            });
        }
    }
}

