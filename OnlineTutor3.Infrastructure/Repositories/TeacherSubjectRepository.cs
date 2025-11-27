using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы со связями учителей и предметов
    /// </summary>
    public class TeacherSubjectRepository : BaseRepository<TeacherSubject>, ITeacherSubjectRepository
    {
        public TeacherSubjectRepository(IDatabaseConnection db) : base(db, "TeacherSubjects")
        {
        }

        public async Task<List<TeacherSubject>> GetByTeacherIdAsync(int teacherId)
        {
            var sql = "SELECT * FROM TeacherSubjects WHERE TeacherId = @TeacherId ORDER BY CreatedAt";
            return await _db.QueryAsync<TeacherSubject>(sql, new { TeacherId = teacherId });
        }

        public async Task<List<TeacherSubject>> GetBySubjectIdAsync(int subjectId)
        {
            var sql = "SELECT * FROM TeacherSubjects WHERE SubjectId = @SubjectId ORDER BY CreatedAt";
            return await _db.QueryAsync<TeacherSubject>(sql, new { SubjectId = subjectId });
        }

        public async Task<TeacherSubject?> GetByTeacherAndSubjectIdAsync(int teacherId, int subjectId)
        {
            var sql = "SELECT * FROM TeacherSubjects WHERE TeacherId = @TeacherId AND SubjectId = @SubjectId";
            return await _db.QueryFirstOrDefaultAsync<TeacherSubject>(sql, new { TeacherId = teacherId, SubjectId = subjectId });
        }

        public async Task<int> DeleteByTeacherIdAsync(int teacherId)
        {
            var sql = "DELETE FROM TeacherSubjects WHERE TeacherId = @TeacherId";
            return await _db.ExecuteAsync(sql, new { TeacherId = teacherId });
        }
    }
}

