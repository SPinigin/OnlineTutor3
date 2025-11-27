using Microsoft.Extensions.Logging;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Services
{
    /// <summary>
    /// Сервис для работы с учениками
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ILogger<StudentService> _logger;

        public StudentService(
            IStudentRepository studentRepository,
            ILogger<StudentService> logger)
        {
            _studentRepository = studentRepository;
            _logger = logger;
        }

        public async Task<Student?> GetByIdAsync(int id)
        {
            try
            {
                return await _studentRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении ученика по ID: {StudentId}", id);
                throw;
            }
        }

        public async Task<Student?> GetByUserIdAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException("UserId не может быть пустым", nameof(userId));
                }

                return await _studentRepository.GetByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении ученика по UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<Student>> GetByClassIdAsync(int classId)
        {
            try
            {
                return await _studentRepository.GetByClassIdAsync(classId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении учеников класса: {ClassId}", classId);
                throw;
            }
        }

        public async Task<List<Student>> GetByTeacherIdAsync(string teacherId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(teacherId))
                {
                    throw new ArgumentException("TeacherId не может быть пустым", nameof(teacherId));
                }

                return await _studentRepository.GetByTeacherIdAsync(teacherId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении учеников учителя: {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<int> CreateAsync(Student student)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(student.UserId))
                {
                    throw new ArgumentException("UserId не может быть пустым", nameof(student));
                }

                student.CreatedAt = DateTime.Now;
                return await _studentRepository.CreateAsync(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании ученика: {UserId}", student.UserId);
                throw;
            }
        }

        public async Task<int> UpdateAsync(Student student)
        {
            try
            {
                return await _studentRepository.UpdateAsync(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении ученика: {StudentId}", student.Id);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            try
            {
                return await _studentRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении ученика: {StudentId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _studentRepository.ExistsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке существования ученика: {StudentId}", id);
                throw;
            }
        }
    }
}

