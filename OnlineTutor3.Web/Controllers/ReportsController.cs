using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;
using System.Text.RegularExpressions;

namespace OnlineTutor3.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Teacher)]
    public class ReportsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ReportsController> _logger;
        private readonly ISpellingTestService _spellingTestService;
        private readonly IPunctuationTestService _punctuationTestService;
        private readonly IOrthoeopyTestService _orthoeopyTestService;
        private readonly IRegularTestService _regularTestService;
        private readonly ISpellingTestResultRepository _spellingTestResultRepository;
        private readonly IPunctuationTestResultRepository _punctuationTestResultRepository;
        private readonly IOrthoeopyTestResultRepository _orthoeopyTestResultRepository;
        private readonly IRegularTestResultRepository _regularTestResultRepository;
        private readonly IAssignmentService _assignmentService;
        private readonly IAssignmentClassRepository _assignmentClassRepository;
        private readonly IClassService _classService;
        private readonly IStudentService _studentService;
        private readonly ISubjectService _subjectService;
        private readonly ITeacherService _teacherService;

        public ReportsController(
            UserManager<ApplicationUser> userManager,
            ILogger<ReportsController> logger,
            ISpellingTestService spellingTestService,
            IPunctuationTestService punctuationTestService,
            IOrthoeopyTestService orthoeopyTestService,
            IRegularTestService regularTestService,
            ISpellingTestResultRepository spellingTestResultRepository,
            IPunctuationTestResultRepository punctuationTestResultRepository,
            IOrthoeopyTestResultRepository orthoeopyTestResultRepository,
            IRegularTestResultRepository regularTestResultRepository,
            IAssignmentService assignmentService,
            IAssignmentClassRepository assignmentClassRepository,
            IClassService classService,
            IStudentService studentService,
            ISubjectService subjectService,
            ITeacherService teacherService)
        {
            _userManager = userManager;
            _logger = logger;
            _spellingTestService = spellingTestService;
            _punctuationTestService = punctuationTestService;
            _orthoeopyTestService = orthoeopyTestService;
            _regularTestService = regularTestService;
            _spellingTestResultRepository = spellingTestResultRepository;
            _punctuationTestResultRepository = punctuationTestResultRepository;
            _orthoeopyTestResultRepository = orthoeopyTestResultRepository;
            _regularTestResultRepository = regularTestResultRepository;
            _assignmentService = assignmentService;
            _assignmentClassRepository = assignmentClassRepository;
            _classService = classService;
            _studentService = studentService;
            _subjectService = subjectService;
            _teacherService = teacherService;
        }

        // GET: Reports
        public async Task<IActionResult> Index(int? subjectId = null, int? classId = null)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                // Получаем предметы учителя
                var teacherSubjects = await _teacherService.GetTeacherSubjectsByUserIdAsync(currentUser.Id) ?? new List<Subject>();
                ViewBag.Subjects = new SelectList(teacherSubjects, "Id", "Name", subjectId);
                var subjectsDict = teacherSubjects.ToDictionary(s => s.Id, s => s.Name);

                // Получаем классы учителя
                var classes = await _classService.GetActiveByTeacherIdAsync(currentUser.Id);
                ViewBag.Classes = new SelectList(classes, "Id", "Name", classId);

                // Получаем все тесты учителя
                var spellingTests = await _spellingTestService.GetByTeacherIdAsync(currentUser.Id);
                var punctuationTests = await _punctuationTestService.GetByTeacherIdAsync(currentUser.Id);
                var orthoeopyTests = await _orthoeopyTestService.GetByTeacherIdAsync(currentUser.Id);
                var regularTests = await _regularTestService.GetByTeacherIdAsync(currentUser.Id);

                // Получаем все задания учителя
                var assignments = await _assignmentService.GetByTeacherSubjectsAsync(currentUser.Id);
                var assignmentsDict = assignments.ToDictionary(a => a.Id, a => a.Title);

                // Получаем студентов
                var allStudents = await _studentService.GetByTeacherIdAsync(currentUser.Id);
                var studentsDict = allStudents.ToDictionary(s => s.Id, s => s);

                // Фильтрация студентов по классу, если выбран
                if (classId.HasValue)
                {
                    allStudents = allStudents.Where(s => s.ClassId == classId.Value).ToList();
                }

                var testReports = new List<TestReportItemViewModel>();

                // Обрабатываем тесты по орфографии
                foreach (var test in spellingTests)
                {
                    if (subjectId.HasValue)
                    {
                        var assignment = assignments.FirstOrDefault(a => a.Id == test.AssignmentId);
                        if (assignment == null || assignment.SubjectId != subjectId.Value)
                            continue;
                    }

                    var report = await BuildTestReportItemAsync(test, "spelling", assignmentsDict, subjectsDict, allStudents);
                    if (report != null)
                    {
                        testReports.Add(report);
                    }
                }

                // Обрабатываем тесты по пунктуации
                foreach (var test in punctuationTests)
                {
                    if (subjectId.HasValue)
                    {
                        var assignment = assignments.FirstOrDefault(a => a.Id == test.AssignmentId);
                        if (assignment == null || assignment.SubjectId != subjectId.Value)
                            continue;
                    }

                    var report = await BuildTestReportItemAsync(test, "punctuation", assignmentsDict, subjectsDict, allStudents);
                    if (report != null)
                    {
                        testReports.Add(report);
                    }
                }

                // Обрабатываем тесты по орфоэпии
                foreach (var test in orthoeopyTests)
                {
                    if (subjectId.HasValue)
                    {
                        var assignment = assignments.FirstOrDefault(a => a.Id == test.AssignmentId);
                        if (assignment == null || assignment.SubjectId != subjectId.Value)
                            continue;
                    }

                    var report = await BuildTestReportItemAsync(test, "orthoeopy", assignmentsDict, subjectsDict, allStudents);
                    if (report != null)
                    {
                        testReports.Add(report);
                    }
                }

                // Обрабатываем классические тесты
                foreach (var test in regularTests)
                {
                    if (subjectId.HasValue)
                    {
                        var assignment = assignments.FirstOrDefault(a => a.Id == test.AssignmentId);
                        if (assignment == null || assignment.SubjectId != subjectId.Value)
                            continue;
                    }

                    var report = await BuildTestReportItemAsync(test, "regular", assignmentsDict, subjectsDict, allStudents);
                    if (report != null)
                    {
                        testReports.Add(report);
                    }
                }

                // Сортируем по предмету и названию задания
                testReports = testReports.OrderBy(r => r.SubjectName)
                    .ThenBy(r => r.AssignmentTitle)
                    .ThenBy(r => r.TestTitle)
                    .ToList();

                var viewModel = new TestReportIndexViewModel
                {
                    TestReports = testReports,
                    SubjectsDict = subjectsDict,
                    AssignmentsDict = assignmentsDict,
                    SelectedSubjectId = subjectId,
                    SelectedClassId = classId
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке отчетов");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке отчетов.";
                return View(new TestReportIndexViewModel());
            }
        }

        // GET: Reports/TestDetails
        public async Task<IActionResult> TestDetails(string testType, int testId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var viewModel = await BuildTestReportDetailViewModelAsync(testType, testId, currentUser.Id);
                if (viewModel == null)
                {
                    TempData["ErrorMessage"] = "Тест не найден или у вас нет доступа к нему.";
                    return RedirectToAction(nameof(Index));
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке детального отчета для теста {TestType}/{TestId}", testType, testId);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке отчета.";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<TestReportItemViewModel?> BuildTestReportItemAsync(
            Test test,
            string testType,
            Dictionary<int, string> assignmentsDict,
            Dictionary<int, string> subjectsDict,
            List<Student> students)
        {
            try
            {
                var assignment = assignmentsDict.ContainsKey(test.AssignmentId)
                    ? await _assignmentService.GetByIdAsync(test.AssignmentId)
                    : null;

                if (assignment == null)
                    return null;

                var subjectName = subjectsDict.ContainsKey(assignment.SubjectId)
                    ? subjectsDict[assignment.SubjectId]
                    : "Не указан";

                // Получаем результаты теста
                List<TestResult> allResults = testType switch
                {
                    "spelling" => (await _spellingTestResultRepository.GetByTestIdAsync(test.Id)).Cast<TestResult>().ToList(),
                    "punctuation" => (await _punctuationTestResultRepository.GetByTestIdAsync(test.Id)).Cast<TestResult>().ToList(),
                    "orthoeopy" => (await _orthoeopyTestResultRepository.GetByTestIdAsync(test.Id)).Cast<TestResult>().ToList(),
                    "regular" => (await _regularTestResultRepository.GetByTestIdAsync(test.Id)).Cast<TestResult>().ToList(),
                    _ => new List<TestResult>()
                };

                // Получаем студентов, которые должны были пройти тест
                var assignmentClasses = await _assignmentClassRepository.GetByAssignmentIdAsync(test.AssignmentId);
                var classIds = assignmentClasses.Select(ac => ac.ClassId).ToList();
                var relevantStudents = students.Where(s => 
                    s.ClassId.HasValue && classIds.Contains(s.ClassId.Value)).ToList();

                var completedResults = allResults.Where(r => r.IsCompleted && r.CompletedAt.HasValue).ToList();
                var inProgressResults = allResults.Where(r => !r.IsCompleted).ToList();
                var completedStudentIds = completedResults.Select(r => r.StudentId).Distinct().ToList();
                var inProgressStudentIds = inProgressResults.Select(r => r.StudentId).Distinct().ToList();

                var completedCount = completedStudentIds.Count;
                var inProgressCount = inProgressStudentIds.Count;
                var notStartedCount = Math.Max(0, relevantStudents.Count - completedCount - inProgressCount);

                var averagePercentage = completedResults.Any()
                    ? completedResults.Average(r => r.Percentage)
                    : 0.0;

                var averageGrade = completedResults.Any(r => r.Grade.HasValue)
                    ? (int?)completedResults.Where(r => r.Grade.HasValue).Average(r => r.Grade!.Value)
                    : null;

                var lastCompletionDate = completedResults.Any()
                    ? completedResults.Max(r => r.CompletedAt!.Value)
                    : (DateTime?)null;

                return new TestReportItemViewModel
                {
                    TestId = test.Id,
                    TestTitle = test.Title,
                    TestType = testType,
                    AssignmentId = test.AssignmentId,
                    AssignmentTitle = assignmentsDict.ContainsKey(test.AssignmentId)
                        ? assignmentsDict[test.AssignmentId]
                        : "Неизвестное задание",
                    SubjectId = assignment.SubjectId,
                    SubjectName = subjectName,
                    TotalStudents = relevantStudents.Count,
                    CompletedCount = completedCount,
                    InProgressCount = inProgressCount,
                    NotStartedCount = notStartedCount,
                    AveragePercentage = averagePercentage,
                    AverageGrade = averageGrade,
                    LastCompletionDate = lastCompletionDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при построении отчета для теста {TestType}/{TestId}", testType, test.Id);
                return null;
            }
        }

        private async Task<TestReportDetailViewModel?> BuildTestReportDetailViewModelAsync(
            string testType,
            int testId,
            string teacherId)
        {
            try
            {
                Test? test = null;
                List<TestResult> allResults = new List<TestResult>();

                switch (testType.ToLower())
                {
                    case "spelling":
                        var spellingTest = await _spellingTestService.GetByIdAsync(testId);
                        if (spellingTest == null || spellingTest.TeacherId != teacherId)
                            return null;
                        test = spellingTest;
                        allResults = (await _spellingTestResultRepository.GetByTestIdAsync(testId))
                            .Cast<TestResult>().ToList();
                        break;

                    case "punctuation":
                        var punctuationTest = await _punctuationTestService.GetByIdAsync(testId);
                        if (punctuationTest == null || punctuationTest.TeacherId != teacherId)
                            return null;
                        test = punctuationTest;
                        allResults = (await _punctuationTestResultRepository.GetByTestIdAsync(testId))
                            .Cast<TestResult>().ToList();
                        break;

                    case "orthoeopy":
                        var orthoeopyTest = await _orthoeopyTestService.GetByIdAsync(testId);
                        if (orthoeopyTest == null || orthoeopyTest.TeacherId != teacherId)
                            return null;
                        test = orthoeopyTest;
                        allResults = (await _orthoeopyTestResultRepository.GetByTestIdAsync(testId))
                            .Cast<TestResult>().ToList();
                        break;

                    case "regular":
                        var regularTest = await _regularTestService.GetByIdAsync(testId);
                        if (regularTest == null || regularTest.TeacherId != teacherId)
                            return null;
                        test = regularTest;
                        allResults = (await _regularTestResultRepository.GetByTestIdAsync(testId))
                            .Cast<TestResult>().ToList();
                        break;

                    default:
                        return null;
                }

                if (test == null)
                    return null;

                var assignment = await _assignmentService.GetByIdAsync(test.AssignmentId);
                if (assignment == null)
                    return null;

                var subject = await _subjectService.GetByIdAsync(assignment.SubjectId);
                var subjectName = subject?.Name ?? "Не указан";

                // Получаем студентов, которые должны были пройти тест
                var assignmentClasses = await _assignmentClassRepository.GetByAssignmentIdAsync(test.AssignmentId);
                var classIds = assignmentClasses.Select(ac => ac.ClassId).ToList();
                var allStudents = await _studentService.GetByTeacherIdAsync(teacherId);
                var relevantStudents = allStudents.Where(s =>
                    s.ClassId.HasValue && classIds.Contains(s.ClassId.Value)).ToList();

                var classes = await _classService.GetActiveByTeacherIdAsync(teacherId);
                var classesDict = classes.ToDictionary(c => c.Id, c => c.Name);

                // Строим статистику
                var completedResults = allResults.Where(r => r.IsCompleted && r.CompletedAt.HasValue).ToList();
                var inProgressResults = allResults.Where(r => !r.IsCompleted).ToList();
                var completedStudentIds = completedResults.Select(r => r.StudentId).Distinct().ToList();
                var inProgressStudentIds = inProgressResults.Select(r => r.StudentId).Distinct().ToList();

                var statistics = new TestReportStatistics
                {
                    TotalStudents = relevantStudents.Count,
                    CompletedCount = completedStudentIds.Count,
                    InProgressCount = inProgressStudentIds.Count,
                    NotStartedCount = Math.Max(0, relevantStudents.Count - completedStudentIds.Count - inProgressStudentIds.Count),
                    AverageScore = completedResults.Any() ? completedResults.Average(r => r.Score) : 0.0,
                    AveragePercentage = completedResults.Any() ? completedResults.Average(r => r.Percentage) : 0.0,
                    AverageGrade = completedResults.Any(r => r.Grade.HasValue)
                        ? (int?)completedResults.Where(r => r.Grade.HasValue).Average(r => r.Grade!.Value)
                        : null,
                    HighestScore = completedResults.Any() ? completedResults.Max(r => r.Score) : 0,
                    LowestScore = completedResults.Any() ? completedResults.Min(r => r.Score) : 0,
                    MaxScore = completedResults.Any() ? completedResults.Max(r => r.MaxScore) : 0,
                    FirstCompletionDate = completedResults.Any() ? completedResults.Min(r => r.CompletedAt!.Value) : null,
                    LastCompletionDate = completedResults.Any() ? completedResults.Max(r => r.CompletedAt!.Value) : null
                };

                // Распределение по оценкам
                foreach (var result in completedResults.Where(r => r.Grade.HasValue))
                {
                    var gradeStr = result.Grade!.Value.ToString();
                    if (!statistics.GradeDistribution.ContainsKey(gradeStr))
                        statistics.GradeDistribution[gradeStr] = 0;
                    statistics.GradeDistribution[gradeStr]++;
                }

                // Строим отчеты по студентам
                var studentReports = new List<TestReportStudentViewModel>();
                var studentsDict = relevantStudents.ToDictionary(s => s.Id, s => s);

                foreach (var student in relevantStudents)
                {
                    var studentResults = allResults.Where(r => r.StudentId == student.Id).ToList();
                    var completedStudentResults = studentResults.Where(r => r.IsCompleted && r.CompletedAt.HasValue).ToList();
                    var bestResult = completedStudentResults.OrderByDescending(r => r.Percentage)
                        .ThenByDescending(r => r.Score)
                        .FirstOrDefault();
                    var latestResult = completedStudentResults.OrderByDescending(r => r.CompletedAt)
                        .FirstOrDefault();

                    var className = student.ClassId.HasValue && classesDict.ContainsKey(student.ClassId.Value)
                        ? classesDict[student.ClassId.Value]
                        : null;

                    var user = await _userManager.FindByIdAsync(student.UserId);

                    studentReports.Add(new TestReportStudentViewModel
                    {
                        StudentId = student.Id,
                        StudentName = user?.FullName ?? "Неизвестный студент",
                        ClassName = className,
                        AttemptsCount = studentResults.Count,
                        HasCompleted = completedStudentResults.Any(),
                        IsInProgress = studentResults.Any(r => !r.IsCompleted),
                        BestScore = bestResult?.Score,
                        BestPercentage = bestResult != null ? (int?)bestResult.Percentage : null,
                        BestGrade = bestResult?.Grade,
                        LatestScore = latestResult?.Score,
                        LatestPercentage = latestResult != null ? (int?)latestResult.Percentage : null,
                        LatestGrade = latestResult?.Grade,
                        FirstAttemptDate = studentResults.Any() ? studentResults.Min(r => r.StartedAt) : null,
                        LastAttemptDate = studentResults.Any() ? studentResults.Max(r => r.StartedAt) : null,
                        LastCompletionDate = completedStudentResults.Any()
                            ? completedStudentResults.Max(r => r.CompletedAt!.Value)
                            : null
                    });
                }

                // Сортируем студентов по классу и имени
                studentReports = studentReports
                    .OrderBy(s => s.ClassName ?? "")
                    .ThenBy(s => s.StudentName)
                    .ToList();

                return new TestReportDetailViewModel
                {
                    TestId = test.Id,
                    TestTitle = test.Title,
                    TestType = testType,
                    TestDescription = test.Description ?? "",
                    AssignmentId = test.AssignmentId,
                    AssignmentTitle = assignment.Title,
                    SubjectId = assignment.SubjectId,
                    SubjectName = subjectName,
                    Statistics = statistics,
                    StudentReports = studentReports
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при построении детального отчета для теста {TestType}/{TestId}", testType, testId);
                return null;
            }
        }

        // GET: Reports/ExportAll
        public async Task<IActionResult> ExportAll(int? subjectId = null, int? classId = null)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                // Лицензия EPPlus устанавливается в Program.cs при запуске приложения

                // Получаем данные для экспорта (аналогично методу Index)
                var teacherSubjects = await _teacherService.GetTeacherSubjectsByUserIdAsync(currentUser.Id) ?? new List<Subject>();
                var subjectsDict = teacherSubjects.ToDictionary(s => s.Id, s => s.Name);

                var allStudents = await _studentService.GetByTeacherIdAsync(currentUser.Id);
                if (classId.HasValue)
                {
                    allStudents = allStudents.Where(s => s.ClassId == classId.Value).ToList();
                }

                var spellingTests = await _spellingTestService.GetByTeacherIdAsync(currentUser.Id);
                var punctuationTests = await _punctuationTestService.GetByTeacherIdAsync(currentUser.Id);
                var orthoeopyTests = await _orthoeopyTestService.GetByTeacherIdAsync(currentUser.Id);
                var regularTests = await _regularTestService.GetByTeacherIdAsync(currentUser.Id);

                var assignments = await _assignmentService.GetByTeacherSubjectsAsync(currentUser.Id);
                var assignmentsDict = assignments.ToDictionary(a => a.Id, a => a.Title);

                var testReports = new List<TestReportItemViewModel>();

                foreach (var test in spellingTests)
                {
                    if (subjectId.HasValue)
                    {
                        var assignment = assignments.FirstOrDefault(a => a.Id == test.AssignmentId);
                        if (assignment == null || assignment.SubjectId != subjectId.Value)
                            continue;
                    }

                    var report = await BuildTestReportItemAsync(test, "spelling", assignmentsDict, subjectsDict, allStudents);
                    if (report != null)
                        testReports.Add(report);
                }

                foreach (var test in punctuationTests)
                {
                    if (subjectId.HasValue)
                    {
                        var assignment = assignments.FirstOrDefault(a => a.Id == test.AssignmentId);
                        if (assignment == null || assignment.SubjectId != subjectId.Value)
                            continue;
                    }

                    var report = await BuildTestReportItemAsync(test, "punctuation", assignmentsDict, subjectsDict, allStudents);
                    if (report != null)
                        testReports.Add(report);
                }

                foreach (var test in orthoeopyTests)
                {
                    if (subjectId.HasValue)
                    {
                        var assignment = assignments.FirstOrDefault(a => a.Id == test.AssignmentId);
                        if (assignment == null || assignment.SubjectId != subjectId.Value)
                            continue;
                    }

                    var report = await BuildTestReportItemAsync(test, "orthoeopy", assignmentsDict, subjectsDict, allStudents);
                    if (report != null)
                        testReports.Add(report);
                }

                foreach (var test in regularTests)
                {
                    if (subjectId.HasValue)
                    {
                        var assignment = assignments.FirstOrDefault(a => a.Id == test.AssignmentId);
                        if (assignment == null || assignment.SubjectId != subjectId.Value)
                            continue;
                    }

                    var report = await BuildTestReportItemAsync(test, "regular", assignmentsDict, subjectsDict, allStudents);
                    if (report != null)
                        testReports.Add(report);
                }

                // Генерируем Excel файл
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Отчеты по тестам");

                // Заголовки
                worksheet.Cells[1, 1].Value = "Тест";
                worksheet.Cells[1, 2].Value = "Тип теста";
                worksheet.Cells[1, 3].Value = "Предмет";
                worksheet.Cells[1, 4].Value = "Задание";
                worksheet.Cells[1, 5].Value = "Всего учеников";
                worksheet.Cells[1, 6].Value = "Завершено";
                worksheet.Cells[1, 7].Value = "В процессе";
                worksheet.Cells[1, 8].Value = "Не начато";
                worksheet.Cells[1, 9].Value = "Средний балл (%)";
                worksheet.Cells[1, 10].Value = "Средняя оценка";
                worksheet.Cells[1, 11].Value = "Последнее прохождение";

                // Форматирование заголовков
                for (int i = 1; i <= 11; i++)
                {
                    worksheet.Cells[1, i].Style.Font.Bold = true;
                    try
                    {
                        worksheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        worksheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }
                    catch
                    {
                        // Если установка цвета не удалась, продолжаем без цвета
                    }
                }

                // Данные
                int row = 2;
                foreach (var report in testReports.OrderBy(r => r.SubjectName).ThenBy(r => r.AssignmentTitle).ThenBy(r => r.TestTitle))
                {
                    var testTypeName = report.TestType switch
                    {
                        "spelling" => "Орфография",
                        "punctuation" => "Пунктуация",
                        "orthoeopy" => "Орфоэпия",
                        "regular" => "Классический",
                        _ => report.TestType
                    };

                    worksheet.Cells[row, 1].Value = report.TestTitle;
                    worksheet.Cells[row, 2].Value = testTypeName;
                    worksheet.Cells[row, 3].Value = report.SubjectName;
                    worksheet.Cells[row, 4].Value = report.AssignmentTitle;
                    worksheet.Cells[row, 5].Value = report.TotalStudents;
                    worksheet.Cells[row, 6].Value = report.CompletedCount;
                    worksheet.Cells[row, 7].Value = report.InProgressCount;
                    worksheet.Cells[row, 8].Value = report.NotStartedCount;
                    worksheet.Cells[row, 9].Value = report.CompletedCount > 0 ? Math.Round(report.AveragePercentage, 2) : 0;
                    worksheet.Cells[row, 10].Value = report.AverageGrade?.ToString() ?? "-";
                    worksheet.Cells[row, 11].Value = report.LastCompletionDate?.ToString("dd.MM.yyyy HH:mm") ?? "-";
                    row++;
                }

                // Автоподбор ширины столбцов
                worksheet.Cells.AutoFitColumns();

                // Генерируем имя файла
                var fileName = $"Отчеты_по_тестам_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var fileBytes = package.GetAsByteArray();

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при экспорте отчетов в Excel");
                TempData["ErrorMessage"] = "Произошла ошибка при экспорте отчетов.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Reports/ExportTestDetails
        public async Task<IActionResult> ExportTestDetails(string testType, int testId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var viewModel = await BuildTestReportDetailViewModelAsync(testType, testId, currentUser.Id);
                if (viewModel == null)
                {
                    TempData["ErrorMessage"] = "Тест не найден или у вас нет доступа к нему.";
                    return RedirectToAction(nameof(Index));
                }

                // Лицензия EPPlus устанавливается в Program.cs при запуске приложения

                using var package = new ExcelPackage();
                
                // Лист 1: Общая статистика
                var statsSheet = package.Workbook.Worksheets.Add("Общая статистика");
                
                statsSheet.Cells[1, 1].Value = "Детальный отчет по тесту";
                statsSheet.Cells[1, 1].Style.Font.Bold = true;
                statsSheet.Cells[1, 1].Style.Font.Size = 14;

                statsSheet.Cells[3, 1].Value = "Название теста:";
                statsSheet.Cells[3, 2].Value = viewModel.TestTitle;
                statsSheet.Cells[4, 1].Value = "Предмет:";
                statsSheet.Cells[4, 2].Value = viewModel.SubjectName;
                statsSheet.Cells[5, 1].Value = "Задание:";
                statsSheet.Cells[5, 2].Value = viewModel.AssignmentTitle;

                statsSheet.Cells[7, 1].Value = "Статистика";
                statsSheet.Cells[7, 1].Style.Font.Bold = true;
                statsSheet.Cells[7, 1].Style.Font.Size = 12;

                int statRow = 8;
                statsSheet.Cells[statRow, 1].Value = "Всего учеников:";
                statsSheet.Cells[statRow, 2].Value = viewModel.Statistics.TotalStudents;
                statRow++;
                statsSheet.Cells[statRow, 1].Value = "Завершено:";
                statsSheet.Cells[statRow, 2].Value = viewModel.Statistics.CompletedCount;
                statRow++;
                statsSheet.Cells[statRow, 1].Value = "В процессе:";
                statsSheet.Cells[statRow, 2].Value = viewModel.Statistics.InProgressCount;
                statRow++;
                statsSheet.Cells[statRow, 1].Value = "Не начато:";
                statsSheet.Cells[statRow, 2].Value = viewModel.Statistics.NotStartedCount;
                statRow++;
                statsSheet.Cells[statRow, 1].Value = "Средний балл:";
                statsSheet.Cells[statRow, 2].Value = Math.Round(viewModel.Statistics.AveragePercentage, 2) + "%";
                statRow++;
                statsSheet.Cells[statRow, 1].Value = "Средняя оценка:";
                statsSheet.Cells[statRow, 2].Value = viewModel.Statistics.AverageGrade?.ToString() ?? "-";
                statRow++;
                statsSheet.Cells[statRow, 1].Value = "Максимальный балл:";
                statsSheet.Cells[statRow, 2].Value = $"{viewModel.Statistics.HighestScore} / {viewModel.Statistics.MaxScore}";
                statRow++;
                statsSheet.Cells[statRow, 1].Value = "Минимальный балл:";
                statsSheet.Cells[statRow, 2].Value = $"{viewModel.Statistics.LowestScore} / {viewModel.Statistics.MaxScore}";
                statRow++;
                if (viewModel.Statistics.FirstCompletionDate.HasValue)
                {
                    statsSheet.Cells[statRow, 1].Value = "Первое прохождение:";
                    statsSheet.Cells[statRow, 2].Value = viewModel.Statistics.FirstCompletionDate.Value.ToString("dd.MM.yyyy HH:mm");
                    statRow++;
                }
                if (viewModel.Statistics.LastCompletionDate.HasValue)
                {
                    statsSheet.Cells[statRow, 1].Value = "Последнее прохождение:";
                    statsSheet.Cells[statRow, 2].Value = viewModel.Statistics.LastCompletionDate.Value.ToString("dd.MM.yyyy HH:mm");
                }

                statsSheet.Cells.AutoFitColumns();

                // Лист 2: Результаты учеников
                var studentsSheet = package.Workbook.Worksheets.Add("Результаты учеников");

                // Заголовки
                studentsSheet.Cells[1, 1].Value = "Ученик";
                studentsSheet.Cells[1, 2].Value = "Класс";
                studentsSheet.Cells[1, 3].Value = "Попыток";
                studentsSheet.Cells[1, 4].Value = "Статус";
                studentsSheet.Cells[1, 5].Value = "Лучший балл";
                studentsSheet.Cells[1, 6].Value = "Лучший %";
                studentsSheet.Cells[1, 7].Value = "Лучшая оценка";
                studentsSheet.Cells[1, 8].Value = "Последний балл";
                studentsSheet.Cells[1, 9].Value = "Последний %";
                studentsSheet.Cells[1, 10].Value = "Последняя оценка";
                studentsSheet.Cells[1, 11].Value = "Дата последнего прохождения";

                // Форматирование заголовков
                for (int i = 1; i <= 11; i++)
                {
                    studentsSheet.Cells[1, i].Style.Font.Bold = true;
                    try
                    {
                        studentsSheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        studentsSheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }
                    catch
                    {
                        // Если установка цвета не удалась, продолжаем без цвета
                    }
                }

                // Данные
                int dataRow = 2;
                foreach (var studentReport in viewModel.StudentReports.OrderBy(s => s.ClassName ?? "").ThenBy(s => s.StudentName))
                {
                    var status = studentReport.HasCompleted ? "Завершено" 
                        : studentReport.IsInProgress ? "В процессе" 
                        : "Не начато";

                    studentsSheet.Cells[dataRow, 1].Value = studentReport.StudentName;
                    studentsSheet.Cells[dataRow, 2].Value = studentReport.ClassName ?? "-";
                    studentsSheet.Cells[dataRow, 3].Value = studentReport.AttemptsCount;
                    studentsSheet.Cells[dataRow, 4].Value = status;
                    studentsSheet.Cells[dataRow, 5].Value = studentReport.BestScore?.ToString() ?? "-";
                    studentsSheet.Cells[dataRow, 6].Value = studentReport.BestPercentage?.ToString() ?? "-";
                    studentsSheet.Cells[dataRow, 7].Value = studentReport.BestGrade?.ToString() ?? "-";
                    studentsSheet.Cells[dataRow, 8].Value = studentReport.LatestScore?.ToString() ?? "-";
                    studentsSheet.Cells[dataRow, 9].Value = studentReport.LatestPercentage?.ToString() ?? "-";
                    studentsSheet.Cells[dataRow, 10].Value = studentReport.LatestGrade?.ToString() ?? "-";
                    studentsSheet.Cells[dataRow, 11].Value = studentReport.LastCompletionDate?.ToString("dd.MM.yyyy HH:mm") 
                        ?? studentReport.LastAttemptDate?.ToString("dd.MM.yyyy HH:mm") ?? "-";
                    dataRow++;
                }

                studentsSheet.Cells.AutoFitColumns();

                // Генерируем имя файла (убираем недопустимые символы)
                var sanitizedTitle = Regex.Replace(
                    viewModel.TestTitle, 
                    "[<>:\"/\\|?*]", 
                    "_");
                var fileName = $"Отчет_по_тесту_{sanitizedTitle.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var fileBytes = package.GetAsByteArray();

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при экспорте детального отчета в Excel");
                TempData["ErrorMessage"] = "Произошла ошибка при экспорте отчета.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

