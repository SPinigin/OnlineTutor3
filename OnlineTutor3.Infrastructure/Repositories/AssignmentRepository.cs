using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с заданиями
    /// </summary>
    public class AssignmentRepository : BaseRepository<Assignment>, IAssignmentRepository
    {
        public AssignmentRepository(IDatabaseConnection db) : base(db, "Assignments")
        {
        }

        public async Task<List<Assignment>> GetByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT * FROM Assignments WHERE TeacherId = @TeacherId ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<Assignment>(sql, new { TeacherId = teacherId });
        }

        public async Task<List<Assignment>> GetBySubjectIdAsync(int subjectId)
        {
            var sql = "SELECT * FROM Assignments WHERE SubjectId = @SubjectId ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<Assignment>(sql, new { SubjectId = subjectId });
        }

        public async Task<List<Assignment>> GetByTeacherIdAndSubjectIdAsync(string teacherId, int subjectId)
        {
            var sql = "SELECT * FROM Assignments WHERE TeacherId = @TeacherId AND SubjectId = @SubjectId ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<Assignment>(sql, new { TeacherId = teacherId, SubjectId = subjectId });
        }

        /// <summary>
        /// Получает задания учителя только по предметам, которые он ведет
        /// </summary>
        public async Task<List<Assignment>> GetByTeacherSubjectsAsync(string teacherId)
        {
            var sql = @"
                SELECT DISTINCT a.*
                FROM Assignments a
                INNER JOIN Teachers t ON a.TeacherId = t.UserId
                INNER JOIN TeacherSubjects ts ON t.Id = ts.TeacherId
                WHERE a.TeacherId = @TeacherId 
                  AND a.SubjectId = ts.SubjectId
                ORDER BY a.CreatedAt DESC";
            return await _db.QueryAsync<Assignment>(sql, new { TeacherId = teacherId });
        }
    }
}

