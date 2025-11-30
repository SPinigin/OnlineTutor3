using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с материалами
    /// </summary>
    public interface IMaterialRepository : IRepository<Material>
    {
        Task<List<Material>> GetByClassIdAsync(int classId);
        Task<List<Material>> GetByAssignmentIdAsync(int assignmentId);
        Task<List<Material>> GetByUploadedByIdAsync(string uploadedById);
        Task<List<Material>> GetActiveByClassIdAsync(int classId);
        Task<List<Material>> GetActiveByAssignmentIdAsync(int assignmentId);
        Task<List<Material>> GetFilteredAsync(
            string? uploadedById,
            string? searchString = null,
            int? classFilter = null,
            int? assignmentFilter = null,
            MaterialType? typeFilter = null,
            string? sortOrder = null);
        Task<Material?> GetByIdWithDetailsAsync(int id, string uploadedById);
        Task<List<Material>> GetAvailableForStudentAsync(int studentId);
    }
}

