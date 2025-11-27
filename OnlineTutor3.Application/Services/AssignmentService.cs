using Microsoft.Extensions.Logging;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Services
{
    /// <summary>
    /// Сервис для работы с заданиями
    /// </summary>
    public class AssignmentService : IAssignmentService
    {
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly ITeacherService _teacherService;
        private readonly ILogger<AssignmentService> _logger;

        public AssignmentService(
            IAssignmentRepository assignmentRepository,
            ITeacherService teacherService,
            ILogger<AssignmentService> logger)
        {
            _assignmentRepository = assignmentRepository;
            _teacherService = teacherService;
            _logger = logger;
        }

        public async Task<Assignment?> GetByIdAsync(int id)
        {
            try
            {
                return await _assignmentRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении задания по ID: {AssignmentId}", id);
                throw;
            }
        }

        public async Task<List<Assignment>> GetByTeacherIdAsync(string teacherId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(teacherId))
                {
                    throw new ArgumentException("TeacherId не может быть пустым", nameof(teacherId));
                }

                // Возвращаем только задания по предметам, которые ведет учитель
                return await GetByTeacherSubjectsAsync(teacherId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заданий учителя: {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<List<Assignment>> GetBySubjectIdAsync(int subjectId)
        {
            try
            {
                return await _assignmentRepository.GetBySubjectIdAsync(subjectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заданий по предмету: {SubjectId}", subjectId);
                throw;
            }
        }

        public async Task<List<Assignment>> GetByTeacherIdAndSubjectIdAsync(string teacherId, int subjectId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(teacherId))
                {
                    throw new ArgumentException("TeacherId не может быть пустым", nameof(teacherId));
                }

                // Проверяем, что учитель ведет этот предмет
                var teachesSubject = await _teacherService.TeacherTeachesSubjectAsync(teacherId, subjectId);
                if (!teachesSubject)
                {
                    _logger.LogWarning("Учитель {TeacherId} пытается получить задания по предмету {SubjectId}, который он не ведет", teacherId, subjectId);
                    return new List<Assignment>();
                }

                return await _assignmentRepository.GetByTeacherIdAndSubjectIdAsync(teacherId, subjectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заданий учителя по предмету: TeacherId={TeacherId}, SubjectId={SubjectId}", teacherId, subjectId);
                throw;
            }
        }

        public async Task<List<Assignment>> GetByTeacherSubjectsAsync(string teacherId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(teacherId))
                {
                    throw new ArgumentException("TeacherId не может быть пустым", nameof(teacherId));
                }

                return await _assignmentRepository.GetByTeacherSubjectsAsync(teacherId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заданий учителя по его предметам: {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<int> CreateAsync(Assignment assignment)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(assignment.TeacherId))
                {
                    throw new ArgumentException("TeacherId не может быть пустым", nameof(assignment));
                }

                // Проверяем, что учитель ведет этот предмет
                var teachesSubject = await _teacherService.TeacherTeachesSubjectAsync(assignment.TeacherId, assignment.SubjectId);
                if (!teachesSubject)
                {
                    throw new UnauthorizedAccessException($"Учитель не ведет предмет с ID {assignment.SubjectId}");
                }

                assignment.CreatedAt = DateTime.Now;
                return await _assignmentRepository.CreateAsync(assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании задания: TeacherId={TeacherId}, SubjectId={SubjectId}", assignment.TeacherId, assignment.SubjectId);
                throw;
            }
        }

        public async Task<int> UpdateAsync(Assignment assignment)
        {
            try
            {
                // Проверяем, что учитель ведет этот предмет
                var teachesSubject = await _teacherService.TeacherTeachesSubjectAsync(assignment.TeacherId, assignment.SubjectId);
                if (!teachesSubject)
                {
                    throw new UnauthorizedAccessException($"Учитель не ведет предмет с ID {assignment.SubjectId}");
                }

                return await _assignmentRepository.UpdateAsync(assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении задания: {AssignmentId}", assignment.Id);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            try
            {
                return await _assignmentRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении задания: {AssignmentId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _assignmentRepository.ExistsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке существования задания: {AssignmentId}", id);
                throw;
            }
        }

        public async Task<bool> TeacherCanAccessAssignmentAsync(string teacherId, int assignmentId)
        {
            try
            {
                var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
                if (assignment == null)
                {
                    return false;
                }

                // Проверяем, что это задание создано этим учителем
                if (assignment.TeacherId != teacherId)
                {
                    return false;
                }

                // Проверяем, что учитель ведет предмет этого задания
                return await _teacherService.TeacherTeachesSubjectAsync(teacherId, assignment.SubjectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке доступа учителя к заданию: TeacherId={TeacherId}, AssignmentId={AssignmentId}", teacherId, assignmentId);
                return false;
            }
        }
    }
}

