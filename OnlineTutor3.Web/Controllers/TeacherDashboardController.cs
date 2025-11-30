using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.Hubs;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Teacher)]
    public class TeacherDashboardController : Controller
    {
        private readonly ISpellingTestService _spellingTestService;
        private readonly IPunctuationTestService _punctuationTestService;
        private readonly IOrthoeopyTestService _orthoeopyTestService;
        private readonly IRegularTestService _regularTestService;
        private readonly ISpellingTestResultRepository _spellingTestResultRepository;
        private readonly IPunctuationTestResultRepository _punctuationTestResultRepository;
        private readonly IOrthoeopyTestResultRepository _orthoeopyTestResultRepository;
        private readonly IRegularTestResultRepository _regularTestResultRepository;
        private readonly IStudentService _studentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TeacherDashboardController> _logger;

        public TeacherDashboardController(
            ISpellingTestService spellingTestService,
            IPunctuationTestService punctuationTestService,
            IOrthoeopyTestService orthoeopyTestService,
            IRegularTestService regularTestService,
            ISpellingTestResultRepository spellingTestResultRepository,
            IPunctuationTestResultRepository punctuationTestResultRepository,
            IOrthoeopyTestResultRepository orthoeopyTestResultRepository,
            IRegularTestResultRepository regularTestResultRepository,
            IStudentService studentService,
            UserManager<ApplicationUser> userManager,
            ILogger<TeacherDashboardController> logger)
        {
            _spellingTestService = spellingTestService;
            _punctuationTestService = punctuationTestService;
            _orthoeopyTestService = orthoeopyTestService;
            _regularTestService = regularTestService;
            _spellingTestResultRepository = spellingTestResultRepository;
            _punctuationTestResultRepository = punctuationTestResultRepository;
            _orthoeopyTestResultRepository = orthoeopyTestResultRepository;
            _regularTestResultRepository = regularTestResultRepository;
            _studentService = studentService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: TeacherDashboard
        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                // Получаем активные тесты учителя
                var spellingTests = await _spellingTestService.GetActiveByTeacherIdAsync(currentUser.Id);
                var punctuationTests = await _punctuationTestService.GetActiveByTeacherIdAsync(currentUser.Id);
                var orthoeopyTests = await _orthoeopyTestService.GetActiveByTeacherIdAsync(currentUser.Id);
                var regularTests = await _regularTestService.GetActiveByTeacherIdAsync(currentUser.Id);

                // Ограничиваем количество для отображения (последние 20)
                var viewModel = new TeacherDashboardViewModel
                {
                    Teacher = currentUser,
                    SpellingTests = spellingTests.Take(20).ToList(),
                    PunctuationTests = punctuationTests.Take(20).ToList(),
                    OrthoeopyTests = orthoeopyTests.Take(20).ToList(),
                    RegularTests = regularTests.Take(20).ToList()
                };

                // Подсчитываем статистику
                await CalculateStatisticsAsync(viewModel, currentUser.Id);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в TeacherDashboard/Index");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке панели мониторинга.";
                return View(new TeacherDashboardViewModel { Teacher = await _userManager.GetUserAsync(User) ?? new ApplicationUser() });
            }
        }

        /// <summary>
        /// Получение последней активности студентов
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecentActivity()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                var activities = new List<TestActivityViewModel>();

                // Получаем тесты учителя
                var spellingTests = await _spellingTestService.GetByTeacherIdAsync(currentUser.Id);
                var punctuationTests = await _punctuationTestService.GetByTeacherIdAsync(currentUser.Id);
                var orthoeopyTests = await _orthoeopyTestService.GetByTeacherIdAsync(currentUser.Id);
                var regularTests = await _regularTestService.GetByTeacherIdAsync(currentUser.Id);

                var spellingTestIds = spellingTests.Select(t => t.Id).ToList();
                var punctuationTestIds = punctuationTests.Select(t => t.Id).ToList();
                var orthoeopyTestIds = orthoeopyTests.Select(t => t.Id).ToList();
                var regularTestIds = regularTests.Select(t => t.Id).ToList();

                var spellingTestsDict = spellingTests.ToDictionary(t => t.Id);
                var punctuationTestsDict = punctuationTests.ToDictionary(t => t.Id);
                var orthoeopyTestsDict = orthoeopyTests.ToDictionary(t => t.Id);
                var regularTestsDict = regularTests.ToDictionary(t => t.Id);

                // Получаем результаты тестов по орфографии
                foreach (var testId in spellingTestIds)
                {
                    var testResults = await _spellingTestResultRepository.GetByTestIdAsync(testId);
                    var test = spellingTestsDict[testId];

                    foreach (var result in testResults)
                    {
                        var student = await _studentService.GetByIdAsync(result.StudentId);
                        if (student != null)
                        {
                            var user = await _userManager.FindByIdAsync(student.UserId);
                            activities.Add(new TestActivityViewModel
                            {
                                TestId = result.SpellingTestId,
                                TestTitle = test.Title,
                                TestType = "spelling",
                                StudentId = result.StudentId,
                                StudentName = user?.FullName ?? "Неизвестный студент",
                                Status = result.IsCompleted ? "completed" : "in_progress",
                                Score = result.Score,
                                MaxScore = result.MaxScore,
                                Percentage = result.Percentage,
                                StartedAt = result.StartedAt,
                                CompletedAt = result.CompletedAt,
                                LastActivityAt = result.CompletedAt ?? result.StartedAt,
                                TestResultId = result.Id,
                                IsAutoCompleted = false
                            });
                        }
                    }
                }

                // Получаем результаты тестов по пунктуации
                foreach (var testId in punctuationTestIds)
                {
                    var testResults = await _punctuationTestResultRepository.GetByTestIdAsync(testId);
                    var test = punctuationTestsDict[testId];

                    foreach (var result in testResults)
                    {
                        var student = await _studentService.GetByIdAsync(result.StudentId);
                        if (student != null)
                        {
                            var user = await _userManager.FindByIdAsync(student.UserId);
                            activities.Add(new TestActivityViewModel
                            {
                                TestId = result.PunctuationTestId,
                                TestTitle = test.Title,
                                TestType = "punctuation",
                                StudentId = result.StudentId,
                                StudentName = user?.FullName ?? "Неизвестный студент",
                                Status = result.IsCompleted ? "completed" : "in_progress",
                                Score = result.Score,
                                MaxScore = result.MaxScore,
                                Percentage = result.Percentage,
                                StartedAt = result.StartedAt,
                                CompletedAt = result.CompletedAt,
                                LastActivityAt = result.CompletedAt ?? result.StartedAt,
                                TestResultId = result.Id,
                                IsAutoCompleted = false
                            });
                        }
                    }
                }

                // Получаем результаты тестов по орфоэпии
                foreach (var testId in orthoeopyTestIds)
                {
                    var testResults = await _orthoeopyTestResultRepository.GetByTestIdAsync(testId);
                    var test = orthoeopyTestsDict[testId];

                    foreach (var result in testResults)
                    {
                        var student = await _studentService.GetByIdAsync(result.StudentId);
                        if (student != null)
                        {
                            var user = await _userManager.FindByIdAsync(student.UserId);
                            activities.Add(new TestActivityViewModel
                            {
                                TestId = result.OrthoeopyTestId,
                                TestTitle = test.Title,
                                TestType = "orthoeopy",
                                StudentId = result.StudentId,
                                StudentName = user?.FullName ?? "Неизвестный студент",
                                Status = result.IsCompleted ? "completed" : "in_progress",
                                Score = result.Score,
                                MaxScore = result.MaxScore,
                                Percentage = result.Percentage,
                                StartedAt = result.StartedAt,
                                CompletedAt = result.CompletedAt,
                                LastActivityAt = result.CompletedAt ?? result.StartedAt,
                                TestResultId = result.Id,
                                IsAutoCompleted = false
                            });
                        }
                    }
                }

                // Получаем результаты классических тестов
                foreach (var testId in regularTestIds)
                {
                    var testResults = await _regularTestResultRepository.GetByTestIdAsync(testId);
                    var test = regularTestsDict[testId];

                    foreach (var result in testResults)
                    {
                        var student = await _studentService.GetByIdAsync(result.StudentId);
                        if (student != null)
                        {
                            var user = await _userManager.FindByIdAsync(student.UserId);
                            activities.Add(new TestActivityViewModel
                            {
                                TestId = result.RegularTestId,
                                TestTitle = test.Title,
                                TestType = "regular",
                                StudentId = result.StudentId,
                                StudentName = user?.FullName ?? "Неизвестный студент",
                                Status = result.IsCompleted ? "completed" : "in_progress",
                                Score = result.Score,
                                MaxScore = result.MaxScore,
                                Percentage = result.Percentage,
                                StartedAt = result.StartedAt,
                                CompletedAt = result.CompletedAt,
                                LastActivityAt = result.CompletedAt ?? result.StartedAt,
                                TestResultId = result.Id,
                                IsAutoCompleted = false
                            });
                        }
                    }
                }

                // Сортируем по дате последней активности и берем последние 50
                var sortedActivities = activities
                    .OrderByDescending(a => a.LastActivityAt)
                    .Take(50)
                    .ToList();

                return Json(sortedActivities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении последней активности");
                return StatusCode(500, new { error = "Ошибка при загрузке активности" });
            }
        }

        /// <summary>
        /// Получение результата теста для модального окна
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTestResult(string testType, int testResultId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                if (string.IsNullOrEmpty(testType))
                {
                    return BadRequest("Тип теста не указан");
                }

                // Перенаправляем на соответствующий action в StudentTestController
                return RedirectToAction(
                    $"{testType}Result",
                    "StudentTest",
                    new { id = testResultId }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки результата теста {TestType} {ResultId}", testType, testResultId);
                return StatusCode(500, "Ошибка загрузки результата");
            }
        }

        /// <summary>
        /// Подсчет статистики для панели
        /// </summary>
        private async Task CalculateStatisticsAsync(TeacherDashboardViewModel viewModel, string teacherId)
        {
            try
            {
                var today = DateTime.Today;
                var allResults = new List<TestResult>();

                // Получаем тесты учителя
                var spellingTests = await _spellingTestService.GetByTeacherIdAsync(teacherId);
                var punctuationTests = await _punctuationTestService.GetByTeacherIdAsync(teacherId);
                var orthoeopyTests = await _orthoeopyTestService.GetByTeacherIdAsync(teacherId);
                var regularTests = await _regularTestService.GetByTeacherIdAsync(teacherId);

                var spellingTestIds = spellingTests.Select(t => t.Id).ToList();
                var punctuationTestIds = punctuationTests.Select(t => t.Id).ToList();
                var orthoeopyTestIds = orthoeopyTests.Select(t => t.Id).ToList();
                var regularTestIds = regularTests.Select(t => t.Id).ToList();

                // Получаем результаты по каждому тесту
                foreach (var testId in spellingTestIds)
                {
                    var results = await _spellingTestResultRepository.GetByTestIdAsync(testId);
                    allResults.AddRange(results);
                }

                foreach (var testId in punctuationTestIds)
                {
                    var results = await _punctuationTestResultRepository.GetByTestIdAsync(testId);
                    allResults.AddRange(results);
                }

                foreach (var testId in orthoeopyTestIds)
                {
                    var results = await _orthoeopyTestResultRepository.GetByTestIdAsync(testId);
                    allResults.AddRange(results);
                }

                foreach (var testId in regularTestIds)
                {
                    var results = await _regularTestResultRepository.GetByTestIdAsync(testId);
                    allResults.AddRange(results);
                }

                // Подсчитываем статистику
                viewModel.TotalStudentsInProgress = allResults.Count(r => !r.IsCompleted);
                viewModel.TotalCompletedToday = allResults.Count(r => 
                    r.IsCompleted && 
                    r.CompletedAt.HasValue && 
                    r.CompletedAt.Value.Date == today);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при подсчете статистики");
                // Устанавливаем значения по умолчанию
                viewModel.TotalStudentsInProgress = 0;
                viewModel.TotalCompletedToday = 0;
            }
        }
    }
}

