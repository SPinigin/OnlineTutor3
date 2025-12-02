using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Controllers
{
    [Authorize]
    public class CalendarController : Controller
    {
        private readonly ICalendarService _calendarService;
        private readonly IClassService _classService;
        private readonly IStudentService _studentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CalendarController> _logger;

        public CalendarController(
            ICalendarService calendarService,
            IClassService classService,
            IStudentService studentService,
            UserManager<ApplicationUser> userManager,
            ILogger<CalendarController> logger)
        {
            _calendarService = calendarService;
            _classService = classService;
            _studentService = studentService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Calendar
        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var now = DateTime.Now;
                
                // Проверяем роль пользователя
                var isTeacher = await _userManager.IsInRoleAsync(currentUser, ApplicationRoles.Teacher);
                
                if (isTeacher)
                {
                    // Для учителя - события учителя
                    var upcomingEvents = await _calendarService.GetUpcomingCountAsync(currentUser.Id, now);
                    var todayEvents = await _calendarService.GetTodayCountAsync(currentUser.Id, now);
                    var completedThisMonth = await _calendarService.GetCompletedThisMonthCountAsync(currentUser.Id, now);

                    ViewBag.UpcomingEvents = upcomingEvents;
                    ViewBag.TodayEvents = todayEvents;
                    ViewBag.CompletedThisMonth = completedThisMonth;
                }
                else if (await _userManager.IsInRoleAsync(currentUser, ApplicationRoles.Student))
                {
                    // Для студента - получаем события его класса и напрямую связанные с ним
                    var student = await _studentService.GetByUserIdAsync(currentUser.Id);
                    if (student != null)
                    {
                        var studentEvents = new List<CalendarEvent>();
                        
                        // События класса студента
                        if (student.ClassId.HasValue)
                        {
                            var classEvents = await _calendarService.GetByClassIdAsync(student.ClassId.Value);
                            studentEvents.AddRange(classEvents);
                        }
                        
                        // События, напрямую связанные со студентом
                        var directStudentEvents = await _calendarService.GetByStudentIdAsync(student.Id);
                        studentEvents.AddRange(directStudentEvents);
                        
                        // Убираем дубликаты
                        studentEvents = studentEvents.GroupBy(e => e.Id).Select(g => g.First()).ToList();
                        
                        // Подсчитываем статистику
                        var upcomingCount = studentEvents.Count(e => e.StartDateTime > now && !e.IsCompleted);
                        var todayCount = studentEvents.Count(e => e.StartDateTime.Date == now.Date && !e.IsCompleted);
                        var completedCount = studentEvents.Count(e => e.IsCompleted && e.StartDateTime.Month == now.Month && e.StartDateTime.Year == now.Year);

                        ViewBag.UpcomingEvents = upcomingCount;
                        ViewBag.TodayEvents = todayCount;
                        ViewBag.CompletedThisMonth = completedCount;
                    }
                    else
                    {
                        ViewBag.UpcomingEvents = 0;
                        ViewBag.TodayEvents = 0;
                        ViewBag.CompletedThisMonth = 0;
                    }
                }
                else
                {
                    ViewBag.UpcomingEvents = 0;
                    ViewBag.TodayEvents = 0;
                    ViewBag.CompletedThisMonth = 0;
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Calendar/Index");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке календаря.";
                return View();
            }
        }

        // GET: Calendar/Create
        [Authorize(Roles = ApplicationRoles.Teacher)]
        public async Task<IActionResult> Create(DateTime? date, int? classId, int? studentId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var model = new CreateCalendarEventViewModel
                {
                    StartDateTime = date ?? DateTime.Now,
                    EndDateTime = (date ?? DateTime.Now).AddHours(1),
                    ClassId = classId,
                    StudentId = studentId,
                    Color = "#007bff"
                };

                await LoadSelectLists();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Calendar/Create (GET)");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Calendar/Create
        [HttpPost]
        [Authorize(Roles = ApplicationRoles.Teacher)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCalendarEventViewModel model)
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
                    // Валидация: должен быть выбран класс или ученик
                    if (!model.ClassId.HasValue && !model.StudentId.HasValue)
                    {
                        ModelState.AddModelError("", "Выберите класс или ученика для занятия");
                        await LoadSelectLists();
                        return View(model);
                    }

                    // Валидация: нельзя выбрать и класс и ученика одновременно
                    if (model.ClassId.HasValue && model.StudentId.HasValue)
                    {
                        ModelState.AddModelError("", "Выберите либо класс, либо ученика, но не оба");
                        await LoadSelectLists();
                        return View(model);
                    }

                    // Валидация дат
                    if (model.EndDateTime <= model.StartDateTime)
                    {
                        ModelState.AddModelError("EndDateTime", "Время окончания должно быть позже времени начала");
                        await LoadSelectLists();
                        return View(model);
                    }

                    // Проверяем, что класс или ученик принадлежит текущему учителю
                    if (model.ClassId.HasValue)
                    {
                        var @class = await _classService.GetByIdAsync(model.ClassId.Value);
                        if (@class == null || @class.TeacherId != currentUser.Id)
                        {
                            TempData["ErrorMessage"] = "Указанный класс не найден";
                            return RedirectToAction(nameof(Index));
                        }
                    }

                    if (model.StudentId.HasValue)
                    {
                        var student = await _studentService.GetByIdAsync(model.StudentId.Value);
                        if (student == null || !student.ClassId.HasValue)
                        {
                            TempData["ErrorMessage"] = "Указанный ученик не найден или не состоит в вашем классе";
                            return RedirectToAction(nameof(Index));
                        }
                        var studentClass = await _classService.GetByIdAsync(student.ClassId.Value);
                        if (studentClass == null || studentClass.TeacherId != currentUser.Id)
                        {
                            TempData["ErrorMessage"] = "Указанный ученик не найден или не состоит в вашем классе";
                            return RedirectToAction(nameof(Index));
                        }
                    }

                    // Дополнительная валидация повторяющихся событий
                    if (model.IsRecurring && string.IsNullOrWhiteSpace(model.RecurrencePattern))
                    {
                        ModelState.AddModelError("RecurrencePattern", "Выберите периодичность повторения для повторяющегося события");
                        await LoadSelectLists();
                        return View(model);
                    }

                    // Округляем секунды до 0
                    model.StartDateTime = new DateTime(
                        model.StartDateTime.Year,
                        model.StartDateTime.Month,
                        model.StartDateTime.Day,
                        model.StartDateTime.Hour,
                        model.StartDateTime.Minute,
                        0
                    );

                    model.EndDateTime = new DateTime(
                        model.EndDateTime.Year,
                        model.EndDateTime.Month,
                        model.EndDateTime.Day,
                        model.EndDateTime.Hour,
                        model.EndDateTime.Minute,
                        0
                    );

                    var calendarEvent = new CalendarEvent
                    {
                        Title = model.Title,
                        Description = model.Description,
                        StartDateTime = model.StartDateTime,
                        EndDateTime = model.EndDateTime,
                        TeacherId = currentUser.Id,
                        ClassId = model.ClassId,
                        StudentId = model.StudentId,
                        Location = model.Location,
                        Color = model.Color ?? "#007bff",
                        IsRecurring = model.IsRecurring,
                        RecurrencePattern = model.RecurrencePattern,
                        CreatedAt = DateTime.Now
                    };

                    await _calendarService.CreateAsync(calendarEvent);

                    TempData["SuccessMessage"] = "Занятие успешно добавлено в календарь!";
                    return RedirectToAction(nameof(Index));
                }

                await LoadSelectLists();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Calendar/Create (POST)");
                TempData["ErrorMessage"] = "Произошла ошибка при создании занятия.";
                await LoadSelectLists();
                return View(model);
            }
        }

        // GET: Calendar/Edit/5
        [Authorize(Roles = ApplicationRoles.Teacher)]
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var calendarEvent = await _calendarService.GetByIdWithRelationsAsync(id.Value, currentUser.Id);
                if (calendarEvent == null) return NotFound();

                var model = new EditCalendarEventViewModel
                {
                    Id = calendarEvent.Id,
                    Title = calendarEvent.Title,
                    Description = calendarEvent.Description,
                    StartDateTime = calendarEvent.StartDateTime,
                    EndDateTime = calendarEvent.EndDateTime,
                    ClassId = calendarEvent.ClassId,
                    StudentId = calendarEvent.StudentId,
                    Location = calendarEvent.Location,
                    Color = calendarEvent.Color,
                    IsRecurring = calendarEvent.IsRecurring,
                    RecurrencePattern = calendarEvent.RecurrencePattern,
                    IsCompleted = calendarEvent.IsCompleted,
                    Notes = calendarEvent.Notes
                };

                await LoadSelectLists();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Calendar/Edit (GET) для ID: {EventId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Calendar/Edit/5
        [HttpPost]
        [Authorize(Roles = ApplicationRoles.Teacher)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditCalendarEventViewModel model)
        {
            if (id != model.Id) return NotFound();

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                if (ModelState.IsValid)
                {
                    if (!model.ClassId.HasValue && !model.StudentId.HasValue)
                    {
                        ModelState.AddModelError("", "Выберите класс или ученика для занятия");
                        await LoadSelectLists();
                        return View(model);
                    }

                    if (model.ClassId.HasValue && model.StudentId.HasValue)
                    {
                        ModelState.AddModelError("", "Выберите либо класс, либо ученика, но не оба");
                        await LoadSelectLists();
                        return View(model);
                    }

                    if (model.EndDateTime <= model.StartDateTime)
                    {
                        ModelState.AddModelError("EndDateTime", "Время окончания должно быть позже времени начала");
                        await LoadSelectLists();
                        return View(model);
                    }

                    if (model.IsRecurring && string.IsNullOrWhiteSpace(model.RecurrencePattern))
                    {
                        ModelState.AddModelError("RecurrencePattern", "Выберите периодичность повторения для повторяющегося события");
                        await LoadSelectLists();
                        return View(model);
                    }

                    // Округляем секунды до 0
                    model.StartDateTime = new DateTime(
                        model.StartDateTime.Year,
                        model.StartDateTime.Month,
                        model.StartDateTime.Day,
                        model.StartDateTime.Hour,
                        model.StartDateTime.Minute,
                        0
                    );

                    model.EndDateTime = new DateTime(
                        model.EndDateTime.Year,
                        model.EndDateTime.Month,
                        model.EndDateTime.Day,
                        model.EndDateTime.Hour,
                        model.EndDateTime.Minute,
                        0
                    );

                    var calendarEvent = await _calendarService.GetByIdWithRelationsAsync(id, currentUser.Id);
                    if (calendarEvent == null) return NotFound();

                    calendarEvent.Title = model.Title;
                    calendarEvent.Description = model.Description;
                    calendarEvent.StartDateTime = model.StartDateTime;
                    calendarEvent.EndDateTime = model.EndDateTime;
                    calendarEvent.ClassId = model.ClassId;
                    calendarEvent.StudentId = model.StudentId;
                    calendarEvent.Location = model.Location;
                    calendarEvent.Color = model.Color ?? "#007bff";
                    calendarEvent.IsRecurring = model.IsRecurring;
                    calendarEvent.RecurrencePattern = model.RecurrencePattern;
                    calendarEvent.IsCompleted = model.IsCompleted;
                    calendarEvent.Notes = model.Notes;

                    await _calendarService.UpdateAsync(calendarEvent);

                    TempData["SuccessMessage"] = "Занятие успешно обновлено!";
                    return RedirectToAction(nameof(Index));
                }

                await LoadSelectLists();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Calendar/Edit (POST) для ID: {EventId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при обновлении занятия.";
                await LoadSelectLists();
                return View(model);
            }
        }

        // GET: Calendar/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                // Проверяем роль пользователя
                var isTeacher = await _userManager.IsInRoleAsync(currentUser, ApplicationRoles.Teacher);
                CalendarEvent? calendarEvent;
                
                if (isTeacher)
                {
                    // Для учителя - используем существующий метод
                    calendarEvent = await _calendarService.GetByIdWithRelationsAsync(id.Value, currentUser.Id);
                }
                else if (await _userManager.IsInRoleAsync(currentUser, ApplicationRoles.Student))
                {
                    // Для студента - проверяем доступ к событию
                    calendarEvent = await _calendarService.GetByIdAsync(id.Value);
                    if (calendarEvent == null) return NotFound();
                    
                    // Проверяем, что событие связано с классом или студентом
                    var student = await _studentService.GetByUserIdAsync(currentUser.Id);
                    if (student == null) return NotFound();
                    
                    bool hasAccess = false;
                    
                    // Проверяем, связано ли событие с классом студента
                    if (calendarEvent.ClassId.HasValue && student.ClassId.HasValue)
                    {
                        hasAccess = calendarEvent.ClassId.Value == student.ClassId.Value;
                    }
                    
                    // Проверяем, связано ли событие напрямую со студентом
                    if (!hasAccess && calendarEvent.StudentId.HasValue)
                    {
                        hasAccess = calendarEvent.StudentId.Value == student.Id;
                    }
                    
                    if (!hasAccess)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
                
                if (calendarEvent == null) return NotFound();

                var @class = calendarEvent.ClassId.HasValue ? await _classService.GetByIdAsync(calendarEvent.ClassId.Value) : null;
                var eventStudent = calendarEvent.StudentId.HasValue ? await _studentService.GetByIdAsync(calendarEvent.StudentId.Value) : null;
                var studentUser = eventStudent != null ? await _userManager.FindByIdAsync(eventStudent.UserId) : null;

                var model = new CalendarEventDetailsViewModel
                {
                    Id = calendarEvent.Id,
                    Title = calendarEvent.Title,
                    Description = calendarEvent.Description,
                    StartDateTime = calendarEvent.StartDateTime,
                    EndDateTime = calendarEvent.EndDateTime,
                    ClassName = @class?.Name,
                    StudentName = studentUser?.FullName,
                    Location = calendarEvent.Location,
                    Color = calendarEvent.Color,
                    IsCompleted = calendarEvent.IsCompleted,
                    Notes = calendarEvent.Notes,
                    CreatedAt = calendarEvent.CreatedAt,
                    IsRecurring = calendarEvent.IsRecurring,
                    RecurrencePattern = calendarEvent.RecurrencePattern
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Calendar/Details для ID: {EventId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке занятия.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Calendar/Delete/5
        [Authorize(Roles = ApplicationRoles.Teacher)]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var calendarEvent = await _calendarService.GetByIdWithRelationsAsync(id.Value, currentUser.Id);
                if (calendarEvent == null) return NotFound();

                var @class = calendarEvent.ClassId.HasValue ? await _classService.GetByIdAsync(calendarEvent.ClassId.Value) : null;
                var student = calendarEvent.StudentId.HasValue ? await _studentService.GetByIdAsync(calendarEvent.StudentId.Value) : null;
                var studentUser = student != null ? await _userManager.FindByIdAsync(student.UserId) : null;

                ViewBag.ClassName = @class?.Name;
                ViewBag.StudentName = studentUser?.FullName;

                return View(calendarEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Calendar/Delete (GET) для ID: {EventId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке занятия для удаления.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Calendar/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = ApplicationRoles.Teacher)]
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

                var calendarEvent = await _calendarService.GetByIdWithRelationsAsync(id, currentUser.Id);
                if (calendarEvent == null) return NotFound();

                var eventTitle = calendarEvent.Title;
                await _calendarService.DeleteAsync(id);

                TempData["SuccessMessage"] = "Занятие удалено из календаря";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Calendar/Delete (POST) для ID: {EventId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении занятия.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Calendar/GetEvents - для AJAX запросов (FullCalendar)
        [HttpGet]
        public async Task<IActionResult> GetEvents(DateTime? start, DateTime? end)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                var searchStart = start ?? DateTime.Now.AddMonths(-3);
                var searchEnd = end ?? DateTime.Now.AddMonths(6);

                List<CalendarEvent> events;
                
                // Проверяем роль пользователя
                var isTeacher = await _userManager.IsInRoleAsync(currentUser, ApplicationRoles.Teacher);
                
                if (isTeacher)
                {
                    // Для учителя - события учителя
                    events = await _calendarService.GetByTeacherIdInDateRangeAsync(currentUser.Id, searchStart, searchEnd);
                }
                else if (await _userManager.IsInRoleAsync(currentUser, ApplicationRoles.Student))
                {
                    // Для студента - события его класса и напрямую связанные с ним
                    var student = await _studentService.GetByUserIdAsync(currentUser.Id);
                    if (student == null)
                    {
                        return Json(new List<object>());
                    }
                    
                    var studentEvents = new List<CalendarEvent>();
                    
                    // События класса студента
                    if (student.ClassId.HasValue)
                    {
                        var classEvents = await _calendarService.GetByClassIdAsync(student.ClassId.Value);
                        // Фильтруем по дате
                        studentEvents.AddRange(classEvents.Where(e => 
                            e.StartDateTime >= searchStart && e.StartDateTime <= searchEnd));
                    }
                    
                    // События, напрямую связанные со студентом
                    var directStudentEvents = await _calendarService.GetByStudentIdAsync(student.Id);
                    // Фильтруем по дате
                    studentEvents.AddRange(directStudentEvents.Where(e => 
                        e.StartDateTime >= searchStart && e.StartDateTime <= searchEnd));
                    
                    // Убираем дубликаты
                    events = studentEvents.GroupBy(e => e.Id).Select(g => g.First()).ToList();
                }
                else
                {
                    return Json(new List<object>());
                }

                var result = new List<object>();

                foreach (var e in events)
                {
                    if (e.IsRecurring && !string.IsNullOrEmpty(e.RecurrencePattern))
                    {
                        var recurringEvents = await GenerateRecurringEventsAsync(e, searchStart, searchEnd);
                        result.AddRange(recurringEvents);
                    }
                    else
                    {
                        result.Add(await CreateEventObjectAsync(e));
                    }
                }

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Calendar/GetEvents");
                return Json(new List<object>());
            }
        }

        // POST: Calendar/ToggleComplete/5
        [HttpPost]
        [Authorize(Roles = ApplicationRoles.Teacher)]
        public async Task<IActionResult> ToggleComplete(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var calendarEvent = await _calendarService.GetByIdWithRelationsAsync(id, currentUser.Id);
                if (calendarEvent == null) return NotFound();

                var oldStatus = calendarEvent.IsCompleted;
                calendarEvent.IsCompleted = !calendarEvent.IsCompleted;

                await _calendarService.UpdateAsync(calendarEvent);

                var status = calendarEvent.IsCompleted ? "завершено" : "возвращено в активные";
                TempData["InfoMessage"] = $"Занятие \"{calendarEvent.Title}\" {status}";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Calendar/ToggleComplete для ID: {EventId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при изменении статуса.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Вспомогательные методы
        private async Task<object> CreateEventObjectAsync(CalendarEvent e, DateTime? overrideStart = null, DateTime? overrideEnd = null)
        {
            var startTime = overrideStart ?? e.StartDateTime;
            var endTime = overrideEnd ?? e.EndDateTime;

            var @class = e.ClassId.HasValue ? await _classService.GetByIdAsync(e.ClassId.Value) : null;
            var student = e.StudentId.HasValue ? await _studentService.GetByIdAsync(e.StudentId.Value) : null;
            var studentUser = student != null ? await _userManager.FindByIdAsync(student.UserId) : null;

            var title = await GetEventTitleAsync(e);

            return new
            {
                id = e.Id,
                title = title,
                start = startTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = endTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                description = e.Description,
                className = @class?.Name,
                studentName = studentUser?.FullName,
                location = e.Location,
                color = e.Color,
                backgroundColor = e.Color,
                borderColor = e.Color,
                textColor = "#ffffff",
                isCompleted = e.IsCompleted,
                isRecurring = e.IsRecurring,
                extendedProps = new
                {
                    classId = e.ClassId,
                    studentId = e.StudentId,
                    location = e.Location,
                    notes = e.Notes,
                    originalTitle = e.Title,
                    isRecurringInstance = overrideStart.HasValue
                }
            };
        }

        private async Task<List<object>> GenerateRecurringEventsAsync(CalendarEvent calendarEvent, DateTime rangeStart, DateTime rangeEnd)
        {
            var events = new List<object>();
            var duration = calendarEvent.EndDateTime - calendarEvent.StartDateTime;
            var currentDate = calendarEvent.StartDateTime;
            var maxIterations = 365;
            var iterations = 0;

            while (currentDate <= rangeEnd && iterations < maxIterations)
            {
                iterations++;

                if (currentDate >= rangeStart && currentDate <= rangeEnd)
                {
                    var eventEnd = currentDate.Add(duration);
                    events.Add(await CreateEventObjectAsync(calendarEvent, currentDate, eventEnd));
                }

                currentDate = GetNextOccurrence(currentDate, calendarEvent.RecurrencePattern);

                if (currentDate <= calendarEvent.StartDateTime)
                {
                    break;
                }
            }

            return events;
        }

        private DateTime GetNextOccurrence(DateTime currentDate, string? recurrencePattern)
        {
            return recurrencePattern?.ToLower() switch
            {
                "daily" => currentDate.AddDays(1),
                "weekly" => currentDate.AddDays(7),
                "biweekly" => currentDate.AddDays(14),
                "monthly" => currentDate.AddMonths(1),
                _ => currentDate.AddDays(7)
            };
        }

        private async Task<string> GetEventTitleAsync(CalendarEvent calendarEvent)
        {
            if (calendarEvent.ClassId.HasValue)
            {
                var @class = await _classService.GetByIdAsync(calendarEvent.ClassId.Value);
                if (@class != null)
                {
                    return @class.Name;
                }
            }
            else if (calendarEvent.StudentId.HasValue)
            {
                var student = await _studentService.GetByIdAsync(calendarEvent.StudentId.Value);
                if (student != null)
                {
                    var studentUser = await _userManager.FindByIdAsync(student.UserId);
                    if (studentUser != null)
                    {
                        var lastName = studentUser.LastName;
                        var firstNameInitial = studentUser.FirstName?.FirstOrDefault();
                        return $"{lastName} {firstNameInitial}.";
                    }
                }
            }

            return calendarEvent.Title;
        }

        private async Task LoadSelectLists()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return;

                var classes = (await _classService.GetByTeacherIdAsync(currentUser.Id))
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToList();

                var teacherClasses = await _classService.GetByTeacherIdAsync(currentUser.Id);
                var teacherClassIds = teacherClasses.Select(c => c.Id).ToHashSet();

                var students = new List<Student>();
                foreach (var classId in teacherClassIds)
                {
                    var classStudents = await _studentService.GetByClassIdAsync(classId);
                    students.AddRange(classStudents);
                }

                var studentsWithUser = new List<object>();
                foreach (var student in students.OrderBy(s => s.UserId))
                {
                    var user = await _userManager.FindByIdAsync(student.UserId);
                    var className = teacherClasses.FirstOrDefault(c => c.Id == student.ClassId)?.Name ?? "Unknown";
                    studentsWithUser.Add(new
                    {
                        Id = student.Id,
                        Name = $"{user?.FullName ?? "Unknown"} ({className})"
                    });
                }

                ViewBag.Classes = new SelectList(classes, "Id", "Name");
                ViewBag.Students = new SelectList(studentsWithUser, "Id", "Name");

                ViewBag.RecurrencePatterns = new SelectList(new[]
                {
                    new { Value = "daily", Text = "Ежедневно" },
                    new { Value = "weekly", Text = "Еженедельно" },
                    new { Value = "biweekly", Text = "Раз в две недели" },
                    new { Value = "monthly", Text = "Ежемесячно" }
                }, "Value", "Text");

                ViewBag.Colors = new[]
                {
                    new { Value = "#007bff", Text = "Синий", Class = "primary" },
                    new { Value = "#28a745", Text = "Зеленый", Class = "success" },
                    new { Value = "#dc3545", Text = "Красный", Class = "danger" },
                    new { Value = "#ffc107", Text = "Желтый", Class = "warning" },
                    new { Value = "#6c757d", Text = "Серый", Class = "secondary" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке списков для формы календаря");
            }
        }
    }
}

