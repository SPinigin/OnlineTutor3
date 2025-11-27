using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы со связями заданий и классов
    /// </summary>
    public class AssignmentClassRepository : BaseRepository<AssignmentClass>, IAssignmentClassRepository
    {
        public AssignmentClassRepository(IDatabaseConnection db) : base(db, "AssignmentClasses")
        {
        }

        public async Task<List<AssignmentClass>> GetByAssignmentIdAsync(int assignmentId)
        {
            var sql = "SELECT * FROM AssignmentClasses WHERE AssignmentId = @AssignmentId ORDER BY AssignedAt";
            return await _db.QueryAsync<AssignmentClass>(sql, new { AssignmentId = assignmentId });
        }

        public async Task<List<AssignmentClass>> GetByClassIdAsync(int classId)
        {
            var sql = "SELECT * FROM AssignmentClasses WHERE ClassId = @ClassId ORDER BY AssignedAt";
            return await _db.QueryAsync<AssignmentClass>(sql, new { ClassId = classId });
        }

        public async Task<AssignmentClass?> GetByAssignmentAndClassIdAsync(int assignmentId, int classId)
        {
            var sql = "SELECT * FROM AssignmentClasses WHERE AssignmentId = @AssignmentId AND ClassId = @ClassId";
            return await _db.QueryFirstOrDefaultAsync<AssignmentClass>(sql, new { AssignmentId = assignmentId, ClassId = classId });
        }

        public async Task<int> DeleteByAssignmentIdAsync(int assignmentId)
        {
            var sql = "DELETE FROM AssignmentClasses WHERE AssignmentId = @AssignmentId";
            return await _db.ExecuteAsync(sql, new { AssignmentId = assignmentId });
        }

        public async Task<int> DeleteByClassIdAsync(int classId)
        {
            var sql = "DELETE FROM AssignmentClasses WHERE ClassId = @ClassId";
            return await _db.ExecuteAsync(sql, new { ClassId = classId });
        }
    }
}

