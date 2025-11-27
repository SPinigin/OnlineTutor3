using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с учениками
    /// </summary>
    public class StudentRepository : BaseRepository<Student>, IStudentRepository
    {
        public StudentRepository(IDatabaseConnection db) : base(db, "Students")
        {
        }

        public async Task<Student?> GetByUserIdAsync(string userId)
        {
            var sql = "SELECT * FROM Students WHERE UserId = @UserId";
            return await _db.QueryFirstOrDefaultAsync<Student>(sql, new { UserId = userId });
        }

        public async Task<List<Student>> GetByClassIdAsync(int classId)
        {
            var sql = "SELECT * FROM Students WHERE ClassId = @ClassId ORDER BY CreatedAt";
            return await _db.QueryAsync<Student>(sql, new { ClassId = classId });
        }

        public async Task<List<Student>> GetByTeacherIdAsync(string teacherId)
        {
            var sql = @"
                SELECT s.* 
                FROM Students s
                INNER JOIN Classes c ON s.ClassId = c.Id
                WHERE c.TeacherId = @TeacherId
                ORDER BY c.Name, s.CreatedAt";
            return await _db.QueryAsync<Student>(sql, new { TeacherId = teacherId });
        }
    }
}

