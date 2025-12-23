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
        private readonly INotParticleTestService _notParticleTestService;
        private readonly ISpellingTestResultRepository _spellingTestResultRepository;
        private readonly IPunctuationTestResultRepository _punctuationTestResultRepository;
        private readonly IOrthoeopyTestResultRepository _orthoeopyTestResultRepository;
        private readonly IRegularTestResultRepository _regularTestResultRepository;
        private readonly INotParticleTestResultRepository _notParticleTestResultRepository;
        private readonly ISpellingQuestionRepository _spellingQuestionRepository;
        private readonly IPunctuationQuestionRepository _punctuationQuestionRepository;
        private readonly IOrthoeopyQuestionRepository _orthoeopyQuestionRepository;
        private readonly IRegularQuestionRepository _regularQuestionRepository;
        private readonly INotParticleQuestionRepository _notParticleQuestionRepository;
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
            INotParticleTestService notParticleTestService,
            ISpellingTestResultRepository spellingTestResultRepository,
            IPunctuationTestResultRepository punctuationTestResultRepository,
            IOrthoeopyTestResultRepository orthoeopyTestResultRepository,
            IRegularTestResultRepository regularTestResultRepository,
            INotParticleTestResultRepository notParticleTestResultRepository,
            ISpellingQuestionRepository spellingQuestionRepository,
            IPunctuationQuestionRepository punctuationQuestionRepository,
            IOrthoeopyQuestionRepository orthoeopyQuestionRepository,
            IRegularQuestionRepository regularQuestionRepository,
            INotParticleQuestionRepository notParticleQuestionRepository,
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
            _notParticleTestService = notParticleTestService;
            _spellingTestResultRepository = spellingTestResultRepository;
            _punctuationTestResultRepository = punctuationTestResultRepository;
            _orthoeopyTestResultRepository = orthoeopyTestResultRepository;
            _regularTestResultRepository = regularTestResultRepository;
            _notParticleTestResultRepository = notParticleTestResultRepository;
            _spellingQuestionRepository = spellingQuestionRepository;
            _punctuationQuestionRepository = punctuationQuestionRepository;
            _orthoeopyQuestionRepository = orthoeopyQuestionRepository;
            _regularQuestionRepository = regularQuestionRepository;
            _notParticleQuestionRepository = notParticleQuestionRepository;
            _regularQuestionOptionRepository = regularQuestionOptionRepository;
            _answerService = answerService;
            _studentService = studentService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Вычисляет фактическое время прохождения теста с учетом пауз
        /// </summary>
        private TimeSpan CalculateTestDuration(TestResult testResult, int timeLimitMinutes)
        {
            if (!testResult.CompletedAt.HasValue)
            {
                return TimeSpan.Zero;
            }

            // Если есть информация об оставшемся времени, вычисляем фактическое время прохождения
            if (testResult.TimeRemainingSeconds.HasValue)
            {
                var timeLimit = TimeSpan.FromMinutes(timeLimitMinutes);
                var timeRemaining = TimeSpan.FromSeconds(testResult.TimeRemainingSeconds.Value);
                
                // Фактическое время прохождения = лимит времени - оставшееся время
                var actualDuration = timeLimit - timeRemaining;
                
                // Если время отрицательное или нулевое (тест завершен автоматически), используем лимит времени
                if (actualDuration <= TimeSpan.Zero)
                {
                    return timeLimit;
                }
                
                // Если время больше лимита (не должно быть, но на всякий случай), ограничиваем лимитом
                if (actualDuration > timeLimit)
                {
                    return timeLimit;
                }
                
                return actualDuration;
            }

            // Fallback: используем разницу между CompletedAt и StartedAt (может быть неточным из-за пауз)
            return testResult.CompletedAt.Value - testResult.StartedAt;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                var spellingTests = await _spellingTestService.GetActiveByTeacherIdAsync(currentUser.Id);
                var punctuationTests = await _punctuationTestService.GetActiveByTeacherIdAsync(currentUser.Id);
                var orthoeopyTests = await _orthoeopyTestService.GetActiveByTeacherIdAsync(currentUser.Id);
                var regularTests = await _regularTestService.GetActiveByTeacherIdAsync(currentUser.Id);
                var notParticleTests = await _notParticleTestService.GetActiveByTeacherIdAsync(currentUser.Id);

                var viewModel = new TeacherDashboardViewModel
                {
                    Teacher = currentUser,
                    SpellingTests = spellingTests.Take(20).ToList(),
                    PunctuationTests = punctuationTests.Take(20).ToList(),
                    OrthoeopyTests = orthoeopyTests.Take(20).ToList(),
                    RegularTests = regularTests.Take(20).ToList(),
                    NotParticleTests = notParticleTests.Take(20).ToList()
                };
                await CalculateStatisticsAsync(viewModel, currentUser.Id);
                await CalculateTestStatisticsAsync(viewModel);

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

                var spellingTests = await _spellingTestService.GetByTeacherIdAsync(currentUser.Id);
                var punctuationTests = await _punctuationTestService.GetByTeacherIdAsync(currentUser.Id);
                var orthoeopyTests = await _orthoeopyTestService.GetByTeacherIdAsync(currentUser.Id);
                var regularTests = await _regularTestService.GetByTeacherIdAsync(currentUser.Id);
                var notParticleTests = await _notParticleTestService.GetByTeacherIdAsync(currentUser.Id);

                var spellingTestIds = spellingTests.Select(t => t.Id).ToList();
                var punctuationTestIds = punctuationTests.Select(t => t.Id).ToList();
                var orthoeopyTestIds = orthoeopyTests.Select(t => t.Id).ToList();
                var regularTestIds = regularTests.Select(t => t.Id).ToList();
                var notParticleTestIds = notParticleTests.Select(t => t.Id).ToList();

                var spellingTestsDict = spellingTests.ToDictionary(t => t.Id);
                var punctuationTestsDict = punctuationTests.ToDictionary(t => t.Id);
                var orthoeopyTestsDict = orthoeopyTests.ToDictionary(t => t.Id);
                var regularTestsDict = regularTests.ToDictionary(t => t.Id);
                var notParticleTestsDict = notParticleTests.ToDictionary(t => t.Id);

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

                foreach (var testId in notParticleTestIds)
                {
                    var testResults = await _notParticleTestResultRepository.GetByTestIdAsync(testId);
                    var test = notParticleTestsDict[testId];

                    foreach (var result in testResults)
                    {
                        var student = await _studentService.GetByIdAsync(result.StudentId);
                        if (student != null)
                        {
                            var user = await _userManager.FindByIdAsync(student.UserId);
                            activities.Add(new TestActivityViewModel
                            {
                                TestId = result.NotParticleTestId,
                                TestTitle = test.Title,
                                TestType = "notparticle",
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

                object? result = null;
                string? studentName = null;

                switch (testType.ToLower())
                {
                    case "spelling":
                        var spellingResult = await _spellingTestResultRepository.GetByIdAsync(testResultId);
                        if (spellingResult != null)
                        {
                            var spellingTest = await _spellingTestService.GetByIdAsync(spellingResult.SpellingTestId);
                            if (spellingTest == null || spellingTest.TeacherId != currentUser.Id)
                            {
                                return Forbid();
                            }
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
                                Duration = CalculateTestDuration(spellingResult, spellingTest.TimeLimit),
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
                                Duration = CalculateTestDuration(punctuationResult, punctuationTest.TimeLimit),
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
                                Duration = CalculateTestDuration(orthoeopyResult, orthoeopyTest.TimeLimit),
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
                                Duration = CalculateTestDuration(regularResult, regularTest.TimeLimit),
                                StudentName = studentName,
                                ShowCorrectAnswers = regularTest.ShowCorrectAnswers,
                                TestIcon = "fa-list-ul",
                                TestColor = "info",
                                AttemptNumber = regularResult.AttemptNumber
                            };
                        }
                        break;

                    case "notparticle":
                        var notParticleResult = await _notParticleTestResultRepository.GetByIdAsync(testResultId);
                        if (notParticleResult != null)
                        {
                            var notParticleTest = await _notParticleTestService.GetByIdAsync(notParticleResult.NotParticleTestId);
                            if (notParticleTest == null || notParticleTest.TeacherId != currentUser.Id)
                            {
                                return Forbid();
                            }

                            var notParticleQuestions = await _notParticleQuestionRepository.GetByTestIdOrderedAsync(notParticleResult.NotParticleTestId);
                            var notParticleAnswers = await _answerService.GetNotParticleAnswersAsync(testResultId);
                            var student5 = await _studentService.GetByIdAsync(notParticleResult.StudentId);
                            var studentUser5 = student5 != null ? await _userManager.FindByIdAsync(student5.UserId) : null;
                            studentName = studentUser5?.FullName ?? studentUser5?.Email ?? "Неизвестный студент";

                            result = new NotParticleTestResultViewModel
                            {
                                TestResult = notParticleResult,
                                Test = notParticleTest,
                                Questions = notParticleQuestions,
                                Answers = notParticleAnswers,
                                TestTitle = notParticleTest.Title,
                                Score = notParticleResult.Score,
                                MaxScore = notParticleResult.MaxScore,
                                Percentage = notParticleResult.Percentage,
                                Grade = notParticleResult.Grade ?? 0,
                                CompletedAt = notParticleResult.CompletedAt,
                                StartedAt = notParticleResult.StartedAt,
                                Duration = CalculateTestDuration(notParticleResult, notParticleTest.TimeLimit),
                                StudentName = studentName,
                                ShowCorrectAnswers = notParticleTest.ShowCorrectAnswers,
                                TestIcon = "fa-minus-circle",
                                TestColor = "secondary",
                                AttemptNumber = notParticleResult.AttemptNumber
                            };
                        }
                        break;

                    default:
                        return BadRequest($"Неизвестный тип теста: {testType}");
                }

                if (result == null)
                {
                    return NotFound();
                }

                ViewBag.IsModal = true;
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

                var spellingTests = await _spellingTestService.GetByTeacherIdAsync(teacherId);
                var punctuationTests = await _punctuationTestService.GetByTeacherIdAsync(teacherId);
                var orthoeopyTests = await _orthoeopyTestService.GetByTeacherIdAsync(teacherId);
                var regularTests = await _regularTestService.GetByTeacherIdAsync(teacherId);
                var notParticleTests = await _notParticleTestService.GetByTeacherIdAsync(teacherId);

                var spellingTestIds = spellingTests.Select(t => t.Id).ToList();
                var punctuationTestIds = punctuationTests.Select(t => t.Id).ToList();
                var orthoeopyTestIds = orthoeopyTests.Select(t => t.Id).ToList();
                var regularTestIds = regularTests.Select(t => t.Id).ToList();
                var notParticleTestIds = notParticleTests.Select(t => t.Id).ToList();

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

                foreach (var testId in notParticleTestIds)
                {
                    var results = await _notParticleTestResultRepository.GetByTestIdAsync(testId);
                    allResults.AddRange(results);
                }

                viewModel.TotalStudentsInProgress = allResults.Count(r => !r.IsCompleted);
                viewModel.TotalCompletedToday = allResults.Count(r => 
                    r.IsCompleted && 
                    r.CompletedAt.HasValue && 
                    r.CompletedAt.Value.Date == today);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при подсчете статистики");
                viewModel.TotalStudentsInProgress = 0;
                viewModel.TotalCompletedToday = 0;
            }
        }

        /// <summary>
        /// Подсчет статистики по каждому тесту (завершено и в процессе)
        /// </summary>
        private async Task CalculateTestStatisticsAsync(TeacherDashboardViewModel viewModel)
        {
            try
            {
                // Обрабатываем Spelling тесты
                foreach (var test in viewModel.SpellingTests)
                {
                    var results = await _spellingTestResultRepository.GetByTestIdAsync(test.Id);
                    var completed = results.Count(r => r.IsCompleted);
                    var inProgress = results.Count(r => !r.IsCompleted);
                    viewModel.TestStatistics[$"spelling_{test.Id}"] = (completed, inProgress);
                }

                // Обрабатываем Punctuation тесты
                foreach (var test in viewModel.PunctuationTests)
                {
                    var results = await _punctuationTestResultRepository.GetByTestIdAsync(test.Id);
                    var completed = results.Count(r => r.IsCompleted);
                    var inProgress = results.Count(r => !r.IsCompleted);
                    viewModel.TestStatistics[$"punctuation_{test.Id}"] = (completed, inProgress);
                }

                // Обрабатываем Orthoeopy тесты
                foreach (var test in viewModel.OrthoeopyTests)
                {
                    var results = await _orthoeopyTestResultRepository.GetByTestIdAsync(test.Id);
                    var completed = results.Count(r => r.IsCompleted);
                    var inProgress = results.Count(r => !r.IsCompleted);
                    viewModel.TestStatistics[$"orthoeopy_{test.Id}"] = (completed, inProgress);
                }

                // Обрабатываем Regular тесты
                foreach (var test in viewModel.RegularTests)
                {
                    var results = await _regularTestResultRepository.GetByTestIdAsync(test.Id);
                    var completed = results.Count(r => r.IsCompleted);
                    var inProgress = results.Count(r => !r.IsCompleted);
                    viewModel.TestStatistics[$"regular_{test.Id}"] = (completed, inProgress);
                }

                // Обрабатываем NotParticle тесты
                foreach (var test in viewModel.NotParticleTests)
                {
                    var results = await _notParticleTestResultRepository.GetByTestIdAsync(test.Id);
                    var completed = results.Count(r => r.IsCompleted);
                    var inProgress = results.Count(r => !r.IsCompleted);
                    viewModel.TestStatistics[$"notparticle_{test.Id}"] = (completed, inProgress);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при подсчете статистики по тестам");
            }
        }
    }
}

