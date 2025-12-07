using Microsoft.Extensions.Logging;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Services
{
    /// <summary>
    /// Сервис для проверок безопасности при работе с тестами
    /// </summary>
    public class SecurityValidationService
    {
        private readonly ILogger<SecurityValidationService> _logger;

        public SecurityValidationService(ILogger<SecurityValidationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Проверяет, что студент имеет доступ к результату теста
        /// </summary>
        public bool ValidateStudentAccessToResult<T>(T testResult, int studentId) where T : TestResult
        {
            if (testResult == null)
            {
                _logger.LogWarning("Попытка доступа к несуществующему результату теста. StudentId: {StudentId}", studentId);
                return false;
            }

            if (testResult.StudentId != studentId)
            {
                _logger.LogWarning("Попытка доступа к результату теста другого студента. ResultId: {ResultId}, StudentId: {StudentId}, OwnerId: {OwnerId}",
                    testResult.Id, studentId, testResult.StudentId);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет, что тест не завершен
        /// </summary>
        public bool ValidateTestNotCompleted<T>(T testResult) where T : TestResult
        {
            if (testResult == null)
            {
                return false;
            }

            if (testResult.IsCompleted)
            {
                _logger.LogWarning("Попытка изменить завершенный тест. ResultId: {ResultId}", testResult.Id);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет, что время теста не истекло (с учетом небольшого буфера)
        /// </summary>
        public bool ValidateTimeLimit(DateTime startedAt, int timeLimitMinutes, int bufferSeconds = 30)
        {
            var elapsed = DateTime.Now - startedAt;
            var timeLimit = TimeSpan.FromMinutes(timeLimitMinutes);
            var buffer = TimeSpan.FromSeconds(bufferSeconds);

            if (elapsed > timeLimit + buffer)
            {
                _logger.LogWarning("Попытка продолжить тест после истечения времени. StartedAt: {StartedAt}, TimeLimit: {TimeLimit}, Elapsed: {Elapsed}",
                    startedAt, timeLimitMinutes, elapsed);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет, что время теста не истекло, используя сохраненное оставшееся время если оно есть
        /// </summary>
        public bool ValidateTimeLimitWithRemaining(DateTime startedAt, int timeLimitMinutes, int? timeRemainingSeconds, int bufferSeconds = 30)
        {
            // Если есть сохраненное оставшееся время (тест был на паузе), используем его
            if (timeRemainingSeconds.HasValue)
            {
                // Проверяем, что оставшееся время больше буфера
                if (timeRemainingSeconds.Value <= bufferSeconds)
                {
                    _logger.LogWarning("Попытка продолжить тест после истечения времени. TimeRemaining: {TimeRemaining} секунд",
                        timeRemainingSeconds.Value);
                    return false;
                }
                return true;
            }

            // Если сохраненного времени нет, используем стандартную проверку на основе StartedAt
            return ValidateTimeLimit(startedAt, timeLimitMinutes, bufferSeconds);
        }

        /// <summary>
        /// Валидирует ответ студента (защита от инъекций и слишком длинных ответов)
        /// </summary>
        public bool ValidateAnswer(string? answer, int maxLength = 1000)
        {
            if (string.IsNullOrWhiteSpace(answer))
            {
                return true;
            }

            if (answer.Length > maxLength)
            {
                _logger.LogWarning("Попытка отправить слишком длинный ответ. Length: {Length}, MaxLength: {MaxLength}",
                    answer.Length, maxLength);
                return false;
            }

            var dangerousPatterns = new[] { "<script", "javascript:", "onerror=", "onload=" };
            var answerLower = answer.ToLowerInvariant();
            
            foreach (var pattern in dangerousPatterns)
            {
                if (answerLower.Contains(pattern))
                {
                    _logger.LogWarning("Обнаружен потенциально опасный паттерн в ответе: {Pattern}", pattern);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Валидирует ID вопроса (защита от манипуляций)
        /// </summary>
        public bool ValidateQuestionId(int questionId, List<int> validQuestionIds)
        {
            if (questionId <= 0)
            {
                _logger.LogWarning("Попытка использовать невалидный ID вопроса: {QuestionId}", questionId);
                return false;
            }

            if (!validQuestionIds.Contains(questionId))
            {
                _logger.LogWarning("Попытка ответить на вопрос, не принадлежащий тесту. QuestionId: {QuestionId}", questionId);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет, что попытка не превышает максимальное количество
        /// </summary>
        public bool ValidateAttemptLimit(int currentAttempt, int maxAttempts)
        {
            if (maxAttempts > 0 && currentAttempt > maxAttempts)
            {
                _logger.LogWarning("Попытка превышает максимальное количество. CurrentAttempt: {CurrentAttempt}, MaxAttempts: {MaxAttempts}",
                    currentAttempt, maxAttempts);
                return false;
            }

            return true;
        }
    }
}

