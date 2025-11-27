using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы со связями заданий и классов
    /// </summary>
    public interface IAssignmentClassRepository : IRepository<AssignmentClass>
    {
        Task<List<AssignmentClass>> GetByAssignmentIdAsync(int assignmentId);
        Task<List<AssignmentClass>> GetByClassIdAsync(int classId);
        Task<AssignmentClass?> GetByAssignmentAndClassIdAsync(int assignmentId, int classId);
        Task<int> DeleteByAssignmentIdAsync(int assignmentId);
        Task<int> DeleteByClassIdAsync(int classId);
    }
}

