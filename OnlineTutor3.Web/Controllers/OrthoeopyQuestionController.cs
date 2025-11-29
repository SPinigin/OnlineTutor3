using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Teacher)]
    public class OrthoeopyQuestionController : Controller
    {
        private readonly IOrthoeopyQuestionRepository _questionRepository;
        private readonly IOrthoeopyTestService _testService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrthoeopyQuestionController> _logger;

        public OrthoeopyQuestionController(
            IOrthoeopyQuestionRepository questionRepository,
            IOrthoeopyTestService testService,
            UserManager<ApplicationUser> userManager,
            ILogger<OrthoeopyQuestionController> logger)
        {
            _questionRepository = questionRepository;
            _testService = testService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: OrthoeopyQuestion/Create?testId=5
        public async Task<IActionResult> Create(int testId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, testId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому тесту.";
                    return RedirectToAction("Details", "OrthoeopyTest", new { id = testId });
                }

                var test = await _testService.GetByIdAsync(testId);
                if (test == null)
                {
                    TempData["ErrorMessage"] = "Тест не найден.";
                    return RedirectToAction("Index", "Assignment");
                }

                var existingQuestions = await _questionRepository.GetByTestIdAsync(testId);
                var nextOrderIndex = existingQuestions.Count > 0 
                    ? existingQuestions.Max(q => q.OrderIndex) + 1 
                    : 1;

                ViewBag.Test = test;
                ViewBag.TestId = testId;
                ViewBag.NextOrderIndex = nextOrderIndex;

                var model = new CreateOrthoeopyQuestionViewModel
                {
                    OrthoeopyTestId = testId,
                    OrderIndex = nextOrderIndex,
                    Points = 1
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyQuestion/Create (GET) для TestId: {TestId}", testId);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы создания вопроса.";
                return RedirectToAction("Details", "OrthoeopyTest", new { id = testId });
            }
        }

        // POST: OrthoeopyQuestion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrthoeopyQuestionViewModel model)
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
                    var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, model.OrthoeopyTestId);
                    if (!canAccess)
                    {
                        ModelState.AddModelError("OrthoeopyTestId", "У вас нет доступа к этому тесту.");
                    }
                }

                if (ModelState.IsValid)
                {
                    var question = new OrthoeopyQuestion
                    {
                        OrthoeopyTestId = model.OrthoeopyTestId,
                        OrderIndex = model.OrderIndex,
                        Points = model.Points,
                        Word = model.Word,
                        StressPosition = model.StressPosition,
                        WordWithStress = model.WordWithStress,
                        WrongStressPositions = model.WrongStressPositions,
                        Hint = model.Hint
                    };

                    await _questionRepository.CreateAsync(question);

                    _logger.LogInformation("Учитель {TeacherId} создал вопрос по орфоэпии {QuestionId} для теста {TestId}",
                        currentUser.Id, question.Id, model.OrthoeopyTestId);

                    TempData["SuccessMessage"] = "Вопрос успешно добавлен!";
                    return RedirectToAction("Details", "OrthoeopyTest", new { id = model.OrthoeopyTestId });
                }

                var test = await _testService.GetByIdAsync(model.OrthoeopyTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.OrthoeopyTestId;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyQuestion/Create (POST)");
                ModelState.AddModelError(string.Empty, "Произошла ошибка при создании вопроса.");
                
                var test = await _testService.GetByIdAsync(model.OrthoeopyTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.OrthoeopyTestId;
                }

                return View(model);
            }
        }

        // GET: OrthoeopyQuestion/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var question = await _questionRepository.GetByIdAsync(id);
                if (question == null)
                {
                    TempData["ErrorMessage"] = "Вопрос не найден.";
                    return RedirectToAction("Index", "Assignment");
                }

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, question.OrthoeopyTestId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому вопросу.";
                    return RedirectToAction("Details", "OrthoeopyTest", new { id = question.OrthoeopyTestId });
                }

                var test = await _testService.GetByIdAsync(question.OrthoeopyTestId);
                ViewBag.Test = test;
                ViewBag.TestId = question.OrthoeopyTestId;

                var model = new EditOrthoeopyQuestionViewModel
                {
                    Id = question.Id,
                    OrthoeopyTestId = question.OrthoeopyTestId,
                    OrderIndex = question.OrderIndex,
                    Points = question.Points,
                    Word = question.Word,
                    StressPosition = question.StressPosition,
                    WordWithStress = question.WordWithStress,
                    WrongStressPositions = question.WrongStressPositions,
                    Hint = question.Hint
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyQuestion/Edit (GET) для ID: {QuestionId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы редактирования.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: OrthoeopyQuestion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditOrthoeopyQuestionViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Неверный идентификатор вопроса.";
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
                    var question = await _questionRepository.GetByIdAsync(id);
                    if (question == null)
                    {
                        TempData["ErrorMessage"] = "Вопрос не найден.";
                        return RedirectToAction("Index", "Assignment");
                    }

                    var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, question.OrthoeopyTestId);
                    if (!canAccess)
                    {
                        ModelState.AddModelError(string.Empty, "У вас нет доступа к этому вопросу.");
                    }
                }

                if (ModelState.IsValid)
                {
                    var question = await _questionRepository.GetByIdAsync(id);
                    if (question == null)
                    {
                        TempData["ErrorMessage"] = "Вопрос не найден.";
                        return RedirectToAction("Index", "Assignment");
                    }

                    question.OrderIndex = model.OrderIndex;
                    question.Points = model.Points;
                    question.Word = model.Word;
                    question.StressPosition = model.StressPosition;
                    question.WordWithStress = model.WordWithStress;
                    question.WrongStressPositions = model.WrongStressPositions;
                    question.Hint = model.Hint;

                    await _questionRepository.UpdateAsync(question);

                    _logger.LogInformation("Учитель {TeacherId} обновил вопрос по орфоэпии {QuestionId}",
                        currentUser.Id, id);

                    TempData["SuccessMessage"] = "Вопрос успешно обновлен!";
                    return RedirectToAction("Details", "OrthoeopyTest", new { id = model.OrthoeopyTestId });
                }

                var test = await _testService.GetByIdAsync(model.OrthoeopyTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.OrthoeopyTestId;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyQuestion/Edit (POST) для ID: {QuestionId}", id);
                ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении вопроса.");
                
                var test = await _testService.GetByIdAsync(model.OrthoeopyTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.OrthoeopyTestId;
                }

                return View(model);
            }
        }

        // GET: OrthoeopyQuestion/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var question = await _questionRepository.GetByIdAsync(id);
                if (question == null)
                {
                    TempData["ErrorMessage"] = "Вопрос не найден.";
                    return RedirectToAction("Index", "Assignment");
                }

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, question.OrthoeopyTestId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому вопросу.";
                    return RedirectToAction("Details", "OrthoeopyTest", new { id = question.OrthoeopyTestId });
                }

                var test = await _testService.GetByIdAsync(question.OrthoeopyTestId);
                ViewBag.Test = test;

                return View(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyQuestion/Delete (GET) для ID: {QuestionId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке вопроса для удаления.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: OrthoeopyQuestion/Delete/5
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

                var question = await _questionRepository.GetByIdAsync(id);
                if (question == null)
                {
                    TempData["ErrorMessage"] = "Вопрос не найден.";
                    return RedirectToAction("Index", "Assignment");
                }

                var testId = question.OrthoeopyTestId;

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, testId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому вопросу.";
                    return RedirectToAction("Details", "OrthoeopyTest", new { id = testId });
                }

                await _questionRepository.DeleteAsync(id);

                _logger.LogInformation("Учитель {TeacherId} удалил вопрос по орфоэпии {QuestionId}",
                    currentUser.Id, id);

                TempData["SuccessMessage"] = "Вопрос успешно удален!";
                return RedirectToAction("Details", "OrthoeopyTest", new { id = testId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в OrthoeopyQuestion/Delete (POST) для ID: {QuestionId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении вопроса.";
                return RedirectToAction("Index", "Assignment");
            }
        }
    }
}

