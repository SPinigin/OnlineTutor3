using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с учителями
    /// </summary>
    public class TeacherRepository : BaseRepository<Teacher>, ITeacherRepository
    {
        public TeacherRepository(IDatabaseConnection db) : base(db, "Teachers")
        {
        }

        public async Task<Teacher?> GetByUserIdAsync(string userId)
        {
            var sql = "SELECT * FROM Teachers WHERE UserId = @UserId";
            return await _db.QueryFirstOrDefaultAsync<Teacher>(sql, new { UserId = userId });
        }

        public async Task<List<Teacher>> GetApprovedAsync()
        {
            var sql = "SELECT * FROM Teachers WHERE IsApproved = 1 ORDER BY CreatedAt";
            return await _db.QueryAsync<Teacher>(sql);
        }
    }
}

