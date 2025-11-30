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
        private readonly ISpellingQuestionRepository _spellingQuestionRepository;
        private readonly IPunctuationQuestionRepository _punctuationQuestionRepository;
        private readonly IOrthoeopyQuestionRepository _orthoeopyQuestionRepository;
        private readonly IRegularQuestionRepository _regularQuestionRepository;
        private readonly IRegularQuestionOptionRepository _regularQuestionOptionRepository;
        private readonly IAnswerService _answerService;
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
            ISpellingQuestionRepository spellingQuestionRepository,
            IPunctuationQuestionRepository punctuationQuestionRepository,
            IOrthoeopyQuestionRepository orthoeopyQuestionRepository,
            IRegularQuestionRepository regularQuestionRepository,
            IRegularQuestionOptionRepository regularQuestionOptionRepository,
            IAnswerService answerService,
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
            _spellingQuestionRepository = spellingQuestionRepository;
            _punctuationQuestionRepository = punctuationQuestionRepository;
            _orthoeopyQuestionRepository = orthoeopyQuestionRepository;
            _regularQuestionRepository = regularQuestionRepository;
            _regularQuestionOptionRepository = regularQuestionOptionRepository;
            _answerService = answerService;
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

                // Загружаем результат теста и проверяем права доступа
                object? result = null;
                string? studentName = null;

                switch (testType.ToLower())
                {
                    case "spelling":
                        var spellingResult = await _spellingTestResultRepository.GetByIdAsync(testResultId);
                        if (spellingResult != null)
                        {
                            // Проверяем, что тест принадлежит учителю
                            var spellingTest = await _spellingTestService.GetByIdAsync(spellingResult.SpellingTestId);
                            if (spellingTest == null || spellingTest.TeacherId != currentUser.Id)
                            {
                                return Forbid();
                            }

                            // Загружаем данные
                            var spellingQuestions = await _spellingQuestionRepository.GetByTestIdOrderedAsync(spellingResult.SpellingTestId);
                            var spellingAnswers = await _answerService.GetSpellingAnswersAsync(testResultId);
                            var student = await _studentService.GetByIdAsync(spellingResult.StudentId);
                            var studentUser = student != null ? await _userManager.FindByIdAsync(student.UserId) : null;
                            studentName = studentUser?.FullName ?? studentUser?.Email ?? "Неизвестный студент";

                            result = new SpellingTestResultViewModel
                            {
                                TestResult = spellingResult,
                                Test = spellingTest,
                                Questions = spellingQuestions,
                                Answers = spellingAnswers,
                                TestTitle = spellingTest.Title,
                                Score = spellingResult.Score,
                                MaxScore = spellingResult.MaxScore,
                                Percentage = spellingResult.Percentage,
                                Grade = spellingResult.Grade ?? 0,
                                CompletedAt = spellingResult.CompletedAt,
                                StartedAt = spellingResult.StartedAt,
                                Duration = spellingResult.CompletedAt.HasValue ? spellingResult.CompletedAt.Value - spellingResult.StartedAt : TimeSpan.Zero,
                                StudentName = studentName,
                                ShowCorrectAnswers = spellingTest.ShowCorrectAnswers,
                                TestIcon = "fa-spell-check",
                                TestColor = "primary",
                                AttemptNumber = spellingResult.AttemptNumber
                            };
                        }
                        break;

                    case "punctuation":
                        var punctuationResult = await _punctuationTestResultRepository.GetByIdAsync(testResultId);
                        if (punctuationResult != null)
                        {
                            var punctuationTest = await _punctuationTestService.GetByIdAsync(punctuationResult.PunctuationTestId);
                            if (punctuationTest == null || punctuationTest.TeacherId != currentUser.Id)
                            {
                                return Forbid();
                            }

                            var punctuationQuestions = await _punctuationQuestionRepository.GetByTestIdOrderedAsync(punctuationResult.PunctuationTestId);
                            var punctuationAnswers = await _answerService.GetPunctuationAnswersAsync(testResultId);
                            var student2 = await _studentService.GetByIdAsync(punctuationResult.StudentId);
                            var studentUser2 = student2 != null ? await _userManager.FindByIdAsync(student2.UserId) : null;
                            studentName = studentUser2?.FullName ?? studentUser2?.Email ?? "Неизвестный студент";

                            result = new PunctuationTestResultViewModel
                            {
                                TestResult = punctuationResult,
                                Test = punctuationTest,
                                Questions = punctuationQuestions,
                                Answers = punctuationAnswers,
                                TestTitle = punctuationTest.Title,
                                Score = punctuationResult.Score,
                                MaxScore = punctuationResult.MaxScore,
                                Percentage = punctuationResult.Percentage,
                                Grade = punctuationResult.Grade ?? 0,
                                CompletedAt = punctuationResult.CompletedAt,
                                StartedAt = punctuationResult.StartedAt,
                                Duration = punctuationResult.CompletedAt.HasValue ? punctuationResult.CompletedAt.Value - punctuationResult.StartedAt : TimeSpan.Zero,
                                StudentName = studentName,
                                ShowCorrectAnswers = punctuationTest.ShowCorrectAnswers,
                                TestIcon = "fa-exclamation",
                                TestColor = "info",
                                AttemptNumber = punctuationResult.AttemptNumber
                            };
                        }
                        break;

                    case "orthoeopy":
                        var orthoeopyResult = await _orthoeopyTestResultRepository.GetByIdAsync(testResultId);
                        if (orthoeopyResult != null)
                        {
                            var orthoeopyTest = await _orthoeopyTestService.GetByIdAsync(orthoeopyResult.OrthoeopyTestId);
                            if (orthoeopyTest == null || orthoeopyTest.TeacherId != currentUser.Id)
                            {
                                return Forbid();
                            }

                            var orthoeopyQuestions = await _orthoeopyQuestionRepository.GetByTestIdOrderedAsync(orthoeopyResult.OrthoeopyTestId);
                            var orthoeopyAnswers = await _answerService.GetOrthoeopyAnswersAsync(testResultId);
                            var student3 = await _studentService.GetByIdAsync(orthoeopyResult.StudentId);
                            var studentUser3 = student3 != null ? await _userManager.FindByIdAsync(student3.UserId) : null;
                            studentName = studentUser3?.FullName ?? studentUser3?.Email ?? "Неизвестный студент";

                            result = new OrthoeopyTestResultViewModel
                            {
                                TestResult = orthoeopyResult,
                                Test = orthoeopyTest,
                                Questions = orthoeopyQuestions,
                                Answers = orthoeopyAnswers,
                                TestTitle = orthoeopyTest.Title,
                                Score = orthoeopyResult.Score,
                                MaxScore = orthoeopyResult.MaxScore,
                                Percentage = orthoeopyResult.Percentage,
                                Grade = orthoeopyResult.Grade ?? 0,
                                CompletedAt = orthoeopyResult.CompletedAt,
                                StartedAt = orthoeopyResult.StartedAt,
                                Duration = orthoeopyResult.CompletedAt.HasValue ? orthoeopyResult.CompletedAt.Value - orthoeopyResult.StartedAt : TimeSpan.Zero,
                                StudentName = studentName,
                                ShowCorrectAnswers = orthoeopyTest.ShowCorrectAnswers,
                                TestIcon = "fa-volume-up",
                                TestColor = "success",
                                AttemptNumber = orthoeopyResult.AttemptNumber
                            };
                        }
                        break;

                    case "regular":
                        var regularResult = await _regularTestResultRepository.GetByIdAsync(testResultId);
                        if (regularResult != null)
                        {
                            var regularTest = await _regularTestService.GetByIdAsync(regularResult.RegularTestId);
                            if (regularTest == null || regularTest.TeacherId != currentUser.Id)
                            {
                                return Forbid();
                            }

                            var regularQuestions = await _regularQuestionRepository.GetByTestIdOrderedAsync(regularResult.RegularTestId);
                            var regularAnswers = await _answerService.GetRegularAnswersAsync(testResultId);
                            
                            // Загружаем опции для каждого вопроса
                            var allOptions = new List<RegularQuestionOption>();
                            foreach (var question in regularQuestions)
                            {
                                var options = await _regularQuestionOptionRepository.GetByQuestionIdOrderedAsync(question.Id);
                                allOptions.AddRange(options);
                            }

                            var student4 = await _studentService.GetByIdAsync(regularResult.StudentId);
                            var studentUser4 = student4 != null ? await _userManager.FindByIdAsync(student4.UserId) : null;
                            studentName = studentUser4?.FullName ?? studentUser4?.Email ?? "Неизвестный студент";

                            result = new RegularTestResultViewModel
                            {
                                TestResult = regularResult,
                                Test = regularTest,
                                Questions = regularQuestions,
                                Options = allOptions,
                                Answers = regularAnswers,
                                TestTitle = regularTest.Title,
                                Score = regularResult.Score,
                                MaxScore = regularResult.MaxScore,
                                Percentage = regularResult.Percentage,
                                Grade = regularResult.Grade ?? 0,
                                CompletedAt = regularResult.CompletedAt,
                                StartedAt = regularResult.StartedAt,
                                Duration = regularResult.CompletedAt.HasValue ? regularResult.CompletedAt.Value - regularResult.StartedAt : TimeSpan.Zero,
                                StudentName = studentName,
                                ShowCorrectAnswers = regularTest.ShowCorrectAnswers,
                                TestIcon = "fa-list-ul",
                                TestColor = "info",
                                AttemptNumber = regularResult.AttemptNumber
                            };
                        }
                        break;

                    default:
                        _logger.LogWarning("Неизвестный тип теста: {TestType}", testType);
                        return BadRequest($"Неизвестный тип теста: {testType}");
                }

                if (result == null)
                {
                    _logger.LogWarning("Результат теста не найден: TestType={TestType}, ResultId={ResultId}, TeacherId={TeacherId}", 
                        testType, testResultId, currentUser.Id);
                    return NotFound();
                }

                // Возвращаем частичное представление для модального окна
                ViewBag.IsModal = true;
                
                // Возвращаем соответствующее представление в зависимости от типа теста
                return PartialView($"~/Views/StudentTest/{testType}Result.cshtml", result);
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

