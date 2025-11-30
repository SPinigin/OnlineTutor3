using Microsoft.Extensions.Logging;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Services
{
    /// <summary>
    /// Сервис для оценки ответов и подсчета результатов тестов
    /// </summary>
    public class TestEvaluationService : ITestEvaluationService
    {
        private readonly ISpellingQuestionRepository _spellingQuestionRepository;
        private readonly IPunctuationQuestionRepository _punctuationQuestionRepository;
        private readonly IOrthoeopyQuestionRepository _orthoeopyQuestionRepository;
        private readonly IRegularQuestionRepository _regularQuestionRepository;
        private readonly IRegularQuestionOptionRepository _regularQuestionOptionRepository;
        private readonly ISpellingAnswerRepository _spellingAnswerRepository;
        private readonly IPunctuationAnswerRepository _punctuationAnswerRepository;
        private readonly IOrthoeopyAnswerRepository _orthoeopyAnswerRepository;
        private readonly IRegularAnswerRepository _regularAnswerRepository;
        private readonly ILogger<TestEvaluationService> _logger;

        public TestEvaluationService(
            ISpellingQuestionRepository spellingQuestionRepository,
            IPunctuationQuestionRepository punctuationQuestionRepository,
            IOrthoeopyQuestionRepository orthoeopyQuestionRepository,
            IRegularQuestionRepository regularQuestionRepository,
            IRegularQuestionOptionRepository regularQuestionOptionRepository,
            ISpellingAnswerRepository spellingAnswerRepository,
            IPunctuationAnswerRepository punctuationAnswerRepository,
            IOrthoeopyAnswerRepository orthoeopyAnswerRepository,
            IRegularAnswerRepository regularAnswerRepository,
            ILogger<TestEvaluationService> logger)
        {
            _spellingQuestionRepository = spellingQuestionRepository;
            _punctuationQuestionRepository = punctuationQuestionRepository;
            _orthoeopyQuestionRepository = orthoeopyQuestionRepository;
            _regularQuestionRepository = regularQuestionRepository;
            _regularQuestionOptionRepository = regularQuestionOptionRepository;
            _spellingAnswerRepository = spellingAnswerRepository;
            _punctuationAnswerRepository = punctuationAnswerRepository;
            _orthoeopyAnswerRepository = orthoeopyAnswerRepository;
            _regularAnswerRepository = regularAnswerRepository;
            _logger = logger;
        }

        public async Task<(bool IsCorrect, int Points)> EvaluateSpellingAnswerAsync(SpellingQuestion question, string studentAnswer, int pointsPerQuestion)
        {
            try
            {
                var isCorrect = question.CorrectLetter.Equals(studentAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
                return (isCorrect, isCorrect ? pointsPerQuestion : 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при оценке ответа по орфографии. QuestionId: {QuestionId}", question.Id);
                return (false, 0);
            }
        }

        public async Task<(bool IsCorrect, int Points)> EvaluatePunctuationAnswerAsync(PunctuationQuestion question, string studentAnswer, int pointsPerQuestion)
        {
            try
            {
                // Нормализуем ответы: убираем пробелы и сортируем
                var correctPositions = string.Join("", question.CorrectPositions.OrderBy(p => p).Select(p => p.ToString()));
                var studentPositions = string.Join("", studentAnswer.Trim().Where(char.IsDigit).OrderBy(c => c));
                
                var isCorrect = correctPositions.Equals(studentPositions, StringComparison.OrdinalIgnoreCase);
                return (isCorrect, isCorrect ? pointsPerQuestion : 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при оценке ответа по пунктуации. QuestionId: {QuestionId}", question.Id);
                return (false, 0);
            }
        }

        public async Task<(bool IsCorrect, int Points)> EvaluateOrthoeopyAnswerAsync(OrthoeopyQuestion question, int studentAnswer, int pointsPerQuestion)
        {
            try
            {
                var isCorrect = question.StressPosition == studentAnswer;
                return (isCorrect, isCorrect ? pointsPerQuestion : 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при оценке ответа по орфоэпии. QuestionId: {QuestionId}", question.Id);
                return (false, 0);
            }
        }

        public async Task<(bool IsCorrect, int Points)> EvaluateRegularAnswerAsync(RegularQuestion question, string? studentAnswer, int? selectedOptionId, int pointsPerQuestion)
        {
            try
            {
                bool isCorrect = false;

                switch (question.Type)
                {
                    case QuestionType.SingleChoice:
                        var singleChoiceOptions = await _regularQuestionOptionRepository.GetByQuestionIdAsync(question.Id);
                        var correctOptionId = singleChoiceOptions.FirstOrDefault(o => o.IsCorrect)?.Id;
                        isCorrect = selectedOptionId.HasValue && correctOptionId.HasValue && selectedOptionId.Value == correctOptionId.Value;
                        break;

                    case QuestionType.MultipleChoice:
                        // TODO: Реализовать логику для множественного выбора
                        isCorrect = false;
                        break;

                    case QuestionType.TrueFalse:
                        // Для TrueFalse правильный ответ определяется через QuestionOption с IsCorrect = true
                        var options = await _regularQuestionOptionRepository.GetByQuestionIdAsync(question.Id);
                        var correctOption = options.FirstOrDefault(o => o.IsCorrect);
                        if (correctOption != null && !string.IsNullOrEmpty(studentAnswer))
                        {
                            var expectedAnswer = correctOption.Text?.Trim().ToLower();
                            var providedAnswer = studentAnswer.Trim().ToLower();
                            isCorrect = expectedAnswer == providedAnswer;
                        }
                        else
                        {
                            isCorrect = false;
                        }
                        break;
                }

                return (isCorrect, isCorrect ? pointsPerQuestion : 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при оценке ответа на классический тест. QuestionId: {QuestionId}", question.Id);
                return (false, 0);
            }
        }

        public async Task<(int Score, int MaxScore, double Percentage)> CalculateSpellingTestResultAsync(int testResultId, int testId)
        {
            try
            {
                var answers = await _spellingAnswerRepository.GetByTestResultIdAsync(testResultId);
                var questions = await _spellingQuestionRepository.GetByTestIdOrderedAsync(testId);

                int score = 0;
                int maxScore = questions.Count; // Предполагаем 1 балл за вопрос

                foreach (var answer in answers)
                {
                    var question = questions.FirstOrDefault(q => q.Id == answer.SpellingQuestionId);
                    if (question != null)
                    {
                        var (isCorrect, points) = await EvaluateSpellingAnswerAsync(question, answer.StudentAnswer, 1);
                        answer.IsCorrect = isCorrect;
                        answer.Points = points;
                        score += points;
                        await _spellingAnswerRepository.UpdateAsync(answer);
                    }
                }

                var percentage = maxScore > 0 ? (double)score / maxScore * 100 : 0;
                return (score, maxScore, percentage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при вычислении результата теста по орфографии. TestResultId: {TestResultId}", testResultId);
                return (0, 0, 0);
            }
        }

        public async Task<(int Score, int MaxScore, double Percentage)> CalculatePunctuationTestResultAsync(int testResultId, int testId)
        {
            try
            {
                var answers = await _punctuationAnswerRepository.GetByTestResultIdAsync(testResultId);
                var questions = await _punctuationQuestionRepository.GetByTestIdOrderedAsync(testId);

                int score = 0;
                int maxScore = questions.Count;

                foreach (var answer in answers)
                {
                    var question = questions.FirstOrDefault(q => q.Id == answer.PunctuationQuestionId);
                    if (question != null)
                    {
                        var (isCorrect, points) = await EvaluatePunctuationAnswerAsync(question, answer.StudentAnswer, 1);
                        answer.IsCorrect = isCorrect;
                        answer.Points = points;
                        score += points;
                        await _punctuationAnswerRepository.UpdateAsync(answer);
                    }
                }

                var percentage = maxScore > 0 ? (double)score / maxScore * 100 : 0;
                return (score, maxScore, percentage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при вычислении результата теста по пунктуации. TestResultId: {TestResultId}", testResultId);
                return (0, 0, 0);
            }
        }

        public async Task<(int Score, int MaxScore, double Percentage)> CalculateOrthoeopyTestResultAsync(int testResultId, int testId)
        {
            try
            {
                var answers = await _orthoeopyAnswerRepository.GetByTestResultIdAsync(testResultId);
                var questions = await _orthoeopyQuestionRepository.GetByTestIdOrderedAsync(testId);

                int score = 0;
                int maxScore = questions.Count;

                foreach (var answer in answers)
                {
                    var question = questions.FirstOrDefault(q => q.Id == answer.OrthoeopyQuestionId);
                    if (question != null)
                    {
                        var (isCorrect, points) = await EvaluateOrthoeopyAnswerAsync(question, answer.StudentAnswer, 1);
                        answer.IsCorrect = isCorrect;
                        answer.Points = points;
                        score += points;
                        await _orthoeopyAnswerRepository.UpdateAsync(answer);
                    }
                }

                var percentage = maxScore > 0 ? (double)score / maxScore * 100 : 0;
                return (score, maxScore, percentage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при вычислении результата теста по орфоэпии. TestResultId: {TestResultId}", testResultId);
                return (0, 0, 0);
            }
        }

        public async Task<(int Score, int MaxScore, double Percentage)> CalculateRegularTestResultAsync(int testResultId, int testId)
        {
            try
            {
                var answers = await _regularAnswerRepository.GetByTestResultIdAsync(testResultId);
                var questions = await _regularQuestionRepository.GetByTestIdOrderedAsync(testId);

                int score = 0;
                int maxScore = questions.Count;

                foreach (var answer in answers)
                {
                    var question = questions.FirstOrDefault(q => q.Id == answer.RegularQuestionId);
                    if (question != null)
                    {
                        var (isCorrect, points) = await EvaluateRegularAnswerAsync(question, answer.StudentAnswer, answer.SelectedOptionId, 1);
                        answer.IsCorrect = isCorrect;
                        answer.Points = points;
                        score += points;
                        await _regularAnswerRepository.UpdateAsync(answer);
                    }
                }

                var percentage = maxScore > 0 ? (double)score / maxScore * 100 : 0;
                return (score, maxScore, percentage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при вычислении результата классического теста. TestResultId: {TestResultId}", testResultId);
                return (0, 0, 0);
            }
        }
    }
}

