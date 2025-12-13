using Microsoft.Extensions.Logging;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Services
{
    /// <summary>
    /// Сервис для работы с ответами студентов
    /// </summary>
    public class AnswerService : IAnswerService
    {
        private readonly ISpellingAnswerRepository _spellingAnswerRepository;
        private readonly IPunctuationAnswerRepository _punctuationAnswerRepository;
        private readonly IOrthoeopyAnswerRepository _orthoeopyAnswerRepository;
        private readonly IRegularAnswerRepository _regularAnswerRepository;
        private readonly ILogger<AnswerService> _logger;

        public AnswerService(
            ISpellingAnswerRepository spellingAnswerRepository,
            IPunctuationAnswerRepository punctuationAnswerRepository,
            IOrthoeopyAnswerRepository orthoeopyAnswerRepository,
            IRegularAnswerRepository regularAnswerRepository,
            ILogger<AnswerService> logger)
        {
            _spellingAnswerRepository = spellingAnswerRepository;
            _punctuationAnswerRepository = punctuationAnswerRepository;
            _orthoeopyAnswerRepository = orthoeopyAnswerRepository;
            _regularAnswerRepository = regularAnswerRepository;
            _logger = logger;
        }

        public async Task<SpellingAnswer> SaveSpellingAnswerAsync(int testResultId, int questionId, string studentAnswer, bool noLetterNeeded = false)
        {
            try
            {
                var existingAnswers = await _spellingAnswerRepository.GetByTestResultIdAsync(testResultId);
                var existingAnswer = existingAnswers.FirstOrDefault(a => a.SpellingQuestionId == questionId);

                SpellingAnswer savedAnswer;

                if (existingAnswer != null)
                {
                    existingAnswer.StudentAnswer = studentAnswer;
                    existingAnswer.NoLetterNeeded = noLetterNeeded;
                    await _spellingAnswerRepository.UpdateAsync(existingAnswer);
                    savedAnswer = existingAnswer;
                }
                else
                {
                    var answer = new SpellingAnswer
                    {
                        TestResultId = testResultId,
                        SpellingQuestionId = questionId,
                        StudentAnswer = studentAnswer,
                        NoLetterNeeded = noLetterNeeded,
                        IsCorrect = false,
                        Points = 0
                    };

                    var id = await _spellingAnswerRepository.CreateAsync(answer);
                    answer.Id = id;
                    savedAnswer = answer;
                }

                // Переполучаем все ответы и удаляем старые дубликаты для этого вопроса (кроме только что сохраненного ответа)
                var allAnswers = await _spellingAnswerRepository.GetByTestResultIdAsync(testResultId);
                var duplicates = allAnswers
                    .Where(a => a.SpellingQuestionId == questionId && a.Id != savedAnswer.Id)
                    .ToList();

                foreach (var duplicate in duplicates)
                {
                    await _spellingAnswerRepository.DeleteAsync(duplicate.Id);
                }

                return savedAnswer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении ответа по орфографии. TestResultId: {TestResultId}, QuestionId: {QuestionId}", testResultId, questionId);
                throw;
            }
        }

        public async Task<PunctuationAnswer> SavePunctuationAnswerAsync(int testResultId, int questionId, string studentAnswer)
        {
            try
            {
                var existingAnswers = await _punctuationAnswerRepository.GetByTestResultIdAsync(testResultId);
                var existingAnswer = existingAnswers.FirstOrDefault(a => a.PunctuationQuestionId == questionId);

                PunctuationAnswer savedAnswer;

                if (existingAnswer != null)
                {
                    existingAnswer.StudentAnswer = studentAnswer;
                    await _punctuationAnswerRepository.UpdateAsync(existingAnswer);
                    savedAnswer = existingAnswer;
                }
                else
                {
                    var answer = new PunctuationAnswer
                    {
                        TestResultId = testResultId,
                        PunctuationQuestionId = questionId,
                        StudentAnswer = studentAnswer,
                        IsCorrect = false,
                        Points = 0
                    };

                    var id = await _punctuationAnswerRepository.CreateAsync(answer);
                    answer.Id = id;
                    savedAnswer = answer;
                }

                // Переполучаем все ответы и удаляем старые дубликаты для этого вопроса (кроме только что сохраненного ответа)
                var allAnswers = await _punctuationAnswerRepository.GetByTestResultIdAsync(testResultId);
                var duplicates = allAnswers
                    .Where(a => a.PunctuationQuestionId == questionId && a.Id != savedAnswer.Id)
                    .ToList();

                foreach (var duplicate in duplicates)
                {
                    await _punctuationAnswerRepository.DeleteAsync(duplicate.Id);
                }

                return savedAnswer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении ответа по пунктуации. TestResultId: {TestResultId}, QuestionId: {QuestionId}", testResultId, questionId);
                throw;
            }
        }

        public async Task<OrthoeopyAnswer> SaveOrthoeopyAnswerAsync(int testResultId, int questionId, int studentAnswer)
        {
            try
            {
                var existingAnswers = await _orthoeopyAnswerRepository.GetByTestResultIdAsync(testResultId);
                var existingAnswer = existingAnswers.FirstOrDefault(a => a.OrthoeopyQuestionId == questionId);

                OrthoeopyAnswer savedAnswer;

                if (existingAnswer != null)
                {
                    existingAnswer.StudentAnswer = studentAnswer;
                    await _orthoeopyAnswerRepository.UpdateAsync(existingAnswer);
                    savedAnswer = existingAnswer;
                }
                else
                {
                    var answer = new OrthoeopyAnswer
                    {
                        TestResultId = testResultId,
                        OrthoeopyQuestionId = questionId,
                        StudentAnswer = studentAnswer,
                        IsCorrect = false,
                        Points = 0
                    };

                    var id = await _orthoeopyAnswerRepository.CreateAsync(answer);
                    answer.Id = id;
                    savedAnswer = answer;
                }

                // Переполучаем все ответы и удаляем старые дубликаты для этого вопроса (кроме только что сохраненного ответа)
                var allAnswers = await _orthoeopyAnswerRepository.GetByTestResultIdAsync(testResultId);
                var duplicates = allAnswers
                    .Where(a => a.OrthoeopyQuestionId == questionId && a.Id != savedAnswer.Id)
                    .ToList();

                foreach (var duplicate in duplicates)
                {
                    await _orthoeopyAnswerRepository.DeleteAsync(duplicate.Id);
                }

                return savedAnswer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении ответа по орфоэпии. TestResultId: {TestResultId}, QuestionId: {QuestionId}", testResultId, questionId);
                throw;
            }
        }

        public async Task<RegularAnswer> SaveRegularAnswerAsync(int testResultId, int questionId, string? studentAnswer, int? selectedOptionId)
        {
            try
            {
                var existingAnswers = await _regularAnswerRepository.GetByTestResultIdAsync(testResultId);
                var existingAnswer = existingAnswers.FirstOrDefault(a => a.RegularQuestionId == questionId);

                RegularAnswer savedAnswer;

                if (existingAnswer != null)
                {
                    existingAnswer.StudentAnswer = studentAnswer;
                    existingAnswer.SelectedOptionId = selectedOptionId;
                    await _regularAnswerRepository.UpdateAsync(existingAnswer);
                    savedAnswer = existingAnswer;
                }
                else
                {
                    var answer = new RegularAnswer
                    {
                        TestResultId = testResultId,
                        RegularQuestionId = questionId,
                        StudentAnswer = studentAnswer,
                        SelectedOptionId = selectedOptionId,
                        IsCorrect = false,
                        Points = 0
                    };

                    var id = await _regularAnswerRepository.CreateAsync(answer);
                    answer.Id = id;
                    savedAnswer = answer;
                }

                // Переполучаем все ответы и удаляем старые дубликаты для этого вопроса (кроме только что сохраненного ответа)
                var allAnswers = await _regularAnswerRepository.GetByTestResultIdAsync(testResultId);
                var duplicates = allAnswers
                    .Where(a => a.RegularQuestionId == questionId && a.Id != savedAnswer.Id)
                    .ToList();

                foreach (var duplicate in duplicates)
                {
                    await _regularAnswerRepository.DeleteAsync(duplicate.Id);
                }

                return savedAnswer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении ответа на классический тест. TestResultId: {TestResultId}, QuestionId: {QuestionId}", testResultId, questionId);
                throw;
            }
        }

        public async Task<List<SpellingAnswer>> GetSpellingAnswersAsync(int testResultId)
        {
            return await _spellingAnswerRepository.GetByTestResultIdAsync(testResultId);
        }

        public async Task<List<PunctuationAnswer>> GetPunctuationAnswersAsync(int testResultId)
        {
            return await _punctuationAnswerRepository.GetByTestResultIdAsync(testResultId);
        }

        public async Task<List<OrthoeopyAnswer>> GetOrthoeopyAnswersAsync(int testResultId)
        {
            return await _orthoeopyAnswerRepository.GetByTestResultIdAsync(testResultId);
        }

        public async Task<List<RegularAnswer>> GetRegularAnswersAsync(int testResultId)
        {
            return await _regularAnswerRepository.GetByTestResultIdAsync(testResultId);
        }

        public async Task UpdateAnswerAsync<T>(T answer) where T : Answer
        {
            try
            {
                if (answer is SpellingAnswer spellingAnswer)
                {
                    await _spellingAnswerRepository.UpdateAsync(spellingAnswer);
                }
                else if (answer is PunctuationAnswer punctuationAnswer)
                {
                    await _punctuationAnswerRepository.UpdateAsync(punctuationAnswer);
                }
                else if (answer is OrthoeopyAnswer orthoeopyAnswer)
                {
                    await _orthoeopyAnswerRepository.UpdateAsync(orthoeopyAnswer);
                }
                else if (answer is RegularAnswer regularAnswer)
                {
                    await _regularAnswerRepository.UpdateAsync(regularAnswer);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении ответа. AnswerId: {AnswerId}", answer.Id);
                throw;
            }
        }
    }
}

