using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Teacher)]
    public class OrthoeopyTestController : Controller
    {
        private readonly IOrthoeopyTestService _testService;
        private readonly IAssignmentService _assignmentService;
        private readonly ISubjectService _subjectService;
        private readonly IOrthoeopyQuestionRepository _questionRepository;
        private readonly OnlineTutor3.Web.Services.OrthoeopyQuestionImportService _importService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrthoeopyTestController> _logger;

        public OrthoeopyTestController(
            IOrthoeopyTestService testService,
            IAssignmentService assignmentService,
            ISubjectService subjectService,
            IOrthoeopyQuestionRepository questionRepository,
            OnlineTutor3.Web.Services.OrthoeopyQuestionImportService importService,
            UserManager<ApplicationUser> userManager,
            ILogger<OrthoeopyTestController> logger)
        {
            _testService = testService;
            _assignmentService = assignmentService;
            _subjectService = subjectService;
            _questionRepository = questionRepository;
            _importService = importService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: OrthoeopyTest/Create?assignmentId=5
        public async Task<IActionResult> Create(int assignmentId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var canAccess = await _assignmentService.TeacherCanAccessAssignmentAsync(currentUser.Id, assignmentId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому заданию.";
                    return RedirectToAction("Index", "Assignment");
                }

                var assignment = await _assignmentService.GetByIdAsync(assignmentId);
                if (assignment == null)
                {
                    TempData["ErrorMessage"] = "Задание не найдено.";
                    return RedirectToAction("Index", "Assignment");
                }

                // Проверяем, что предмет - Русский язык
                var subject = await _subjectService.GetByIdAsync(assignment.SubjectId);
                if (subject == null || !subject.Name.Equals("Русский язык", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "Тесты по орфоэпии можно создавать только для предмета 'Русский язык'.";
                    return RedirectToAction("Index", "Assignment");
                }

                ViewBag.Assignment = assignment;
                var model = new CreateOrthoeopyTestViewModel
                {
                    AssignmentId = assignmentId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyTest/Create (GET) для AssignmentId: {AssignmentId}", assignmentId);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы создания теста.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: OrthoeopyTest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrthoeopyTestViewModel model)
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
                    var canAccess = await _assignmentService.TeacherCanAccessAssignmentAsync(currentUser.Id, model.AssignmentId);
                    if (!canAccess)
                    {
                        ModelState.AddModelError("AssignmentId", "У вас нет доступа к этому заданию.");
                    }
                    else
                    {
                        // Проверяем, что предмет - Русский язык
                        var assignmentToCheck = await _assignmentService.GetByIdAsync(model.AssignmentId);
                        if (assignmentToCheck != null)
                        {
                            var subject = await _subjectService.GetByIdAsync(assignmentToCheck.SubjectId);
                            if (subject == null || !subject.Name.Equals("Русский язык", StringComparison.OrdinalIgnoreCase))
                            {
                                ModelState.AddModelError("AssignmentId", "Тесты по орфоэпии можно создавать только для предмета 'Русский язык'.");
                            }
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    var test = new OrthoeopyTest
                        {
                            Title = model.Title,
                            Description = model.Description,
                            AssignmentId = model.AssignmentId,
                            TeacherId = currentUser.Id,
                            TimeLimit = model.TimeLimit,
                            MaxAttempts = model.MaxAttempts,
                            StartDate = model.StartDate,
                            EndDate = model.EndDate,
                            ShowHints = model.ShowHints,
                            ShowCorrectAnswers = model.ShowCorrectAnswers,
                            IsActive = model.IsActive,
                            CreatedAt = DateTime.Now
                        };

                    var testId = await _testService.CreateAsync(test);

                    TempData["SuccessMessage"] = $"Тест по орфоэпии \"{test.Title}\" успешно создан! Теперь добавьте вопросы.";
                    return RedirectToAction("Details", new { id = testId });
                }

                var assignment = await _assignmentService.GetByIdAsync(model.AssignmentId);
                if (assignment != null)
                {
                    ViewBag.Assignment = assignment;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyTest/Create (POST)");
                TempData["ErrorMessage"] = "Произошла ошибка при создании теста.";

                var assignment = await _assignmentService.GetByIdAsync(model.AssignmentId);
                if (assignment != null)
                {
                    ViewBag.Assignment = assignment;
                }

                return View(model);
            }
        }

        // GET: OrthoeopyTest/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var test = await _testService.GetByIdAsync(id);
                if (test == null)
                {
                    TempData["ErrorMessage"] = "Тест не найден.";
                    return RedirectToAction("Index", "Assignment");
                }

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, id);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому тесту.";
                    return RedirectToAction("Index", "Assignment");
                }

                var assignment = await _assignmentService.GetByIdAsync(test.AssignmentId);
                ViewBag.Assignment = assignment;

                var questions = await _questionRepository.GetByTestIdOrderedAsync(id);
                ViewBag.Questions = questions;

                return View(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyTest/Details для ID: {TestId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке теста.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // GET: OrthoeopyTest/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var test = await _testService.GetByIdAsync(id);
                if (test == null)
                {
                    TempData["ErrorMessage"] = "Тест не найден.";
                    return RedirectToAction("Index", "Assignment");
                }

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, id);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому тесту.";
                    return RedirectToAction("Index", "Assignment");
                }

                var assignment = await _assignmentService.GetByIdAsync(test.AssignmentId);
                ViewBag.Assignment = assignment;

                // Загружаем список доступных заданий учителя для выпадающего списка (только для предмета "Русский язык")
                var allAssignments = await _assignmentService.GetByTeacherIdAsync(currentUser.Id) ?? new List<Assignment>();
                var russianLanguageAssignments = new List<Assignment>();
                foreach (var ass in allAssignments)
                {
                    var subject = await _subjectService.GetByIdAsync(ass.SubjectId);
                    if (subject != null && subject.Name.Equals("Русский язык", StringComparison.OrdinalIgnoreCase))
                    {
                        russianLanguageAssignments.Add(ass);
                    }
                }
                ViewBag.Assignments = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(russianLanguageAssignments, "Id", "Title", test.AssignmentId);

                var model = new CreateOrthoeopyTestViewModel
                {
                    Title = test.Title,
                    Description = test.Description,
                    AssignmentId = test.AssignmentId,
                    TimeLimit = test.TimeLimit,
                    MaxAttempts = test.MaxAttempts,
                    StartDate = test.StartDate,
                    EndDate = test.EndDate,
                    ShowHints = test.ShowHints,
                    ShowCorrectAnswers = test.ShowCorrectAnswers,
                    IsActive = test.IsActive
                };

                ViewBag.TestId = id;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyTest/Edit (GET) для ID: {TestId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы редактирования.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: OrthoeopyTest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateOrthoeopyTestViewModel model)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Неверный идентификатор теста.";
                return RedirectToAction("Index", "Assignment");
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
                    var test = await _testService.GetByIdAsync(id);
                    if (test == null)
                    {
                        TempData["ErrorMessage"] = "Тест не найден.";
                        return RedirectToAction("Index", "Assignment");
                    }

                    var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, id);
                    if (!canAccess)
                    {
                        TempData["ErrorMessage"] = "У вас нет доступа к этому тесту.";
                        return RedirectToAction("Index", "Assignment");
                    }

                    // Проверяем доступ к новому заданию, если оно изменилось
                    if (test.AssignmentId != model.AssignmentId)
                    {
                        var canAccessNewAssignment = await _assignmentService.TeacherCanAccessAssignmentAsync(currentUser.Id, model.AssignmentId);
                        if (!canAccessNewAssignment)
                        {
                            ModelState.AddModelError("AssignmentId", "У вас нет доступа к выбранному заданию.");
                        }
                        else
                        {
                            // Проверяем, что новое задание для предмета "Русский язык"
                            var assignmentToCheck = await _assignmentService.GetByIdAsync(model.AssignmentId);
                            if (assignmentToCheck != null)
                            {
                                var subject = await _subjectService.GetByIdAsync(assignmentToCheck.SubjectId);
                                if (subject == null || !subject.Name.Equals("Русский язык", StringComparison.OrdinalIgnoreCase))
                                {
                                    ModelState.AddModelError("AssignmentId", "Тесты по орфоэпии можно привязывать только к заданиям по предмету 'Русский язык'.");
                                }
                            }
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        test.Title = model.Title;
                        test.Description = model.Description;
                        test.AssignmentId = model.AssignmentId;
                        test.TimeLimit = model.TimeLimit;
                        test.MaxAttempts = model.MaxAttempts;
                        test.StartDate = model.StartDate;
                        test.EndDate = model.EndDate;
                        test.ShowHints = model.ShowHints;
                        test.ShowCorrectAnswers = model.ShowCorrectAnswers;
                        test.IsActive = model.IsActive;

                        await _testService.UpdateAsync(test);

                        TempData["SuccessMessage"] = $"Тест \"{test.Title}\" успешно обновлен!";
                        return RedirectToAction("Details", new { id = id });
                    }
                }

                var assignment = await _assignmentService.GetByIdAsync(model.AssignmentId);
                if (assignment != null)
                {
                    ViewBag.Assignment = assignment;
                }

                // Загружаем список доступных заданий учителя для выпадающего списка (только для предмета "Русский язык")
                var allAssignments = await _assignmentService.GetByTeacherIdAsync(currentUser.Id) ?? new List<Assignment>();
                var russianLanguageAssignments = new List<Assignment>();
                foreach (var ass in allAssignments)
                {
                    var subject = await _subjectService.GetByIdAsync(ass.SubjectId);
                    if (subject != null && subject.Name.Equals("Русский язык", StringComparison.OrdinalIgnoreCase))
                    {
                        russianLanguageAssignments.Add(ass);
                    }
                }
                ViewBag.Assignments = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(russianLanguageAssignments, "Id", "Title", model.AssignmentId);

                ViewBag.TestId = id;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyTest/Edit (POST) для ID: {TestId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при обновлении теста.";

                var currentUser = await _userManager.GetUserAsync(User);
                var assignment = await _assignmentService.GetByIdAsync(model.AssignmentId);
                if (assignment != null)
                {
                    ViewBag.Assignment = assignment;
                }

                // Загружаем список доступных заданий учителя для выпадающего списка (только для предмета "Русский язык")
                if (currentUser != null)
                {
                    var allAssignments = await _assignmentService.GetByTeacherIdAsync(currentUser.Id) ?? new List<Assignment>();
                    var russianLanguageAssignments = new List<Assignment>();
                    foreach (var ass in allAssignments)
                    {
                        var subject = await _subjectService.GetByIdAsync(ass.SubjectId);
                        if (subject != null && subject.Name.Equals("Русский язык", StringComparison.OrdinalIgnoreCase))
                        {
                            russianLanguageAssignments.Add(ass);
                        }
                    }
                    ViewBag.Assignments = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(russianLanguageAssignments, "Id", "Title", model.AssignmentId);
                }
                else
                {
                    ViewBag.Assignments = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new List<Assignment>(), "Id", "Title", model.AssignmentId);
                }

                ViewBag.TestId = id;

                return View(model);
            }
        }

        // GET: OrthoeopyTest/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var test = await _testService.GetByIdAsync(id);
                if (test == null)
                {
                    TempData["ErrorMessage"] = "Тест не найден.";
                    return RedirectToAction("Index", "Assignment");
                }

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, id);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому тесту.";
                    return RedirectToAction("Index", "Assignment");
                }

                var assignment = await _assignmentService.GetByIdAsync(test.AssignmentId);
                ViewBag.Assignment = assignment;

                var questions = await _questionRepository.GetByTestIdAsync(id);
                ViewBag.QuestionsCount = questions.Count;

                return View(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyTest/Delete (GET) для ID: {TestId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке теста для удаления.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: OrthoeopyTest/Delete/5
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

                var test = await _testService.GetByIdAsync(id);
                if (test == null)
                {
                    TempData["ErrorMessage"] = "Тест не найден.";
                    return RedirectToAction("Index", "Assignment");
                }

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, id);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому тесту.";
                    return RedirectToAction("Index", "Assignment");
                }

                var title = test.Title;
                var assignmentId = test.AssignmentId;
                await _testService.DeleteAsync(id);

                TempData["SuccessMessage"] = $"Тест \"{title}\" успешно удален!";
                return RedirectToAction("Details", "Assignment", new { id = assignmentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyTest/Delete (POST) для ID: {TestId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении теста.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        #region Question Import

        // GET: OrthoeopyTest/ImportQuestions/5
        public async Task<IActionResult> ImportQuestions(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var test = await _testService.GetByIdAsync(id);
                if (test == null)
                {
                    TempData["ErrorMessage"] = "Тест не найден.";
                    return RedirectToAction("Index", "Assignment");
                }

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, id);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому тесту.";
                    return RedirectToAction("Index", "Assignment");
                }

                ViewBag.Test = test;
                return View(new OrthoeopyQuestionImportViewModel { OrthoeopyTestId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyTest/ImportQuestions (GET) для ID: {TestId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы импорта.";
                return RedirectToAction("Details", new { id });
            }
        }

        // POST: OrthoeopyTest/ImportQuestions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportQuestions(OrthoeopyQuestionImportViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var test = await _testService.GetByIdAsync(model.OrthoeopyTestId);
                if (test == null)
                {
                    TempData["ErrorMessage"] = "Тест не найден.";
                    return RedirectToAction("Index", "Assignment");
                }

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, model.OrthoeopyTestId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому тесту.";
                    return RedirectToAction("Index", "Assignment");
                }

                ViewBag.Test = test;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (model.ExcelFile == null || model.ExcelFile.Length == 0)
                {
                    ModelState.AddModelError("ExcelFile", "Выберите файл для импорта.");
                    return View(model);
                }

                if (model.ExcelFile.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("ExcelFile", "Размер файла не должен превышать 10 МБ.");
                    return View(model);
                }

                var allowedExtensions = new[] { ".xlsx", ".xls" };
                var fileExtension = Path.GetExtension(model.ExcelFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ExcelFile", "Поддерживаются только файлы .xlsx и .xls.");
                    return View(model);
                }

                var questions = await _importService.ParseExcelFileAsync(model.ExcelFile);

                if (questions == null || !questions.Any())
                {
                    TempData["ErrorMessage"] = "Файл не содержит данных для импорта.";
                    return View(model);
                }

                var sessionKey = $"ImportQuestions_{model.OrthoeopyTestId}_{DateTime.Now.Ticks}";
                var importData = new
                {
                    TestId = model.OrthoeopyTestId,
                    Questions = questions,
                    PointsPerQuestion = model.PointsPerQuestion
                };

                HttpContext.Session.SetString(sessionKey, System.Text.Json.JsonSerializer.Serialize(importData));
                TempData["ImportSessionKey"] = sessionKey;

                return RedirectToAction(nameof(PreviewQuestions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyTest/ImportQuestions (POST)");
                TempData["ErrorMessage"] = $"Ошибка при импорте: {ex.Message}";
                
                var test = await _testService.GetByIdAsync(model.OrthoeopyTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                }
                
                return View(model);
            }
        }

        // GET: OrthoeopyTest/DownloadTemplate
        public async Task<IActionResult> DownloadTemplate()
        {
            try
            {
                var templateBytes = await _importService.GenerateTemplateAsync();
                return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Шаблон_импорта_вопросов_орфоэпия.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при генерации шаблона импорта");
                TempData["ErrorMessage"] = "Ошибка при генерации шаблона.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // GET: OrthoeopyTest/PreviewQuestions
        public async Task<IActionResult> PreviewQuestions()
        {
            try
            {
                var sessionKey = TempData["ImportSessionKey"] as string;
                if (string.IsNullOrEmpty(sessionKey))
                {
                    TempData["ErrorMessage"] = "Данные импорта не найдены.";
                    return RedirectToAction("Index", "Assignment");
                }

                var importDataJson = HttpContext.Session.GetString(sessionKey);
                if (string.IsNullOrEmpty(importDataJson))
                {
                    TempData["ErrorMessage"] = "Данные импорта истекли.";
                    return RedirectToAction("Index", "Assignment");
                }

                var importData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(importDataJson);
                var testId = importData.GetProperty("TestId").GetInt32();
                var pointsPerQuestion = importData.GetProperty("PointsPerQuestion").GetInt32();

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var test = await _testService.GetByIdAsync(testId);
                if (test == null || !await _testService.TeacherCanAccessTestAsync(currentUser.Id, testId))
                {
                    TempData["ErrorMessage"] = "Тест не найден или нет доступа.";
                    return RedirectToAction("Index", "Assignment");
                }

                var questions = new List<ImportOrthoeopyQuestionRow>();
                foreach (var questionElement in importData.GetProperty("Questions").EnumerateArray())
                {
                    var question = System.Text.Json.JsonSerializer.Deserialize<ImportOrthoeopyQuestionRow>(questionElement.GetRawText());
                    if (question != null) questions.Add(question);
                }

                ViewBag.Test = test;
                ViewBag.PointsPerQuestion = pointsPerQuestion;
                TempData["ImportSessionKey"] = sessionKey;

                return View(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyTest/PreviewQuestions");
                TempData["ErrorMessage"] = "Ошибка обработки данных.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: OrthoeopyTest/ConfirmImport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmImport()
        {
            try
            {
                var sessionKey = TempData["ImportSessionKey"] as string;
                if (string.IsNullOrEmpty(sessionKey))
                {
                    TempData["ErrorMessage"] = "Данные импорта не найдены.";
                    return RedirectToAction("Index", "Assignment");
                }

                var importDataJson = HttpContext.Session.GetString(sessionKey);
                if (string.IsNullOrEmpty(importDataJson))
                {
                    TempData["ErrorMessage"] = "Данные импорта истекли.";
                    return RedirectToAction("Index", "Assignment");
                }

                var importData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(importDataJson);
                var testId = importData.GetProperty("TestId").GetInt32();
                var pointsPerQuestion = importData.GetProperty("PointsPerQuestion").GetInt32();

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var test = await _testService.GetByIdAsync(testId);
                if (test == null || !await _testService.TeacherCanAccessTestAsync(currentUser.Id, testId))
                {
                    TempData["ErrorMessage"] = "Тест не найден или нет доступа.";
                    return RedirectToAction("Index", "Assignment");
                }

                var questions = new List<ImportOrthoeopyQuestionRow>();
                foreach (var questionElement in importData.GetProperty("Questions").EnumerateArray())
                {
                    var question = System.Text.Json.JsonSerializer.Deserialize<ImportOrthoeopyQuestionRow>(questionElement.GetRawText());
                    if (question != null && question.IsValid) questions.Add(question);
                }

                var existingQuestions = await _questionRepository.GetByTestIdAsync(testId);
                var nextOrderIndex = existingQuestions.Count > 0 
                    ? existingQuestions.Max(q => q.OrderIndex) + 1 
                    : 1;

                int importedCount = 0;
                foreach (var importQuestion in questions)
                {
                    try
                    {
                        var question = new OrthoeopyQuestion
                        {
                            OrthoeopyTestId = testId,
                            OrderIndex = nextOrderIndex++,
                            Points = pointsPerQuestion,
                            Word = importQuestion.Word,
                            StressPosition = importQuestion.StressPosition,
                            WordWithStress = importQuestion.WordWithStress,
                            WrongStressPositions = importQuestion.WrongStressPositions,
                            Hint = importQuestion.Hint
                        };

                        await _questionRepository.CreateAsync(question);
                        importedCount++;
                    }
                    catch
                    {
                        // Пропускаем невалидные вопросы
                    }
                }

                HttpContext.Session.Remove(sessionKey);

                TempData["SuccessMessage"] = $"Успешно импортировано {importedCount} вопросов!";
                return RedirectToAction("Details", new { id = testId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyTest/ConfirmImport");
                TempData["ErrorMessage"] = "Ошибка при импорте вопросов.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        #endregion
    }
}

