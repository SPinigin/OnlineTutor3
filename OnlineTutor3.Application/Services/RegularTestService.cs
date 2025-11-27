using Microsoft.Extensions.Logging;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Services
{
    /// <summary>
    /// Сервис для работы с классическими тестами
    /// </summary>
    public class RegularTestService : IRegularTestService
    {
        private readonly IRegularTestRepository _testRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly ITeacherService _teacherService;
        private readonly ILogger<RegularTestService> _logger;

        public RegularTestService(
            IRegularTestRepository testRepository,
            IAssignmentRepository assignmentRepository,
            ITeacherService teacherService,
            ILogger<RegularTestService> logger)
        {
            _testRepository = testRepository;
            _assignmentRepository = assignmentRepository;
            _teacherService = teacherService;
            _logger = logger;
        }

        public async Task<RegularTest?> GetByIdAsync(int id)
        {
            try
            {
                return await _testRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении классического теста по ID: {TestId}", id);
                throw;
            }
        }

        public async Task<List<RegularTest>> GetByTeacherIdAsync(string teacherId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(teacherId))
                {
                    throw new ArgumentException("TeacherId не может быть пустым", nameof(teacherId));
                }

                return await _testRepository.GetByTeacherIdAsync(teacherId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении классических тестов учителя: {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<List<RegularTest>> GetByAssignmentIdAsync(int assignmentId)
        {
            try
            {
                return await _testRepository.GetByAssignmentIdAsync(assignmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении классических тестов по заданию: {AssignmentId}", assignmentId);
                throw;
            }
        }

        public async Task<List<RegularTest>> GetActiveByTeacherIdAsync(string teacherId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(teacherId))
                {
                    throw new ArgumentException("TeacherId не может быть пустым", nameof(teacherId));
                }

                return await _testRepository.GetActiveByTeacherIdAsync(teacherId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении активных классических тестов учителя: {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<int> CreateAsync(RegularTest test)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(test.TeacherId))
                {
                    throw new ArgumentException("TeacherId не может быть пустым", nameof(test));
                }

                // Проверяем, что задание существует и учитель имеет к нему доступ
                var assignment = await _assignmentRepository.GetByIdAsync(test.AssignmentId);
                if (assignment == null)
                {
                    throw new ArgumentException($"Задание с ID {test.AssignmentId} не найдено", nameof(test));
                }

                if (assignment.TeacherId != test.TeacherId)
                {
                    throw new UnauthorizedAccessException("Учитель не может создавать тесты для чужих заданий");
                }

                // Проверяем, что учитель ведет предмет этого задания
                var teachesSubject = await _teacherService.TeacherTeachesSubjectAsync(test.TeacherId, assignment.SubjectId);
                if (!teachesSubject)
                {
                    throw new UnauthorizedAccessException($"Учитель не ведет предмет задания");
                }

                test.CreatedAt = DateTime.Now;
                return await _testRepository.CreateAsync(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании классического теста: TeacherId={TeacherId}, AssignmentId={AssignmentId}", test.TeacherId, test.AssignmentId);
                throw;
            }
        }

        public async Task<int> UpdateAsync(RegularTest test)
        {
            try
            {
                // Проверяем доступ учителя к тесту
                var canAccess = await TeacherCanAccessTestAsync(test.TeacherId, test.Id);
                if (!canAccess)
                {
                    throw new UnauthorizedAccessException("Учитель не имеет доступа к этому тесту");
                }

                return await _testRepository.UpdateAsync(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении классического теста: {TestId}", test.Id);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            try
            {
                return await _testRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении классического теста: {TestId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _testRepository.ExistsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке существования классического теста: {TestId}", id);
                throw;
            }
        }

        public async Task<bool> TeacherCanAccessTestAsync(string teacherId, int testId)
        {
            try
            {
                var test = await _testRepository.GetByIdAsync(testId);
                if (test == null)
                {
                    return false;
                }

                // Проверяем, что это тест этого учителя
                if (test.TeacherId != teacherId)
                {
                    return false;
                }

                // Проверяем, что учитель ведет предмет задания
                var assignment = await _assignmentRepository.GetByIdAsync(test.AssignmentId);
                if (assignment == null)
                {
                    return false;
                }

                return await _teacherService.TeacherTeachesSubjectAsync(teacherId, assignment.SubjectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке доступа учителя к тесту: TeacherId={TeacherId}, TestId={TestId}", teacherId, testId);
                return false;
            }
        }
    }
}

