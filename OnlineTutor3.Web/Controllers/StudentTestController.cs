using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Student)]
    public class StudentTestController : Controller
    {
        private readonly IStudentTestService _studentTestService;
        private readonly IStudentRepository _studentRepository;
        private readonly ITestResultService _testResultService;
        private readonly ITestAccessService _testAccessService;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IClassRepository _classRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly ISpellingTestRepository _spellingTestRepository;
        private readonly IPunctuationTestRepository _punctuationTestRepository;
        private readonly IOrthoeopyTestRepository _orthoeopyTestRepository;
        private readonly IRegularTestRepository _regularTestRepository;
        private readonly ISpellingQuestionRepository _spellingQuestionRepository;
        private readonly IPunctuationQuestionRepository _punctuationQuestionRepository;
        private readonly IOrthoeopyQuestionRepository _orthoeopyQuestionRepository;
        private readonly IRegularQuestionRepository _regularQuestionRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StudentTestController> _logger;

        public StudentTestController(
            IStudentTestService studentTestService,
            IStudentRepository studentRepository,
            ITestResultService testResultService,
            ITestAccessService testAccessService,
            IAssignmentRepository assignmentRepository,
            IClassRepository classRepository,
            ISubjectRepository subjectRepository,
            ISpellingTestRepository spellingTestRepository,
            IPunctuationTestRepository punctuationTestRepository,
            IOrthoeopyTestRepository orthoeopyTestRepository,
            IRegularTestRepository regularTestRepository,
            ISpellingQuestionRepository spellingQuestionRepository,
            IPunctuationQuestionRepository punctuationQuestionRepository,
            IOrthoeopyQuestionRepository orthoeopyQuestionRepository,
            IRegularQuestionRepository regularQuestionRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<StudentTestController> logger)
        {
            _studentTestService = studentTestService;
            _studentRepository = studentRepository;
            _testResultService = testResultService;
            _testAccessService = testAccessService;
            _assignmentRepository = assignmentRepository;
            _classRepository = classRepository;
            _subjectRepository = subjectRepository;
            _spellingTestRepository = spellingTestRepository;
            _punctuationTestRepository = punctuationTestRepository;
            _orthoeopyTestRepository = orthoeopyTestRepository;
            _regularTestRepository = regularTestRepository;
            _spellingQuestionRepository = spellingQuestionRepository;
            _punctuationQuestionRepository = punctuationQuestionRepository;
            _orthoeopyQuestionRepository = orthoeopyQuestionRepository;
            _regularQuestionRepository = regularQuestionRepository;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: StudentTest
        public async Task<IActionResult> Index(string? category, string? search, int? assignment)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var student = await _studentRepository.GetByUserIdAsync(currentUser.Id);
                if (student == null)
                {
                    _logger.LogWarning("Студент не найден для пользователя {UserId}", currentUser.Id);
                    TempData["ErrorMessage"] = "Профиль студента не найден. Обратитесь к администратору.";
                    return RedirectToAction("Index", "Student");
                }

                var viewModel = new StudentTestIndexViewModel
                {
                    Student = student,
                    CurrentCategory = category,
                    SearchQuery = search,
                    AssignmentFilter = assignment
                };

                // Загружаем класс, если есть
                if (student.ClassId.HasValue)
                {
                    viewModel.Class = await _classRepository.GetByIdAsync(student.ClassId.Value);
                }

                // Получаем доступные тесты
                var availableTests = await _studentTestService.GetAvailableTestsAsync(student.Id, category);

                // Обрабатываем тесты по орфографии
                foreach (var test in availableTests.SpellingTests)
                {
                    var testInfo = await BuildSpellingTestInfoAsync(test, student.Id);
                    if (testInfo != null)
                    {
                        viewModel.SpellingTests.Add(testInfo);
                    }
                }

                // Обрабатываем тесты по пунктуации
                foreach (var test in availableTests.PunctuationTests)
                {
                    var testInfo = await BuildPunctuationTestInfoAsync(test, student.Id);
                    if (testInfo != null)
                    {
                        viewModel.PunctuationTests.Add(testInfo);
                    }
                }

                // Обрабатываем тесты по орфоэпии
                foreach (var test in availableTests.OrthoeopyTests)
                {
                    var testInfo = await BuildOrthoeopyTestInfoAsync(test, student.Id);
                    if (testInfo != null)
                    {
                        viewModel.OrthoeopyTests.Add(testInfo);
                    }
                }

                // Обрабатываем классические тесты
                foreach (var test in availableTests.RegularTests)
                {
                    var testInfo = await BuildRegularTestInfoAsync(test, student.Id);
                    if (testInfo != null)
                    {
                        viewModel.RegularTests.Add(testInfo);
                    }
                }

                // Получаем доступные задания для фильтра
                if (viewModel.Class != null)
                {
                    var assignments = await _assignmentRepository.GetByTeacherIdAsync(viewModel.Class.TeacherId);
                    viewModel.AvailableAssignments = assignments.Where(a => a.IsActive).ToList();
                }

                // Применяем фильтр по заданию, если указан
                if (assignment.HasValue)
                {
                    viewModel.SpellingTests = viewModel.SpellingTests.Where(t => t.AssignmentId == assignment.Value).ToList();
                    viewModel.PunctuationTests = viewModel.PunctuationTests.Where(t => t.AssignmentId == assignment.Value).ToList();
                    viewModel.OrthoeopyTests = viewModel.OrthoeopyTests.Where(t => t.AssignmentId == assignment.Value).ToList();
                    viewModel.RegularTests = viewModel.RegularTests.Where(t => t.AssignmentId == assignment.Value).ToList();
                }

                // Применяем поиск, если указан
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    viewModel.SpellingTests = viewModel.SpellingTests.Where(t => t.Title.ToLower().Contains(searchLower)).ToList();
                    viewModel.PunctuationTests = viewModel.PunctuationTests.Where(t => t.Title.ToLower().Contains(searchLower)).ToList();
                    viewModel.OrthoeopyTests = viewModel.OrthoeopyTests.Where(t => t.Title.ToLower().Contains(searchLower)).ToList();
                    viewModel.RegularTests = viewModel.RegularTests.Where(t => t.Title.ToLower().Contains(searchLower)).ToList();
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке списка доступных тестов");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке тестов. Попробуйте обновить страницу.";
                return RedirectToAction("Index", "Student");
            }
        }

        private async Task<AvailableTestInfo?> BuildSpellingTestInfoAsync(SpellingTest test, int studentId)
        {
            return await BuildTestInfoAsync(test.Id, "Spelling", studentId, test, async () =>
            {
                var questions = await _spellingQuestionRepository.GetByTestIdOrderedAsync(test.Id);
                return questions.Count;
            });
        }

        private async Task<AvailableTestInfo?> BuildPunctuationTestInfoAsync(PunctuationTest test, int studentId)
        {
            return await BuildTestInfoAsync(test.Id, "Punctuation", studentId, test, async () =>
            {
                var questions = await _punctuationQuestionRepository.GetByTestIdOrderedAsync(test.Id);
                return questions.Count;
            });
        }

        private async Task<AvailableTestInfo?> BuildOrthoeopyTestInfoAsync(OrthoeopyTest test, int studentId)
        {
            return await BuildTestInfoAsync(test.Id, "Orthoeopy", studentId, test, async () =>
            {
                var questions = await _orthoeopyQuestionRepository.GetByTestIdOrderedAsync(test.Id);
                return questions.Count;
            });
        }

        private async Task<AvailableTestInfo?> BuildRegularTestInfoAsync(RegularTest test, int studentId)
        {
            return await BuildTestInfoAsync(test.Id, "Regular", studentId, test, async () =>
            {
                var questions = await _regularQuestionRepository.GetByTestIdOrderedAsync(test.Id);
                return questions.Count;
            });
        }

        private async Task<AvailableTestInfo?> BuildTestInfoAsync(int testId, string testType, int studentId, Test test, Func<Task<int>> getQuestionsCount)
        {
            try
            {
                var testInfo = new AvailableTestInfo
                {
                    Id = test.Id,
                    Title = test.Title,
                    Description = test.Description,
                    TimeLimit = test.TimeLimit,
                    MaxAttempts = test.MaxAttempts,
                    AssignmentId = test.AssignmentId,
                    TestType = testType,
                    QuestionsCount = await getQuestionsCount()
                };

                // Получаем информацию о задании
                var assignment = await _assignmentRepository.GetByIdAsync(testInfo.AssignmentId);
                if (assignment != null)
                {
                    testInfo.AssignmentTitle = assignment.Title;
                    var subject = await _subjectRepository.GetByIdAsync(assignment.SubjectId);
                    testInfo.SubjectName = subject?.Name ?? "Неизвестно";
                }

                // Получаем результаты студента
                var results = await GetStudentResultsAsync(testId, testType, studentId);
                testInfo.AttemptsUsed = results.Count;
                
                var ongoingResult = results.FirstOrDefault(r => !r.IsCompleted);
                var completedResults = results.Where(r => r.IsCompleted).ToList();

                // Определяем статус
                if (ongoingResult != null)
                {
                    testInfo.Status = TestStatus.Ongoing;
                    testInfo.OngoingTestResultId = ongoingResult.Id;
                }
                else if (testInfo.AttemptsUsed >= testInfo.MaxAttempts)
                {
                    testInfo.Status = TestStatus.Exhausted;
                }
                else
                {
                    testInfo.Status = TestStatus.CanStart;
                }

                // Получаем лучший результат
                if (completedResults.Any())
                {
                    var best = completedResults.OrderByDescending(r => r.Percentage).First();
                    testInfo.BestPercentage = best.Percentage;
                    testInfo.BestScore = best.Score;
                    testInfo.BestMaxScore = best.MaxScore;
                }

                return testInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при построении информации о тесте. TestId: {TestId}, TestType: {TestType}", testId, testType);
                return null;
            }
        }

        private async Task<List<TestResult>> GetStudentResultsAsync(int testId, string testType, int studentId)
        {
            return testType switch
            {
                "Spelling" => (await _testResultService.GetStudentResultsAsync<SpellingTestResult>(studentId))
                    .Where(r => r.SpellingTestId == testId).Cast<TestResult>().ToList(),
                "Punctuation" => (await _testResultService.GetStudentResultsAsync<PunctuationTestResult>(studentId))
                    .Where(r => r.PunctuationTestId == testId).Cast<TestResult>().ToList(),
                "Orthoeopy" => (await _testResultService.GetStudentResultsAsync<OrthoeopyTestResult>(studentId))
                    .Where(r => r.OrthoeopyTestId == testId).Cast<TestResult>().ToList(),
                "Regular" => (await _testResultService.GetStudentResultsAsync<RegularTestResult>(studentId))
                    .Where(r => r.RegularTestId == testId).Cast<TestResult>().ToList(),
                _ => new List<TestResult>()
            };
        }
    }
}

