using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с предметами
    /// </summary>
    public class SubjectRepository : BaseRepository<Subject>, ISubjectRepository
    {
        public SubjectRepository(IDatabaseConnection db) : base(db, "Subjects")
        {
        }

        public async Task<List<Subject>> GetActiveAsync()
        {
            var sql = "SELECT * FROM Subjects WHERE IsActive = 1 ORDER BY OrderIndex, Name";
            return await _db.QueryAsync<Subject>(sql);
        }
    }
}

