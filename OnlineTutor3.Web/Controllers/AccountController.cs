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

            try
            {
                ApplicationUser? user = null;
                try
                {
                    user = await _userManager.FindByEmailAsync(model.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при поиске пользователя по email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Произошла ошибка при входе. Попробуйте позже.");
                    return View(model);
                }

                if (user != null)
                {
                    try
                    {
                        // Проверка активности аккаунта
                        if (!user.IsActive)
                        {
                            ModelState.AddModelError(string.Empty, "Ваш аккаунт заблокирован. Обратитесь к администратору.");
                            return View(model);
                        }

                        // Проверка подтверждения email отключена в продакшн
                        // if (!await _userManager.IsEmailConfirmedAsync(user))
                        // {
                        //     TempData["WarningMessage"] = "Пожалуйста, подтвердите ваш email перед входом.";
                        //     return View(model);
                        // }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при проверке пользователя. UserId: {UserId}", user?.Id);
                        ModelState.AddModelError(string.Empty, "Произошла ошибка при проверке аккаунта. Попробуйте позже.");
                        return View(model);
                    }
                }

                Microsoft.AspNetCore.Identity.SignInResult result;
                try
                {
                    result = await _signInManager.PasswordSignInAsync(
                        model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при попытке входа. Email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Произошла ошибка при входе. Попробуйте позже.");
                    return View(model);
                }

                if (result.Succeeded)
                {
                    if (user != null)
                    {
                        try
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
                                    TempData["ErrorMessage"] = "Ваш аккаунт учителя еще не одобрен администратором.";
                                    return View(model);
                                }
                            }

                            return await RedirectToLocalAsync(returnUrl, user);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Ошибка после успешного входа. UserId: {UserId}", user.Id);
                            // Даже если обновление не удалось, продолжаем редирект
                            return await RedirectToLocalAsync(returnUrl, user);
                        }
                    }
                }

                if (result.IsLockedOut)
                {
                    TempData["ErrorMessage"] = "Ваш аккаунт временно заблокирован. Попробуйте позже.";
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при входе. Email: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Произошла критическая ошибка при входе. Попробуйте позже или обратитесь к администратору.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
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

            var normalizedFirstName = CapitalizeFirstLetter(model.FirstName?.Trim() ?? string.Empty);
            var normalizedLastName = CapitalizeFirstLetter(model.LastName?.Trim() ?? string.Empty);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = normalizedFirstName,
                LastName = normalizedLastName,
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

                        // Добавляем выбранные предметы
                        if (model.SelectedSubjectIds != null && model.SelectedSubjectIds.Any())
                        {
                            foreach (var subjectId in model.SelectedSubjectIds)
                            {
                                await _teacherService.AddSubjectToTeacherAsync(teacherId, subjectId);
                            }
                        }
                    }

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

        // GET: Account/EditProfile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound($"Невозможно загрузить пользователя с ID '{_userManager.GetUserId(User)}'.");
                }

                var model = new EditProfileViewModel
                {
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    DateOfBirth = user.DateOfBirth
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке профиля для редактирования");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы редактирования.";
                return RedirectToAction(nameof(Profile));
            }
        }

        // POST: Account/EditProfile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound($"Невозможно загрузить пользователя с ID '{_userManager.GetUserId(User)}'.");
                }

                // Обновляем данные пользователя
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.DateOfBirth = model.DateOfBirth;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Профиль успешно обновлен!";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении профиля");
                TempData["ErrorMessage"] = "Произошла ошибка при обновлении профиля.";
                return View(model);
            }
        }

        #endregion

        #region Change Password

        // GET: Account/ChangePassword
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Невозможно загрузить пользователя с ID '{_userManager.GetUserId(User)}'.");
            }

            // Проверяем текущий пароль
            var checkPassword = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!checkPassword)
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Текущий пароль неверен.");
                return View(model);
            }

            // Меняем пароль
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    if (error.Code == "PasswordTooShort")
                    {
                        ModelState.AddModelError(nameof(model.NewPassword), error.Description);
                    }
                    else if (error.Code == "PasswordRequiresNonAlphanumeric")
                    {
                        ModelState.AddModelError(nameof(model.NewPassword), error.Description);
                    }
                    else if (error.Code == "PasswordRequiresDigit")
                    {
                        ModelState.AddModelError(nameof(model.NewPassword), error.Description);
                    }
                    else if (error.Code == "PasswordRequiresLower")
                    {
                        ModelState.AddModelError(nameof(model.NewPassword), error.Description);
                    }
                    else if (error.Code == "PasswordRequiresUpper")
                    {
                        ModelState.AddModelError(nameof(model.NewPassword), error.Description);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                return View(model);
            }

            // Перезаходим пользователя, чтобы обновить cookie
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Пароль успешно изменен.";
            return RedirectToAction(nameof(Profile));
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

        private static string CapitalizeFirstLetter(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            value = value.Trim();
            var firstChar = value[0];
            var restOfString = value.Substring(1);

            return char.ToUpperInvariant(firstChar) + restOfString.ToLowerInvariant();
        }

        #endregion
    }
}

