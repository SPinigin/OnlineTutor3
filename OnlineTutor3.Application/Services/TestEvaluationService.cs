using Microsoft.Extensions.Logging;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using System.Text.RegularExpressions;

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
        private readonly INotParticleQuestionRepository _notParticleQuestionRepository;
        private readonly IRegularQuestionOptionRepository _regularQuestionOptionRepository;
        private readonly ISpellingAnswerRepository _spellingAnswerRepository;
        private readonly IPunctuationAnswerRepository _punctuationAnswerRepository;
        private readonly IOrthoeopyAnswerRepository _orthoeopyAnswerRepository;
        private readonly IRegularAnswerRepository _regularAnswerRepository;
        private readonly INotParticleAnswerRepository _notParticleAnswerRepository;
        private readonly ILogger<TestEvaluationService> _logger;

        public TestEvaluationService(
            ISpellingQuestionRepository spellingQuestionRepository,
            IPunctuationQuestionRepository punctuationQuestionRepository,
            IOrthoeopyQuestionRepository orthoeopyQuestionRepository,
            IRegularQuestionRepository regularQuestionRepository,
            INotParticleQuestionRepository notParticleQuestionRepository,
            IRegularQuestionOptionRepository regularQuestionOptionRepository,
            ISpellingAnswerRepository spellingAnswerRepository,
            IPunctuationAnswerRepository punctuationAnswerRepository,
            IOrthoeopyAnswerRepository orthoeopyAnswerRepository,
            IRegularAnswerRepository regularAnswerRepository,
            INotParticleAnswerRepository notParticleAnswerRepository,
            ILogger<TestEvaluationService> logger)
        {
            _spellingQuestionRepository = spellingQuestionRepository;
            _punctuationQuestionRepository = punctuationQuestionRepository;
            _orthoeopyQuestionRepository = orthoeopyQuestionRepository;
            _regularQuestionRepository = regularQuestionRepository;
            _notParticleQuestionRepository = notParticleQuestionRepository;
            _regularQuestionOptionRepository = regularQuestionOptionRepository;
            _spellingAnswerRepository = spellingAnswerRepository;
            _punctuationAnswerRepository = punctuationAnswerRepository;
            _orthoeopyAnswerRepository = orthoeopyAnswerRepository;
            _regularAnswerRepository = regularAnswerRepository;
            _notParticleAnswerRepository = notParticleAnswerRepository;
            _logger = logger;
        }

        public async Task<(bool IsCorrect, int Points)> EvaluateSpellingAnswerAsync(SpellingQuestion question, string studentAnswer, bool noLetterNeeded, int pointsPerQuestion)
        {
            try
            {
                bool isCorrect;
                
                if (!question.RequiresAnswer)
                {
                    isCorrect = noLetterNeeded;
                    return (isCorrect, isCorrect ? pointsPerQuestion : 0);
                }

                if (noLetterNeeded)
                {
                    return (false, 0);
                }

                var normalizedCorrect = NormalizeSpellingAnswer(question.CorrectLetter);
                var normalizedStudent = NormalizeSpellingAnswer(studentAnswer);
                
                isCorrect = normalizedCorrect.Equals(normalizedStudent, StringComparison.OrdinalIgnoreCase);
                return (isCorrect, isCorrect ? pointsPerQuestion : 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при оценке ответа по орфографии. QuestionId: {QuestionId}", question.Id);
                return (false, 0);
            }
        }

        /// <summary>
        /// Нормализует ответ по орфографии: убирает пробелы в начале и конце, а также пробелы после запятых
        /// </summary>
        private static string NormalizeSpellingAnswer(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer))
                return string.Empty;

            // Убираем пробелы в начале и конце
            var normalized = answer.Trim();
            
            // Убираем пробелы после запятых (заменяем ", " на "," и множественные пробелы после запятой)
            normalized = Regex.Replace(normalized, @",\s+", ",");
            
            // Убираем пробелы перед запятыми (на случай, если пользователь ввел "и ,и")
            normalized = Regex.Replace(normalized, @"\s+,", ",");
            
            return normalized;
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
                        if (!string.IsNullOrEmpty(studentAnswer))
                        {
                            // Парсим выбранные ID из строки (через запятую)
                            var selectedIds = studentAnswer.Split(',')
                                .Select(id => int.TryParse(id.Trim(), out var parsed) ? parsed : 0)
                                .Where(id => id > 0)
                                .OrderBy(id => id)
                                .ToList();

                            // Получаем правильные опции
                            var correctOptions = await _regularQuestionOptionRepository.GetByQuestionIdAsync(question.Id);
                            var correctIds = correctOptions
                                .Where(o => o.IsCorrect)
                                .Select(o => o.Id)
                                .OrderBy(id => id)
                                .ToList();

                            // Сравниваем множества
                            isCorrect = correctIds.Count == selectedIds.Count && 
                                       correctIds.SequenceEqual(selectedIds);
                        }
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
                int maxScore = questions.Sum(q => q.Points); // Сумма баллов всех вопросов

                // Группируем ответы по вопросу и берем только последний ответ на каждый вопрос (по Id)
                var uniqueAnswers = answers
                    .GroupBy(a => a.SpellingQuestionId)
                    .Select(g => g.OrderByDescending(a => a.Id).First())
                    .ToList();

                foreach (var answer in uniqueAnswers)
                {
                    var question = questions.FirstOrDefault(q => q.Id == answer.SpellingQuestionId);
                    if (question != null)
                    {
                        var (isCorrect, points) = await EvaluateSpellingAnswerAsync(question, answer.StudentAnswer, answer.NoLetterNeeded, question.Points);
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

        /// <summary>
        /// Вычисляет оценку по 5-балльной шкале на основе процента выполнения
        /// </summary>
        /// <param name="percentage">Процент выполнения (0-100)</param>
        /// <returns>Оценка: 5 (100%), 4 (91-99%), 3 (80-90%), 2 (<80%)</returns>
        public static int CalculateGrade(double percentage)
        {
            if (percentage >= 100.0)
                return 5;
            if (percentage >= 91.0)
                return 4;
            if (percentage >= 80.0)
                return 3;
            return 2;
        }

        public async Task<(int Score, int MaxScore, double Percentage)> CalculatePunctuationTestResultAsync(int testResultId, int testId)
        {
            try
            {
                var answers = await _punctuationAnswerRepository.GetByTestResultIdAsync(testResultId);
                var questions = await _punctuationQuestionRepository.GetByTestIdOrderedAsync(testId);

                int score = 0;
                int maxScore = questions.Count;

                // Группируем ответы по вопросу и берем только последний ответ на каждый вопрос (по Id)
                var uniqueAnswers = answers
                    .GroupBy(a => a.PunctuationQuestionId)
                    .Select(g => g.OrderByDescending(a => a.Id).First())
                    .ToList();

                foreach (var answer in uniqueAnswers)
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

                // Группируем ответы по вопросу и берем только последний ответ на каждый вопрос (по Id)
                var uniqueAnswers = answers
                    .GroupBy(a => a.OrthoeopyQuestionId)
                    .Select(g => g.OrderByDescending(a => a.Id).First())
                    .ToList();

                foreach (var answer in uniqueAnswers)
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

                // Группируем ответы по вопросу и берем только последний ответ на каждый вопрос (по Id)
                var uniqueAnswers = answers
                    .GroupBy(a => a.RegularQuestionId)
                    .Select(g => g.OrderByDescending(a => a.Id).First())
                    .ToList();

                foreach (var answer in uniqueAnswers)
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

        public async Task<(bool IsCorrect, int Points)> EvaluateNotParticleAnswerAsync(NotParticleQuestion question, bool studentAnswerIsMerged, int pointsPerQuestion)
        {
            try
            {
                // Преобразуем CorrectAnswer ("слитно"/"раздельно") в bool
                var normalizedCorrect = question.CorrectAnswer?.Trim().ToLower() ?? "";
                bool correctIsMerged = normalizedCorrect == "слитно";
                
                // Сравниваем ответ студента с правильным ответом
                bool isCorrect = correctIsMerged == studentAnswerIsMerged;
                return (isCorrect, isCorrect ? pointsPerQuestion : 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при оценке ответа на тест на правописание частицы \"не\". QuestionId: {QuestionId}", question.Id);
                return (false, 0);
            }
        }

        public async Task<(int Score, int MaxScore, double Percentage)> CalculateNotParticleTestResultAsync(int testResultId, int testId)
        {
            try
            {
                var answers = await _notParticleAnswerRepository.GetByTestResultIdAsync(testResultId);
                var questions = await _notParticleQuestionRepository.GetByTestIdOrderedAsync(testId);

                int score = 0;
                int maxScore = questions.Sum(q => q.Points);

                // Группируем ответы по вопросу и берем только последний ответ на каждый вопрос (по Id)
                var uniqueAnswers = answers
                    .GroupBy(a => a.NotParticleQuestionId)
                    .Select(g => g.OrderByDescending(a => a.Id).First())
                    .ToList();

                foreach (var answer in uniqueAnswers)
                {
                    var question = questions.FirstOrDefault(q => q.Id == answer.NotParticleQuestionId);
                    if (question != null)
                    {
                        // Преобразуем строку "слитно"/"раздельно" в bool
                        var normalizedAnswer = answer.StudentAnswer?.Trim().ToLower() ?? "";
                        bool studentAnswerIsMerged = normalizedAnswer == "слитно" || normalizedAnswer == "true" || normalizedAnswer == "1";
                        
                        var (isCorrect, points) = await EvaluateNotParticleAnswerAsync(question, studentAnswerIsMerged, question.Points);
                        answer.IsCorrect = isCorrect;
                        answer.Points = points;
                        score += points;
                        await _notParticleAnswerRepository.UpdateAsync(answer);
                    }
                }

                var percentage = maxScore > 0 ? (double)score / maxScore * 100 : 0;
                return (score, maxScore, percentage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при вычислении результата теста на правописание частицы \"не\". TestResultId: {TestResultId}", testResultId);
                return (0, 0, 0);
            }
        }
    }
}

