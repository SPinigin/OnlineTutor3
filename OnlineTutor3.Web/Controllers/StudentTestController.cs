using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Application.Services;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.Hubs;
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
        private readonly ITestEvaluationService _testEvaluationService;
        private readonly IAnswerService _answerService;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IClassRepository _classRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly ISpellingTestRepository _spellingTestRepository;
        private readonly IPunctuationTestRepository _punctuationTestRepository;
        private readonly IOrthoeopyTestRepository _orthoeopyTestRepository;
        private readonly IRegularTestRepository _regularTestRepository;
        private readonly ISpellingTestResultRepository _spellingTestResultRepository;
        private readonly IPunctuationTestResultRepository _punctuationTestResultRepository;
        private readonly IOrthoeopyTestResultRepository _orthoeopyTestResultRepository;
        private readonly IRegularTestResultRepository _regularTestResultRepository;
        private readonly IRegularQuestionOptionRepository _regularQuestionOptionRepository;
        private readonly ISpellingQuestionRepository _spellingQuestionRepository;
        private readonly IPunctuationQuestionRepository _punctuationQuestionRepository;
        private readonly IOrthoeopyQuestionRepository _orthoeopyQuestionRepository;
        private readonly IRegularQuestionRepository _regularQuestionRepository;
        private readonly IHubContext<TestAnalyticsHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SecurityValidationService _securityValidation;
        private readonly ILogger<StudentTestController> _logger;

        public StudentTestController(
            IStudentTestService studentTestService,
            IStudentRepository studentRepository,
            ITestResultService testResultService,
            ITestAccessService testAccessService,
            ITestEvaluationService testEvaluationService,
            IAnswerService answerService,
            IAssignmentRepository assignmentRepository,
            IClassRepository classRepository,
            ISubjectRepository subjectRepository,
            ISpellingTestRepository spellingTestRepository,
            IPunctuationTestRepository punctuationTestRepository,
            IOrthoeopyTestRepository orthoeopyTestRepository,
            IRegularTestRepository regularTestRepository,
            ISpellingTestResultRepository spellingTestResultRepository,
            IPunctuationTestResultRepository punctuationTestResultRepository,
            IOrthoeopyTestResultRepository orthoeopyTestResultRepository,
            IRegularTestResultRepository regularTestResultRepository,
            IRegularQuestionOptionRepository regularQuestionOptionRepository,
            ISpellingQuestionRepository spellingQuestionRepository,
            IPunctuationQuestionRepository punctuationQuestionRepository,
            IOrthoeopyQuestionRepository orthoeopyQuestionRepository,
            IRegularQuestionRepository regularQuestionRepository,
            IHubContext<TestAnalyticsHub> hubContext,
            UserManager<ApplicationUser> userManager,
            SecurityValidationService securityValidation,
            ILogger<StudentTestController> logger)
        {
            _studentTestService = studentTestService;
            _studentRepository = studentRepository;
            _testResultService = testResultService;
            _testAccessService = testAccessService;
            _testEvaluationService = testEvaluationService;
            _answerService = answerService;
            _assignmentRepository = assignmentRepository;
            _classRepository = classRepository;
            _subjectRepository = subjectRepository;
            _spellingTestRepository = spellingTestRepository;
            _punctuationTestRepository = punctuationTestRepository;
            _orthoeopyTestRepository = orthoeopyTestRepository;
            _regularTestRepository = regularTestRepository;
            _spellingTestResultRepository = spellingTestResultRepository;
            _punctuationTestResultRepository = punctuationTestResultRepository;
            _orthoeopyTestResultRepository = orthoeopyTestResultRepository;
            _regularTestResultRepository = regularTestResultRepository;
            _regularQuestionOptionRepository = regularQuestionOptionRepository;
            _spellingQuestionRepository = spellingQuestionRepository;
            _punctuationQuestionRepository = punctuationQuestionRepository;
            _orthoeopyQuestionRepository = orthoeopyQuestionRepository;
            _regularQuestionRepository = regularQuestionRepository;
            _hubContext = hubContext;
            _userManager = userManager;
            _securityValidation = securityValidation;
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
        public async Task<IActionResult> History(string? search)
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

                // Получаем историю через сервис (без фильтра по типу)
                var historyData = await _studentTestService.GetTestHistoryAsync(student.Id, null);

                // Создаем ViewModel для Web слоя
                var viewModel = new ViewModels.StudentTestHistoryViewModel
                {
                    Student = student,
                    SearchQuery = search,
                    SpellingResults = historyData.SpellingResults,
                    PunctuationResults = historyData.PunctuationResults,
                    OrthoeopyResults = historyData.OrthoeopyResults,
                    RegularResults = historyData.RegularResults
                };

                // Загружаем связанные данные (тесты) для каждого результата
                await LoadTestDataForResultsAsync(viewModel);

                // Применяем поиск, если указан
                if (!string.IsNullOrWhiteSpace(search))
                {
                    ApplySearchFilter(viewModel, search);
                }

                // Группируем результаты по заданиям
                await GroupResultsByAssignmentsAsync(viewModel);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке истории тестов");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке истории. Попробуйте обновить страницу.";
                return RedirectToAction("Index", "Student");
            }
        }

        private void ApplySearchFilter(ViewModels.StudentTestHistoryViewModel viewModel, string searchQuery)
        {
            var searchLower = searchQuery.ToLowerInvariant().Trim();

            // Фильтруем результаты по названию теста
            viewModel.SpellingResults = viewModel.SpellingResults
                .Where(r => r.SpellingTest != null && 
                           (r.SpellingTest.Title.ToLowerInvariant().Contains(searchLower) ||
                            r.SpellingTest.Description?.ToLowerInvariant().Contains(searchLower) == true))
                .ToList();

            viewModel.PunctuationResults = viewModel.PunctuationResults
                .Where(r => r.PunctuationTest != null && 
                           (r.PunctuationTest.Title.ToLowerInvariant().Contains(searchLower) ||
                            r.PunctuationTest.Description?.ToLowerInvariant().Contains(searchLower) == true))
                .ToList();

            viewModel.OrthoeopyResults = viewModel.OrthoeopyResults
                .Where(r => r.OrthoeopyTest != null && 
                           (r.OrthoeopyTest.Title.ToLowerInvariant().Contains(searchLower) ||
                            r.OrthoeopyTest.Description?.ToLowerInvariant().Contains(searchLower) == true))
                .ToList();

            viewModel.RegularResults = viewModel.RegularResults
                .Where(r => r.RegularTest != null && 
                           (r.RegularTest.Title.ToLowerInvariant().Contains(searchLower) ||
                            r.RegularTest.Description?.ToLowerInvariant().Contains(searchLower) == true))
                .ToList();
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

        private async Task GroupResultsByAssignmentsAsync(ViewModels.StudentTestHistoryViewModel viewModel)
        {
            var assignmentsDict = new Dictionary<int, ViewModels.AssignmentHistoryInfo>();
            var subjectsDict = new Dictionary<int, string>();

            // Группируем результаты по орфографии
            foreach (var result in viewModel.SpellingResults)
            {
                if (result.SpellingTest != null)
                {
                    var assignmentId = result.SpellingTest.AssignmentId;
                    if (!assignmentsDict.ContainsKey(assignmentId))
                    {
                        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
                        if (assignment != null)
                        {
                            assignmentsDict[assignmentId] = new ViewModels.AssignmentHistoryInfo { Assignment = assignment };
                            
                            // Загружаем предмет
                            if (!subjectsDict.ContainsKey(assignment.SubjectId))
                            {
                                var subject = await _subjectRepository.GetByIdAsync(assignment.SubjectId);
                                subjectsDict[assignment.SubjectId] = subject?.Name ?? $"Предмет #{assignment.SubjectId}";
                            }
                        }
                    }
                    if (assignmentsDict.ContainsKey(assignmentId))
                    {
                        assignmentsDict[assignmentId].SpellingResults.Add(result);
                    }
                }
            }

            // Группируем результаты по пунктуации
            foreach (var result in viewModel.PunctuationResults)
            {
                if (result.PunctuationTest != null)
                {
                    var assignmentId = result.PunctuationTest.AssignmentId;
                    if (!assignmentsDict.ContainsKey(assignmentId))
                    {
                        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
                        if (assignment != null)
                        {
                            assignmentsDict[assignmentId] = new ViewModels.AssignmentHistoryInfo { Assignment = assignment };
                            
                            if (!subjectsDict.ContainsKey(assignment.SubjectId))
                            {
                                var subject = await _subjectRepository.GetByIdAsync(assignment.SubjectId);
                                subjectsDict[assignment.SubjectId] = subject?.Name ?? $"Предмет #{assignment.SubjectId}";
                            }
                        }
                    }
                    if (assignmentsDict.ContainsKey(assignmentId))
                    {
                        assignmentsDict[assignmentId].PunctuationResults.Add(result);
                    }
                }
            }

            // Группируем результаты по орфоэпии
            foreach (var result in viewModel.OrthoeopyResults)
            {
                if (result.OrthoeopyTest != null)
                {
                    var assignmentId = result.OrthoeopyTest.AssignmentId;
                    if (!assignmentsDict.ContainsKey(assignmentId))
                    {
                        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
                        if (assignment != null)
                        {
                            assignmentsDict[assignmentId] = new ViewModels.AssignmentHistoryInfo { Assignment = assignment };
                            
                            if (!subjectsDict.ContainsKey(assignment.SubjectId))
                            {
                                var subject = await _subjectRepository.GetByIdAsync(assignment.SubjectId);
                                subjectsDict[assignment.SubjectId] = subject?.Name ?? $"Предмет #{assignment.SubjectId}";
                            }
                        }
                    }
                    if (assignmentsDict.ContainsKey(assignmentId))
                    {
                        assignmentsDict[assignmentId].OrthoeopyResults.Add(result);
                    }
                }
            }

            // Группируем классические результаты
            foreach (var result in viewModel.RegularResults)
            {
                if (result.RegularTest != null)
                {
                    var assignmentId = result.RegularTest.AssignmentId;
                    if (!assignmentsDict.ContainsKey(assignmentId))
                    {
                        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
                        if (assignment != null)
                        {
                            assignmentsDict[assignmentId] = new ViewModels.AssignmentHistoryInfo { Assignment = assignment };
                            
                            if (!subjectsDict.ContainsKey(assignment.SubjectId))
                            {
                                var subject = await _subjectRepository.GetByIdAsync(assignment.SubjectId);
                                subjectsDict[assignment.SubjectId] = subject?.Name ?? $"Предмет #{assignment.SubjectId}";
                            }
                        }
                    }
                    if (assignmentsDict.ContainsKey(assignmentId))
                    {
                        assignmentsDict[assignmentId].RegularResults.Add(result);
                    }
                }
            }

            viewModel.AssignmentsWithResults = assignmentsDict;
            viewModel.SubjectsDict = subjectsDict;
        }

        // GET: StudentTest/StartSpelling/{id}
        public async Task<IActionResult> StartSpelling(int id)
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
                    TempData["ErrorMessage"] = "Профиль студента не найден.";
                    return RedirectToAction("Index");
                }

                // Используем сервис для начала теста
                var testResult = await _studentTestService.StartSpellingTestAsync(student.Id, id);
                
                _logger.LogInformation("Студент {StudentId} начал тест по орфографии {TestId}", student.Id, id);
                
                // Отправляем уведомление через SignalR
                await SendTestStartedNotificationAsync(testResult, "spelling", currentUser.FullName ?? currentUser.Email ?? "Студент");
                
                return RedirectToAction(nameof(TakeSpelling), new { id = testResult.Id });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Попытка доступа к недоступному тесту. TestId: {TestId}", id);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при начале теста по орфографии. TestId: {TestId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при начале теста. Попробуйте позже.";
                return RedirectToAction("Index");
            }
        }

        // GET: StudentTest/TakeSpelling/{id}
        public async Task<IActionResult> TakeSpelling(int id)
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
                    return NotFound();
                }

                // Получаем результат теста через репозиторий
                var testResult = await _spellingTestResultRepository.GetByIdAsync(id);
                
                if (testResult == null || testResult.StudentId != student.Id)
                {
                    return NotFound();
                }

                // Проверяем, не завершен ли тест
                if (testResult.IsCompleted)
                {
                    return RedirectToAction("SpellingResult", new { id = testResult.Id });
                }

                // Загружаем тест
                var test = await _spellingTestRepository.GetByIdAsync(testResult.SpellingTestId);
                if (test == null)
                {
                    return NotFound();
                }

                // Загружаем вопросы
                var questions = await _spellingQuestionRepository.GetByTestIdOrderedAsync(test.Id);
                if (!questions.Any())
                {
                    TempData["ErrorMessage"] = "В тесте нет вопросов.";
                    return RedirectToAction("Index");
                }

                // Загружаем ответы
                var answers = await _answerService.GetSpellingAnswersAsync(testResult.Id);

                // Вычисляем оставшееся время
                // Если есть сохраненное время (пауза), используем его, иначе вычисляем на основе StartedAt
                TimeSpan timeRemaining;
                if (testResult.TimeRemainingSeconds.HasValue && testResult.TimeRemainingSeconds.Value > 0)
                {
                    // Используем сохраненное время (тест был на паузе)
                    timeRemaining = TimeSpan.FromSeconds(testResult.TimeRemainingSeconds.Value);
                }
                else
                {
                    // Вычисляем время на основе StartedAt
                    var timeElapsed = DateTime.Now - testResult.StartedAt;
                    var timeLimit = TimeSpan.FromMinutes(test.TimeLimit);
                    timeRemaining = timeLimit - timeElapsed;
                    
                    // Сохраняем вычисленное время для будущих пауз
                    if (timeRemaining > TimeSpan.Zero)
                    {
                        testResult.TimeRemainingSeconds = (int)timeRemaining.TotalSeconds;
                        await _spellingTestResultRepository.UpdateAsync(testResult);
                    }
                }

                // Если время истекло, завершаем тест автоматически
                if (timeRemaining <= TimeSpan.Zero)
                {
                    // Вычисляем результат
                    var (score, maxScore, percentage) = await _testEvaluationService.CalculateSpellingTestResultAsync(testResult.Id, test.Id);
                    
                    // Вычисляем оценку
                    var grade = TestEvaluationService.CalculateGrade(percentage);
                    
                    // Обновляем результат теста
                    testResult.Score = score;
                    testResult.MaxScore = maxScore;
                    testResult.Percentage = percentage;
                    testResult.Grade = grade;
                    
                    // Завершаем тест
                    await _testResultService.CompleteTestResultAsync(testResult);
                    return RedirectToAction("SpellingResult", new { id = testResult.Id });
                }

                // Создаем ViewModel
                var viewModel = new ViewModels.TakeSpellingTestViewModel
                {
                    TestResult = testResult,
                    Test = test,
                    Questions = questions,
                    Answers = answers,
                    TimeRemaining = timeRemaining,
                    CurrentQuestionIndex = 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке страницы прохождения теста. ResultId: {ResultId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке теста.";
                return RedirectToAction("Index");
            }
        }

        // POST: StudentTest/SubmitSpellingAnswer
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SubmitSpellingAnswer([FromBody] ViewModels.SubmitSpellingAnswerViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Пользователь не авторизован" });
                }

                var student = await _studentRepository.GetByUserIdAsync(currentUser.Id);
                if (student == null)
                {
                    return Json(new { success = false, message = "Студент не найден" });
                }

                // Проверяем результат теста через репозиторий
                var testResult = await _spellingTestResultRepository.GetByIdAsync(model.TestResultId);
                
                // Проверки безопасности
                if (!_securityValidation.ValidateStudentAccessToResult(testResult, student.Id))
                {
                    return Json(new { success = false, message = "Результат теста не найден" });
                }

                if (!_securityValidation.ValidateTestNotCompleted(testResult))
                {
                    return Json(new { success = false, message = "Тест уже завершен" });
                }

                // Загружаем тест для проверки времени
                var test = await _spellingTestRepository.GetByIdAsync(testResult.SpellingTestId);
                if (test == null)
                {
                    return Json(new { success = false, message = "Тест не найден" });
                }

                // Проверяем время (с буфером 30 секунд)
                if (!_securityValidation.ValidateTimeLimit(testResult.StartedAt, test.TimeLimit, 30))
                {
                    return Json(new { success = false, message = "Время теста истекло" });
                }

                // Проверяем вопрос
                var question = await _spellingQuestionRepository.GetByIdAsync(model.QuestionId);
                if (question == null || question.SpellingTestId != testResult.SpellingTestId)
                {
                    return Json(new { success = false, message = "Вопрос не найден" });
                }

                // Валидация ответа
                if (!_securityValidation.ValidateAnswer(model.StudentAnswer, 500))
                {
                    return Json(new { success = false, message = "Некорректный ответ" });
                }

                // Проверяем, что вопрос принадлежит тесту
                var allQuestionIds = (await _spellingQuestionRepository.GetByTestIdOrderedAsync(test.Id))
                    .Select(q => q.Id).ToList();
                if (!_securityValidation.ValidateQuestionId(model.QuestionId, allQuestionIds))
                {
                    return Json(new { success = false, message = "Вопрос не принадлежит тесту" });
                }

                // Сохраняем ответ
                var answer = await _answerService.SaveSpellingAnswerAsync(model.TestResultId, model.QuestionId, model.StudentAnswer ?? "");

                // Оцениваем ответ
                var (isCorrect, points) = await _testEvaluationService.EvaluateSpellingAnswerAsync(
                    question, 
                    model.StudentAnswer ?? "", 
                    question.Points);

                // Обновляем ответ с результатом оценки
                answer.IsCorrect = isCorrect;
                answer.Points = points;
                await _answerService.UpdateAnswerAsync(answer);

                return Json(new 
                { 
                    success = true, 
                    isCorrect = isCorrect,
                    points = points
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении ответа. QuestionId: {QuestionId}, TestResultId: {TestResultId}", 
                    model.QuestionId, model.TestResultId);
                return Json(new { success = false, message = "Произошла ошибка при сохранении ответа" });
            }
        }

        // POST: StudentTest/UpdateTimeRemaining
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateTimeRemaining(int testResultId, int timeRemainingSeconds, string testType)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                var student = await _studentRepository.GetByUserIdAsync(currentUser.Id);
                if (student == null)
                {
                    return Unauthorized();
                }

                // Обновляем время в зависимости от типа теста
                switch (testType.ToLower())
                {
                    case "spelling":
                        await _testResultService.UpdateTimeRemainingAsync<SpellingTestResult>(testResultId, timeRemainingSeconds);
                        break;
                    case "punctuation":
                        await _testResultService.UpdateTimeRemainingAsync<PunctuationTestResult>(testResultId, timeRemainingSeconds);
                        break;
                    case "orthoeopy":
                        await _testResultService.UpdateTimeRemainingAsync<OrthoeopyTestResult>(testResultId, timeRemainingSeconds);
                        break;
                    case "regular":
                        await _testResultService.UpdateTimeRemainingAsync<RegularTestResult>(testResultId, timeRemainingSeconds);
                        break;
                    default:
                        return BadRequest("Неизвестный тип теста");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении оставшегося времени. ResultId: {ResultId}", testResultId);
                return StatusCode(500, "Ошибка при обновлении времени");
            }
        }

        // POST: StudentTest/CompleteSpellingTest/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteSpellingTest(int id)
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
                    return NotFound();
                }

                // Получаем результат теста через репозиторий
                var testResult = await _spellingTestResultRepository.GetByIdAsync(id);
                
                // Проверки безопасности
                if (!_securityValidation.ValidateStudentAccessToResult(testResult, student.Id))
                {
                    _logger.LogWarning("Попытка завершить тест другого студента. ResultId: {ResultId}, StudentId: {StudentId}", 
                        id, student.Id);
                    return NotFound();
                }

                if (!_securityValidation.ValidateTestNotCompleted(testResult))
                {
                    return RedirectToAction("SpellingResult", new { id = testResult.Id });
                }

                // Загружаем тест для проверки времени
                var test = await _spellingTestRepository.GetByIdAsync(testResult.SpellingTestId);
                if (test == null)
                {
                    return NotFound();
                }

                // Проверяем время (с буфером 60 секунд для завершения)
                if (!_securityValidation.ValidateTimeLimit(testResult.StartedAt, test.TimeLimit, 60))
                {
                    _logger.LogWarning("Попытка завершить тест после истечения времени. ResultId: {ResultId}", id);
                    TempData["ErrorMessage"] = "Время теста истекло. Тест будет завершен автоматически.";
                    // Все равно завершаем тест, но логируем
                }

                // Вычисляем результат
                var (score, maxScore, percentage) = await _testEvaluationService.CalculateSpellingTestResultAsync(testResult.Id, testResult.SpellingTestId);
                
                // Вычисляем оценку
                var grade = TestEvaluationService.CalculateGrade(percentage);
                
                // Обновляем результат теста
                testResult.Score = score;
                testResult.MaxScore = maxScore;
                testResult.Percentage = percentage;
                testResult.Grade = grade;
                
                // Завершаем тест (устанавливает CompletedAt и IsCompleted)
                await _testResultService.CompleteTestResultAsync(testResult);

                _logger.LogInformation("Студент {StudentId} завершил тест по орфографии {ResultId}. Баллы: {Score}/{MaxScore}, Процент: {Percentage}",
                    student.Id, testResult.Id, testResult.Score, testResult.MaxScore, testResult.Percentage);

                // Отправляем уведомление через SignalR
                await SendTestCompletedNotificationAsync(testResult, "spelling", currentUser.FullName ?? currentUser.Email ?? "Студент");

                return RedirectToAction("SpellingResult", new { id = testResult.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при завершении теста по орфографии. ResultId: {ResultId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при завершении теста.";
                return RedirectToAction("Index");
            }
        }

        // GET: StudentTest/StartPunctuation/{id}
        public async Task<IActionResult> StartPunctuation(int id)
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
                    TempData["ErrorMessage"] = "Профиль студента не найден.";
                    return RedirectToAction("Index");
                }

                // Используем сервис для начала теста
                var testResult = await _studentTestService.StartPunctuationTestAsync(student.Id, id);
                
                _logger.LogInformation("Студент {StudentId} начал тест по пунктуации {TestId}", student.Id, id);
                
                // Отправляем уведомление через SignalR
                await SendTestStartedNotificationAsync(testResult, "punctuation", currentUser.FullName ?? currentUser.Email ?? "Студент");
                
                return RedirectToAction(nameof(TakePunctuation), new { id = testResult.Id });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Попытка доступа к недоступному тесту. TestId: {TestId}", id);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при начале теста по пунктуации. TestId: {TestId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при начале теста. Попробуйте позже.";
                return RedirectToAction("Index");
            }
        }

        // GET: StudentTest/TakePunctuation/{id}
        public async Task<IActionResult> TakePunctuation(int id)
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
                    return NotFound();
                }

                // Получаем результат теста через репозиторий
                var testResult = await _punctuationTestResultRepository.GetByIdAsync(id);
                
                if (testResult == null || testResult.StudentId != student.Id)
                {
                    return NotFound();
                }

                // Проверяем, не завершен ли тест
                if (testResult.IsCompleted)
                {
                    return RedirectToAction("PunctuationResult", new { id = testResult.Id });
                }

                // Загружаем тест
                var test = await _punctuationTestRepository.GetByIdAsync(testResult.PunctuationTestId);
                if (test == null)
                {
                    return NotFound();
                }

                // Загружаем вопросы
                var questions = await _punctuationQuestionRepository.GetByTestIdOrderedAsync(test.Id);
                if (!questions.Any())
                {
                    TempData["ErrorMessage"] = "В тесте нет вопросов.";
                    return RedirectToAction("Index");
                }

                // Загружаем ответы
                var answers = await _answerService.GetPunctuationAnswersAsync(testResult.Id);

                // Вычисляем оставшееся время
                // Если есть сохраненное время (пауза), используем его, иначе вычисляем на основе StartedAt
                TimeSpan timeRemaining;
                if (testResult.TimeRemainingSeconds.HasValue && testResult.TimeRemainingSeconds.Value > 0)
                {
                    // Используем сохраненное время (тест был на паузе)
                    timeRemaining = TimeSpan.FromSeconds(testResult.TimeRemainingSeconds.Value);
                }
                else
                {
                    // Вычисляем время на основе StartedAt
                    var timeElapsed = DateTime.Now - testResult.StartedAt;
                    var timeLimit = TimeSpan.FromMinutes(test.TimeLimit);
                    timeRemaining = timeLimit - timeElapsed;
                    
                    // Сохраняем вычисленное время для будущих пауз
                    if (timeRemaining > TimeSpan.Zero)
                    {
                        testResult.TimeRemainingSeconds = (int)timeRemaining.TotalSeconds;
                        await _punctuationTestResultRepository.UpdateAsync(testResult);
                    }
                }

                // Если время истекло, завершаем тест автоматически
                if (timeRemaining <= TimeSpan.Zero)
                {
                    // Вычисляем результат
                    var (score, maxScore, percentage) = await _testEvaluationService.CalculatePunctuationTestResultAsync(testResult.Id, test.Id);
                    
                    // Вычисляем оценку
                    var grade = TestEvaluationService.CalculateGrade(percentage);
                    
                    // Обновляем результат теста
                    testResult.Score = score;
                    testResult.MaxScore = maxScore;
                    testResult.Percentage = percentage;
                    testResult.Grade = grade;
                    
                    // Завершаем тест
                    await _testResultService.CompleteTestResultAsync(testResult);
                    
                    return RedirectToAction("PunctuationResult", new { id = testResult.Id });
                }

                var viewModel = new TakePunctuationTestViewModel
                {
                    TestResult = testResult,
                    Test = test,
                    Questions = questions,
                    Answers = answers,
                    TimeRemaining = timeRemaining,
                    CurrentQuestionIndex = 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке теста по пунктуации. ResultId: {ResultId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке теста.";
                return RedirectToAction("Index");
            }
        }

        // POST: StudentTest/SubmitPunctuationAnswer
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SubmitPunctuationAnswer([FromBody] SubmitPunctuationAnswerViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Пользователь не авторизован" });
                }

                var student = await _studentRepository.GetByUserIdAsync(currentUser.Id);
                if (student == null)
                {
                    return Json(new { success = false, message = "Студент не найден" });
                }

                // Получаем результат теста
                var testResult = await _punctuationTestResultRepository.GetByIdAsync(model.TestResultId);
                if (testResult == null || testResult.StudentId != student.Id || testResult.IsCompleted)
                {
                    return Json(new { success = false, message = "Тест не найден или уже завершен" });
                }

                // Получаем вопрос
                var question = await _punctuationQuestionRepository.GetByIdAsync(model.QuestionId);
                if (question == null || question.PunctuationTestId != testResult.PunctuationTestId)
                {
                    return Json(new { success = false, message = "Вопрос не найден" });
                }

                // Сохраняем ответ
                var answer = await _answerService.SavePunctuationAnswerAsync(
                    testResult.Id, 
                    question.Id, 
                    model.StudentAnswer ?? "");

                // Оцениваем ответ
                var (isCorrect, points) = await _testEvaluationService.EvaluatePunctuationAnswerAsync(
                    question, 
                    model.StudentAnswer ?? "", 
                    question.Points);

                // Обновляем ответ с результатом оценки
                answer.IsCorrect = isCorrect;
                answer.Points = points;
                await _answerService.UpdateAnswerAsync(answer);

                return Json(new 
                { 
                    success = true, 
                    isCorrect = isCorrect,
                    points = points
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении ответа. QuestionId: {QuestionId}, TestResultId: {TestResultId}", 
                    model.QuestionId, model.TestResultId);
                return Json(new { success = false, message = "Произошла ошибка при сохранении ответа" });
            }
        }

        // POST: StudentTest/CompletePunctuationTest/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletePunctuationTest(int id)
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
                    return NotFound();
                }

                // Получаем результат теста через репозиторий
                var testResult = await _punctuationTestResultRepository.GetByIdAsync(id);
                
                if (testResult == null || testResult.StudentId != student.Id)
                {
                    return NotFound();
                }

                if (testResult.IsCompleted)
                {
                    return RedirectToAction("PunctuationResult", new { id = testResult.Id });
                }

                // Вычисляем результат
                var (score, maxScore, percentage) = await _testEvaluationService.CalculatePunctuationTestResultAsync(testResult.Id, testResult.PunctuationTestId);
                
                // Вычисляем оценку
                var grade = TestEvaluationService.CalculateGrade(percentage);
                
                // Обновляем результат теста
                testResult.Score = score;
                testResult.MaxScore = maxScore;
                testResult.Percentage = percentage;
                testResult.Grade = grade;
                
                // Завершаем тест (устанавливает CompletedAt и IsCompleted)
                await _testResultService.CompleteTestResultAsync(testResult);

                _logger.LogInformation("Студент {StudentId} завершил тест по пунктуации {ResultId}. Баллы: {Score}/{MaxScore}, Процент: {Percentage}",
                    student.Id, testResult.Id, testResult.Score, testResult.MaxScore, testResult.Percentage);

                // Отправляем уведомление через SignalR
                await SendTestCompletedNotificationAsync(testResult, "punctuation", currentUser.FullName ?? currentUser.Email ?? "Студент");

                return RedirectToAction("PunctuationResult", new { id = testResult.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при завершении теста по пунктуации. ResultId: {ResultId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при завершении теста.";
                return RedirectToAction("Index");
            }
        }

        // GET: StudentTest/StartOrthoeopy/{id}
        public async Task<IActionResult> StartOrthoeopy(int id)
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
                    TempData["ErrorMessage"] = "Профиль студента не найден.";
                    return RedirectToAction("Index");
                }

                // Используем сервис для начала теста
                var testResult = await _studentTestService.StartOrthoeopyTestAsync(student.Id, id);
                
                _logger.LogInformation("Студент {StudentId} начал тест по орфоэпии {TestId}", student.Id, id);
                
                // Отправляем уведомление через SignalR
                await SendTestStartedNotificationAsync(testResult, "orthoeopy", currentUser.FullName ?? currentUser.Email ?? "Студент");
                
                return RedirectToAction(nameof(TakeOrthoeopy), new { id = testResult.Id });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Попытка доступа к недоступному тесту. TestId: {TestId}", id);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при начале теста по орфоэпии. TestId: {TestId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при начале теста. Попробуйте позже.";
                return RedirectToAction("Index");
            }
        }

        // GET: StudentTest/TakeOrthoeopy/{id}
        public async Task<IActionResult> TakeOrthoeopy(int id)
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
                    return NotFound();
                }

                // Получаем результат теста через репозиторий
                var testResult = await _orthoeopyTestResultRepository.GetByIdAsync(id);
                
                if (testResult == null || testResult.StudentId != student.Id)
                {
                    return NotFound();
                }

                // Проверяем, не завершен ли тест
                if (testResult.IsCompleted)
                {
                    return RedirectToAction("OrthoeopyResult", new { id = testResult.Id });
                }

                // Загружаем тест
                var test = await _orthoeopyTestRepository.GetByIdAsync(testResult.OrthoeopyTestId);
                if (test == null)
                {
                    return NotFound();
                }

                // Загружаем вопросы
                var questions = await _orthoeopyQuestionRepository.GetByTestIdOrderedAsync(test.Id);
                if (!questions.Any())
                {
                    TempData["ErrorMessage"] = "В тесте нет вопросов.";
                    return RedirectToAction("Index");
                }

                // Загружаем ответы
                var answers = await _answerService.GetOrthoeopyAnswersAsync(testResult.Id);

                // Вычисляем оставшееся время
                // Если есть сохраненное время (пауза), используем его, иначе вычисляем на основе StartedAt
                TimeSpan timeRemaining;
                if (testResult.TimeRemainingSeconds.HasValue && testResult.TimeRemainingSeconds.Value > 0)
                {
                    // Используем сохраненное время (тест был на паузе)
                    timeRemaining = TimeSpan.FromSeconds(testResult.TimeRemainingSeconds.Value);
                }
                else
                {
                    // Вычисляем время на основе StartedAt
                    var timeElapsed = DateTime.Now - testResult.StartedAt;
                    var timeLimit = TimeSpan.FromMinutes(test.TimeLimit);
                    timeRemaining = timeLimit - timeElapsed;
                    
                    // Сохраняем вычисленное время для будущих пауз
                    if (timeRemaining > TimeSpan.Zero)
                    {
                        testResult.TimeRemainingSeconds = (int)timeRemaining.TotalSeconds;
                        await _orthoeopyTestResultRepository.UpdateAsync(testResult);
                    }
                }

                // Если время истекло, завершаем тест автоматически
                if (timeRemaining <= TimeSpan.Zero)
                {
                    // Вычисляем результат
                    var (score, maxScore, percentage) = await _testEvaluationService.CalculateOrthoeopyTestResultAsync(testResult.Id, test.Id);
                    
                    // Вычисляем оценку
                    var grade = TestEvaluationService.CalculateGrade(percentage);
                    
                    // Обновляем результат теста
                    testResult.Score = score;
                    testResult.MaxScore = maxScore;
                    testResult.Percentage = percentage;
                    testResult.Grade = grade;
                    
                    // Завершаем тест
                    await _testResultService.CompleteTestResultAsync(testResult);
                    
                    return RedirectToAction("OrthoeopyResult", new { id = testResult.Id });
                }

                var viewModel = new TakeOrthoeopyTestViewModel
                {
                    TestResult = testResult,
                    Test = test,
                    Questions = questions,
                    Answers = answers,
                    TimeRemaining = timeRemaining,
                    CurrentQuestionIndex = 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке теста по орфоэпии. ResultId: {ResultId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке теста.";
                return RedirectToAction("Index");
            }
        }

        // POST: StudentTest/SubmitOrthoeopyAnswer
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SubmitOrthoeopyAnswer([FromBody] SubmitOrthoeopyAnswerViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Пользователь не авторизован" });
                }

                var student = await _studentRepository.GetByUserIdAsync(currentUser.Id);
                if (student == null)
                {
                    return Json(new { success = false, message = "Студент не найден" });
                }

                // Получаем результат теста
                var testResult = await _orthoeopyTestResultRepository.GetByIdAsync(model.TestResultId);
                if (testResult == null || testResult.StudentId != student.Id || testResult.IsCompleted)
                {
                    return Json(new { success = false, message = "Тест не найден или уже завершен" });
                }

                // Получаем вопрос
                var question = await _orthoeopyQuestionRepository.GetByIdAsync(model.QuestionId);
                if (question == null || question.OrthoeopyTestId != testResult.OrthoeopyTestId)
                {
                    return Json(new { success = false, message = "Вопрос не найден" });
                }

                // Сохраняем ответ
                var answer = await _answerService.SaveOrthoeopyAnswerAsync(
                    testResult.Id, 
                    question.Id, 
                    model.SelectedStressPosition);

                // Оцениваем ответ
                var (isCorrect, points) = await _testEvaluationService.EvaluateOrthoeopyAnswerAsync(
                    question, 
                    model.SelectedStressPosition, 
                    question.Points);

                // Обновляем ответ с результатом оценки
                answer.IsCorrect = isCorrect;
                answer.Points = points;
                await _answerService.UpdateAnswerAsync(answer);

                return Json(new 
                { 
                    success = true, 
                    isCorrect = isCorrect,
                    points = points
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении ответа. QuestionId: {QuestionId}, TestResultId: {TestResultId}", 
                    model.QuestionId, model.TestResultId);
                return Json(new { success = false, message = "Произошла ошибка при сохранении ответа" });
            }
        }

        // POST: StudentTest/CompleteOrthoeopyTest/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrthoeopyTest(int id)
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
                    return NotFound();
                }

                // Получаем результат теста через репозиторий
                var testResult = await _orthoeopyTestResultRepository.GetByIdAsync(id);
                
                if (testResult == null || testResult.StudentId != student.Id)
                {
                    return NotFound();
                }

                if (testResult.IsCompleted)
                {
                    return RedirectToAction("OrthoeopyResult", new { id = testResult.Id });
                }

                // Вычисляем результат
                var (score, maxScore, percentage) = await _testEvaluationService.CalculateOrthoeopyTestResultAsync(testResult.Id, testResult.OrthoeopyTestId);
                
                // Вычисляем оценку
                var grade = TestEvaluationService.CalculateGrade(percentage);
                
                // Обновляем результат теста
                testResult.Score = score;
                testResult.MaxScore = maxScore;
                testResult.Percentage = percentage;
                testResult.Grade = grade;
                
                // Завершаем тест (устанавливает CompletedAt и IsCompleted)
                await _testResultService.CompleteTestResultAsync(testResult);

                _logger.LogInformation("Студент {StudentId} завершил тест по орфоэпии {ResultId}. Баллы: {Score}/{MaxScore}, Процент: {Percentage}",
                    student.Id, testResult.Id, testResult.Score, testResult.MaxScore, testResult.Percentage);

                // Отправляем уведомление через SignalR
                await SendTestCompletedNotificationAsync(testResult, "orthoeopy", currentUser.FullName ?? currentUser.Email ?? "Студент");

                return RedirectToAction("OrthoeopyResult", new { id = testResult.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при завершении теста по орфоэпии. ResultId: {ResultId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при завершении теста.";
                return RedirectToAction("Index");
            }
        }

        // GET: StudentTest/StartRegular/{id}
        public async Task<IActionResult> StartRegular(int id)
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
                    TempData["ErrorMessage"] = "Профиль студента не найден.";
                    return RedirectToAction("Index");
                }

                // Используем сервис для начала теста
                var testResult = await _studentTestService.StartRegularTestAsync(student.Id, id);
                
                _logger.LogInformation("Студент {StudentId} начал классический тест {TestId}", student.Id, id);
                
                // Отправляем уведомление через SignalR
                await SendTestStartedNotificationAsync(testResult, "regular", currentUser.FullName ?? currentUser.Email ?? "Студент");
                
                return RedirectToAction(nameof(TakeRegular), new { id = testResult.Id });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Попытка доступа к недоступному тесту. TestId: {TestId}", id);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при начале классического теста. TestId: {TestId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при начале теста. Попробуйте позже.";
                return RedirectToAction("Index");
            }
        }

        // GET: StudentTest/TakeRegular/{id}
        public async Task<IActionResult> TakeRegular(int id)
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
                    return NotFound();
                }

                // Получаем результат теста через репозиторий
                var testResult = await _regularTestResultRepository.GetByIdAsync(id);
                
                if (testResult == null || testResult.StudentId != student.Id)
                {
                    return NotFound();
                }

                // Проверяем, не завершен ли тест
                if (testResult.IsCompleted)
                {
                    return RedirectToAction("RegularResult", new { id = testResult.Id });
                }

                // Загружаем тест
                var test = await _regularTestRepository.GetByIdAsync(testResult.RegularTestId);
                if (test == null)
                {
                    return NotFound();
                }

                // Загружаем вопросы
                var questions = await _regularQuestionRepository.GetByTestIdOrderedAsync(test.Id);
                if (!questions.Any())
                {
                    TempData["ErrorMessage"] = "В тесте нет вопросов.";
                    return RedirectToAction("Index");
                }

                // Загружаем опции для всех вопросов
                var options = new List<RegularQuestionOption>();
                foreach (var question in questions)
                {
                    var questionOptions = await _regularQuestionOptionRepository.GetByQuestionIdOrderedAsync(question.Id);
                    options.AddRange(questionOptions);
                }

                // Загружаем ответы
                var answers = await _answerService.GetRegularAnswersAsync(testResult.Id);

                // Вычисляем оставшееся время
                // Если есть сохраненное время (пауза), используем его, иначе вычисляем на основе StartedAt
                TimeSpan timeRemaining;
                if (testResult.TimeRemainingSeconds.HasValue && testResult.TimeRemainingSeconds.Value > 0)
                {
                    // Используем сохраненное время (тест был на паузе)
                    timeRemaining = TimeSpan.FromSeconds(testResult.TimeRemainingSeconds.Value);
                }
                else
                {
                    // Вычисляем время на основе StartedAt
                    var timeElapsed = DateTime.Now - testResult.StartedAt;
                    var timeLimit = TimeSpan.FromMinutes(test.TimeLimit);
                    timeRemaining = timeLimit - timeElapsed;
                    
                    // Сохраняем вычисленное время для будущих пауз
                    if (timeRemaining > TimeSpan.Zero)
                    {
                        testResult.TimeRemainingSeconds = (int)timeRemaining.TotalSeconds;
                        await _regularTestResultRepository.UpdateAsync(testResult);
                    }
                }

                // Если время истекло, завершаем тест автоматически
                if (timeRemaining <= TimeSpan.Zero)
                {
                    // Вычисляем результат
                    var (score, maxScore, percentage) = await _testEvaluationService.CalculateRegularTestResultAsync(testResult.Id, test.Id);
                    
                    // Вычисляем оценку
                    var grade = TestEvaluationService.CalculateGrade(percentage);
                    
                    // Обновляем результат теста
                    testResult.Score = score;
                    testResult.MaxScore = maxScore;
                    testResult.Percentage = percentage;
                    testResult.Grade = grade;
                    
                    // Завершаем тест
                    await _testResultService.CompleteTestResultAsync(testResult);
                    
                    return RedirectToAction("RegularResult", new { id = testResult.Id });
                }

                var viewModel = new TakeRegularTestViewModel
                {
                    TestResult = testResult,
                    Test = test,
                    Questions = questions,
                    Options = options,
                    Answers = answers,
                    TimeRemaining = timeRemaining,
                    CurrentQuestionIndex = 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке классического теста. ResultId: {ResultId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке теста.";
                return RedirectToAction("Index");
            }
        }

        // POST: StudentTest/SubmitRegularAnswer
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SubmitRegularAnswer([FromBody] SubmitRegularAnswerViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Пользователь не авторизован" });
                }

                var student = await _studentRepository.GetByUserIdAsync(currentUser.Id);
                if (student == null)
                {
                    return Json(new { success = false, message = "Студент не найден" });
                }

                // Получаем результат теста
                var testResult = await _regularTestResultRepository.GetByIdAsync(model.TestResultId);
                if (testResult == null || testResult.StudentId != student.Id || testResult.IsCompleted)
                {
                    return Json(new { success = false, message = "Тест не найден или уже завершен" });
                }

                // Получаем вопрос
                var question = await _regularQuestionRepository.GetByIdAsync(model.QuestionId);
                if (question == null || question.RegularTestId != testResult.RegularTestId)
                {
                    return Json(new { success = false, message = "Вопрос не найден" });
                }

                // Обрабатываем ответ в зависимости от типа вопроса
                string? studentAnswerStr = null;
                int? selectedOptionId = null;

                if (question.Type == QuestionType.MultipleChoice && model.SelectedOptionIds != null && model.SelectedOptionIds.Any())
                {
                    // Для множественного выбора сохраняем ID через запятую в StudentAnswer
                    var sortedIds = new List<int>(model.SelectedOptionIds);
                    sortedIds.Sort();
                    studentAnswerStr = string.Join(",", sortedIds);
                }
                else if (model.SelectedOptionId.HasValue)
                {
                    // Для одиночного выбора и TrueFalse
                    selectedOptionId = model.SelectedOptionId.Value;
                }
                else if (!string.IsNullOrEmpty(model.StudentAnswer))
                {
                    // Для текстовых ответов
                    studentAnswerStr = model.StudentAnswer;
                }

                // Сохраняем ответ
                var answer = await _answerService.SaveRegularAnswerAsync(
                    testResult.Id, 
                    question.Id, 
                    studentAnswerStr, 
                    selectedOptionId);

                // Оцениваем ответ
                var (isCorrect, points) = await _testEvaluationService.EvaluateRegularAnswerAsync(
                    question, 
                    studentAnswerStr, 
                    selectedOptionId, 
                    question.Points);

                // Обновляем ответ с результатом оценки
                answer.IsCorrect = isCorrect;
                answer.Points = points;
                await _answerService.UpdateAnswerAsync(answer);

                return Json(new 
                { 
                    success = true, 
                    isCorrect = isCorrect,
                    points = points
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении ответа. QuestionId: {QuestionId}, TestResultId: {TestResultId}", 
                    model.QuestionId, model.TestResultId);
                return Json(new { success = false, message = "Произошла ошибка при сохранении ответа" });
            }
        }

        // POST: StudentTest/CompleteRegularTest/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteRegularTest(int id)
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
                    return NotFound();
                }

                // Получаем результат теста через репозиторий
                var testResult = await _regularTestResultRepository.GetByIdAsync(id);
                
                if (testResult == null || testResult.StudentId != student.Id)
                {
                    return NotFound();
                }

                if (testResult.IsCompleted)
                {
                    return RedirectToAction("RegularResult", new { id = testResult.Id });
                }

                // Вычисляем результат
                var (score, maxScore, percentage) = await _testEvaluationService.CalculateRegularTestResultAsync(testResult.Id, testResult.RegularTestId);
                
                // Вычисляем оценку
                var grade = TestEvaluationService.CalculateGrade(percentage);
                
                // Обновляем результат теста
                testResult.Score = score;
                testResult.MaxScore = maxScore;
                testResult.Percentage = percentage;
                testResult.Grade = grade;
                
                // Завершаем тест (устанавливает CompletedAt и IsCompleted)
                await _testResultService.CompleteTestResultAsync(testResult);

                _logger.LogInformation("Студент {StudentId} завершил классический тест {ResultId}. Баллы: {Score}/{MaxScore}, Процент: {Percentage}",
                    student.Id, testResult.Id, testResult.Score, testResult.MaxScore, testResult.Percentage);

                // Отправляем уведомление через SignalR
                await SendTestCompletedNotificationAsync(testResult, "regular", currentUser.FullName ?? currentUser.Email ?? "Студент");

                return RedirectToAction("RegularResult", new { id = testResult.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при завершении классического теста. ResultId: {ResultId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при завершении теста.";
                return RedirectToAction("Index");
            }
        }

        // GET: StudentTest/SpellingResult/{id}
        public async Task<IActionResult> SpellingResult(int id)
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
                    return NotFound();
                }

                var testResult = await _spellingTestResultRepository.GetByIdAsync(id);
                if (testResult == null || testResult.StudentId != student.Id)
                {
                    return NotFound();
                }

                if (!testResult.IsCompleted)
                {
                    return RedirectToAction(nameof(TakeSpelling), new { id = testResult.Id });
                }

                // Загружаем тест
                var test = await _spellingTestRepository.GetByIdAsync(testResult.SpellingTestId);
                if (test == null)
                {
                    return NotFound();
                }

                // Загружаем вопросы
                var questions = await _spellingQuestionRepository.GetByTestIdOrderedAsync(test.Id);
                
                // Загружаем ответы
                var answers = await _answerService.GetSpellingAnswersAsync(testResult.Id);

                var viewModel = new SpellingTestResultViewModel
                {
                    TestResult = testResult,
                    Test = test,
                    Questions = questions,
                    Answers = answers,
                    TestTitle = test.Title,
                    Score = testResult.Score,
                    MaxScore = testResult.MaxScore,
                    Percentage = testResult.Percentage,
                    Grade = testResult.Grade ?? 0,
                    CompletedAt = testResult.CompletedAt,
                    StartedAt = testResult.StartedAt,
                    Duration = testResult.CompletedAt.HasValue ? testResult.CompletedAt.Value - testResult.StartedAt : TimeSpan.Zero,
                    StudentName = currentUser.FullName ?? currentUser.Email ?? "",
                    ShowCorrectAnswers = test.ShowCorrectAnswers,
                    TestIcon = "fa-spell-check",
                    TestColor = "primary",
                    AttemptNumber = testResult.AttemptNumber
                };

                return View("SpellingResult", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке результата теста по орфографии. ResultId: {ResultId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке результата.";
                return RedirectToAction("Index");
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

        // GET: StudentTest/PunctuationResult/{id}
        public async Task<IActionResult> PunctuationResult(int id)
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
                    return NotFound();
                }

                var testResult = await _punctuationTestResultRepository.GetByIdAsync(id);
                if (testResult == null || testResult.StudentId != student.Id)
                {
                    return NotFound();
                }

                if (!testResult.IsCompleted)
                {
                    return RedirectToAction(nameof(TakePunctuation), new { id = testResult.Id });
                }

                // Загружаем тест
                var test = await _punctuationTestRepository.GetByIdAsync(testResult.PunctuationTestId);
                if (test == null)
                {
                    return NotFound();
                }

                // Загружаем вопросы
                var questions = await _punctuationQuestionRepository.GetByTestIdOrderedAsync(test.Id);
                
                // Загружаем ответы
                var answers = await _answerService.GetPunctuationAnswersAsync(testResult.Id);

                var viewModel = new PunctuationTestResultViewModel
                {
                    TestResult = testResult,
                    Test = test,
                    Questions = questions,
                    Answers = answers,
                    TestTitle = test.Title,
                    Score = testResult.Score,
                    MaxScore = testResult.MaxScore,
                    Percentage = testResult.Percentage,
                    Grade = testResult.Grade ?? 0,
                    CompletedAt = testResult.CompletedAt,
                    StartedAt = testResult.StartedAt,
                    Duration = testResult.CompletedAt.HasValue ? testResult.CompletedAt.Value - testResult.StartedAt : TimeSpan.Zero,
                    StudentName = currentUser.FullName ?? currentUser.Email ?? "",
                    ShowCorrectAnswers = test.ShowCorrectAnswers,
                    TestIcon = "fa-exclamation",
                    TestColor = "warning",
                    AttemptNumber = testResult.AttemptNumber
                };

                return View("PunctuationResult", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке результата теста по пунктуации. ResultId: {ResultId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке результата.";
                return RedirectToAction("Index");
            }
        }

        // GET: StudentTest/OrthoeopyResult/{id}
        public async Task<IActionResult> OrthoeopyResult(int id)
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
                    return NotFound();
                }

                var testResult = await _orthoeopyTestResultRepository.GetByIdAsync(id);
                if (testResult == null || testResult.StudentId != student.Id)
                {
                    return NotFound();
                }

                if (!testResult.IsCompleted)
                {
                    return RedirectToAction(nameof(TakeOrthoeopy), new { id = testResult.Id });
                }

                // Загружаем тест
                var test = await _orthoeopyTestRepository.GetByIdAsync(testResult.OrthoeopyTestId);
                if (test == null)
                {
                    return NotFound();
                }

                // Загружаем вопросы
                var questions = await _orthoeopyQuestionRepository.GetByTestIdOrderedAsync(test.Id);
                
                // Загружаем ответы
                var answers = await _answerService.GetOrthoeopyAnswersAsync(testResult.Id);

                var viewModel = new OrthoeopyTestResultViewModel
                {
                    TestResult = testResult,
                    Test = test,
                    Questions = questions,
                    Answers = answers,
                    TestTitle = test.Title,
                    Score = testResult.Score,
                    MaxScore = testResult.MaxScore,
                    Percentage = testResult.Percentage,
                    Grade = testResult.Grade ?? 0,
                    CompletedAt = testResult.CompletedAt,
                    StartedAt = testResult.StartedAt,
                    Duration = testResult.CompletedAt.HasValue ? testResult.CompletedAt.Value - testResult.StartedAt : TimeSpan.Zero,
                    StudentName = currentUser.FullName ?? currentUser.Email ?? "",
                    ShowCorrectAnswers = test.ShowCorrectAnswers,
                    TestIcon = "fa-volume-up",
                    TestColor = "success",
                    AttemptNumber = testResult.AttemptNumber
                };

                return View("OrthoeopyResult", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке результата теста по орфоэпии. ResultId: {ResultId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке результата.";
                return RedirectToAction("Index");
            }
        }

        // GET: StudentTest/RegularResult/{id}
        public async Task<IActionResult> RegularResult(int id)
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
                    return NotFound();
                }

                var testResult = await _regularTestResultRepository.GetByIdAsync(id);
                if (testResult == null || testResult.StudentId != student.Id)
                {
                    return NotFound();
                }

                if (!testResult.IsCompleted)
                {
                    return RedirectToAction(nameof(TakeRegular), new { id = testResult.Id });
                }

                // Загружаем тест
                var test = await _regularTestRepository.GetByIdAsync(testResult.RegularTestId);
                if (test == null)
                {
                    return NotFound();
                }

                // Загружаем вопросы
                var questions = await _regularQuestionRepository.GetByTestIdOrderedAsync(test.Id);
                
                // Загружаем опции для всех вопросов
                var options = new List<RegularQuestionOption>();
                foreach (var question in questions)
                {
                    var questionOptions = await _regularQuestionOptionRepository.GetByQuestionIdOrderedAsync(question.Id);
                    options.AddRange(questionOptions);
                }
                
                // Загружаем ответы
                var answers = await _answerService.GetRegularAnswersAsync(testResult.Id);

                var viewModel = new RegularTestResultViewModel
                {
                    TestResult = testResult,
                    Test = test,
                    Questions = questions,
                    Options = options,
                    Answers = answers,
                    TestTitle = test.Title,
                    Score = testResult.Score,
                    MaxScore = testResult.MaxScore,
                    Percentage = testResult.Percentage,
                    Grade = testResult.Grade ?? 0,
                    CompletedAt = testResult.CompletedAt,
                    StartedAt = testResult.StartedAt,
                    Duration = testResult.CompletedAt.HasValue ? testResult.CompletedAt.Value - testResult.StartedAt : TimeSpan.Zero,
                    StudentName = currentUser.FullName ?? currentUser.Email ?? "",
                    ShowCorrectAnswers = test.ShowCorrectAnswers,
                    TestIcon = "fa-list-ul",
                    TestColor = "info",
                    AttemptNumber = testResult.AttemptNumber
                };

                return View("RegularResult", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке результата классического теста. ResultId: {ResultId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке результата.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Отправка уведомления о завершении теста через SignalR
        /// </summary>
        private async Task SendTestCompletedNotificationAsync(TestResult testResult, string testType, string studentName)
        {
            try
            {
                // Получаем тест для определения TeacherId
                string? teacherId = null;
                string? testTitle = null;
                int testId = 0;

                switch (testType)
                {
                    case "spelling":
                        if (testResult is SpellingTestResult spellingResult)
                        {
                            testId = spellingResult.SpellingTestId;
                            var spellingTest = await _spellingTestRepository.GetByIdAsync(testId);
                            if (spellingTest != null)
                            {
                                teacherId = spellingTest.TeacherId;
                                testTitle = spellingTest.Title;
                            }
                        }
                        break;
                    case "punctuation":
                        if (testResult is PunctuationTestResult punctuationResult)
                        {
                            testId = punctuationResult.PunctuationTestId;
                            var punctuationTest = await _punctuationTestRepository.GetByIdAsync(testId);
                            if (punctuationTest != null)
                            {
                                teacherId = punctuationTest.TeacherId;
                                testTitle = punctuationTest.Title;
                            }
                        }
                        break;
                    case "orthoeopy":
                        if (testResult is OrthoeopyTestResult orthoeopyResult)
                        {
                            testId = orthoeopyResult.OrthoeopyTestId;
                            var orthoeopyTest = await _orthoeopyTestRepository.GetByIdAsync(testId);
                            if (orthoeopyTest != null)
                            {
                                teacherId = orthoeopyTest.TeacherId;
                                testTitle = orthoeopyTest.Title;
                            }
                        }
                        break;
                    case "regular":
                        if (testResult is RegularTestResult regularResult)
                        {
                            testId = regularResult.RegularTestId;
                            var regularTest = await _regularTestRepository.GetByIdAsync(testId);
                            if (regularTest != null)
                            {
                                teacherId = regularTest.TeacherId;
                                testTitle = regularTest.Title;
                            }
                        }
                        break;
                }

                if (string.IsNullOrEmpty(teacherId) || string.IsNullOrEmpty(testTitle) || testId == 0)
                {
                    _logger.LogWarning("Не удалось определить TeacherId или TestTitle для отправки уведомления. TestType: {TestType}, TestResultId: {TestResultId}", 
                        testType, testResult.Id);
                    return;
                }

                var notificationData = new
                {
                    testId = testId,
                    testTitle = testTitle,
                    testType = testType,
                    studentId = testResult.StudentId,
                    studentName = studentName,
                    score = testResult.Score,
                    maxScore = testResult.MaxScore,
                    percentage = testResult.Percentage,
                    timestamp = DateTime.Now,
                    action = "completed",
                    isAutoCompleted = false,
                    testResultId = testResult.Id
                };

                await _hubContext.Clients.Group($"teacher_{teacherId}")
                    .SendAsync("StudentTestActivity", notificationData);

                _logger.LogInformation("SignalR: Отправлено уведомление о завершении теста {TestType} {TestId} студентом {StudentName}",
                    testType, testId, studentName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки SignalR уведомления о завершении теста");
            }
        }

        /// <summary>
        /// Отправка уведомления о начале теста через SignalR
        /// </summary>
        private async Task SendTestStartedNotificationAsync(TestResult testResult, string testType, string studentName)
        {
            try
            {
                // Получаем тест для определения TeacherId
                string? teacherId = null;
                string? testTitle = null;
                int testId = 0;

                switch (testType)
                {
                    case "spelling":
                        if (testResult is SpellingTestResult spellingResult)
                        {
                            testId = spellingResult.SpellingTestId;
                            var spellingTest = await _spellingTestRepository.GetByIdAsync(testId);
                            if (spellingTest != null)
                            {
                                teacherId = spellingTest.TeacherId;
                                testTitle = spellingTest.Title;
                            }
                        }
                        break;
                    case "punctuation":
                        if (testResult is PunctuationTestResult punctuationResult)
                        {
                            testId = punctuationResult.PunctuationTestId;
                            var punctuationTest = await _punctuationTestRepository.GetByIdAsync(testId);
                            if (punctuationTest != null)
                            {
                                teacherId = punctuationTest.TeacherId;
                                testTitle = punctuationTest.Title;
                            }
                        }
                        break;
                    case "orthoeopy":
                        if (testResult is OrthoeopyTestResult orthoeopyResult)
                        {
                            testId = orthoeopyResult.OrthoeopyTestId;
                            var orthoeopyTest = await _orthoeopyTestRepository.GetByIdAsync(testId);
                            if (orthoeopyTest != null)
                            {
                                teacherId = orthoeopyTest.TeacherId;
                                testTitle = orthoeopyTest.Title;
                            }
                        }
                        break;
                    case "regular":
                        if (testResult is RegularTestResult regularResult)
                        {
                            testId = regularResult.RegularTestId;
                            var regularTest = await _regularTestRepository.GetByIdAsync(testId);
                            if (regularTest != null)
                            {
                                teacherId = regularTest.TeacherId;
                                testTitle = regularTest.Title;
                            }
                        }
                        break;
                }

                if (string.IsNullOrEmpty(teacherId) || string.IsNullOrEmpty(testTitle) || testId == 0)
                {
                    _logger.LogWarning("Не удалось определить TeacherId или TestTitle для отправки уведомления о начале теста. TestType: {TestType}, TestResultId: {TestResultId}", 
                        testType, testResult.Id);
                    return;
                }

                var notificationData = new
                {
                    testId = testId,
                    testTitle = testTitle,
                    testType = testType,
                    studentId = testResult.StudentId,
                    studentName = studentName,
                    score = 0,
                    maxScore = 0,
                    percentage = 0.0,
                    timestamp = DateTime.Now,
                    action = "started",
                    isAutoCompleted = false,
                    testResultId = testResult.Id
                };

                await _hubContext.Clients.Group($"teacher_{teacherId}")
                    .SendAsync("StudentTestActivity", notificationData);

                _logger.LogInformation("SignalR: Отправлено уведомление о начале теста {TestType} {TestId} студентом {StudentName}",
                    testType, testId, studentName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки SignalR уведомления о начале теста");
            }
        }
    }
}

