using Microsoft.Extensions.Logging;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Services
{
    /// <summary>
    /// Сервис для работы с предметами
    /// </summary>
    public class SubjectService : ISubjectService
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly ILogger<SubjectService> _logger;

        public SubjectService(
            ISubjectRepository subjectRepository,
            ILogger<SubjectService> logger)
        {
            _subjectRepository = subjectRepository;
            _logger = logger;
        }

        public async Task<Subject?> GetByIdAsync(int id)
        {
            try
            {
                return await _subjectRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении предмета по ID: {SubjectId}", id);
                throw;
            }
        }

        public async Task<List<Subject>> GetAllAsync()
        {
            try
            {
                return await _subjectRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех предметов");
                throw;
            }
        }

        public async Task<List<Subject>> GetActiveAsync()
        {
            try
            {
                return await _subjectRepository.GetActiveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении активных предметов");
                throw;
            }
        }

        public async Task<int> CreateAsync(Subject subject)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(subject.Name))
                {
                    throw new ArgumentException("Название предмета не может быть пустым", nameof(subject));
                }

                return await _subjectRepository.CreateAsync(subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании предмета: {SubjectName}", subject.Name);
                throw;
            }
        }

        public async Task<int> UpdateAsync(Subject subject)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(subject.Name))
                {
                    throw new ArgumentException("Название предмета не может быть пустым", nameof(subject));
                }

                return await _subjectRepository.UpdateAsync(subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении предмета: {SubjectId}", subject.Id);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            try
            {
                return await _subjectRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении предмета: {SubjectId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _subjectRepository.ExistsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке существования предмета: {SubjectId}", id);
                throw;
            }
        }
    }
}

