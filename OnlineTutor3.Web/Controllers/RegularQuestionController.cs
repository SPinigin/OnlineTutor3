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
    public class RegularQuestionController : Controller
    {
        private readonly IRegularQuestionRepository _questionRepository;
        private readonly IRegularQuestionOptionRepository _optionRepository;
        private readonly IRegularTestService _testService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegularQuestionController> _logger;

        public RegularQuestionController(
            IRegularQuestionRepository questionRepository,
            IRegularQuestionOptionRepository optionRepository,
            IRegularTestService testService,
            UserManager<ApplicationUser> userManager,
            ILogger<RegularQuestionController> logger)
        {
            _questionRepository = questionRepository;
            _optionRepository = optionRepository;
            _testService = testService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: RegularQuestion/Create?testId=5
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
                    return RedirectToAction("Details", "RegularTest", new { id = testId });
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
                ViewBag.QuestionTypes = new SelectList(Enum.GetValues(typeof(QuestionType))
                    .Cast<QuestionType>()
                    .Select(t => new { Value = (int)t, Text = GetQuestionTypeName(t) }), 
                    "Value", "Text");

                var model = new CreateRegularQuestionViewModel
                {
                    RegularTestId = testId,
                    OrderIndex = nextOrderIndex,
                    Points = 1,
                    Type = QuestionType.SingleChoice
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в RegularQuestion/Create (GET) для TestId: {TestId}", testId);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы создания вопроса.";
                return RedirectToAction("Details", "RegularTest", new { id = testId });
            }
        }

        // POST: RegularQuestion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRegularQuestionViewModel model)
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
                    var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, model.RegularTestId);
                    if (!canAccess)
                    {
                        ModelState.AddModelError("RegularTestId", "У вас нет доступа к этому тесту.");
                    }

                    // Валидация вариантов ответов
                    if (model.Options == null || !model.Options.Any())
                    {
                        ModelState.AddModelError("Options", "Необходимо добавить хотя бы один вариант ответа.");
                    }
                    else
                    {
                        var correctCount = model.Options.Count(o => o.IsCorrect);
                        if (model.Type == QuestionType.SingleChoice && correctCount != 1)
                        {
                            ModelState.AddModelError("Options", "Для вопроса с одиночным выбором должен быть ровно один правильный ответ.");
                        }
                        else if (model.Type == QuestionType.MultipleChoice && correctCount < 1)
                        {
                            ModelState.AddModelError("Options", "Для вопроса с множественным выбором должен быть хотя бы один правильный ответ.");
                        }
                        else if (model.Type == QuestionType.TrueFalse && correctCount != 1)
                        {
                            ModelState.AddModelError("Options", "Для вопроса Верно/Неверно должен быть ровно один правильный ответ.");
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    var question = new RegularQuestion
                    {
                        RegularTestId = model.RegularTestId,
                        OrderIndex = model.OrderIndex,
                        Points = model.Points,
                        Text = model.Text,
                        Type = model.Type,
                        Explanation = model.Explanation,
                        Hint = model.Hint
                    };

                    var questionId = await _questionRepository.CreateAsync(question);

                    // Создаем варианты ответов
                    if (model.Options != null && model.Options.Any())
                    {
                        for (int i = 0; i < model.Options.Count; i++)
                        {
                            var option = model.Options[i];
                            var questionOption = new RegularQuestionOption
                            {
                                RegularQuestionId = questionId,
                                Text = option.Text,
                                IsCorrect = option.IsCorrect,
                                OrderIndex = i + 1
                            };
                            await _optionRepository.CreateAsync(questionOption);
                        }
                    }

                    _logger.LogInformation("Учитель {TeacherId} создал вопрос классического теста {QuestionId} для теста {TestId}",
                        currentUser.Id, questionId, model.RegularTestId);

                    TempData["SuccessMessage"] = "Вопрос успешно добавлен!";
                    return RedirectToAction("Details", "RegularTest", new { id = model.RegularTestId });
                }

                var test = await _testService.GetByIdAsync(model.RegularTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.RegularTestId;
                    ViewBag.QuestionTypes = new SelectList(Enum.GetValues(typeof(QuestionType))
                        .Cast<QuestionType>()
                        .Select(t => new { Value = (int)t, Text = GetQuestionTypeName(t) }), 
                        "Value", "Text", (int)model.Type);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в RegularQuestion/Create (POST)");
                ModelState.AddModelError(string.Empty, "Произошла ошибка при создании вопроса.");
                
                var test = await _testService.GetByIdAsync(model.RegularTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.RegularTestId;
                    ViewBag.QuestionTypes = new SelectList(Enum.GetValues(typeof(QuestionType))
                        .Cast<QuestionType>()
                        .Select(t => new { Value = (int)t, Text = GetQuestionTypeName(t) }), 
                        "Value", "Text", (int)model.Type);
                }

                return View(model);
            }
        }

        // GET: RegularQuestion/Edit/5
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

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, question.RegularTestId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому вопросу.";
                    return RedirectToAction("Details", "RegularTest", new { id = question.RegularTestId });
                }

                var test = await _testService.GetByIdAsync(question.RegularTestId);
                ViewBag.Test = test;
                ViewBag.TestId = question.RegularTestId;
                ViewBag.QuestionTypes = new SelectList(Enum.GetValues(typeof(QuestionType))
                    .Cast<QuestionType>()
                    .Select(t => new { Value = (int)t, Text = GetQuestionTypeName(t) }), 
                    "Value", "Text", (int)question.Type);

                // Загружаем варианты ответов
                var options = await _optionRepository.GetByQuestionIdOrderedAsync(id);

                var model = new EditRegularQuestionViewModel
                {
                    Id = question.Id,
                    RegularTestId = question.RegularTestId,
                    OrderIndex = question.OrderIndex,
                    Points = question.Points,
                    Text = question.Text,
                    Type = question.Type,
                    Explanation = question.Explanation,
                    Hint = question.Hint,
                    Options = options.Select(o => new QuestionOptionViewModel
                    {
                        Id = o.Id,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect,
                        OrderIndex = o.OrderIndex
                    }).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в RegularQuestion/Edit (GET) для ID: {QuestionId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы редактирования.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: RegularQuestion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditRegularQuestionViewModel model)
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

                    var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, question.RegularTestId);
                    if (!canAccess)
                    {
                        ModelState.AddModelError(string.Empty, "У вас нет доступа к этому вопросу.");
                    }

                    // Валидация вариантов ответов
                    if (model.Options == null || !model.Options.Any())
                    {
                        ModelState.AddModelError("Options", "Необходимо добавить хотя бы один вариант ответа.");
                    }
                    else
                    {
                        var correctCount = model.Options.Count(o => o.IsCorrect);
                        if (model.Type == QuestionType.SingleChoice && correctCount != 1)
                        {
                            ModelState.AddModelError("Options", "Для вопроса с одиночным выбором должен быть ровно один правильный ответ.");
                        }
                        else if (model.Type == QuestionType.MultipleChoice && correctCount < 1)
                        {
                            ModelState.AddModelError("Options", "Для вопроса с множественным выбором должен быть хотя бы один правильный ответ.");
                        }
                        else if (model.Type == QuestionType.TrueFalse && correctCount != 1)
                        {
                            ModelState.AddModelError("Options", "Для вопроса Верно/Неверно должен быть ровно один правильный ответ.");
                        }
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
                    question.Text = model.Text;
                    question.Type = model.Type;
                    question.Explanation = model.Explanation;
                    question.Hint = model.Hint;

                    await _questionRepository.UpdateAsync(question);

                    // Удаляем старые варианты ответов
                    var existingOptions = await _optionRepository.GetByQuestionIdAsync(id);
                    foreach (var option in existingOptions)
                    {
                        await _optionRepository.DeleteAsync(option.Id);
                    }

                    // Создаем новые варианты ответов
                    if (model.Options != null && model.Options.Any())
                    {
                        for (int i = 0; i < model.Options.Count; i++)
                        {
                            var option = model.Options[i];
                            var questionOption = new RegularQuestionOption
                            {
                                RegularQuestionId = id,
                                Text = option.Text,
                                IsCorrect = option.IsCorrect,
                                OrderIndex = i + 1
                            };
                            await _optionRepository.CreateAsync(questionOption);
                        }
                    }

                    _logger.LogInformation("Учитель {TeacherId} обновил вопрос классического теста {QuestionId}",
                        currentUser.Id, id);

                    TempData["SuccessMessage"] = "Вопрос успешно обновлен!";
                    return RedirectToAction("Details", "RegularTest", new { id = model.RegularTestId });
                }

                var test = await _testService.GetByIdAsync(model.RegularTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.RegularTestId;
                    ViewBag.QuestionTypes = new SelectList(Enum.GetValues(typeof(QuestionType))
                        .Cast<QuestionType>()
                        .Select(t => new { Value = (int)t, Text = GetQuestionTypeName(t) }), 
                        "Value", "Text", (int)model.Type);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в RegularQuestion/Edit (POST) для ID: {QuestionId}", id);
                ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении вопроса.");
                
                var test = await _testService.GetByIdAsync(model.RegularTestId);
                if (test != null)
                {
                    ViewBag.Test = test;
                    ViewBag.TestId = model.RegularTestId;
                    ViewBag.QuestionTypes = new SelectList(Enum.GetValues(typeof(QuestionType))
                        .Cast<QuestionType>()
                        .Select(t => new { Value = (int)t, Text = GetQuestionTypeName(t) }), 
                        "Value", "Text", (int)model.Type);
                }

                return View(model);
            }
        }

        // GET: RegularQuestion/Delete/5
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

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, question.RegularTestId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому вопросу.";
                    return RedirectToAction("Details", "RegularTest", new { id = question.RegularTestId });
                }

                var test = await _testService.GetByIdAsync(question.RegularTestId);
                ViewBag.Test = test;

                // Загружаем варианты ответов
                var options = await _optionRepository.GetByQuestionIdOrderedAsync(id);
                ViewBag.Options = options;

                return View(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в RegularQuestion/Delete (GET) для ID: {QuestionId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке вопроса для удаления.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        // POST: RegularQuestion/Delete/5
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

                var testId = question.RegularTestId;

                var canAccess = await _testService.TeacherCanAccessTestAsync(currentUser.Id, testId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому вопросу.";
                    return RedirectToAction("Details", "RegularTest", new { id = testId });
                }

                await _questionRepository.DeleteAsync(id);

                _logger.LogInformation("Учитель {TeacherId} удалил вопрос классического теста {QuestionId}",
                    currentUser.Id, id);

                TempData["SuccessMessage"] = "Вопрос успешно удален!";
                return RedirectToAction("Details", "RegularTest", new { id = testId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в RegularQuestion/Delete (POST) для ID: {QuestionId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении вопроса.";
                return RedirectToAction("Index", "Assignment");
            }
        }

        private string GetQuestionTypeName(QuestionType type)
        {
            return type switch
            {
                QuestionType.SingleChoice => "Одиночный выбор",
                QuestionType.MultipleChoice => "Множественный выбор",
                QuestionType.TrueFalse => "Верно/Неверно",
                _ => type.ToString()
            };
        }
    }
}

