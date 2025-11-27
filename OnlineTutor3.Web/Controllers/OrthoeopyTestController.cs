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
        private readonly IOrthoeopyQuestionRepository _questionRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrthoeopyTestController> _logger;

        public OrthoeopyTestController(
            IOrthoeopyTestService testService,
            IAssignmentService assignmentService,
            IOrthoeopyQuestionRepository questionRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<OrthoeopyTestController> logger)
        {
            _testService = testService;
            _assignmentService = assignmentService;
            _questionRepository = questionRepository;
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
                _logger.LogError(ex, "Ошибка в OrthoeopyTest/Edit (POST) для ID: {TestId}", id);
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
    }
}

