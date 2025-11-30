using Microsoft.Extensions.Logging;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Services
{
    /// <summary>
    /// Сервис для управления результатами тестов
    /// </summary>
    public class TestResultService : ITestResultService
    {
        private readonly ISpellingTestResultRepository _spellingTestResultRepository;
        private readonly IPunctuationTestResultRepository _punctuationTestResultRepository;
        private readonly IOrthoeopyTestResultRepository _orthoeopyTestResultRepository;
        private readonly IRegularTestResultRepository _regularTestResultRepository;
        private readonly ISpellingQuestionRepository _spellingQuestionRepository;
        private readonly IPunctuationQuestionRepository _punctuationQuestionRepository;
        private readonly IOrthoeopyQuestionRepository _orthoeopyQuestionRepository;
        private readonly IRegularQuestionRepository _regularQuestionRepository;
        private readonly ILogger<TestResultService> _logger;

        public TestResultService(
            ISpellingTestResultRepository spellingTestResultRepository,
            IPunctuationTestResultRepository punctuationTestResultRepository,
            IOrthoeopyTestResultRepository orthoeopyTestResultRepository,
            IRegularTestResultRepository regularTestResultRepository,
            ISpellingQuestionRepository spellingQuestionRepository,
            IPunctuationQuestionRepository punctuationQuestionRepository,
            IOrthoeopyQuestionRepository orthoeopyQuestionRepository,
            IRegularQuestionRepository regularQuestionRepository,
            ILogger<TestResultService> logger)
        {
            _spellingTestResultRepository = spellingTestResultRepository;
            _punctuationTestResultRepository = punctuationTestResultRepository;
            _orthoeopyTestResultRepository = orthoeopyTestResultRepository;
            _regularTestResultRepository = regularTestResultRepository;
            _spellingQuestionRepository = spellingQuestionRepository;
            _punctuationQuestionRepository = punctuationQuestionRepository;
            _orthoeopyQuestionRepository = orthoeopyQuestionRepository;
            _regularQuestionRepository = regularQuestionRepository;
            _logger = logger;
        }

        public async Task<SpellingTestResult> CreateSpellingTestResultAsync(int studentId, int testId)
        {
            try
            {
                var existingResults = await _spellingTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId);
                var attemptNumber = existingResults.Count + 1;

                // Вычисляем максимальный балл (сумма баллов всех вопросов)
                var questions = await _spellingQuestionRepository.GetByTestIdOrderedAsync(testId);
                var maxScore = questions.Sum(q => q.Points);

                var testResult = new SpellingTestResult
                {
                    StudentId = studentId,
                    SpellingTestId = testId,
                    AttemptNumber = attemptNumber,
                    StartedAt = DateTime.Now,
                    Score = 0,
                    MaxScore = maxScore,
                    Percentage = 0.0,
                    IsCompleted = false
                };

                var id = await _spellingTestResultRepository.CreateAsync(testResult);
                testResult.Id = id;
                return testResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании результата теста по орфографии. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                throw;
            }
        }

        public async Task<PunctuationTestResult> CreatePunctuationTestResultAsync(int studentId, int testId)
        {
            try
            {
                var existingResults = await _punctuationTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId);
                var attemptNumber = existingResults.Count + 1;

                var testResult = new PunctuationTestResult
                {
                    StudentId = studentId,
                    PunctuationTestId = testId,
                    AttemptNumber = attemptNumber,
                    StartedAt = DateTime.Now,
                    Score = 0,
                    MaxScore = 0,
                    Percentage = 0.0,
                    IsCompleted = false
                };

                var id = await _punctuationTestResultRepository.CreateAsync(testResult);
                testResult.Id = id;
                return testResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании результата теста по пунктуации. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                throw;
            }
        }

        public async Task<OrthoeopyTestResult> CreateOrthoeopyTestResultAsync(int studentId, int testId)
        {
            try
            {
                var existingResults = await _orthoeopyTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId);
                var attemptNumber = existingResults.Count + 1;

                var testResult = new OrthoeopyTestResult
                {
                    StudentId = studentId,
                    OrthoeopyTestId = testId,
                    AttemptNumber = attemptNumber,
                    StartedAt = DateTime.Now,
                    Score = 0,
                    MaxScore = 0,
                    Percentage = 0.0,
                    IsCompleted = false
                };

                var id = await _orthoeopyTestResultRepository.CreateAsync(testResult);
                testResult.Id = id;
                return testResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании результата теста по орфоэпии. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                throw;
            }
        }

        public async Task<RegularTestResult> CreateRegularTestResultAsync(int studentId, int testId)
        {
            try
            {
                var existingResults = await _regularTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId);
                var attemptNumber = existingResults.Count + 1;

                var testResult = new RegularTestResult
                {
                    StudentId = studentId,
                    RegularTestId = testId,
                    AttemptNumber = attemptNumber,
                    StartedAt = DateTime.Now,
                    Score = 0,
                    MaxScore = 0,
                    Percentage = 0.0,
                    IsCompleted = false
                };

                var id = await _regularTestResultRepository.CreateAsync(testResult);
                testResult.Id = id;
                return testResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании результата классического теста. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                throw;
            }
        }

        public async Task<SpellingTestResult?> GetOngoingSpellingTestResultAsync(int studentId, int testId)
        {
            try
            {
                var results = await _spellingTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId);
                return results.FirstOrDefault(r => !r.IsCompleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении незавершенного результата теста по орфографии. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                return null;
            }
        }

        public async Task<PunctuationTestResult?> GetOngoingPunctuationTestResultAsync(int studentId, int testId)
        {
            try
            {
                var results = await _punctuationTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId);
                return results.FirstOrDefault(r => !r.IsCompleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении незавершенного результата теста по пунктуации. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                return null;
            }
        }

        public async Task<OrthoeopyTestResult?> GetOngoingOrthoeopyTestResultAsync(int studentId, int testId)
        {
            try
            {
                var results = await _orthoeopyTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId);
                return results.FirstOrDefault(r => !r.IsCompleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении незавершенного результата теста по орфоэпии. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                return null;
            }
        }

        public async Task<RegularTestResult?> GetOngoingRegularTestResultAsync(int studentId, int testId)
        {
            try
            {
                var results = await _regularTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId);
                return results.FirstOrDefault(r => !r.IsCompleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении незавершенного результата классического теста. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                return null;
            }
        }

        public async Task<int> GetAttemptCountAsync(int studentId, int testId, TestType testType)
        {
            try
            {
                return testType switch
                {
                    TestType.Spelling => (await _spellingTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId)).Count,
                    TestType.Punctuation => (await _punctuationTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId)).Count,
                    TestType.Orthoeopy => (await _orthoeopyTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId)).Count,
                    TestType.Regular => (await _regularTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId)).Count,
                    _ => 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении количества попыток. StudentId: {StudentId}, TestId: {TestId}, TestType: {TestType}", studentId, testId, testType);
                return 0;
            }
        }

        public async Task CompleteTestResultAsync<T>(T testResult) where T : TestResult
        {
            try
            {
                testResult.CompletedAt = DateTime.Now;
                testResult.IsCompleted = true;

                // Вычисляем процент
                if (testResult.MaxScore > 0)
                {
                    testResult.Percentage = (double)testResult.Score / testResult.MaxScore * 100;
                }

                // Обновляем в зависимости от типа
                if (testResult is SpellingTestResult spellingResult)
                {
                    await _spellingTestResultRepository.UpdateAsync(spellingResult);
                }
                else if (testResult is PunctuationTestResult punctuationResult)
                {
                    await _punctuationTestResultRepository.UpdateAsync(punctuationResult);
                }
                else if (testResult is OrthoeopyTestResult orthoeopyResult)
                {
                    await _orthoeopyTestResultRepository.UpdateAsync(orthoeopyResult);
                }
                else if (testResult is RegularTestResult regularResult)
                {
                    await _regularTestResultRepository.UpdateAsync(regularResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при завершении результата теста. TestResultId: {TestResultId}", testResult.Id);
                throw;
            }
        }

        public async Task<List<T>> GetStudentResultsAsync<T>(int studentId) where T : TestResult
        {
            try
            {
                if (typeof(T) == typeof(SpellingTestResult))
                {
                    var results = await _spellingTestResultRepository.GetByStudentIdAsync(studentId);
                    return results.Cast<T>().ToList();
                }
                else if (typeof(T) == typeof(PunctuationTestResult))
                {
                    var results = await _punctuationTestResultRepository.GetByStudentIdAsync(studentId);
                    return results.Cast<T>().ToList();
                }
                else if (typeof(T) == typeof(OrthoeopyTestResult))
                {
                    var results = await _orthoeopyTestResultRepository.GetByStudentIdAsync(studentId);
                    return results.Cast<T>().ToList();
                }
                else if (typeof(T) == typeof(RegularTestResult))
                {
                    var results = await _regularTestResultRepository.GetByStudentIdAsync(studentId);
                    return results.Cast<T>().ToList();
                }

                return new List<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении результатов студента. StudentId: {StudentId}", studentId);
                return new List<T>();
            }
        }

        public async Task<T?> GetBestResultAsync<T>(int studentId, int testId) where T : TestResult
        {
            try
            {
                List<TestResult> results = typeof(T) switch
                {
                    Type t when t == typeof(SpellingTestResult) => (await _spellingTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId)).Cast<TestResult>().ToList(),
                    Type t when t == typeof(PunctuationTestResult) => (await _punctuationTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId)).Cast<TestResult>().ToList(),
                    Type t when t == typeof(OrthoeopyTestResult) => (await _orthoeopyTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId)).Cast<TestResult>().ToList(),
                    Type t when t == typeof(RegularTestResult) => (await _regularTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId)).Cast<TestResult>().ToList(),
                    _ => new List<TestResult>()
                };

                var bestResult = results
                    .Where(r => r.IsCompleted)
                    .OrderByDescending(r => r.Percentage)
                    .ThenByDescending(r => r.Score)
                    .FirstOrDefault();

                return bestResult as T;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении лучшего результата. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                return null;
            }
        }
    }
}

