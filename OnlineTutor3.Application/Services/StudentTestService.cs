using Microsoft.Extensions.Logging;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Services
{
    /// <summary>
    /// Основной сервис для работы студента с тестами
    /// </summary>
    public class StudentTestService : IStudentTestService
    {
        private readonly ITestAccessService _testAccessService;
        private readonly ITestResultService _testResultService;
        private readonly IStudentRepository _studentRepository;
        private readonly ILogger<StudentTestService> _logger;

        public StudentTestService(
            ITestAccessService testAccessService,
            ITestResultService testResultService,
            IStudentRepository studentRepository,
            ILogger<StudentTestService> logger)
        {
            _testAccessService = testAccessService;
            _testResultService = testResultService;
            _studentRepository = studentRepository;
            _logger = logger;
        }

        public async Task<StudentAvailableTestsViewModel> GetAvailableTestsAsync(int studentId, string? category = null)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null)
                {
                    throw new ArgumentException($"Студент с ID {studentId} не найден");
                }

                var viewModel = new StudentAvailableTestsViewModel
                {
                    Student = student
                };

                if (category == null || category == "spelling")
                {
                    viewModel.SpellingTests = await _testAccessService.GetAvailableSpellingTestsAsync(studentId);
                }

                if (category == null || category == "punctuation")
                {
                    viewModel.PunctuationTests = await _testAccessService.GetAvailablePunctuationTestsAsync(studentId);
                }

                if (category == null || category == "orthoepy")
                {
                    viewModel.OrthoeopyTests = await _testAccessService.GetAvailableOrthoeopyTestsAsync(studentId);
                }

                if (category == null || category == "regular")
                {
                    viewModel.RegularTests = await _testAccessService.GetAvailableRegularTestsAsync(studentId);
                }

                if (category == null || category == "notparticle")
                {
                    viewModel.NotParticleTests = await _testAccessService.GetAvailableNotParticleTestsAsync(studentId);
                }

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении доступных тестов. StudentId: {StudentId}, Category: {Category}", studentId, category);
                throw;
            }
        }

        public async Task<StudentTestHistoryViewModel> GetTestHistoryAsync(int studentId, string? testType = null)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null)
                {
                    throw new ArgumentException($"Студент с ID {studentId} не найден");
                }

                var viewModel = new StudentTestHistoryViewModel
                {
                    Student = student
                };

                if (testType == null || testType == "spelling")
                {
                    var allSpelling = await _testResultService.GetStudentResultsAsync<SpellingTestResult>(studentId);
                    viewModel.SpellingResults = allSpelling
                        .Where(r => r.IsCompleted)
                        .OrderByDescending(r => r.CompletedAt ?? r.StartedAt)
                        .ToList();
                }

                if (testType == null || testType == "punctuation")
                {
                    var allPunctuation = await _testResultService.GetStudentResultsAsync<PunctuationTestResult>(studentId);
                    viewModel.PunctuationResults = allPunctuation
                        .Where(r => r.IsCompleted)
                        .OrderByDescending(r => r.CompletedAt ?? r.StartedAt)
                        .ToList();
                }

                if (testType == null || testType == "orthoepy")
                {
                    var allOrthoeopy = await _testResultService.GetStudentResultsAsync<OrthoeopyTestResult>(studentId);
                    viewModel.OrthoeopyResults = allOrthoeopy
                        .Where(r => r.IsCompleted)
                        .OrderByDescending(r => r.CompletedAt ?? r.StartedAt)
                        .ToList();
                }

                if (testType == null || testType == "regular")
                {
                    var allRegular = await _testResultService.GetStudentResultsAsync<RegularTestResult>(studentId);
                    viewModel.RegularResults = allRegular
                        .Where(r => r.IsCompleted)
                        .OrderByDescending(r => r.CompletedAt ?? r.StartedAt)
                        .ToList();
                }

                if (testType == null || testType == "notparticle")
                {
                    var allNotParticle = await _testResultService.GetStudentResultsAsync<NotParticleTestResult>(studentId);
                    viewModel.NotParticleResults = allNotParticle
                        .Where(r => r.IsCompleted)
                        .OrderByDescending(r => r.CompletedAt ?? r.StartedAt)
                        .ToList();
                }

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении истории тестов. StudentId: {StudentId}, TestType: {TestType}", studentId, testType);
                throw;
            }
        }

        public async Task<SpellingTestResult> StartSpellingTestAsync(int studentId, int testId)
        {
            try
            {
                // Проверяем доступ
                if (!await _testAccessService.CanAccessSpellingTestAsync(studentId, testId))
                {
                    throw new UnauthorizedAccessException($"Студент {studentId} не имеет доступа к тесту {testId}");
                }

                // Проверяем незавершенный тест
                var ongoing = await _testResultService.GetOngoingSpellingTestResultAsync(studentId, testId);
                if (ongoing != null)
                {
                    return ongoing;
                }

                // Создаем новый результат
                return await _testResultService.CreateSpellingTestResultAsync(studentId, testId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при начале теста по орфографии. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                throw;
            }
        }

        public async Task<PunctuationTestResult> StartPunctuationTestAsync(int studentId, int testId)
        {
            try
            {
                if (!await _testAccessService.CanAccessPunctuationTestAsync(studentId, testId))
                {
                    throw new UnauthorizedAccessException($"Студент {studentId} не имеет доступа к тесту {testId}");
                }

                var ongoing = await _testResultService.GetOngoingPunctuationTestResultAsync(studentId, testId);
                if (ongoing != null)
                {
                    return ongoing;
                }

                return await _testResultService.CreatePunctuationTestResultAsync(studentId, testId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при начале теста по пунктуации. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                throw;
            }
        }

        public async Task<OrthoeopyTestResult> StartOrthoeopyTestAsync(int studentId, int testId)
        {
            try
            {
                if (!await _testAccessService.CanAccessOrthoeopyTestAsync(studentId, testId))
                {
                    throw new UnauthorizedAccessException($"Студент {studentId} не имеет доступа к тесту {testId}");
                }

                var ongoing = await _testResultService.GetOngoingOrthoeopyTestResultAsync(studentId, testId);
                if (ongoing != null)
                {
                    return ongoing;
                }

                return await _testResultService.CreateOrthoeopyTestResultAsync(studentId, testId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при начале теста по орфоэпии. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                throw;
            }
        }

        public async Task<RegularTestResult> StartRegularTestAsync(int studentId, int testId)
        {
            try
            {
                if (!await _testAccessService.CanAccessRegularTestAsync(studentId, testId))
                {
                    throw new UnauthorizedAccessException($"Студент {studentId} не имеет доступа к тесту {testId}");
                }

                var ongoing = await _testResultService.GetOngoingRegularTestResultAsync(studentId, testId);
                if (ongoing != null)
                {
                    return ongoing;
                }

                return await _testResultService.CreateRegularTestResultAsync(studentId, testId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при начале классического теста. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                throw;
            }
        }

        public async Task<NotParticleTestResult> StartNotParticleTestAsync(int studentId, int testId)
        {
            try
            {
                if (!await _testAccessService.CanAccessNotParticleTestAsync(studentId, testId))
                {
                    throw new UnauthorizedAccessException($"Студент {studentId} не имеет доступа к тесту {testId}");
                }

                var ongoing = await _testResultService.GetOngoingNotParticleTestResultAsync(studentId, testId);
                if (ongoing != null)
                {
                    return ongoing;
                }

                return await _testResultService.CreateNotParticleTestResultAsync(studentId, testId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при начале теста на правописание частицы \"не\". StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                throw;
            }
        }
    }
}

