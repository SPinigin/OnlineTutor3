using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Если пользователь залогинен, перенаправляем его на соответствующую страницу
            if (User.Identity?.IsAuthenticated == true)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    if (await _userManager.IsInRoleAsync(currentUser, ApplicationRoles.Teacher))
                    {
                        return RedirectToAction("Index", "Teacher");
                    }
                    else if (await _userManager.IsInRoleAsync(currentUser, ApplicationRoles.Student))
                    {
                        return RedirectToAction("Index", "Student");
                    }
                }
            }

            // Если пользователь не залогинен, показываем общую главную страницу
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}

