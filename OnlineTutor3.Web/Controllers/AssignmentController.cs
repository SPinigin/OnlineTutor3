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
    public class AssignmentController : Controller
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ISubjectService _subjectService;
        private readonly IClassService _classService;
        private readonly IAssignmentClassRepository _assignmentClassRepository;
        private readonly ITeacherService _teacherService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AssignmentController> _logger;

        public AssignmentController(
            IAssignmentService assignmentService,
            ISubjectService subjectService,
            IClassService classService,
            IAssignmentClassRepository assignmentClassRepository,
            ITeacherService teacherService,
            UserManager<ApplicationUser> userManager,
            ILogger<AssignmentController> logger)
        {
            _assignmentService = assignmentService;
            _subjectService = subjectService;
            _classService = classService;
            _assignmentClassRepository = assignmentClassRepository;
            _teacherService = teacherService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Assignment
        public async Task<IActionResult> Index(int? subjectId = null)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    _logger.LogWarning("Current user is null in Assignment/Index");
                    return Challenge();
                }

                // Получаем предметы, которые ведет учитель
                var teacherSubjects = await _teacherService.GetTeacherSubjectsByUserIdAsync(currentUser.Id);
                ViewBag.Subjects = new SelectList(teacherSubjects, "Id", "Name", subjectId);

                // Получаем задания учителя
                List<Assignment> assignments;
                if (subjectId.HasValue)
                {
                    assignments = await _assignmentService.GetByTeacherIdAndSubjectIdAsync(currentUser.Id, subjectId.Value);
                }
                else
                {
                    assignments = await _assignmentService.GetByTeacherIdAsync(currentUser.Id);
                }

                // Загружаем предметы для отображения
                var subjectsDict = teacherSubjects.ToDictionary(s => s.Id, s => s.Name);
                ViewBag.SubjectsDict = subjectsDict;

                return View(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Assignment/Index");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке заданий. Попробуйте обновить страницу.";
                return View(new List<Assignment>());
            }
        }

        // GET: Assignment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var assignment = await _assignmentService.GetByIdAsync(id);
                if (assignment == null)
                {
                    TempData["ErrorMessage"] = "Задание не найдено.";
                    return RedirectToAction(nameof(Index));
                }

                // Проверяем доступ
                var canAccess = await _assignmentService.TeacherCanAccessAssignmentAsync(currentUser.Id, id);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому заданию.";
                    return RedirectToAction(nameof(Index));
                }

                // Загружаем предмет
                var subject = await _subjectService.GetByIdAsync(assignment.SubjectId);
                ViewBag.Subject = subject;

                // Загружаем назначенные классы
                var assignmentClasses = await _assignmentClassRepository.GetByAssignmentIdAsync(id);
                var classIds = assignmentClasses.Select(ac => ac.ClassId).ToList();
                var classes = new List<Class>();
                foreach (var classId in classIds)
                {
                    var classEntity = await _classService.GetByIdAsync(classId);
                    if (classEntity != null)
                    {
                        classes.Add(classEntity);
                    }
                }
                ViewBag.Classes = classes;

                return View(assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Assignment/Details для ID: {AssignmentId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке задания.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Assignment/Create
        public async Task<IActionResult> Create(int? subjectId = null)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                // Получаем предметы, которые ведет учитель
                var teacherSubjects = await _teacherService.GetTeacherSubjectsByUserIdAsync(currentUser.Id);
                ViewBag.Subjects = new SelectList(teacherSubjects, "Id", "Name", subjectId);

                // Получаем классы учителя
                var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                var activeClasses = classes.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
                ViewBag.Classes = activeClasses;

                var model = new CreateAssignmentViewModel();
                if (subjectId.HasValue)
                {
                    model.SubjectId = subjectId.Value;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Assignment/Create (GET)");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы создания задания.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Assignment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAssignmentViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                if (ModelState.IsValid)
                {
                    // Проверяем, что учитель ведет этот предмет
                    var teachesSubject = await _teacherService.TeacherTeachesSubjectAsync(currentUser.Id, model.SubjectId);
                    if (!teachesSubject)
                    {
                        ModelState.AddModelError("SubjectId", "Вы не ведете этот предмет.");
                    }
                    else
                    {
                        var assignment = new Assignment
                        {
                            Title = model.Title,
                            Description = model.Description,
                            SubjectId = model.SubjectId,
                            TeacherId = currentUser.Id,
                            DueDate = model.DueDate,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        };

                        var assignmentId = await _assignmentService.CreateAsync(assignment);

                        // Назначаем классы
                        if (model.SelectedClassIds != null && model.SelectedClassIds.Any())
                        {
                            foreach (var classId in model.SelectedClassIds)
                            {
                                // Проверяем, что класс принадлежит учителю
                                var classEntity = await _classService.GetByIdAsync(classId);
                                if (classEntity != null && classEntity.TeacherId == currentUser.Id)
                                {
                                    var assignmentClass = new AssignmentClass
                                    {
                                        AssignmentId = assignmentId,
                                        ClassId = classId,
                                        AssignedAt = DateTime.Now
                                    };
                                    await _assignmentClassRepository.CreateAsync(assignmentClass);
                                }
                            }
                        }

                        TempData["SuccessMessage"] = $"Задание \"{assignment.Title}\" успешно создано!";
                        return RedirectToAction(nameof(Details), new { id = assignmentId });
                    }
                }

                // Если ошибка, загружаем данные для формы
                var teacherSubjects = await _teacherService.GetTeacherSubjectsByUserIdAsync(currentUser.Id);
                ViewBag.Subjects = new SelectList(teacherSubjects, "Id", "Name", model.SubjectId);

                var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                var activeClasses = classes.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
                ViewBag.Classes = activeClasses;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Assignment/Create (POST)");
                TempData["ErrorMessage"] = "Произошла ошибка при создании задания.";
                
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    var teacherSubjects = await _teacherService.GetTeacherSubjectsByUserIdAsync(currentUser.Id);
                    ViewBag.Subjects = new SelectList(teacherSubjects, "Id", "Name", model.SubjectId);

                    var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                    var activeClasses = classes.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
                    ViewBag.Classes = activeClasses;
                }

                return View(model);
            }
        }

        // GET: Assignment/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var assignment = await _assignmentService.GetByIdAsync(id);
                if (assignment == null)
                {
                    TempData["ErrorMessage"] = "Задание не найдено.";
                    return RedirectToAction(nameof(Index));
                }

                // Проверяем доступ
                var canAccess = await _assignmentService.TeacherCanAccessAssignmentAsync(currentUser.Id, id);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому заданию.";
                    return RedirectToAction(nameof(Index));
                }

                // Получаем предметы, которые ведет учитель
                var teacherSubjects = await _teacherService.GetTeacherSubjectsByUserIdAsync(currentUser.Id);
                ViewBag.Subjects = new SelectList(teacherSubjects, "Id", "Name", assignment.SubjectId);

                // Получаем классы учителя
                var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                var activeClasses = classes.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
                ViewBag.Classes = activeClasses;

                // Загружаем назначенные классы
                var assignmentClasses = await _assignmentClassRepository.GetByAssignmentIdAsync(id);
                var selectedClassIds = assignmentClasses.Select(ac => ac.ClassId).ToList();

                var model = new EditAssignmentViewModel
                {
                    Id = assignment.Id,
                    Title = assignment.Title,
                    Description = assignment.Description,
                    SubjectId = assignment.SubjectId,
                    DueDate = assignment.DueDate,
                    IsActive = assignment.IsActive,
                    SelectedClassIds = selectedClassIds
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Assignment/Edit (GET) для ID: {AssignmentId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы редактирования.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Assignment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditAssignmentViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Неверный идентификатор задания.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                if (ModelState.IsValid)
                {
                    var assignment = await _assignmentService.GetByIdAsync(id);
                    if (assignment == null)
                    {
                        TempData["ErrorMessage"] = "Задание не найдено.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Проверяем доступ
                    var canAccess = await _assignmentService.TeacherCanAccessAssignmentAsync(currentUser.Id, id);
                    if (!canAccess)
                    {
                        TempData["ErrorMessage"] = "У вас нет доступа к этому заданию.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Проверяем, что учитель ведет новый предмет (если изменился)
                    if (assignment.SubjectId != model.SubjectId)
                    {
                        var teachesSubject = await _teacherService.TeacherTeachesSubjectAsync(currentUser.Id, model.SubjectId);
                        if (!teachesSubject)
                        {
                            ModelState.AddModelError("SubjectId", "Вы не ведете этот предмет.");
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        assignment.Title = model.Title;
                        assignment.Description = model.Description;
                        assignment.SubjectId = model.SubjectId;
                        assignment.DueDate = model.DueDate;
                        assignment.IsActive = model.IsActive;

                        await _assignmentService.UpdateAsync(assignment);

                        // Обновляем назначенные классы
                        var existingAssignmentClasses = await _assignmentClassRepository.GetByAssignmentIdAsync(id);
                        var existingClassIds = existingAssignmentClasses.Select(ac => ac.ClassId).ToList();

                        // Удаляем классы, которые не выбраны
                        foreach (var existingClassId in existingClassIds)
                        {
                            if (!model.SelectedClassIds.Contains(existingClassId))
                            {
                                var assignmentClass = existingAssignmentClasses.First(ac => ac.ClassId == existingClassId);
                                await _assignmentClassRepository.DeleteAsync(assignmentClass.Id);
                            }
                        }

                        // Добавляем новые классы
                        foreach (var classId in model.SelectedClassIds)
                        {
                            if (!existingClassIds.Contains(classId))
                            {
                                // Проверяем, что класс принадлежит учителю
                                var classEntity = await _classService.GetByIdAsync(classId);
                                if (classEntity != null && classEntity.TeacherId == currentUser.Id)
                                {
                                    var assignmentClass = new AssignmentClass
                                    {
                                        AssignmentId = id,
                                        ClassId = classId,
                                        AssignedAt = DateTime.Now
                                    };
                                    await _assignmentClassRepository.CreateAsync(assignmentClass);
                                }
                            }
                        }

                        TempData["SuccessMessage"] = $"Задание \"{assignment.Title}\" успешно обновлено!";
                        return RedirectToAction(nameof(Details), new { id = id });
                    }
                }

                // Если ошибка, загружаем данные для формы
                var teacherSubjects = await _teacherService.GetTeacherSubjectsByUserIdAsync(currentUser.Id);
                ViewBag.Subjects = new SelectList(teacherSubjects, "Id", "Name", model.SubjectId);

                var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                var activeClasses = classes.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
                ViewBag.Classes = activeClasses;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Assignment/Edit (POST) для ID: {AssignmentId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при обновлении задания.";

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    var teacherSubjects = await _teacherService.GetTeacherSubjectsByUserIdAsync(currentUser.Id);
                    ViewBag.Subjects = new SelectList(teacherSubjects, "Id", "Name", model.SubjectId);

                    var classes = await _classService.GetByTeacherIdAsync(currentUser.Id);
                    var activeClasses = classes.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
                    ViewBag.Classes = activeClasses;
                }

                return View(model);
            }
        }

        // GET: Assignment/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var assignment = await _assignmentService.GetByIdAsync(id);
                if (assignment == null)
                {
                    TempData["ErrorMessage"] = "Задание не найдено.";
                    return RedirectToAction(nameof(Index));
                }

                // Проверяем доступ
                var canAccess = await _assignmentService.TeacherCanAccessAssignmentAsync(currentUser.Id, id);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому заданию.";
                    return RedirectToAction(nameof(Index));
                }

                // Загружаем предмет
                var subject = await _subjectService.GetByIdAsync(assignment.SubjectId);
                ViewBag.Subject = subject;

                // Загружаем назначенные классы
                var assignmentClasses = await _assignmentClassRepository.GetByAssignmentIdAsync(id);
                var classIds = assignmentClasses.Select(ac => ac.ClassId).ToList();
                var classes = new List<Class>();
                foreach (var classId in classIds)
                {
                    var classEntity = await _classService.GetByIdAsync(classId);
                    if (classEntity != null)
                    {
                        classes.Add(classEntity);
                    }
                }
                ViewBag.Classes = classes;

                return View(assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Assignment/Delete (GET) для ID: {AssignmentId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке задания для удаления.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Assignment/Delete/5
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

                var assignment = await _assignmentService.GetByIdAsync(id);
                if (assignment == null)
                {
                    TempData["ErrorMessage"] = "Задание не найдено.";
                    return RedirectToAction(nameof(Index));
                }

                // Проверяем доступ
                var canAccess = await _assignmentService.TeacherCanAccessAssignmentAsync(currentUser.Id, id);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому заданию.";
                    return RedirectToAction(nameof(Index));
                }

                var title = assignment.Title;
                await _assignmentService.DeleteAsync(id);

                TempData["SuccessMessage"] = $"Задание \"{title}\" успешно удалено!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Assignment/Delete (POST) для ID: {AssignmentId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении задания.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

