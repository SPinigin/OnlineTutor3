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
    public class SpellingTestController : Controller
    {
        private readonly ISpellingTestService _testService;
        private readonly IAssignmentService _assignmentService;
        private readonly ISubjectService _subjectService;
        private readonly ISpellingQuestionRepository _questionRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SpellingTestController> _logger;

        public SpellingTestController(
            ISpellingTestService testService,
            IAssignmentService assignmentService,
            ISubjectService subjectService,
            ISpellingQuestionRepository questionRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<SpellingTestController> logger)
        {
            _testService = testService;
            _assignmentService = assignmentService;
            _subjectService = subjectService;
            _questionRepository = questionRepository;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: SpellingTest/Create?assignmentId=5
        public async Task<IActionResult> Create(int assignmentId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                // Проверяем доступ к заданию
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
                    TempData["ErrorMessage"] = "Тесты по орфографии можно создавать только для предмета 'Русский язык'.";
                    return RedirectToAction("Index", "Assignment");
                }

                ViewBag.Assignment = assignment;
                var model = new CreateSpellingTestViewModel
                {
                    AssignmentId = assignmentId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в SpellingTest/Create (GET) для AssignmentId: {AssignmentId}", assignmentId);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы создания теста.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: SpellingTest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSpellingTestViewModel model)
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
                    // Проверяем доступ к заданию
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
                                ModelState.AddModelError("AssignmentId", "Тесты по орфографии можно создавать только для предмета 'Русский язык'.");
                            }
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    var test = new SpellingTest
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

                    TempData["SuccessMessage"] = $"Тест по орфографии \"{test.Title}\" успешно создан! Теперь добавьте вопросы.";
                    return RedirectToAction("Details", new { id = testId });
                }

                // Если ошибка, загружаем данные для формы
                var assignment = await _assignmentService.GetByIdAsync(model.AssignmentId);
                if (assignment != null)
                {
                    ViewBag.Assignment = assignment;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в SpellingTest/Create (POST)");
                TempData["ErrorMessage"] = "Произошла ошибка при создании теста.";

                var assignment = await _assignmentService.GetByIdAsync(model.AssignmentId);
                if (assignment != null)
                {
                    ViewBag.Assignment = assignment;
                }

                return View(model);
            }
        }

        // GET: SpellingTest/Details/5
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

                // Проверяем доступ
                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, id);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому тесту.";
                    return RedirectToAction("Index", "Assignment");
                }

                // Загружаем задание
                var assignment = await _assignmentService.GetByIdAsync(test.AssignmentId);
                ViewBag.Assignment = assignment;

                // Загружаем вопросы
                var questions = await _questionRepository.GetByTestIdOrderedAsync(id);
                ViewBag.Questions = questions;

                return View(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в SpellingTest/Details для ID: {TestId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке теста.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // GET: SpellingTest/Edit/5
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

                // Проверяем доступ
                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, id);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому тесту.";
                    return RedirectToAction("Index", "Assignment");
                }

                var assignment = await _assignmentService.GetByIdAsync(test.AssignmentId);
                ViewBag.Assignment = assignment;

                var model = new CreateSpellingTestViewModel
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
                _logger.LogError(ex, "Ошибка в SpellingTest/Edit (GET) для ID: {TestId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы редактирования.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: SpellingTest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateSpellingTestViewModel model)
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

                    // Проверяем доступ
                    var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, id);
                    if (!canAccess)
                    {
                        TempData["ErrorMessage"] = "У вас нет доступа к этому тесту.";
                        return RedirectToAction("Index", "Assignment");
                    }

                    test.Title = model.Title;
                    test.Description = model.Description;
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

                // Если ошибка, загружаем данные для формы
                var assignment = await _assignmentService.GetByIdAsync(model.AssignmentId);
                if (assignment != null)
                {
                    ViewBag.Assignment = assignment;
                }
                ViewBag.TestId = id;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в SpellingTest/Edit (POST) для ID: {TestId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при обновлении теста.";

                var assignment = await _assignmentService.GetByIdAsync(model.AssignmentId);
                if (assignment != null)
                {
                    ViewBag.Assignment = assignment;
                }
                ViewBag.TestId = id;

                return View(model);
            }
        }

        // GET: SpellingTest/Delete/5
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

                // Проверяем доступ
                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, id);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому тесту.";
                    return RedirectToAction("Index", "Assignment");
                }

                // Загружаем задание
                var assignment = await _assignmentService.GetByIdAsync(test.AssignmentId);
                ViewBag.Assignment = assignment;

                // Загружаем вопросы для предупреждения
                var questions = await _questionRepository.GetByTestIdAsync(id);
                ViewBag.QuestionsCount = questions.Count;

                return View(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в SpellingTest/Delete (GET) для ID: {TestId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке теста для удаления.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: SpellingTest/Delete/5
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

                // Проверяем доступ
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
                _logger.LogError(ex, "Ошибка в SpellingTest/Delete (POST) для ID: {TestId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении теста.";
                return RedirectToAction("Index", "Assignment");
            }
        }
    }
}

