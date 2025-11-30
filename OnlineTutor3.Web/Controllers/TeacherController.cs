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
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Teacher
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
                    _logger.LogWarning("Учитель не найден для пользователя {UserId}", currentUser.Id);
                    TempData["ErrorMessage"] = "Профиль учителя не найден. Обратитесь к администратору.";
                    return RedirectToAction("Index", "Home");
                }

                // Получаем статистику
                var classes = await _classService.GetActiveByTeacherIdAsync(currentUser.Id);
                var students = await _studentService.GetByTeacherIdAsync(currentUser.Id);
                var assignments = await _assignmentService.GetByTeacherSubjectsAsync(currentUser.Id);
                var activeAssignments = assignments.Where(a => a.IsActive).ToList();

                // Загружаем предметы для отображения
                var allSubjects = await _subjectService.GetAllAsync();
                var subjectsDict = allSubjects.ToDictionary(s => s.Id, s => s.Name);

                // Получаем активные тесты
                var spellingTests = await _spellingTestService.GetActiveByTeacherIdAsync(currentUser.Id);
                var punctuationTests = await _punctuationTestService.GetActiveByTeacherIdAsync(currentUser.Id);
                var orthoeopyTests = await _orthoeopyTestService.GetActiveByTeacherIdAsync(currentUser.Id);
                var regularTests = await _regularTestService.GetActiveByTeacherIdAsync(currentUser.Id);
                var totalActiveTests = spellingTests.Count() + punctuationTests.Count() + 
                                      orthoeopyTests.Count() + regularTests.Count();

                var viewModel = new TeacherIndexViewModel
                {
                    Teacher = currentUser,
                    TotalClasses = classes.Count,
                    TotalStudents = students.Count,
                    TotalActiveAssignments = activeAssignments.Count,
                    TotalActiveTests = totalActiveTests,
                    RecentAssignments = activeAssignments
                        .OrderByDescending(a => a.CreatedAt)
                        .Take(5)
                        .ToList(),
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
    }
}

