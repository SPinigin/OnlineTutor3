using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Teacher)]
    public class NotParticleQuestionController : Controller
    {
        private readonly INotParticleQuestionRepository _questionRepository;
        private readonly INotParticleTestService _testService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<NotParticleQuestionController> _logger;

        public NotParticleQuestionController(
            INotParticleQuestionRepository questionRepository,
            INotParticleTestService testService,
            UserManager<ApplicationUser> userManager,
            ILogger<NotParticleQuestionController> logger)
        {
            _questionRepository = questionRepository;
            _testService = testService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: NotParticleQuestion/Create?testId=5
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
                    return RedirectToAction("Details", "NotParticleTest", new { id = testId });
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

                var model = new CreateNotParticleQuestionViewModel
                {
                    NotParticleTestId = testId,
                    OrderIndex = nextOrderIndex,
                    Points = 1,
                    CorrectAnswer = "слитно"
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в NotParticleQuestion/Create (GET) для TestId: {TestId}", testId);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы создания вопроса.";
                return RedirectToAction("Details", "NotParticleTest", new { id = testId });
            }
        }

        // POST: NotParticleQuestion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNotParticleQuestionViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                // Нормализуем ответ
                if (!string.IsNullOrWhiteSpace(model.CorrectAnswer))
                {
                    model.CorrectAnswer = model.CorrectAnswer.Trim().ToLower();
                }

                if (ModelState.IsValid)
                {
                    var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, model.NotParticleTestId);
                    if (!canAccess)
                    {
                        ModelState.AddModelError("NotParticleTestId", "У вас нет доступа к этому тесту.");
                    }
                }

                if (ModelState.IsValid)
                {
                    var question = new NotParticleQuestion
                    {
                        NotParticleTestId = model.NotParticleTestId,
                        OrderIndex = model.OrderIndex,
                        Points = model.Points,
                        TextWithGap = model.TextWithGap,
                        CorrectAnswer = model.CorrectAnswer,
                        FullText = model.FullText,
                        Hint = model.Hint
                    };

                    await _questionRepository.CreateAsync(question);

                    TempData["SuccessMessage"] = "Вопрос успешно добавлен!";
                    return RedirectToAction("Details", "NotParticleTest", new { id = model.NotParticleTestId });
                }

                var test = await _testService.GetByIdAsync(model.NotParticleTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.NotParticleTestId;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в NotParticleQuestion/Create (POST)");
                ModelState.AddModelError(string.Empty, "Произошла ошибка при создании вопроса.");
                
                var test = await _testService.GetByIdAsync(model.NotParticleTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.NotParticleTestId;
                }

                return View(model);
            }
        }

        // GET: NotParticleQuestion/Edit/5
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

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, question.NotParticleTestId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому вопросу.";
                    return RedirectToAction("Details", "NotParticleTest", new { id = question.NotParticleTestId });
                }

                var test = await _testService.GetByIdAsync(question.NotParticleTestId);
                ViewBag.Test = test;
                ViewBag.TestId = question.NotParticleTestId;

                var model = new EditNotParticleQuestionViewModel
                {
                    Id = question.Id,
                    NotParticleTestId = question.NotParticleTestId,
                    OrderIndex = question.OrderIndex,
                    Points = question.Points,
                    TextWithGap = question.TextWithGap,
                    CorrectAnswer = question.CorrectAnswer,
                    FullText = question.FullText,
                    Hint = question.Hint
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в NotParticleQuestion/Edit (GET) для ID: {QuestionId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы редактирования.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: NotParticleQuestion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditNotParticleQuestionViewModel model)
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

                // Нормализуем ответ
                if (!string.IsNullOrWhiteSpace(model.CorrectAnswer))
                {
                    model.CorrectAnswer = model.CorrectAnswer.Trim().ToLower();
                }

                if (ModelState.IsValid)
                {
                    var question = await _questionRepository.GetByIdAsync(id);
                    if (question == null)
                    {
                        TempData["ErrorMessage"] = "Вопрос не найден.";
                        return RedirectToAction("Index", "Assignment");
                    }

                    var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, question.NotParticleTestId);
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
                    question.TextWithGap = model.TextWithGap;
                    question.CorrectAnswer = model.CorrectAnswer;
                    question.FullText = model.FullText;
                    question.Hint = model.Hint;

                    await _questionRepository.UpdateAsync(question);

                    TempData["SuccessMessage"] = "Вопрос успешно обновлен!";
                    return RedirectToAction("Details", "NotParticleTest", new { id = model.NotParticleTestId });
                }

                var test = await _testService.GetByIdAsync(model.NotParticleTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.NotParticleTestId;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в NotParticleQuestion/Edit (POST) для ID: {QuestionId}", id);
                ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении вопроса.");
                
                var test = await _testService.GetByIdAsync(model.NotParticleTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.NotParticleTestId;
                }

                return View(model);
            }
        }

        // GET: NotParticleQuestion/Delete/5
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

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, question.NotParticleTestId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому вопросу.";
                    return RedirectToAction("Details", "NotParticleTest", new { id = question.NotParticleTestId });
                }

                var test = await _testService.GetByIdAsync(question.NotParticleTestId);
                ViewBag.Test = test;

                return View(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в NotParticleQuestion/Delete (GET) для ID: {QuestionId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке вопроса для удаления.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: NotParticleQuestion/Delete/5
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

                var testId = question.NotParticleTestId;

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, testId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому вопросу.";
                    return RedirectToAction("Details", "NotParticleTest", new { id = testId });
                }

                await _questionRepository.DeleteAsync(id);

                TempData["SuccessMessage"] = "Вопрос успешно удален!";
                return RedirectToAction("Details", "NotParticleTest", new { id = testId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в NotParticleQuestion/Delete (POST) для ID: {QuestionId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении вопроса.";
                return RedirectToAction("Index", "Assignment");
            }
        }
    }
}

