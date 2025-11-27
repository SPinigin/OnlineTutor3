using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для работы с заданиями
    /// </summary>
    public interface IAssignmentService
    {
        Task<Assignment?> GetByIdAsync(int id);
        Task<List<Assignment>> GetByTeacherIdAsync(string teacherId);
        Task<List<Assignment>> GetBySubjectIdAsync(int subjectId);
        Task<List<Assignment>> GetByTeacherIdAndSubjectIdAsync(string teacherId, int subjectId);
        Task<List<Assignment>> GetByTeacherSubjectsAsync(string teacherId); // Только по предметам учителя
        Task<int> CreateAsync(Assignment assignment);
        Task<int> UpdateAsync(Assignment assignment);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> TeacherCanAccessAssignmentAsync(string teacherId, int assignmentId);
    }
}

