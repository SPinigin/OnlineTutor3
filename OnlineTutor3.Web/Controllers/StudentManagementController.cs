using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Teacher)]
    public class StudentManagementController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IClassService _classService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StudentManagementController> _logger;

        public StudentManagementController(
            IStudentService studentService,
            IClassService classService,
            UserManager<ApplicationUser> userManager,
            ILogger<StudentManagementController> logger)
        {
            _studentService = studentService;
            _classService = classService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: StudentManagement
        public async Task<IActionResult> Index(string? searchString, int? classFilter)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var students = await _studentService.GetByTeacherIdAsync(currentUser.Id);
                
                _logger.LogInformation("Получено студентов для учителя {TeacherId}: {Count}", currentUser.Id, students?.Count ?? 0);
                
                if (students == null)
                {
                    students = new List<Student>();
                }
                
                // Загружаем данные пользователей для студентов
                var studentsWithUsers = new List<(Student Student, ApplicationUser? User)>();
                foreach (var student in students)
                {
                    var user = await _userManager.FindByIdAsync(student.UserId);
                    studentsWithUsers.Add((student, user));
                }
                
                _logger.LogInformation("Загружено студентов с пользователями: {Count}", studentsWithUsers.Count);

                // Фильтрация по классу
                if (classFilter.HasValue && classFilter.Value > 0)
                {
                    studentsWithUsers = studentsWithUsers
                        .Where(s => s.Student.ClassId == classFilter.Value)
                        .ToList();
                }
                else if (classFilter == 0)
                {
                    studentsWithUsers = studentsWithUsers
                        .Where(s => !s.Student.ClassId.HasValue)
                        .ToList();
                }

                // Поиск
                if (!string.IsNullOrEmpty(searchString))
                {
                    var search = searchString.ToLower();
                    studentsWithUsers = studentsWithUsers
                        .Where(s => s.User != null && (
                            s.User.FirstName.ToLower().Contains(search) ||
                            s.User.LastName.ToLower().Contains(search) ||
                            (s.User.Email != null && s.User.Email.ToLower().Contains(search)) ||
                            (s.Student.School != null && s.Student.School.ToLower().Contains(search))
                        ))
                        .ToList();
                }

                ViewBag.StudentsWithUsers = studentsWithUsers;
                
                // Загружаем классы для фильтра
                var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                ViewBag.Classes = new SelectList(classes, "Id", "Name");
                ViewBag.CurrentFilter = searchString;
                ViewBag.ClassFilter = classFilter;
                
                // Создаем словарь для быстрого доступа к названиям классов
                var classesDict = classes.ToDictionary(c => c.Id, c => c.Name);
                ViewBag.ClassesDict = classesDict;

                return View(studentsWithUsers.Select(s => s.Student).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в StudentManagement/Index");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке учеников.";
                return View(new List<Student>());
            }
        }

        // GET: StudentManagement/Details/5
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

                var student = await _studentService.GetByIdAsync(id.Value);
                if (student == null)
                {
                    return NotFound();
                }

                // Проверяем доступ
                if (student.ClassId.HasValue)
                {
                    var @class = await _classService.GetByIdAsync(student.ClassId.Value);
                    if (@class != null && @class.TeacherId != currentUser.Id)
                    {
                        return Forbid();
                    }
                }

                var user = await _userManager.FindByIdAsync(student.UserId);
                ViewBag.User = user;

                // Загружаем классы для модального окна
                var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                ViewBag.Classes = new SelectList(classes, "Id", "Name");

                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в StudentManagement/Details для ученика {StudentId}", id);
                TempData["ErrorMessage"] = "Ошибка при загрузке ученика.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: StudentManagement/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
            ViewBag.Classes = new SelectList(classes, "Id", "Name");
            return View();
        }

        // POST: StudentManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                ViewBag.Classes = new SelectList(classes, "Id", "Name");
                return View(model);
            }

            try
            {
                // Проверяем, существует ли пользователь с таким email
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Пользователь с таким email уже существует.");
                    var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                    ViewBag.Classes = new SelectList(classes, "Id", "Name");
                    return View(model);
                }

                // Создаем пользователя
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = model.DateOfBirth,
                    PhoneNumber = model.PhoneNumber,
                    EmailConfirmed = false,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Добавляем роль студента
                    await _userManager.AddToRoleAsync(user, ApplicationRoles.Student);

                    // Генерируем номер ученика
                    var studentNumber = await GenerateStudentNumber();

                    // Создаем профиль студента
                    var student = new Student
                    {
                        UserId = user.Id,
                        School = model.School,
                        Grade = model.Grade,
                        ClassId = model.ClassId,
                        StudentNumber = studentNumber,
                        CreatedAt = DateTime.Now
                    };

                    await _studentService.CreateAsync(student);

                    _logger.LogInformation("Учитель {TeacherId} создал ученика {StudentId}: {StudentName}, Email: {Email}",
                        currentUser.Id, student.Id, user.FullName, model.Email);

                    TempData["SuccessMessage"] = $"Ученик {user.FullName} успешно создан!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                var classesForView = await _classService.GetByTeacherIdAsync(currentUser.Id);
                ViewBag.Classes = new SelectList(classesForView, "Id", "Name");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании ученика");
                ModelState.AddModelError(string.Empty, "Произошла ошибка при создании ученика.");
                var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                ViewBag.Classes = new SelectList(classes, "Id", "Name");
                return View(model);
            }
        }

        // GET: StudentManagement/Edit/5
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

                var student = await _studentService.GetByIdAsync(id.Value);
                if (student == null)
                {
                    return NotFound();
                }

                // Проверяем доступ
                if (student.ClassId.HasValue)
                {
                    var @class = await _classService.GetByIdAsync(student.ClassId.Value);
                    if (@class != null && @class.TeacherId != currentUser.Id)
                    {
                        return Forbid();
                    }
                }

                var user = await _userManager.FindByIdAsync(student.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                var model = new EditStudentViewModel
                {
                    Id = student.Id,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    DateOfBirth = user.DateOfBirth,
                    School = student.School,
                    Grade = student.Grade,
                    ClassId = student.ClassId,
                    StudentNumber = student.StudentNumber
                };

                var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                ViewBag.Classes = new SelectList(classes, "Id", "Name");

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке ученика для редактирования {StudentId}", id);
                TempData["ErrorMessage"] = "Ошибка при загрузке ученика.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: StudentManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditStudentViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                ViewBag.Classes = new SelectList(classes, "Id", "Name");
                return View(model);
            }

            try
            {
                var student = await _studentService.GetByIdAsync(id);
                if (student == null)
                {
                    return NotFound();
                }

                // Проверяем доступ
                if (student.ClassId.HasValue)
                {
                    var @class = await _classService.GetByIdAsync(student.ClassId.Value);
                    if (@class != null && @class.TeacherId != currentUser.Id)
                    {
                        return Forbid();
                    }
                }

                var user = await _userManager.FindByIdAsync(student.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                // Проверяем изменение email
                if (user.Email != model.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Пользователь с таким email уже существует.");
                        var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                        ViewBag.Classes = new SelectList(classes, "Id", "Name");
                        return View(model);
                    }
                }

                // Обновляем данные пользователя
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.DateOfBirth = model.DateOfBirth;

                await _userManager.UpdateAsync(user);

                // Обновляем данные студента
                student.School = model.School;
                student.Grade = model.Grade;
                student.ClassId = model.ClassId;
                student.StudentNumber = model.StudentNumber;

                await _studentService.UpdateAsync(student);

                _logger.LogInformation("Учитель {TeacherId} обновил ученика {StudentId}: {StudentName}",
                    currentUser.Id, id, user.FullName);

                TempData["SuccessMessage"] = $"Данные ученика {user.FullName} успешно обновлены!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении ученика {StudentId}", id);
                ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении ученика.");
                var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                ViewBag.Classes = new SelectList(classes, "Id", "Name");
                return View(model);
            }
        }

        // GET: StudentManagement/Delete/5
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

                var student = await _studentService.GetByIdAsync(id.Value);
                if (student == null)
                {
                    return NotFound();
                }

                // Проверяем доступ
                if (student.ClassId.HasValue)
                {
                    var @class = await _classService.GetByIdAsync(student.ClassId.Value);
                    if (@class != null && @class.TeacherId != currentUser.Id)
                    {
                        return Forbid();
                    }
                }

                var user = await _userManager.FindByIdAsync(student.UserId);
                ViewBag.User = user;

                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке ученика для удаления {StudentId}", id);
                TempData["ErrorMessage"] = "Ошибка при загрузке ученика.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: StudentManagement/Delete/5
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

                var student = await _studentService.GetByIdAsync(id);
                if (student == null)
                {
                    return NotFound();
                }

                // Проверяем доступ
                if (student.ClassId.HasValue)
                {
                    var @class = await _classService.GetByIdAsync(student.ClassId.Value);
                    if (@class != null && @class.TeacherId != currentUser.Id)
                    {
                        return Forbid();
                    }
                }

                var user = await _userManager.FindByIdAsync(student.UserId);
                var studentName = user != null ? user.FullName : "Ученик";

                // Удаляем студента
                await _studentService.DeleteAsync(id);

                // Удаляем пользователя
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                _logger.LogInformation("Учитель {TeacherId} удалил ученика {StudentId}: {StudentName}",
                    currentUser.Id, id, studentName);

                TempData["SuccessMessage"] = $"Ученик {studentName} успешно удален!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении ученика {StudentId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении ученика.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: StudentManagement/AssignToClass/5
        public async Task<IActionResult> AssignToClass(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var student = await _studentService.GetByIdAsync(id.Value);
                if (student == null)
                {
                    return NotFound();
                }

                // Проверяем доступ
                if (student.ClassId.HasValue)
                {
                    var @class = await _classService.GetByIdAsync(student.ClassId.Value);
                    if (@class != null && @class.TeacherId != currentUser.Id)
                    {
                        return Forbid();
                    }
                }

                var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                var activeClasses = classes.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
                ViewBag.Classes = new SelectList(activeClasses, "Id", "Name", student.ClassId);
                ViewBag.StudentId = id.Value;
                ViewBag.CurrentClassId = student.ClassId;

                return PartialView("_AssignToClassModal", student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке данных для назначения класса ученику {StudentId}", id);
                return NotFound();
            }
        }

        // POST: StudentManagement/AssignToClass/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToClass(int id, int? classId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var student = await _studentService.GetByIdAsync(id);
                if (student == null)
                {
                    return NotFound();
                }

                // Проверяем доступ
                if (student.ClassId.HasValue)
                {
                    var @class = await _classService.GetByIdAsync(student.ClassId.Value);
                    if (@class != null && @class.TeacherId != currentUser.Id)
                    {
                        return Forbid();
                    }
                }

                // Проверяем, что выбранный класс принадлежит учителю
                if (classId.HasValue)
                {
                    var selectedClass = await _classService.GetByIdAsync(classId.Value);
                    if (selectedClass == null || selectedClass.TeacherId != currentUser.Id)
                    {
                        TempData["ErrorMessage"] = "Выбранный класс не найден или у вас нет доступа к нему.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Обновляем класс ученика
                student.ClassId = classId;
                await _studentService.UpdateAsync(student);

                var user = await _userManager.FindByIdAsync(student.UserId);
                var studentName = user != null ? user.FullName : "Ученик";
                var className = classId.HasValue 
                    ? (await _classService.GetByIdAsync(classId.Value))?.Name ?? "класс"
                    : "не назначен";

                _logger.LogInformation("Учитель {TeacherId} назначил ученика {StudentId} ({StudentName}) в класс {ClassId}",
                    currentUser.Id, id, studentName, classId);

                TempData["SuccessMessage"] = $"Ученик {studentName} успешно назначен в {className}!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при назначении класса ученику {StudentId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при назначении класса.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Генерация номера ученика
        private async Task<string> GenerateStudentNumber()
        {
            var year = DateTime.Now.Year;
            var prefix = $"ST{year}";
            
            // Получаем всех студентов текущего учителя
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return $"{prefix}0001";
            }
            
            var students = await _studentService.GetByTeacherIdAsync(currentUser.Id);
            var lastNumber = 0;
            
            foreach (var student in students)
            {
                if (!string.IsNullOrEmpty(student.StudentNumber) && 
                    student.StudentNumber.StartsWith(prefix))
                {
                    var numberPart = student.StudentNumber.Replace(prefix, "");
                    if (int.TryParse(numberPart, out var num) && num > lastNumber)
                    {
                        lastNumber = num;
                    }
                }
            }
            
            return $"{prefix}{(lastNumber + 1):D4}";
        }
    }
}

