using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Teacher)]
    public class PunctuationQuestionController : Controller
    {
        private readonly IPunctuationQuestionRepository _questionRepository;
        private readonly IPunctuationTestService _testService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PunctuationQuestionController> _logger;

        public PunctuationQuestionController(
            IPunctuationQuestionRepository questionRepository,
            IPunctuationTestService testService,
            UserManager<ApplicationUser> userManager,
            ILogger<PunctuationQuestionController> logger)
        {
            _questionRepository = questionRepository;
            _testService = testService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: PunctuationQuestion/Create?testId=5
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
                    return RedirectToAction("Details", "PunctuationTest", new { id = testId });
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

                var model = new CreatePunctuationQuestionViewModel
                {
                    PunctuationTestId = testId,
                    OrderIndex = nextOrderIndex,
                    Points = 1
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в PunctuationQuestion/Create (GET) для TestId: {TestId}", testId);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы создания вопроса.";
                return RedirectToAction("Details", "PunctuationTest", new { id = testId });
            }
        }

        // POST: PunctuationQuestion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePunctuationQuestionViewModel model)
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
                    var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, model.PunctuationTestId);
                    if (!canAccess)
                    {
                        ModelState.AddModelError("PunctuationTestId", "У вас нет доступа к этому тесту.");
                    }
                }

                if (ModelState.IsValid)
                {
                    var question = new PunctuationQuestion
                    {
                        PunctuationTestId = model.PunctuationTestId,
                        OrderIndex = model.OrderIndex,
                        Points = model.Points,
                        SentenceWithNumbers = model.SentenceWithNumbers,
                        CorrectPositions = model.CorrectPositions,
                        PlainSentence = model.PlainSentence,
                        Hint = model.Hint
                    };

                    await _questionRepository.CreateAsync(question);


                    TempData["SuccessMessage"] = "Вопрос успешно добавлен!";
                    return RedirectToAction("Details", "PunctuationTest", new { id = model.PunctuationTestId });
                }

                var test = await _testService.GetByIdAsync(model.PunctuationTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.PunctuationTestId;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в PunctuationQuestion/Create (POST)");
                ModelState.AddModelError(string.Empty, "Произошла ошибка при создании вопроса.");
                
                var test = await _testService.GetByIdAsync(model.PunctuationTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.PunctuationTestId;
                }

                return View(model);
            }
        }

        // GET: PunctuationQuestion/Edit/5
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

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, question.PunctuationTestId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому вопросу.";
                    return RedirectToAction("Details", "PunctuationTest", new { id = question.PunctuationTestId });
                }

                var test = await _testService.GetByIdAsync(question.PunctuationTestId);
                ViewBag.Test = test;
                ViewBag.TestId = question.PunctuationTestId;

                var model = new EditPunctuationQuestionViewModel
                {
                    Id = question.Id,
                    PunctuationTestId = question.PunctuationTestId,
                    OrderIndex = question.OrderIndex,
                    Points = question.Points,
                    SentenceWithNumbers = question.SentenceWithNumbers,
                    CorrectPositions = question.CorrectPositions,
                    PlainSentence = question.PlainSentence,
                    Hint = question.Hint
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в PunctuationQuestion/Edit (GET) для ID: {QuestionId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы редактирования.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: PunctuationQuestion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditPunctuationQuestionViewModel model)
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

                    var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, question.PunctuationTestId);
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
                    question.SentenceWithNumbers = model.SentenceWithNumbers;
                    question.CorrectPositions = model.CorrectPositions;
                    question.PlainSentence = model.PlainSentence;
                    question.Hint = model.Hint;

                    await _questionRepository.UpdateAsync(question);


                    TempData["SuccessMessage"] = "Вопрос успешно обновлен!";
                    return RedirectToAction("Details", "PunctuationTest", new { id = model.PunctuationTestId });
                }

                var test = await _testService.GetByIdAsync(model.PunctuationTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.PunctuationTestId;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в PunctuationQuestion/Edit (POST) для ID: {QuestionId}", id);
                ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении вопроса.");
                
                var test = await _testService.GetByIdAsync(model.PunctuationTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.PunctuationTestId;
                }

                return View(model);
            }
        }

        // GET: PunctuationQuestion/Delete/5
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

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, question.PunctuationTestId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому вопросу.";
                    return RedirectToAction("Details", "PunctuationTest", new { id = question.PunctuationTestId });
                }

                var test = await _testService.GetByIdAsync(question.PunctuationTestId);
                ViewBag.Test = test;

                return View(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в PunctuationQuestion/Delete (GET) для ID: {QuestionId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке вопроса для удаления.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: PunctuationQuestion/Delete/5
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

                var testId = question.PunctuationTestId;

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, testId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому вопросу.";
                    return RedirectToAction("Details", "PunctuationTest", new { id = testId });
                }

                await _questionRepository.DeleteAsync(id);


                TempData["SuccessMessage"] = "Вопрос успешно удален!";
                return RedirectToAction("Details", "PunctuationTest", new { id = testId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в PunctuationQuestion/Delete (POST) для ID: {QuestionId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении вопроса.";
                return RedirectToAction("Index", "Assignment");
            }
        }
    }
}

