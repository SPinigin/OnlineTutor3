using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для работы с материалами
    /// </summary>
    public interface IMaterialService
    {
        Task<Material?> GetByIdAsync(int id);
        Task<List<Material>> GetByTeacherIdAsync(string teacherId);
        Task<List<Material>> GetByClassIdAsync(int classId);
        Task<List<Material>> GetByAssignmentIdAsync(int assignmentId);
        Task<List<Material>> GetAvailableForStudentAsync(int studentId);
        Task<List<Material>> GetFilteredAsync(
            string teacherId,
            string? searchString = null,
            int? classFilter = null,
            int? assignmentFilter = null,
            MaterialType? typeFilter = null,
            string? sortOrder = null);
        Task<int> CreateAsync(Material material);
        Task<int> UpdateAsync(Material material);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> TeacherCanAccessMaterialAsync(string teacherId, int materialId);
    }
}

