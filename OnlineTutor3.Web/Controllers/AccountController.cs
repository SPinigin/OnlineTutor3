using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly ISubjectService _subjectService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IStudentService studentService,
            ITeacherService teacherService,
            ISubjectService subjectService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _studentService = studentService;
            _teacherService = teacherService;
            _subjectService = subjectService;
            _logger = logger;
        }

        #region Login/Logout

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _logger.LogInformation("Попытка входа пользователя: {Email}", model.Email);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                // Проверка активности аккаунта
                if (!user.IsActive)
                {
                    _logger.LogWarning("Попытка входа заблокированного пользователя. UserId: {UserId}", user.Id);
                    ModelState.AddModelError(string.Empty, "Ваш аккаунт заблокирован. Обратитесь к администратору.");
                    return View(model);
                }

                // Проверка подтверждения email (если требуется)
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    TempData["WarningMessage"] = "Пожалуйста, подтвердите ваш email перед входом.";
                    return View(model);
                }
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (user != null)
                {
                    user.LastLoginAt = DateTime.Now;
                    await _userManager.UpdateAsync(user);

                    // Проверяем, если это учитель, одобрен ли он
                    if (await _userManager.IsInRoleAsync(user, ApplicationRoles.Teacher))
                    {
                        var teacher = await _teacherService.GetByUserIdAsync(user.Id);
                        if (teacher != null && !teacher.IsApproved)
                        {
                            await _signInManager.SignOutAsync();
                            _logger.LogWarning("Попытка входа неодобренного учителя. UserId: {UserId}", user.Id);
                            TempData["ErrorMessage"] = "Ваш аккаунт учителя еще не одобрен администратором.";
                            return View(model);
                        }
                    }

                    _logger.LogInformation("Успешный вход пользователя. UserId: {UserId}, Email: {Email}", user.Id, model.Email);
                    return await RedirectToLocalAsync(returnUrl, user);
                }
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Аккаунт {Email} заблокирован", model.Email);
                TempData["ErrorMessage"] = "Ваш аккаунт временно заблокирован. Попробуйте позже.";
                return View(model);
            }

            _logger.LogWarning("Неудачная попытка входа. Email: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Пользователь вышел из системы");
            TempData["InfoMessage"] = "Вы успешно вышли из системы.";
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Register

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register()
        {
            var subjects = await _subjectService.GetActiveAsync();
            ViewBag.Subjects = subjects;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var subjects = await _subjectService.GetActiveAsync();
                ViewBag.Subjects = subjects;
                return View(model);
            }

            _logger.LogInformation("Попытка регистрации пользователя. Email: {Email}, Role: {Role}", model.Email, model.Role);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = false, // В продакшене нужно подтверждение email
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Добавляем роль
                await _userManager.AddToRoleAsync(user, model.Role);
                _logger.LogInformation("Роль {Role} назначена пользователю {UserId}", model.Role, user.Id);

                try
                {
                    // Создаем профиль в зависимости от роли
                    if (model.Role == ApplicationRoles.Student)
                    {
                        var student = new Student
                        {
                            UserId = user.Id,
                            School = model.School,
                            Grade = model.Grade,
                            CreatedAt = DateTime.Now
                        };
                        await _studentService.CreateAsync(student);
                        _logger.LogInformation("Профиль студента создан. UserId: {UserId}", user.Id);
                    }
                    else if (model.Role == ApplicationRoles.Teacher)
                    {
                        var teacher = new Teacher
                        {
                            UserId = user.Id,
                            Education = model.Education,
                            Experience = model.Experience,
                            IsApproved = false,
                            CreatedAt = DateTime.Now
                        };
                        var teacherId = await _teacherService.CreateAsync(teacher);
                        _logger.LogInformation("Профиль учителя создан. UserId: {UserId}, TeacherId: {TeacherId}", user.Id, teacherId);

                        // Добавляем выбранные предметы
                        if (model.SelectedSubjectIds != null && model.SelectedSubjectIds.Any())
                        {
                            foreach (var subjectId in model.SelectedSubjectIds)
                            {
                                await _teacherService.AddSubjectToTeacherAsync(teacherId, subjectId);
                            }
                        }
                    }

                    _logger.LogInformation("Пользователь успешно зарегистрирован. UserId: {UserId}, Email: {Email}", user.Id, model.Email);
                    TempData["SuccessMessage"] = "Регистрация успешна! Вы можете войти в систему.";
                    return RedirectToAction(nameof(Login));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при создании профиля пользователя. UserId: {UserId}", user.Id);
                    // Удаляем пользователя, если не удалось создать профиль
                    await _userManager.DeleteAsync(user);
                    ModelState.AddModelError(string.Empty, "Произошла ошибка при регистрации. Попробуйте позже.");
                    var subjects = await _subjectService.GetActiveAsync();
                    ViewBag.Subjects = subjects;
                    return View(model);
                }
            }

            // Обработка ошибок регистрации
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var subjectsForView = await _subjectService.GetActiveAsync();
            ViewBag.Subjects = subjectsForView;
            return View(model);
        }

        #endregion

        #region AccessDenied

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #endregion

        #region Profile

        // GET: Account/Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Невозможно загрузить пользователя с ID '{_userManager.GetUserId(User)}'.");
            }

            return View(user);
        }

        #endregion

        #region Helper Methods

        private async Task<IActionResult> RedirectToLocalAsync(string? returnUrl, ApplicationUser? user = null)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Перенаправляем на главную страницу в зависимости от роли
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, ApplicationRoles.Teacher))
                {
                    return RedirectToAction("Index", "Teacher");
                }
                else if (await _userManager.IsInRoleAsync(user, ApplicationRoles.Student))
                {
                    return RedirectToAction("Index", "Student");
                }
            }
            else
            {
                // Если user не передан, пытаемся получить из контекста (fallback)
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

            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}

