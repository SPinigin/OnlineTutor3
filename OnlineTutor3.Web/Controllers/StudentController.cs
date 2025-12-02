using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Student)]
    public class StudentController : Controller
    {
        private readonly IStudentStatisticsService _statisticsService;
        private readonly IStudentRepository _studentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            IStudentStatisticsService statisticsService,
            IStudentRepository studentRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<StudentController> logger)
        {
            _statisticsService = statisticsService;
            _studentRepository = studentRepository;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Student
        public async Task<IActionResult> Index()
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
                    TempData["ErrorMessage"] = "Профиль студента не найден. Обратитесь к администратору.";
                    return RedirectToAction("Index", "Home");
                }

                var dashboardData = await _statisticsService.GetDashboardDataAsync(student.Id);

                var viewModel = new StudentDashboardViewModel
                {
                    Data = dashboardData,
                    User = currentUser
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке главной страницы студента");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке данных. Попробуйте обновить страницу.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}

