using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с классами
    /// </summary>
    public class ClassRepository : BaseRepository<Class>, IClassRepository
    {
        public ClassRepository(IDatabaseConnection db) : base(db, "Classes")
        {
        }

        public async Task<List<Class>> GetByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT * FROM Classes WHERE TeacherId = @TeacherId ORDER BY Name";
            return await _db.QueryAsync<Class>(sql, new { TeacherId = teacherId });
        }

        public async Task<List<Class>> GetActiveByTeacherIdAsync(string teacherId)
        {
            var sql = "SELECT * FROM Classes WHERE TeacherId = @TeacherId AND IsActive = 1 ORDER BY Name";
            return await _db.QueryAsync<Class>(sql, new { TeacherId = teacherId });
        }
    }
}

