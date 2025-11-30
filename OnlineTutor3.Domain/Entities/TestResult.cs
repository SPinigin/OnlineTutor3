using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Domain.Entities
{
    /// <summary>
    /// Базовый класс для результатов тестов
    /// </summary>
    public abstract class TestResult
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        public int AttemptNumber { get; set; } = 1;

        public DateTime StartedAt { get; set; } = DateTime.Now;

        public DateTime? CompletedAt { get; set; }

        public int Score { get; set; } = 0;

        public int MaxScore { get; set; } = 0;

        public double Percentage { get; set; } = 0.0;

        public bool IsCompleted { get; set; } = false;
    }

    /// <summary>
    /// Результат теста по орфографии
    /// </summary>
    public class SpellingTestResult : TestResult
    {
        [Required]
        public int SpellingTestId { get; set; }
    }

    /// <summary>
    /// Результат теста по пунктуации
    /// </summary>
    public class PunctuationTestResult : TestResult
    {
        [Required]
        public int PunctuationTestId { get; set; }
    }

    /// <summary>
    /// Результат теста по орфоэпии
    /// </summary>
    public class OrthoeopyTestResult : TestResult
    {
        [Required]
        public int OrthoeopyTestId { get; set; }
    }

    /// <summary>
    /// Результат классического теста
    /// </summary>
    public class RegularTestResult : TestResult
    {
        [Required]
        public int RegularTestId { get; set; }
    }
}

