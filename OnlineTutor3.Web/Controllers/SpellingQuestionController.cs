using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Teacher)]
    public class SpellingQuestionController : Controller
    {
        private readonly ISpellingQuestionRepository _questionRepository;
        private readonly ISpellingTestService _testService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SpellingQuestionController> _logger;

        public SpellingQuestionController(
            ISpellingQuestionRepository questionRepository,
            ISpellingTestService testService,
            UserManager<ApplicationUser> userManager,
            ILogger<SpellingQuestionController> logger)
        {
            _questionRepository = questionRepository;
            _testService = testService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: SpellingQuestion/Create?testId=5
        public async Task<IActionResult> Create(int testId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                // Проверяем доступ к тесту
                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, testId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому тесту.";
                    return RedirectToAction("Details", "SpellingTest", new { id = testId });
                }

                var test = await _testService.GetByIdAsync(testId);
                if (test == null)
                {
                    TempData["ErrorMessage"] = "Тест не найден.";
                    return RedirectToAction("Index", "Assignment");
                }

                // Получаем количество вопросов для определения OrderIndex
                var existingQuestions = await _questionRepository.GetByTestIdAsync(testId);
                var nextOrderIndex = existingQuestions.Count > 0 
                    ? existingQuestions.Max(q => q.OrderIndex) + 1 
                    : 1;

                ViewBag.Test = test;
                ViewBag.TestId = testId;
                ViewBag.NextOrderIndex = nextOrderIndex;

                var model = new CreateSpellingQuestionViewModel
                {
                    SpellingTestId = testId,
                    OrderIndex = nextOrderIndex,
                    Points = 1
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в SpellingQuestion/Create (GET) для TestId: {TestId}", testId);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы создания вопроса.";
                return RedirectToAction("Details", "SpellingTest", new { id = testId });
            }
        }

        // POST: SpellingQuestion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSpellingQuestionViewModel model)
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
                    // Проверяем доступ к тесту
                    var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, model.SpellingTestId);
                    if (!canAccess)
                    {
                        ModelState.AddModelError("SpellingTestId", "У вас нет доступа к этому тесту.");
                    }
                }

                if (ModelState.IsValid)
                {
                    var question = new SpellingQuestion
                    {
                        SpellingTestId = model.SpellingTestId,
                        OrderIndex = model.OrderIndex,
                        Points = model.Points,
                        WordWithGap = model.WordWithGap,
                        CorrectLetter = model.CorrectLetter,
                        FullWord = model.FullWord,
                        Hint = model.Hint
                    };

                    await _questionRepository.CreateAsync(question);


                    TempData["SuccessMessage"] = $"Вопрос успешно добавлен!";
                    return RedirectToAction("Details", "SpellingTest", new { id = model.SpellingTestId });
                }

                // Если ошибка, загружаем данные для формы
                var test = await _testService.GetByIdAsync(model.SpellingTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.SpellingTestId;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в SpellingQuestion/Create (POST)");
                ModelState.AddModelError(string.Empty, "Произошла ошибка при создании вопроса.");
                
                var test = await _testService.GetByIdAsync(model.SpellingTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.SpellingTestId;
                }

                return View(model);
            }
        }

        // GET: SpellingQuestion/Edit/5
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

                // Проверяем доступ к тесту
                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, question.SpellingTestId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому вопросу.";
                    return RedirectToAction("Details", "SpellingTest", new { id = question.SpellingTestId });
                }

                var test = await _testService.GetByIdAsync(question.SpellingTestId);
                ViewBag.Test = test;
                ViewBag.TestId = question.SpellingTestId;

                var model = new EditSpellingQuestionViewModel
                {
                    Id = question.Id,
                    SpellingTestId = question.SpellingTestId,
                    OrderIndex = question.OrderIndex,
                    Points = question.Points,
                    WordWithGap = question.WordWithGap,
                    CorrectLetter = question.CorrectLetter,
                    FullWord = question.FullWord,
                    Hint = question.Hint
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в SpellingQuestion/Edit (GET) для ID: {QuestionId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы редактирования.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: SpellingQuestion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditSpellingQuestionViewModel model)
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

                    // Проверяем доступ к тесту
                    var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, question.SpellingTestId);
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
                    question.WordWithGap = model.WordWithGap;
                    question.CorrectLetter = model.CorrectLetter;
                    question.FullWord = model.FullWord;
                    question.Hint = model.Hint;

                    await _questionRepository.UpdateAsync(question);


                    TempData["SuccessMessage"] = "Вопрос успешно обновлен!";
                    return RedirectToAction("Details", "SpellingTest", new { id = model.SpellingTestId });
                }

                // Если ошибка, загружаем данные для формы
                var test = await _testService.GetByIdAsync(model.SpellingTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.SpellingTestId;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в SpellingQuestion/Edit (POST) для ID: {QuestionId}", id);
                ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении вопроса.");
                
                var test = await _testService.GetByIdAsync(model.SpellingTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.SpellingTestId;
                }

                return View(model);
            }
        }

        // GET: SpellingQuestion/Delete/5
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

                // Проверяем доступ к тесту
                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, question.SpellingTestId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому вопросу.";
                    return RedirectToAction("Details", "SpellingTest", new { id = question.SpellingTestId });
                }

                var test = await _testService.GetByIdAsync(question.SpellingTestId);
                ViewBag.Test = test;

                return View(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в SpellingQuestion/Delete (GET) для ID: {QuestionId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке вопроса для удаления.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: SpellingQuestion/Delete/5
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

                var testId = question.SpellingTestId;

                // Проверяем доступ к тесту
                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, testId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому вопросу.";
                    return RedirectToAction("Details", "SpellingTest", new { id = testId });
                }

                await _questionRepository.DeleteAsync(id);


                TempData["SuccessMessage"] = "Вопрос успешно удален!";
                return RedirectToAction("Details", "SpellingTest", new { id = testId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в SpellingQuestion/Delete (POST) для ID: {QuestionId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении вопроса.";
                return RedirectToAction("Index", "Assignment");
            }
        }
    }
}

