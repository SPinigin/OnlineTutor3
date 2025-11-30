using Microsoft.Extensions.Logging;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Application.DTOs;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Services
{
    /// <summary>
    /// Сервис для получения статистики студента
    /// </summary>
    public class StudentStatisticsService : IStudentStatisticsService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IClassRepository _classRepository;
        private readonly ITestAccessService _testAccessService;
        private readonly ITestResultService _testResultService;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IAssignmentClassRepository _assignmentClassRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly ISpellingTestRepository _spellingTestRepository;
        private readonly IPunctuationTestRepository _punctuationTestRepository;
        private readonly IOrthoeopyTestRepository _orthoeopyTestRepository;
        private readonly IRegularTestRepository _regularTestRepository;
        private readonly ILogger<StudentStatisticsService> _logger;

        public StudentStatisticsService(
            IStudentRepository studentRepository,
            IClassRepository classRepository,
            ITestAccessService testAccessService,
            ITestResultService testResultService,
            IAssignmentRepository assignmentRepository,
            IAssignmentClassRepository assignmentClassRepository,
            ISubjectRepository subjectRepository,
            ISpellingTestRepository spellingTestRepository,
            IPunctuationTestRepository punctuationTestRepository,
            IOrthoeopyTestRepository orthoeopyTestRepository,
            IRegularTestRepository regularTestRepository,
            ILogger<StudentStatisticsService> logger)
        {
            _studentRepository = studentRepository;
            _classRepository = classRepository;
            _testAccessService = testAccessService;
            _testResultService = testResultService;
            _assignmentRepository = assignmentRepository;
            _assignmentClassRepository = assignmentClassRepository;
            _subjectRepository = subjectRepository;
            _spellingTestRepository = spellingTestRepository;
            _punctuationTestRepository = punctuationTestRepository;
            _orthoeopyTestRepository = orthoeopyTestRepository;
            _regularTestRepository = regularTestRepository;
            _logger = logger;
        }

        public async Task<StudentDashboardData> GetDashboardDataAsync(int studentId)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null)
                {
                    throw new ArgumentException($"Студент с ID {studentId} не найден");
                }

                var data = new StudentDashboardData
                {
                    Student = student
                };

                // Загружаем класс, если есть
                if (student.ClassId.HasValue)
                {
                    data.Class = await _classRepository.GetByIdAsync(student.ClassId.Value);
                }

                // Получаем статистику
                data.TotalTestsCompleted = await GetCompletedTestsCountAsync(studentId);
                data.TotalTestsAvailable = await GetAvailableTestsCountAsync(studentId);
                data.AveragePercentage = await GetAveragePercentageAsync(studentId);
                data.TotalPoints = await GetTotalPointsAsync(studentId);

                // Получаем последние результаты
                data.RecentResults = await GetRecentResultsAsync(studentId);

                // Получаем ближайшие дедлайны
                data.UpcomingDeadlines = await GetUpcomingDeadlinesAsync(studentId);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных для главной страницы студента. StudentId: {StudentId}", studentId);
                throw;
            }
        }

        public async Task<int> GetCompletedTestsCountAsync(int studentId)
        {
            try
            {
                var spellingResults = await _testResultService.GetStudentResultsAsync<SpellingTestResult>(studentId);
                var punctuationResults = await _testResultService.GetStudentResultsAsync<PunctuationTestResult>(studentId);
                var orthoeopyResults = await _testResultService.GetStudentResultsAsync<OrthoeopyTestResult>(studentId);
                var regularResults = await _testResultService.GetStudentResultsAsync<RegularTestResult>(studentId);

                return spellingResults.Count(r => r.IsCompleted) +
                       punctuationResults.Count(r => r.IsCompleted) +
                       orthoeopyResults.Count(r => r.IsCompleted) +
                       regularResults.Count(r => r.IsCompleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении количества завершенных тестов. StudentId: {StudentId}", studentId);
                return 0;
            }
        }

        public async Task<int> GetAvailableTestsCountAsync(int studentId)
        {
            try
            {
                var spellingTests = await _testAccessService.GetAvailableSpellingTestsAsync(studentId);
                var punctuationTests = await _testAccessService.GetAvailablePunctuationTestsAsync(studentId);
                var orthoeopyTests = await _testAccessService.GetAvailableOrthoeopyTestsAsync(studentId);
                var regularTests = await _testAccessService.GetAvailableRegularTestsAsync(studentId);

                return spellingTests.Count + punctuationTests.Count + orthoeopyTests.Count + regularTests.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении количества доступных тестов. StudentId: {StudentId}", studentId);
                return 0;
            }
        }

        public async Task<double> GetAveragePercentageAsync(int studentId)
        {
            try
            {
                var spellingResults = await _testResultService.GetStudentResultsAsync<SpellingTestResult>(studentId);
                var punctuationResults = await _testResultService.GetStudentResultsAsync<PunctuationTestResult>(studentId);
                var orthoeopyResults = await _testResultService.GetStudentResultsAsync<OrthoeopyTestResult>(studentId);
                var regularResults = await _testResultService.GetStudentResultsAsync<RegularTestResult>(studentId);

                var allResults = spellingResults
                    .Cast<TestResult>()
                    .Concat(punctuationResults.Cast<TestResult>())
                    .Concat(orthoeopyResults.Cast<TestResult>())
                    .Concat(regularResults.Cast<TestResult>())
                    .Where(r => r.IsCompleted)
                    .ToList();

                if (allResults.Count == 0)
                {
                    return 0.0;
                }

                return allResults.Average(r => r.Percentage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении среднего процента. StudentId: {StudentId}", studentId);
                return 0.0;
            }
        }

        public async Task<int> GetTotalPointsAsync(int studentId)
        {
            try
            {
                var spellingResults = await _testResultService.GetStudentResultsAsync<SpellingTestResult>(studentId);
                var punctuationResults = await _testResultService.GetStudentResultsAsync<PunctuationTestResult>(studentId);
                var orthoeopyResults = await _testResultService.GetStudentResultsAsync<OrthoeopyTestResult>(studentId);
                var regularResults = await _testResultService.GetStudentResultsAsync<RegularTestResult>(studentId);

                return spellingResults.Where(r => r.IsCompleted).Sum(r => r.Score) +
                       punctuationResults.Where(r => r.IsCompleted).Sum(r => r.Score) +
                       orthoeopyResults.Where(r => r.IsCompleted).Sum(r => r.Score) +
                       regularResults.Where(r => r.IsCompleted).Sum(r => r.Score);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении общего количества баллов. StudentId: {StudentId}", studentId);
                return 0;
            }
        }

        private async Task<List<TestResultSummary>> GetRecentResultsAsync(int studentId)
        {
            try
            {
                var spellingResults = await _testResultService.GetStudentResultsAsync<SpellingTestResult>(studentId);
                var punctuationResults = await _testResultService.GetStudentResultsAsync<PunctuationTestResult>(studentId);
                var orthoeopyResults = await _testResultService.GetStudentResultsAsync<OrthoeopyTestResult>(studentId);
                var regularResults = await _testResultService.GetStudentResultsAsync<RegularTestResult>(studentId);

                var summaries = new List<TestResultSummary>();

                // Обрабатываем результаты по орфографии
                foreach (var result in spellingResults.Where(r => r.IsCompleted).OrderByDescending(r => r.CompletedAt).Take(5))
                {
                    var test = await _spellingTestRepository.GetByIdAsync(result.SpellingTestId);
                    if (test != null)
                    {
                        summaries.Add(new TestResultSummary
                        {
                            Id = result.Id,
                            TestTitle = test.Title,
                            TestType = "Орфография",
                            Score = result.Score,
                            MaxScore = result.MaxScore,
                            Percentage = result.Percentage,
                            CompletedAt = result.CompletedAt ?? result.StartedAt,
                            AttemptNumber = result.AttemptNumber
                        });
                    }
                }

                // Обрабатываем результаты по пунктуации
                foreach (var result in punctuationResults.Where(r => r.IsCompleted).OrderByDescending(r => r.CompletedAt).Take(5))
                {
                    var test = await _punctuationTestRepository.GetByIdAsync(result.PunctuationTestId);
                    if (test != null)
                    {
                        summaries.Add(new TestResultSummary
                        {
                            Id = result.Id,
                            TestTitle = test.Title,
                            TestType = "Пунктуация",
                            Score = result.Score,
                            MaxScore = result.MaxScore,
                            Percentage = result.Percentage,
                            CompletedAt = result.CompletedAt ?? result.StartedAt,
                            AttemptNumber = result.AttemptNumber
                        });
                    }
                }

                // Обрабатываем результаты по орфоэпии
                foreach (var result in orthoeopyResults.Where(r => r.IsCompleted).OrderByDescending(r => r.CompletedAt).Take(5))
                {
                    var test = await _orthoeopyTestRepository.GetByIdAsync(result.OrthoeopyTestId);
                    if (test != null)
                    {
                        summaries.Add(new TestResultSummary
                        {
                            Id = result.Id,
                            TestTitle = test.Title,
                            TestType = "Орфоэпия",
                            Score = result.Score,
                            MaxScore = result.MaxScore,
                            Percentage = result.Percentage,
                            CompletedAt = result.CompletedAt ?? result.StartedAt,
                            AttemptNumber = result.AttemptNumber
                        });
                    }
                }

                // Обрабатываем результаты классических тестов
                foreach (var result in regularResults.Where(r => r.IsCompleted).OrderByDescending(r => r.CompletedAt).Take(5))
                {
                    var test = await _regularTestRepository.GetByIdAsync(result.RegularTestId);
                    if (test != null)
                    {
                        summaries.Add(new TestResultSummary
                        {
                            Id = result.Id,
                            TestTitle = test.Title,
                            TestType = "Классический",
                            Score = result.Score,
                            MaxScore = result.MaxScore,
                            Percentage = result.Percentage,
                            CompletedAt = result.CompletedAt ?? result.StartedAt,
                            AttemptNumber = result.AttemptNumber
                        });
                    }
                }

                return summaries.OrderByDescending(s => s.CompletedAt).Take(10).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении последних результатов. StudentId: {StudentId}", studentId);
                return new List<TestResultSummary>();
            }
        }

        private async Task<List<AssignmentDeadline>> GetUpcomingDeadlinesAsync(int studentId)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null || !student.ClassId.HasValue)
                {
                    return new List<AssignmentDeadline>();
                }

                var @class = await _classRepository.GetByIdAsync(student.ClassId.Value);
                if (@class == null)
                {
                    return new List<AssignmentDeadline>();
                }

                // Получаем все задания, назначенные классу
                var allAssignments = await _assignmentRepository.GetByTeacherIdAsync(@class.TeacherId);
                
                // Получаем все назначения для класса
                var assignmentClasses = await _assignmentClassRepository.GetByClassIdAsync(student.ClassId.Value);
                var assignedAssignmentIds = assignmentClasses.Select(ac => ac.AssignmentId).ToList();

                var deadlines = new List<AssignmentDeadline>();
                var now = DateTime.Now;

                foreach (var assignment in allAssignments.Where(a => a.IsActive && a.DueDate.HasValue && a.DueDate.Value > now && assignedAssignmentIds.Contains(a.Id)))
                {
                    var subject = await _subjectRepository.GetByIdAsync(assignment.SubjectId);
                    
                    // Подсчитываем тесты в задании
                    var spellingTests = await _spellingTestRepository.GetByAssignmentIdAsync(assignment.Id);
                    var punctuationTests = await _punctuationTestRepository.GetByAssignmentIdAsync(assignment.Id);
                    var orthoeopyTests = await _orthoeopyTestRepository.GetByAssignmentIdAsync(assignment.Id);
                    var regularTests = await _regularTestRepository.GetByAssignmentIdAsync(assignment.Id);
                    
                    var totalTests = spellingTests.Count + punctuationTests.Count + orthoeopyTests.Count + regularTests.Count;

                    // Подсчитываем завершенные тесты
                    var completedCount = 0;
                    foreach (var test in spellingTests)
                    {
                        var results = await _testResultService.GetStudentResultsAsync<SpellingTestResult>(studentId);
                        if (results.Any(r => r.SpellingTestId == test.Id && r.IsCompleted))
                            completedCount++;
                    }
                    foreach (var test in punctuationTests)
                    {
                        var results = await _testResultService.GetStudentResultsAsync<PunctuationTestResult>(studentId);
                        if (results.Any(r => r.PunctuationTestId == test.Id && r.IsCompleted))
                            completedCount++;
                    }
                    foreach (var test in orthoeopyTests)
                    {
                        var results = await _testResultService.GetStudentResultsAsync<OrthoeopyTestResult>(studentId);
                        if (results.Any(r => r.OrthoeopyTestId == test.Id && r.IsCompleted))
                            completedCount++;
                    }
                    foreach (var test in regularTests)
                    {
                        var results = await _testResultService.GetStudentResultsAsync<RegularTestResult>(studentId);
                        if (results.Any(r => r.RegularTestId == test.Id && r.IsCompleted))
                            completedCount++;
                    }

                    deadlines.Add(new AssignmentDeadline
                    {
                        Id = assignment.Id,
                        Title = assignment.Title,
                        SubjectName = subject?.Name ?? "Неизвестно",
                        DueDate = assignment.DueDate,
                        TestsCount = totalTests,
                        CompletedTestsCount = completedCount
                    });
                }

                return deadlines.OrderBy(d => d.DueDate).Take(5).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении ближайших дедлайнов. StudentId: {StudentId}", studentId);
                return new List<AssignmentDeadline>();
            }
        }
    }
}

