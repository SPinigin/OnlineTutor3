using Microsoft.Extensions.Logging;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Services
{
    /// <summary>
    /// Сервис для работы с учителями
    /// </summary>
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly ITeacherSubjectRepository _teacherSubjectRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly ILogger<TeacherService> _logger;

        public TeacherService(
            ITeacherRepository teacherRepository,
            ITeacherSubjectRepository teacherSubjectRepository,
            ISubjectRepository subjectRepository,
            ILogger<TeacherService> logger)
        {
            _teacherRepository = teacherRepository;
            _teacherSubjectRepository = teacherSubjectRepository;
            _subjectRepository = subjectRepository;
            _logger = logger;
        }

        public async Task<Teacher?> GetByIdAsync(int id)
        {
            try
            {
                return await _teacherRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении учителя по ID: {TeacherId}", id);
                throw;
            }
        }

        public async Task<Teacher?> GetByUserIdAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException("UserId не может быть пустым", nameof(userId));
                }

                return await _teacherRepository.GetByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении учителя по UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<Teacher>> GetApprovedAsync()
        {
            try
            {
                return await _teacherRepository.GetApprovedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении одобренных учителей");
                throw;
            }
        }

        public async Task<int> CreateAsync(Teacher teacher)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(teacher.UserId))
                {
                    throw new ArgumentException("UserId не может быть пустым", nameof(teacher));
                }

                teacher.CreatedAt = DateTime.Now;
                return await _teacherRepository.CreateAsync(teacher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании учителя: {UserId}", teacher.UserId);
                throw;
            }
        }

        public async Task<int> UpdateAsync(Teacher teacher)
        {
            try
            {
                return await _teacherRepository.UpdateAsync(teacher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении учителя: {TeacherId}", teacher.Id);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            try
            {
                return await _teacherRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении учителя: {TeacherId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _teacherRepository.ExistsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке существования учителя: {TeacherId}", id);
                throw;
            }
        }

        public async Task<List<Subject>> GetTeacherSubjectsAsync(int teacherId)
        {
            try
            {
                var teacherSubjects = await _teacherSubjectRepository.GetByTeacherIdAsync(teacherId);
                var subjectIds = teacherSubjects.Select(ts => ts.SubjectId).ToList();

                if (subjectIds.Count == 0)
                {
                    return new List<Subject>();
                }

                var allSubjects = await _subjectRepository.GetAllAsync();
                return allSubjects.Where(s => subjectIds.Contains(s.Id)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении предметов учителя: {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<int> AddSubjectToTeacherAsync(int teacherId, int subjectId)
        {
            try
            {
                // Проверяем, не существует ли уже такая связь
                var existing = await _teacherSubjectRepository.GetByTeacherAndSubjectIdAsync(teacherId, subjectId);
                if (existing != null)
                {
                    _logger.LogWarning("Связь между учителем {TeacherId} и предметом {SubjectId} уже существует", teacherId, subjectId);
                    return 0;
                }

                var teacherSubject = new TeacherSubject
                {
                    TeacherId = teacherId,
                    SubjectId = subjectId,
                    CreatedAt = DateTime.Now
                };

                return await _teacherSubjectRepository.CreateAsync(teacherSubject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении предмета учителю: TeacherId={TeacherId}, SubjectId={SubjectId}", teacherId, subjectId);
                throw;
            }
        }

        public async Task<int> RemoveSubjectFromTeacherAsync(int teacherId, int subjectId)
        {
            try
            {
                var teacherSubject = await _teacherSubjectRepository.GetByTeacherAndSubjectIdAsync(teacherId, subjectId);
                if (teacherSubject == null)
                {
                    _logger.LogWarning("Связь между учителем {TeacherId} и предметом {SubjectId} не найдена", teacherId, subjectId);
                    return 0;
                }

                return await _teacherSubjectRepository.DeleteAsync(teacherSubject.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении предмета у учителя: TeacherId={TeacherId}, SubjectId={SubjectId}", teacherId, subjectId);
                throw;
            }
        }
    }
}

