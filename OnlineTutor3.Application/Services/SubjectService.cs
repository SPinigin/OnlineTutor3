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
        private readonly ICacheService _cacheService;
        private readonly ILogger<SubjectService> _logger;
        private const string CACHE_KEY_ALL_SUBJECTS = "subjects:all";
        private const string CACHE_KEY_ACTIVE_SUBJECTS = "subjects:active";
        private const string CACHE_KEY_SUBJECT_PREFIX = "subjects:id:";
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);

        public SubjectService(
            ISubjectRepository subjectRepository,
            ICacheService cacheService,
            ILogger<SubjectService> logger)
        {
            _subjectRepository = subjectRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Subject?> GetByIdAsync(int id)
        {
            try
            {
                var cacheKey = $"{CACHE_KEY_SUBJECT_PREFIX}{id}";
                var cached = await _cacheService.GetAsync<Subject>(cacheKey);
                if (cached != null)
                {
                    return cached;
                }

                var subject = await _subjectRepository.GetByIdAsync(id);
                if (subject != null)
                {
                    await _cacheService.SetAsync(cacheKey, subject, CacheExpiration);
                }
                return subject;
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
                var cached = await _cacheService.GetAsync<List<Subject>>(CACHE_KEY_ALL_SUBJECTS);
                if (cached != null)
                {
                    return cached;
                }

                var subjects = await _subjectRepository.GetAllAsync();
                await _cacheService.SetAsync(CACHE_KEY_ALL_SUBJECTS, subjects, CacheExpiration);
                return subjects;
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
                var cached = await _cacheService.GetAsync<List<Subject>>(CACHE_KEY_ACTIVE_SUBJECTS);
                if (cached != null)
                {
                    return cached;
                }

                var subjects = await _subjectRepository.GetActiveAsync();
                await _cacheService.SetAsync(CACHE_KEY_ACTIVE_SUBJECTS, subjects, CacheExpiration);
                return subjects;
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

                var id = await _subjectRepository.CreateAsync(subject);
                
                // Инвалидируем кэш
                await _cacheService.RemoveAsync(CACHE_KEY_ALL_SUBJECTS);
                await _cacheService.RemoveAsync(CACHE_KEY_ACTIVE_SUBJECTS);
                
                return id;
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

                var result = await _subjectRepository.UpdateAsync(subject);
                
                // Инвалидируем кэш
                await _cacheService.RemoveAsync(CACHE_KEY_ALL_SUBJECTS);
                await _cacheService.RemoveAsync(CACHE_KEY_ACTIVE_SUBJECTS);
                await _cacheService.RemoveAsync($"{CACHE_KEY_SUBJECT_PREFIX}{subject.Id}");
                
                return result;
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
                var result = await _subjectRepository.DeleteAsync(id);
                
                // Инвалидируем кэш
                await _cacheService.RemoveAsync(CACHE_KEY_ALL_SUBJECTS);
                await _cacheService.RemoveAsync(CACHE_KEY_ACTIVE_SUBJECTS);
                await _cacheService.RemoveAsync($"{CACHE_KEY_SUBJECT_PREFIX}{id}");
                
                return result;
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

