using Microsoft.Extensions.Logging;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Services
{
    /// <summary>
    /// Сервис для работы с классами
    /// </summary>
    public class ClassService : IClassService
    {
        private readonly IClassRepository _classRepository;
        private readonly ILogger<ClassService> _logger;

        public ClassService(
            IClassRepository classRepository,
            ILogger<ClassService> logger)
        {
            _classRepository = classRepository;
            _logger = logger;
        }

        public async Task<Class?> GetByIdAsync(int id)
        {
            try
            {
                return await _classRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении класса по ID: {ClassId}", id);
                throw;
            }
        }

        public async Task<List<Class>> GetByTeacherIdAsync(string teacherId)
        {
            try
            {
                return await _classRepository.GetByTeacherIdAsync(teacherId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении классов учителя: {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<List<Class>> GetActiveByTeacherIdAsync(string teacherId)
        {
            try
            {
                return await _classRepository.GetActiveByTeacherIdAsync(teacherId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении активных классов учителя: {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<int> CreateAsync(Class @class)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(@class.Name))
                {
                    throw new ArgumentException("Название класса не может быть пустым", nameof(@class));
                }

                if (string.IsNullOrWhiteSpace(@class.TeacherId))
                {
                    throw new ArgumentException("TeacherId не может быть пустым", nameof(@class));
                }

                @class.CreatedAt = DateTime.Now;
                return await _classRepository.CreateAsync(@class);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании класса: {ClassName}", @class.Name);
                throw;
            }
        }

        public async Task<int> UpdateAsync(Class @class)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(@class.Name))
                {
                    throw new ArgumentException("Название класса не может быть пустым", nameof(@class));
                }

                return await _classRepository.UpdateAsync(@class);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении класса: {ClassId}", @class.Id);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            try
            {
                return await _classRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении класса: {ClassId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _classRepository.ExistsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке существования класса: {ClassId}", id);
                throw;
            }
        }
    }
}

