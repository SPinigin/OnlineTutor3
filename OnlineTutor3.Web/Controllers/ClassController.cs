using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Teacher)]
    public class ClassController : Controller
    {
        private readonly IClassService _classService;
        private readonly IStudentService _studentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ClassController> _logger;

        public ClassController(
            IClassService classService,
            IStudentService studentService,
            UserManager<ApplicationUser> userManager,
            ILogger<ClassController> logger)
        {
            _classService = classService;
            _studentService = studentService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Class
        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                
                // Загружаем количество учеников для каждого класса
                var studentsCountDict = new Dictionary<int, int>();
                foreach (var @class in classes)
                {
                    var students = await _studentService.GetByClassIdAsync(@class.Id);
                    studentsCountDict[@class.Id] = students?.Count ?? 0;
                }
                ViewBag.StudentsCountDict = studentsCountDict;
                
                return View(classes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Class/Index");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке классов. Попробуйте обновить страницу.";
                return View(new List<Class>());
            }
        }

        // GET: Class/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var @class = await _classService.GetByIdAsync(id.Value);
                if (@class == null || @class.TeacherId != currentUser.Id)
                {
                    return NotFound();
                }

                // Загружаем студентов класса
                var students = await _studentService.GetByClassIdAsync(id.Value);
                
                // Загружаем данные пользователей для студентов
                var studentsWithUsers = new List<(Student Student, ApplicationUser? User)>();
                if (students != null)
                {
                    foreach (var student in students)
                    {
                        try
                        {
                            var user = await _userManager.FindByIdAsync(student.UserId);
                            studentsWithUsers.Add((student, user));
                        }
                        catch
                        {
                            studentsWithUsers.Add((student, null));
                        }
                    }
                }
                
                ViewBag.Students = students;
                ViewBag.StudentsWithUsers = studentsWithUsers;

                return View(@class);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Class/Details для класса {ClassId}", id);
                TempData["ErrorMessage"] = $"Ошибка при загрузке класса: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Class/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Class/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateClassViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var @class = new Class
                {
                    Name = model.Name,
                    Description = model.Description,
                    TeacherId = currentUser.Id,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                await _classService.CreateAsync(@class);
                TempData["SuccessMessage"] = $"Класс \"{@class.Name}\" успешно создан!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании класса");
                ModelState.AddModelError(string.Empty, "Произошла ошибка при создании класса. Попробуйте еще раз.");
                return View(model);
            }
        }

        // GET: Class/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var @class = await _classService.GetByIdAsync(id.Value);
                if (@class == null || @class.TeacherId != currentUser.Id)
                {
                    return NotFound();
                }

                var model = new EditClassViewModel
                {
                    Id = @class.Id,
                    Name = @class.Name,
                    Description = @class.Description,
                    IsActive = @class.IsActive
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке класса для редактирования {ClassId}", id);
                TempData["ErrorMessage"] = "Ошибка при загрузке класса.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Class/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditClassViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var @class = await _classService.GetByIdAsync(id);
                if (@class == null || @class.TeacherId != currentUser.Id)
                {
                    return NotFound();
                }

                @class.Name = model.Name;
                @class.Description = model.Description;
                @class.IsActive = model.IsActive;

                await _classService.UpdateAsync(@class);
                TempData["SuccessMessage"] = $"Класс \"{@class.Name}\" успешно обновлен!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении класса {ClassId}", id);
                ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении класса. Попробуйте еще раз.");
                return View(model);
            }
        }

        // GET: Class/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var @class = await _classService.GetByIdAsync(id.Value);
                if (@class == null || @class.TeacherId != currentUser.Id)
                {
                    return NotFound();
                }

                // Проверяем, есть ли ученики в классе
                var students = await _studentService.GetByClassIdAsync(id.Value);
                if (students.Any())
                {
                    TempData["ErrorMessage"] = "Нельзя удалить класс, в котором есть ученики. Сначала переместите учеников в другой класс или удалите их.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                return View(@class);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке класса для удаления {ClassId}", id);
                TempData["ErrorMessage"] = "Ошибка при загрузке класса.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Class/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var @class = await _classService.GetByIdAsync(id);
                if (@class == null || @class.TeacherId != currentUser.Id)
                {
                    return NotFound();
                }

                @class.IsActive = !@class.IsActive;
                await _classService.UpdateAsync(@class);
                
                var actionText = @class.IsActive ? "активирован" : "деактивирован";
                TempData["SuccessMessage"] = $"Класс \"{@class.Name}\" успешно {actionText}!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при изменении статуса класса {ClassId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при изменении статуса класса.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Class/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var @class = await _classService.GetByIdAsync(id);
                if (@class == null || @class.TeacherId != currentUser.Id)
                {
                    return NotFound();
                }

                // Проверяем, есть ли ученики в классе
                var students = await _studentService.GetByClassIdAsync(id);
                if (students.Any())
                {
                    TempData["ErrorMessage"] = "Нельзя удалить класс, в котором есть ученики.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var className = @class.Name;
                await _classService.DeleteAsync(id);
                TempData["SuccessMessage"] = $"Класс \"{className}\" успешно удален!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении класса {ClassId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении класса.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

