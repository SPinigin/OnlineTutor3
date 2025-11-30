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

                // Получаем все доступные задания
                var allAssignments = new List<Assignment>();
                if (viewModel.Class != null)
                {
                    allAssignments = await _assignmentRepository.GetByTeacherIdAsync(viewModel.Class.TeacherId);
                    allAssignments = allAssignments.Where(a => a.IsActive).ToList();
                }

                // Создаем словарь для группировки тестов по заданиям
                var assignmentsDict = new Dictionary<int, AssignmentTestsInfo>();
                var subjectsDict = new Dictionary<int, string>();

                // Загружаем предметы
                var subjectRepository = HttpContext.RequestServices.GetRequiredService<ISubjectRepository>();
                foreach (var assignmentEntity in allAssignments)
                {
                    if (!subjectsDict.ContainsKey(assignmentEntity.SubjectId))
                    {
                        var subject = await subjectRepository.GetByIdAsync(assignmentEntity.SubjectId);
                        subjectsDict[assignmentEntity.SubjectId] = subject?.Name ?? $"Предмет #{assignmentEntity.SubjectId}";
                    }
                }
                viewModel.SubjectsDict = subjectsDict;

                // Обрабатываем тесты по орфографии
                foreach (var test in availableTests.SpellingTests)
                {
                    var testInfo = await BuildSpellingTestInfoAsync(test, student.Id);
                    if (testInfo != null)
                    {
                        viewModel.SpellingTests.Add(testInfo);
                        
                        // Группируем по заданию
                        if (!assignmentsDict.ContainsKey(testInfo.AssignmentId))
                        {
                            var assignmentEntity = await _assignmentRepository.GetByIdAsync(testInfo.AssignmentId);
                            if (assignmentEntity != null)
                            {
                                assignmentsDict[testInfo.AssignmentId] = new AssignmentTestsInfo { Assignment = assignmentEntity };
                            }
                        }
                        if (assignmentsDict.ContainsKey(testInfo.AssignmentId))
                        {
                            assignmentsDict[testInfo.AssignmentId].SpellingTests.Add(testInfo);
                        }
                    }
                }

                // Обрабатываем тесты по пунктуации
                foreach (var test in availableTests.PunctuationTests)
                {
                    var testInfo = await BuildPunctuationTestInfoAsync(test, student.Id);
                    if (testInfo != null)
                    {
                        viewModel.PunctuationTests.Add(testInfo);
                        
                        // Группируем по заданию
                        if (!assignmentsDict.ContainsKey(testInfo.AssignmentId))
                        {
                            var assignmentEntity = await _assignmentRepository.GetByIdAsync(testInfo.AssignmentId);
                            if (assignmentEntity != null)
                            {
                                assignmentsDict[testInfo.AssignmentId] = new AssignmentTestsInfo { Assignment = assignmentEntity };
                            }
                        }
                        if (assignmentsDict.ContainsKey(testInfo.AssignmentId))
                        {
                            assignmentsDict[testInfo.AssignmentId].PunctuationTests.Add(testInfo);
                        }
                    }
                }

                // Обрабатываем тесты по орфоэпии
                foreach (var test in availableTests.OrthoeopyTests)
                {
                    var testInfo = await BuildOrthoeopyTestInfoAsync(test, student.Id);
                    if (testInfo != null)
                    {
                        viewModel.OrthoeopyTests.Add(testInfo);
                        
                        // Группируем по заданию
                        if (!assignmentsDict.ContainsKey(testInfo.AssignmentId))
                        {
                            var assignmentEntity = await _assignmentRepository.GetByIdAsync(testInfo.AssignmentId);
                            if (assignmentEntity != null)
                            {
                                assignmentsDict[testInfo.AssignmentId] = new AssignmentTestsInfo { Assignment = assignmentEntity };
                            }
                        }
                        if (assignmentsDict.ContainsKey(testInfo.AssignmentId))
                        {
                            assignmentsDict[testInfo.AssignmentId].OrthoeopyTests.Add(testInfo);
                        }
                    }
                }

                // Обрабатываем классические тесты
                foreach (var test in availableTests.RegularTests)
                {
                    var testInfo = await BuildRegularTestInfoAsync(test, student.Id);
                    if (testInfo != null)
                    {
                        viewModel.RegularTests.Add(testInfo);
                        
                        // Группируем по заданию
                        if (!assignmentsDict.ContainsKey(testInfo.AssignmentId))
                        {
                            var assignmentEntity = await _assignmentRepository.GetByIdAsync(testInfo.AssignmentId);
                            if (assignmentEntity != null)
                            {
                                assignmentsDict[testInfo.AssignmentId] = new AssignmentTestsInfo { Assignment = assignmentEntity };
                            }
                        }
                        if (assignmentsDict.ContainsKey(testInfo.AssignmentId))
                        {
                            assignmentsDict[testInfo.AssignmentId].RegularTests.Add(testInfo);
                        }
                    }
                }

                // Применяем фильтр по заданию, если указан
                if (assignment.HasValue)
                {
                    assignmentsDict = assignmentsDict.Where(kvp => kvp.Key == assignment.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }

                // Применяем поиск, если указан
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    var filteredAssignments = new Dictionary<int, AssignmentTestsInfo>();
                    
                    foreach (var kvp in assignmentsDict)
                    {
                        var assignmentInfo = kvp.Value;
                        var filteredInfo = new AssignmentTestsInfo
                        {
                            Assignment = assignmentInfo.Assignment,
                            SpellingTests = assignmentInfo.SpellingTests.Where(t => t.Title.ToLower().Contains(searchLower)).ToList(),
                            PunctuationTests = assignmentInfo.PunctuationTests.Where(t => t.Title.ToLower().Contains(searchLower)).ToList(),
                            OrthoeopyTests = assignmentInfo.OrthoeopyTests.Where(t => t.Title.ToLower().Contains(searchLower)).ToList(),
                            RegularTests = assignmentInfo.RegularTests.Where(t => t.Title.ToLower().Contains(searchLower)).ToList()
                        };
                        
                        // Добавляем только если есть тесты или название задания совпадает
                        if (filteredInfo.TotalTestsCount > 0 || assignmentInfo.Assignment.Title.ToLower().Contains(searchLower))
                        {
                            filteredAssignments[kvp.Key] = filteredInfo;
                        }
                    }
                    
                    assignmentsDict = filteredAssignments;
                }

                viewModel.AssignmentsWithTests = assignmentsDict;
                viewModel.AvailableAssignments = allAssignments;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке списка доступных тестов");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке тестов. Попробуйте обновить страницу.";
                return RedirectToAction("Index", "Student");
            }
        }

        // GET: StudentTest/History
        public async Task<IActionResult> History(string? testType)
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

                // Получаем историю через сервис
                var historyData = await _studentTestService.GetTestHistoryAsync(student.Id, testType);

                // Создаем ViewModel для Web слоя
                var viewModel = new ViewModels.StudentTestHistoryViewModel
                {
                    Student = student,
                    CurrentTestType = testType,
                    SpellingResults = historyData.SpellingResults,
                    PunctuationResults = historyData.PunctuationResults,
                    OrthoeopyResults = historyData.OrthoeopyResults,
                    RegularResults = historyData.RegularResults
                };

                // Загружаем связанные данные (тесты) для каждого результата
                await LoadTestDataForResultsAsync(viewModel);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке истории тестов");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке истории. Попробуйте обновить страницу.";
                return RedirectToAction("Index", "Student");
            }
        }

        private async Task LoadTestDataForResultsAsync(ViewModels.StudentTestHistoryViewModel viewModel)
        {
            // Загружаем тесты для результатов по орфографии
            foreach (var result in viewModel.SpellingResults)
            {
                if (result.SpellingTest == null && result.SpellingTestId > 0)
                {
                    result.SpellingTest = await _spellingTestRepository.GetByIdAsync(result.SpellingTestId);
                }
            }

            // Загружаем тесты для результатов по пунктуации
            foreach (var result in viewModel.PunctuationResults)
            {
                if (result.PunctuationTest == null && result.PunctuationTestId > 0)
                {
                    result.PunctuationTest = await _punctuationTestRepository.GetByIdAsync(result.PunctuationTestId);
                }
            }

            // Загружаем тесты для результатов по орфоэпии
            foreach (var result in viewModel.OrthoeopyResults)
            {
                if (result.OrthoeopyTest == null && result.OrthoeopyTestId > 0)
                {
                    result.OrthoeopyTest = await _orthoeopyTestRepository.GetByIdAsync(result.OrthoeopyTestId);
                }
            }

            // Загружаем тесты для классических результатов
            foreach (var result in viewModel.RegularResults)
            {
                if (result.RegularTest == null && result.RegularTestId > 0)
                {
                    result.RegularTest = await _regularTestRepository.GetByIdAsync(result.RegularTestId);
                }
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
                var assignmentEntity = await _assignmentRepository.GetByIdAsync(testInfo.AssignmentId);
                if (assignmentEntity != null)
                {
                    testInfo.AssignmentTitle = assignmentEntity.Title;
                    var subject = await _subjectRepository.GetByIdAsync(assignmentEntity.SubjectId);
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

