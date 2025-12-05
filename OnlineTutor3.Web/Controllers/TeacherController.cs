using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Teacher)]
    public class TeacherController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly IClassService _classService;
        private readonly IStudentService _studentService;
        private readonly IAssignmentService _assignmentService;
        private readonly ISubjectService _subjectService;
        private readonly ISpellingTestService _spellingTestService;
        private readonly IPunctuationTestService _punctuationTestService;
        private readonly IOrthoeopyTestService _orthoeopyTestService;
        private readonly IRegularTestService _regularTestService;
        private readonly ISpellingTestResultRepository _spellingTestResultRepository;
        private readonly IPunctuationTestResultRepository _punctuationTestResultRepository;
        private readonly IOrthoeopyTestResultRepository _orthoeopyTestResultRepository;
        private readonly IRegularTestResultRepository _regularTestResultRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(
            ITeacherService teacherService,
            IClassService classService,
            IStudentService studentService,
            IAssignmentService assignmentService,
            ISubjectService subjectService,
            ISpellingTestService spellingTestService,
            IPunctuationTestService punctuationTestService,
            IOrthoeopyTestService orthoeopyTestService,
            IRegularTestService regularTestService,
            ISpellingTestResultRepository spellingTestResultRepository,
            IPunctuationTestResultRepository punctuationTestResultRepository,
            IOrthoeopyTestResultRepository orthoeopyTestResultRepository,
            IRegularTestResultRepository regularTestResultRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<TeacherController> logger)
        {
            _teacherService = teacherService;
            _classService = classService;
            _studentService = studentService;
            _assignmentService = assignmentService;
            _subjectService = subjectService;
            _spellingTestService = spellingTestService;
            _punctuationTestService = punctuationTestService;
            _orthoeopyTestService = orthoeopyTestService;
            _regularTestService = regularTestService;
            _spellingTestResultRepository = spellingTestResultRepository;
            _punctuationTestResultRepository = punctuationTestResultRepository;
            _orthoeopyTestResultRepository = orthoeopyTestResultRepository;
            _regularTestResultRepository = regularTestResultRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var teacher = await _teacherService.GetByUserIdAsync(currentUser.Id);
                if (teacher == null)
                {
                    TempData["ErrorMessage"] = "Профиль учителя не найден. Обратитесь к администратору.";
                    return RedirectToAction("Index", "Home");
                }

                var classes = await _classService.GetActiveByTeacherIdAsync(currentUser.Id);
                var students = await _studentService.GetByTeacherIdAsync(currentUser.Id);
                var assignments = await _assignmentService.GetByTeacherSubjectsAsync(currentUser.Id);
                var activeAssignments = assignments.Where(a => a.IsActive).ToList();

                var allSubjects = await _subjectService.GetAllAsync();
                var subjectsDict = allSubjects.ToDictionary(s => s.Id, s => s.Name);
                var spellingTests = await _spellingTestService.GetActiveByTeacherIdAsync(currentUser.Id);
                var punctuationTests = await _punctuationTestService.GetActiveByTeacherIdAsync(currentUser.Id);
                var orthoeopyTests = await _orthoeopyTestService.GetActiveByTeacherIdAsync(currentUser.Id);
                var regularTests = await _regularTestService.GetActiveByTeacherIdAsync(currentUser.Id);
                var totalActiveTests = spellingTests.Count() + punctuationTests.Count() + 
                                      orthoeopyTests.Count() + regularTests.Count();

                List<RecentTestCompletionViewModel> recentCompletions = new List<RecentTestCompletionViewModel>();
                try
                {
                    recentCompletions = await GetRecentTestCompletionsAsync(currentUser.Id, subjectsDict);
                }
                catch (Exception ex)
                {
                }

                var viewModel = new TeacherIndexViewModel
                {
                    Teacher = currentUser,
                    TotalClasses = classes.Count,
                    TotalStudents = students.Count,
                    TotalActiveAssignments = activeAssignments.Count,
                    TotalActiveTests = totalActiveTests,
                    RecentCompletions = recentCompletions,
                    SubjectsDict = subjectsDict
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке главной страницы учителя");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке данных. Попробуйте обновить страницу.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Получает последние 10 завершенных прохождений тестов учениками учителя
        /// Собирает все завершенные результаты всех тестов учителя и возвращает последние 10 по дате завершения
        /// </summary>
        private async Task<List<RecentTestCompletionViewModel>> GetRecentTestCompletionsAsync(
            string teacherId, 
            Dictionary<int, string> subjectsDict)
        {
            var completions = new List<RecentTestCompletionViewModel>();

            try
            {
                var spellingTests = await _spellingTestService.GetByTeacherIdAsync(teacherId);
                var punctuationTests = await _punctuationTestService.GetByTeacherIdAsync(teacherId);
                var orthoeopyTests = await _orthoeopyTestService.GetByTeacherIdAsync(teacherId);
                var regularTests = await _regularTestService.GetByTeacherIdAsync(teacherId);

                var allAssignments = await _assignmentService.GetByTeacherSubjectsAsync(teacherId);
                var assignmentsDict = allAssignments.ToDictionary(a => a.Id);

                var allStudents = await _studentService.GetByTeacherIdAsync(teacherId);
                var studentsDict = allStudents.ToDictionary(s => s.Id);

                var allClasses = await _classService.GetActiveByTeacherIdAsync(teacherId);
                var classesDict = allClasses.ToDictionary(c => c.Id);

                // Обрабатываем все тесты по орфографии (без ограничения количества)
                foreach (var test in spellingTests)
                {
                    try
                    {
                        var testResults = await _spellingTestResultRepository.GetByTestIdAsync(test.Id);
                        // Берем все завершенные результаты (без ограничения Take(5))
                        var completedResults = testResults
                            .Where(r => r.IsCompleted && r.CompletedAt.HasValue)
                            .ToList();

                        var assignment = assignmentsDict.ContainsKey(test.AssignmentId) 
                            ? assignmentsDict[test.AssignmentId] 
                            : null;
                        var subjectName = assignment != null && subjectsDict.ContainsKey(assignment.SubjectId)
                            ? subjectsDict[assignment.SubjectId]
                            : "Не указан";

                        foreach (var result in completedResults)
                        {
                            if (studentsDict.TryGetValue(result.StudentId, out var student) && student != null)
                            {
                                try
                                {
                                    var user = await _userManager.FindByIdAsync(student.UserId);
                                    var className = student.ClassId.HasValue && classesDict.ContainsKey(student.ClassId.Value)
                                        ? classesDict[student.ClassId.Value]?.Name 
                                        : null;

                                    completions.Add(new RecentTestCompletionViewModel
                                    {
                                        TestResultId = result.Id,
                                        TestId = result.SpellingTestId,
                                        TestTitle = test.Title,
                                        TestType = "spelling",
                                        StudentId = result.StudentId,
                                        StudentName = user?.FullName ?? "Неизвестный студент",
                                        ClassName = className,
                                        Score = result.Score,
                                        MaxScore = result.MaxScore,
                                        Percentage = result.Percentage,
                                        Grade = result.Grade,
                                        CompletedAt = result.CompletedAt!.Value,
                                        SubjectName = subjectName
                                    });
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }

                // Обрабатываем все тесты по пунктуации (без ограничения количества)
                foreach (var test in punctuationTests)
                {
                    try
                    {
                        var testResults = await _punctuationTestResultRepository.GetByTestIdAsync(test.Id);
                        // Берем все завершенные результаты (без ограничения Take(5))
                        var completedResults = testResults
                            .Where(r => r.IsCompleted && r.CompletedAt.HasValue)
                            .ToList();

                        var assignment = assignmentsDict.ContainsKey(test.AssignmentId) 
                            ? assignmentsDict[test.AssignmentId] 
                            : null;
                        var subjectName = assignment != null && subjectsDict.ContainsKey(assignment.SubjectId)
                            ? subjectsDict[assignment.SubjectId]
                            : "Не указан";

                        foreach (var result in completedResults)
                        {
                            if (studentsDict.TryGetValue(result.StudentId, out var student) && student != null)
                            {
                                try
                                {
                                    var user = await _userManager.FindByIdAsync(student.UserId);
                                    var className = student.ClassId.HasValue && classesDict.ContainsKey(student.ClassId.Value)
                                        ? classesDict[student.ClassId.Value]?.Name 
                                        : null;

                                    completions.Add(new RecentTestCompletionViewModel
                                    {
                                        TestResultId = result.Id,
                                        TestId = result.PunctuationTestId,
                                        TestTitle = test.Title,
                                        TestType = "punctuation",
                                        StudentId = result.StudentId,
                                        StudentName = user?.FullName ?? "Неизвестный студент",
                                        ClassName = className,
                                        Score = result.Score,
                                        MaxScore = result.MaxScore,
                                        Percentage = result.Percentage,
                                        Grade = result.Grade,
                                        CompletedAt = result.CompletedAt!.Value,
                                        SubjectName = subjectName
                                    });
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }

                // Обрабатываем все тесты по орфоэпии (без ограничения количества)
                foreach (var test in orthoeopyTests)
                {
                    try
                    {
                        var testResults = await _orthoeopyTestResultRepository.GetByTestIdAsync(test.Id);
                        // Берем все завершенные результаты (без ограничения Take(5))
                        var completedResults = testResults
                            .Where(r => r.IsCompleted && r.CompletedAt.HasValue)
                            .ToList();

                        var assignment = assignmentsDict.ContainsKey(test.AssignmentId) 
                            ? assignmentsDict[test.AssignmentId] 
                            : null;
                        var subjectName = assignment != null && subjectsDict.ContainsKey(assignment.SubjectId)
                            ? subjectsDict[assignment.SubjectId]
                            : "Не указан";

                        foreach (var result in completedResults)
                        {
                            if (studentsDict.TryGetValue(result.StudentId, out var student) && student != null)
                            {
                                try
                                {
                                    var user = await _userManager.FindByIdAsync(student.UserId);
                                    var className = student.ClassId.HasValue && classesDict.ContainsKey(student.ClassId.Value)
                                        ? classesDict[student.ClassId.Value]?.Name 
                                        : null;

                                    completions.Add(new RecentTestCompletionViewModel
                                    {
                                        TestResultId = result.Id,
                                        TestId = result.OrthoeopyTestId,
                                        TestTitle = test.Title,
                                        TestType = "orthoeopy",
                                        StudentId = result.StudentId,
                                        StudentName = user?.FullName ?? "Неизвестный студент",
                                        ClassName = className,
                                        Score = result.Score,
                                        MaxScore = result.MaxScore,
                                        Percentage = result.Percentage,
                                        Grade = result.Grade,
                                        CompletedAt = result.CompletedAt!.Value,
                                        SubjectName = subjectName
                                    });
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }

                // Обрабатываем все классические тесты (без ограничения количества)
                foreach (var test in regularTests)
                {
                    try
                    {
                        var testResults = await _regularTestResultRepository.GetByTestIdAsync(test.Id);
                        // Берем все завершенные результаты (без ограничения Take(5))
                        var completedResults = testResults
                            .Where(r => r.IsCompleted && r.CompletedAt.HasValue)
                            .ToList();

                        var assignment = assignmentsDict.ContainsKey(test.AssignmentId) 
                            ? assignmentsDict[test.AssignmentId] 
                            : null;
                        var subjectName = assignment != null && subjectsDict.ContainsKey(assignment.SubjectId)
                            ? subjectsDict[assignment.SubjectId]
                            : "Не указан";

                        foreach (var result in completedResults)
                        {
                            if (studentsDict.TryGetValue(result.StudentId, out var student) && student != null)
                            {
                                try
                                {
                                    var user = await _userManager.FindByIdAsync(student.UserId);
                                    var className = student.ClassId.HasValue && classesDict.ContainsKey(student.ClassId.Value)
                                        ? classesDict[student.ClassId.Value]?.Name 
                                        : null;

                                    completions.Add(new RecentTestCompletionViewModel
                                    {
                                        TestResultId = result.Id,
                                        TestId = result.RegularTestId,
                                        TestTitle = test.Title,
                                        TestType = "regular",
                                        StudentId = result.StudentId,
                                        StudentName = user?.FullName ?? "Неизвестный студент",
                                        ClassName = className,
                                        Score = result.Score,
                                        MaxScore = result.MaxScore,
                                        Percentage = result.Percentage,
                                        Grade = result.Grade,
                                        CompletedAt = result.CompletedAt!.Value,
                                        SubjectName = subjectName
                                    });
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка в GetRecentTestCompletionsAsync");
            }

            // Сортируем все результаты по дате завершения по убыванию и берем последние 10
            return completions
                .OrderByDescending(c => c.CompletedAt)
                .Take(10)
                .ToList();
        }
    }
}

